<?php
/**
 * Bereinigungsskript für alte Lobbys
 *
 * Löscht alle Matches, die älter als MATCH_TIMEOUT_MINUTES sind.
 * Kann manuell ausgeführt oder als Cron-Job eingerichtet werden.
 *
 * Verwendung:
 * - Manuell: php cleanup_old_lobbies.php
 * - Cron-Job: */5 * * * * php /pfad/zu/cleanup_old_lobbies.php
 * - Browser: http://ihre-domain.com/php/cleanup_old_lobbies.php
 */

require_once __DIR__ . '/config.php';

header('Content-Type: application/json');

try {
    $db = getDbConnection();

    // Timeout-Zeitstempel berechnen
    $timeout_minutes = MATCH_TIMEOUT_MINUTES;
    $cutoff_time = date('Y-m-d H:i:s', time() - ($timeout_minutes * 60));

    // Alte Matches finden
    $stmt = $db->prepare("
        SELECT room_code, created_at, status
        FROM matches
        WHERE created_at < ?
        AND status IN ('waiting', 'running')
    ");
    $stmt->execute([$cutoff_time]);
    $old_matches = $stmt->fetchAll(PDO::FETCH_ASSOC);

    $deleted_count = 0;
    $deleted_codes = [];

    if (count($old_matches) > 0) {
        // Alte Matches löschen (CASCADE löscht auch signaling_messages)
        $delete_stmt = $db->prepare("
            DELETE FROM matches
            WHERE created_at < ?
            AND status IN ('waiting', 'running')
        ");
        $delete_stmt->execute([$cutoff_time]);
        $deleted_count = $delete_stmt->rowCount();

        // Gelöschte Codes für Protokollierung sammeln
        foreach ($old_matches as $match) {
            $deleted_codes[] = $match['room_code'];
        }
    }

    $result = [
        'success' => true,
        'deleted_count' => $deleted_count,
        'cutoff_time' => $cutoff_time,
        'timeout_minutes' => $timeout_minutes,
        'deleted_codes' => $deleted_codes,
        'timestamp' => date('Y-m-d H:i:s')
    ];

    // Protokollierung
    error_log(sprintf(
        "[Lobby-Bereinigung] %d alte Lobbys gelöscht (älter als %s): %s",
        $deleted_count,
        $cutoff_time,
        implode(', ', $deleted_codes)
    ));

    echo json_encode($result, JSON_PRETTY_PRINT);

} catch (PDOException $e) {
    http_response_code(500);
    echo json_encode([
        'success' => false,
        'error' => 'Datenbankfehler: ' . $e->getMessage()
    ], JSON_PRETTY_PRINT);
    error_log("[Lobby-Bereinigungsfehler] " . $e->getMessage());
}
