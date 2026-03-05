<?php
// =============================================================================
// DigiKicker Online Multiplayer - Signaling Message senden
// =============================================================================
// POST Endpoint: Speichert WebRTC Signaling Daten (Offer/Answer/ICE Candidates)
// Parameter: room_code, sender_role (host/joiner), msg_type (offer/answer/candidate), payload (JSON)
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
    $senderRole = strtolower(trim($input['sender_role'] ?? ''));
    $msgType = strtolower(trim($input['msg_type'] ?? ''));
    $payload = $input['payload'] ?? null;

    if (empty($roomCode)) {
        errorResponse('room_code is required');
    }

    // Sender Role validieren
    if (!in_array($senderRole, ['host', 'joiner'])) {
        errorResponse('sender_role must be "host" or "joiner"');
    }

    // Message Type validieren
    if (!in_array($msgType, ['offer', 'answer', 'candidate'])) {
        errorResponse('msg_type must be "offer", "answer" or "candidate"');
    }

    if (empty($payload)) {
        errorResponse('payload is required');
    }

    // Payload als JSON String speichern
    $payloadJson = is_string($payload) ? $payload : json_encode($payload);

    $pdo = getDbConnection();

    // Match suchen
    $stmt = $pdo->prepare("SELECT id, status FROM matches WHERE room_code = ?");
    $stmt->execute([$roomCode]);
    $match = $stmt->fetch();

    if (!$match) {
        errorResponse('Match not found');
    }

    // Signaling Message speichern
    $stmt = $pdo->prepare("
        INSERT INTO signaling_messages (match_id, sender_role, msg_type, payload, created_at)
        VALUES (:match_id, :sender_role, :msg_type, :payload, NOW())
    ");

    $stmt->execute([
        'match_id' => $match['id'],
        'sender_role' => $senderRole,
        'msg_type' => $msgType,
        'payload' => $payloadJson
    ]);

    $messageId = $pdo->lastInsertId();

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'message_id' => (int)$messageId,
        'msg_type' => $msgType,
        'sender_role' => $senderRole
    ]);

} catch (PDOException $e) {
    error_log("push_signal error: " . $e->getMessage());
    errorResponse('Database error', 500);
}
