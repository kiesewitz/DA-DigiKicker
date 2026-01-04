<?php
// =============================================================================
// DigiKicker Online Multiplayer - Match beitreten (Joiner)
// =============================================================================
// POST Endpoint: Tritt einem wartenden Match bei
// Parameter: room_code (string), joiner_name (string)
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// Nur POST erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    errorResponse('Method not allowed', 405);
}

try {
    // JSON Body parsen
    $input = json_decode(file_get_contents('php://input'), true);

    // Parameter validieren
    $roomCode = strtoupper(trim($input['room_code'] ?? ''));
    $joinerName = trim($input['joiner_name'] ?? '');

    if (empty($roomCode)) {
        errorResponse('room_code is required');
    }
    if (empty($joinerName)) {
        errorResponse('joiner_name is required');
    }
    if (strlen($joinerName) > 50) {
        errorResponse('joiner_name too long (max 50 chars)');
    }

    $pdo = getDbConnection();

    // Match suchen (muss status = 'waiting' haben)
    $stmt = $pdo->prepare("
        SELECT id, host_name, host_team, status
        FROM matches
        WHERE room_code = ?
        FOR UPDATE
    ");
    $stmt->execute([$roomCode]);
    $match = $stmt->fetch();

    if (!$match) {
        errorResponse('Match not found');
    }

    if ($match['status'] !== 'waiting') {
        errorResponse('Match is not available (status: ' . $match['status'] . ')');
    }

    // Match auf 'running' setzen und Joiner-Name speichern
    $stmt = $pdo->prepare("
        UPDATE matches
        SET joiner_name = :joiner_name,
            status = 'running',
            started_at = NOW()
        WHERE id = :match_id
    ");

    $stmt->execute([
        'joiner_name' => $joinerName,
        'match_id' => $match['id']
    ]);

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'match_id' => (int)$match['id'],
        'room_code' => $roomCode,
        'host_name' => $match['host_name'],
        'host_team' => $match['host_team'],
        'joiner_name' => $joinerName,
        'status' => 'running'
    ]);

} catch (PDOException $e) {
    error_log("join_match error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
