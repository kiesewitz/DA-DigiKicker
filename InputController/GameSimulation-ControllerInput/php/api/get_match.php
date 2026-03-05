<?php
// =============================================================================
// DigiKicker Online Multiplayer - Match Status abrufen
// =============================================================================
// GET Endpoint: Holt aktuellen Status eines Matches (für Host-Polling)
// Parameter: room_code
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// Nur GET erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    errorResponse('Method not allowed', 405);
}

try {
    $roomCode = strtoupper(trim($_GET['room_code'] ?? ''));

    if (empty($roomCode)) {
        errorResponse('room_code is required');
    }

    $pdo = getDbConnection();

    // Match abrufen
    $stmt = $pdo->prepare("
        SELECT id, room_code, host_name, joiner_name, status,
               created_at, started_at, finished_at,
               winner_name, score_host, score_joiner
        FROM matches
        WHERE room_code = ?
    ");
    $stmt->execute([$roomCode]);
    $match = $stmt->fetch();

    if (!$match) {
        errorResponse('Match not found');
    }

    // Typen korrigieren
    $match['id'] = (int)$match['id'];
    $match['score_host'] = (int)$match['score_host'];
    $match['score_joiner'] = (int)$match['score_joiner'];

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'match' => $match
    ]);

} catch (PDOException $e) {
    error_log("get_match error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
