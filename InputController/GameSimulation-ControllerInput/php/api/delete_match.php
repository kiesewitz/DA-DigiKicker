<?php
// =============================================================================
// DigiKicker Online Multiplayer - Match löschen
// =============================================================================
// DELETE/POST Endpoint: Löscht ein Match (für Lobby-Cleanup)
// Parameter: room_code (string) - Der zu löschende Room Code
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// DELETE oder POST erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'DELETE' && $_SERVER['REQUEST_METHOD'] !== 'POST') {
    errorResponse('Method not allowed', 405);
}

try {
    // JSON Body parsen
    $input = json_decode(file_get_contents('php://input'), true);

    // Room Code validieren
    $roomCode = strtoupper(trim($input['room_code'] ?? ''));

    if (empty($roomCode)) {
        errorResponse('room_code is required');
    }

    $pdo = getDbConnection();

    // Match suchen
    $stmt = $pdo->prepare("SELECT id, status FROM matches WHERE room_code = ?");
    $stmt->execute([$roomCode]);
    $match = $stmt->fetch();

    if (!$match) {
        errorResponse('Match not found');
    }

    // Nur waiting und running Matches können gelöscht werden
    // finished Matches bleiben für Statistiken erhalten
    if ($match['status'] === 'finished') {
        errorResponse('Cannot delete finished match');
    }

    // Match löschen (CASCADE löscht auch signaling_messages)
    $stmt = $pdo->prepare("DELETE FROM matches WHERE id = ?");
    $stmt->execute([$match['id']]);

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'match_id' => (int)$match['id'],
        'room_code' => $roomCode,
        'deleted' => true
    ]);

} catch (PDOException $e) {
    error_log("delete_match error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
