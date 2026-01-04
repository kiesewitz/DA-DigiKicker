<?php
// =============================================================================
// DigiKicker Online Multiplayer - Datenbank Konfiguration
// =============================================================================
// Zentrale Konfigurationsdatei für Datenbankverbindung und globale Einstellungen
// =============================================================================

// Datenbank Zugangsdaten
define('DB_HOST', 'localhost');
define('DB_NAME', 'trath_Diplomarbeit');
define('DB_USER', 'DA');
define('DB_PASS', 'mEOTlct0swzv48__');

// PDO Optionen für sichere Datenbankverbindung
define('DB_OPTIONS', [
    PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,           // Exceptions bei Fehlern
    PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,      // Assoziative Arrays
    PDO::ATTR_EMULATE_PREPARES => false,                   // Echte Prepared Statements
    PDO::MYSQL_ATTR_INIT_COMMAND => "SET NAMES utf8mb4"    // UTF-8 Unterstützung
]);

// Globale Einstellungen
define('ROOM_CODE_LENGTH', 6);                              // Länge des Room Codes
define('MATCH_TIMEOUT_MINUTES', 15);                        // Alte waiting Matches löschen nach X Minuten (etwas länger als 10min Godot-Timer)

/**
 * Erstellt eine PDO Datenbankverbindung
 *
 * @return PDO Die Datenbankverbindung
 * @throws PDOException Bei Verbindungsfehlern
 */
function getDbConnection(): PDO {
    static $pdo = null;

    // Singleton Pattern: Verbindung nur einmal erstellen
    if ($pdo === null) {
        $dsn = "mysql:host=" . DB_HOST . ";dbname=" . DB_NAME . ";charset=utf8mb4";
        $pdo = new PDO($dsn, DB_USER, DB_PASS, DB_OPTIONS);
    }

    return $pdo;
}

/**
 * Generiert einen zufälligen alphanumerischen Room Code
 *
 * @param int $length Länge des Codes (Standard: 6)
 * @return string Der generierte Room Code (Großbuchstaben + Zahlen)
 */
function generateRoomCode(int $length = ROOM_CODE_LENGTH): string {
    // Nur eindeutige Zeichen (keine 0/O, 1/I Verwechslung)
    $chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    $code = '';

    for ($i = 0; $i < $length; $i++) {
        $code .= $chars[random_int(0, strlen($chars) - 1)];
    }

    return $code;
}

/**
 * Setzt CORS Header für Cross-Origin Requests (Godot Client)
 * Muss VOR jeglicher Ausgabe aufgerufen werden
 */
function setCorsHeaders(): void {
    // Erlaubt Zugriff von allen Origins (für Godot Client)
    header('Access-Control-Allow-Origin: *');
    header('Access-Control-Allow-Methods: GET, POST, OPTIONS');
    header('Access-Control-Allow-Headers: Content-Type, Accept');
    header('Access-Control-Max-Age: 86400'); // Preflight Cache 24h

    // OPTIONS Request (Preflight) direkt beantworten
    if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
        http_response_code(204);
        exit();
    }
}

/**
 * Sendet eine JSON Antwort und beendet das Script
 *
 * @param array $data Die zu sendenden Daten
 * @param int $statusCode HTTP Status Code (Standard: 200)
 */
function jsonResponse(array $data, int $statusCode = 200): void {
    http_response_code($statusCode);
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode($data, JSON_UNESCAPED_UNICODE);
    exit();
}

/**
 * Sendet eine Fehler-Antwort
 *
 * @param string $message Fehlermeldung
 * @param int $statusCode HTTP Status Code (Standard: 400)
 */
function errorResponse(string $message, int $statusCode = 400): void {
    jsonResponse(['success' => false, 'error' => $message], $statusCode);
}

/**
 * Bereinigt alte Matches die noch auf "waiting" stehen
 * Wird bei jedem API Call aufgerufen um die DB sauber zu halten
 */
function cleanupOldMatches(): void {
    try {
        $pdo = getDbConnection();
        $minutes = MATCH_TIMEOUT_MINUTES;

        // Lösche waiting Matches älter als X Minuten
        $stmt = $pdo->prepare("
            DELETE FROM matches
            WHERE status = 'waiting'
            AND created_at < DATE_SUB(NOW(), INTERVAL :minutes MINUTE)
        ");
        $stmt->execute(['minutes' => $minutes]);

    } catch (PDOException $e) {
        // Fehler beim Cleanup ignorieren (nicht kritisch)
        error_log("Cleanup error: " . $e->getMessage());
    }
}
