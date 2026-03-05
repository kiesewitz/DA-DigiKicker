<?php
// =============================================================================
// DigiKicker Online Multiplayer - Match beenden
// =============================================================================
// POST Endpoint: Markiert ein Match als beendet und speichert Ergebnis
// Parameter: room_code, winner_name (optional), score_host, score_joiner
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
    $winnerName = trim($input['winner_name'] ?? '');
    $scoreHost = (int)($input['score_host'] ?? 0);
    $scoreJoiner = (int)($input['score_joiner'] ?? 0);

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

    // Match auf 'finished' setzen
    $stmt = $pdo->prepare("
        UPDATE matches
        SET status = 'finished',
            finished_at = NOW(),
            winner_name = :winner_name,
            score_host = :score_host,
            score_joiner = :score_joiner
        WHERE id = :match_id
    ");

    $stmt->execute([
        'winner_name' => $winnerName ?: null,
        'score_host' => $scoreHost,
        'score_joiner' => $scoreJoiner,
        'match_id' => $match['id']
    ]);

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'match_id' => (int)$match['id'],
        'room_code' => $roomCode,
        'status' => 'finished',
        'winner_name' => $winnerName ?: null,
        'score_host' => $scoreHost,
        'score_joiner' => $scoreJoiner
    ]);

} catch (PDOException $e) {
    error_log("finish_match error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
