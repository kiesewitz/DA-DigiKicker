<?php
// =============================================================================
// DigiKicker Online Multiplayer - Match erstellen (Host)
// =============================================================================
// POST Endpoint: Erstellt ein neues Match und gibt Room Code zurück
// Parameter: host_name (string) - Name des Host-Spielers
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// Nur POST erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    errorResponse('Method not allowed', 405);
}

// Alte Matches aufräumen
cleanupOldMatches();

try {
    // JSON Body parsen
    $input = json_decode(file_get_contents('php://input'), true);

    // Host-Name validieren
    $hostName = trim($input['host_name'] ?? '');
    if (empty($hostName)) {
        errorResponse('host_name is required');
    }
    if (strlen($hostName) > 50) {
        errorResponse('host_name too long (max 50 chars)');
    }

    // Host-Team validieren (optional, Default: 'red')
    $hostTeam = strtolower(trim($input['host_team'] ?? 'red'));
    if (!in_array($hostTeam, ['red', 'blue'])) {
        errorResponse('host_team must be "red" or "blue"');
    }

    $pdo = getDbConnection();

    // Room Code generieren (mit Kollisionsprüfung)
    $roomCode = '';
    $maxAttempts = 10;

    for ($i = 0; $i < $maxAttempts; $i++) {
        $roomCode = generateRoomCode();

        // Prüfe ob Code bereits existiert
        $stmt = $pdo->prepare("SELECT id FROM matches WHERE room_code = ?");
        $stmt->execute([$roomCode]);

        if (!$stmt->fetch()) {
            break; // Einzigartiger Code gefunden
        }

        if ($i === $maxAttempts - 1) {
            errorResponse('Could not generate unique room code', 500);
        }
    }

    // Match in DB einfügen
    $stmt = $pdo->prepare("
        INSERT INTO matches (room_code, host_name, host_team, status, created_at)
        VALUES (:room_code, :host_name, :host_team, 'waiting', NOW())
    ");

    $stmt->execute([
        'room_code' => $roomCode,
        'host_name' => $hostName,
        'host_team' => $hostTeam
    ]);

    $matchId = $pdo->lastInsertId();

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'match_id' => (int)$matchId,
        'room_code' => $roomCode,
        'host_name' => $hostName,
        'host_team' => $hostTeam,
        'status' => 'waiting'
    ]);

} catch (PDOException $e) {
    error_log("create_match error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
