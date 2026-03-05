<?php
// =============================================================================
// DigiKicker Online Multiplayer - API Index
// =============================================================================
// Zeigt verfügbare Endpoints an (für Entwickler/Debugging)
// =============================================================================

header('Content-Type: application/json; charset=utf-8');
header('Access-Control-Allow-Origin: *');

$endpoints = [
    'endpoints' => [
        [
            'method' => 'POST',
            'path' => '/api/create_match.php',
            'description' => 'Erstellt ein neues Match (Host)',
            'parameters' => ['host_name' => 'string']
        ],
        [
            'method' => 'POST',
            'path' => '/api/join_match.php',
            'description' => 'Tritt einem Match bei (Joiner)',
            'parameters' => ['room_code' => 'string', 'joiner_name' => 'string']
        ],
        [
            'method' => 'POST',
            'path' => '/api/push_signal.php',
            'description' => 'Sendet WebRTC Signaling Daten',
            'parameters' => ['room_code' => 'string', 'sender_role' => 'host|joiner', 'msg_type' => 'offer|answer|candidate', 'payload' => 'object']
        ],
        [
            'method' => 'GET',
            'path' => '/api/pull_signal.php',
            'description' => 'Holt WebRTC Signaling Daten',
            'parameters' => ['room_code' => 'string', 'role' => 'host|joiner', 'last_id' => 'int (optional)']
        ],
        [
            'method' => 'GET',
            'path' => '/api/get_match.php',
            'description' => 'Holt Match-Status',
            'parameters' => ['room_code' => 'string']
        ],
        [
            'method' => 'GET',
            'path' => '/api/list_matches.php',
            'description' => 'Listet Matches',
            'parameters' => ['status' => 'waiting|running|finished|all (optional)']
        ],
        [
            'method' => 'POST',
            'path' => '/api/finish_match.php',
            'description' => 'Beendet ein Match',
            'parameters' => ['room_code' => 'string', 'winner_name' => 'string (optional)', 'score_host' => 'int', 'score_joiner' => 'int']
        ]
    ],
    'website' => '/public/index.php',
    'version' => '1.0.0'
];

echo json_encode($endpoints, JSON_PRETTY_PRINT | JSON_UNESCAPED_UNICODE);
