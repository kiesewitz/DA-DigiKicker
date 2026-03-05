<?php
// =============================================================================
// DigiKicker Online Multiplayer - Signaling Messages abrufen
// =============================================================================
// GET Endpoint: Holt neue WebRTC Signaling Daten seit last_id
// Parameter: room_code, role (host/joiner), last_id (optional, default 0)
// Host bekommt Nachrichten von Joiner, Joiner bekommt Nachrichten von Host
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// Nur GET erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    errorResponse('Method not allowed', 405);
}

try {
    // Parameter aus Query String
    $roomCode = strtoupper(trim($_GET['room_code'] ?? ''));
    $role = strtolower(trim($_GET['role'] ?? ''));
    $lastId = (int)($_GET['last_id'] ?? 0);

    if (empty($roomCode)) {
        errorResponse('room_code is required');
    }

    // Role validieren
    if (!in_array($role, ['host', 'joiner'])) {
        errorResponse('role must be "host" or "joiner"');
    }

    // Gegenteilige Rolle bestimmen (Host holt Joiner-Nachrichten und umgekehrt)
    $otherRole = ($role === 'host') ? 'joiner' : 'host';

    $pdo = getDbConnection();

    // Match suchen
    $stmt = $pdo->prepare("SELECT id, status, host_name, joiner_name FROM matches WHERE room_code = ?");
    $stmt->execute([$roomCode]);
    $match = $stmt->fetch();

    if (!$match) {
        errorResponse('Match not found');
    }

    // DEBUG: Zähle ALLE Nachrichten für dieses Match (ohne Filter)
    $debugStmt = $pdo->prepare("SELECT COUNT(*) as total, sender_role FROM signaling_messages WHERE match_id = ? GROUP BY sender_role");
    $debugStmt->execute([$match['id']]);
    $debugCounts = $debugStmt->fetchAll();

    // Neue Signaling Messages abrufen (von der anderen Rolle, seit last_id)
    $stmt = $pdo->prepare("
        SELECT id, sender_role, msg_type, payload, created_at
        FROM signaling_messages
        WHERE match_id = :match_id
          AND sender_role = :other_role
          AND id > :last_id
        ORDER BY id ASC
    ");

    $stmt->execute([
        'match_id' => $match['id'],
        'other_role' => $otherRole,
        'last_id' => $lastId
    ]);

    $messages = $stmt->fetchAll();

    // Payload JSON dekodieren
    foreach ($messages as &$msg) {
        $msg['id'] = (int)$msg['id'];
        $decoded = json_decode($msg['payload'], true);
        $msg['payload'] = $decoded ?? $msg['payload'];
    }

    // Erfolg zurückgeben (mit Debug-Info)
    jsonResponse([
        'success' => true,
        'match_status' => $match['status'],
        'host_name' => $match['host_name'],
        'joiner_name' => $match['joiner_name'],
        'messages' => $messages,
        'message_count' => count($messages),
        // DEBUG: Query-Parameter für Diagnose
        'debug' => [
            'requested_room_code' => $roomCode,
            'requested_role' => $role,
            'queried_other_role' => $otherRole,
            'queried_match_id' => (int)$match['id'],
            'queried_last_id' => $lastId,
            'total_messages_in_db' => $debugCounts
        ]
    ]);

} catch (PDOException $e) {
    error_log("pull_signal error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
