<?php
// =============================================================================
// TURN Server Stats Proxy
// =============================================================================
// Ruft die TURN-Server-Statistiken ab und gibt sie als JSON zurück
// Löst das Mixed-Content-Problem (HTTPS -> HTTP)
// =============================================================================

header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');

// TURN Server Status Endpoint
$turn_status_url = 'http://185.119.117.112:8082/status';
$username = 'admin';
$password = 'G7vR!9K@L2p#D4sX8ZQmEwH';

// HTTP-Anfrage mit cURL durchführen
$ch = curl_init($turn_status_url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
curl_setopt($ch, CURLOPT_USERPWD, "$username:$password");
curl_setopt($ch, CURLOPT_TIMEOUT, 5);

$response = curl_exec($ch);
$http_code = curl_getinfo($ch, CURLINFO_HTTP_CODE);
curl_close($ch);

if ($http_code === 200 && $response !== false) {
    // Erfolg - Antwort weiterleiten
    echo $response;
} else {
    // Fehler - Fehlermeldung zurückgeben
    http_response_code(500);
    echo json_encode([
        'error' => 'TURN-Statistiken konnten nicht abgerufen werden',
        'http_code' => $http_code
    ]);
}
