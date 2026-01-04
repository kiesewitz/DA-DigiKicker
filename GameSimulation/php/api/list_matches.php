<?php
// =============================================================================
// DigiKicker Online Multiplayer - Matches auflisten
// =============================================================================
// GET Endpoint: Listet offene und laufende Matches für die Lobby
// Parameter: status (optional: waiting, running, finished, all) - default: all aktiven
// =============================================================================

require_once __DIR__ . '/../config.php';

// CORS Header setzen
setCorsHeaders();

// Nur GET erlaubt
if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    errorResponse('Method not allowed', 405);
}

// Alte Matches aufräumen
cleanupOldMatches();
expire_long_running_matches();

try {
    $status = strtolower(trim($_GET['status'] ?? ''));
    $runningLimitMinutes = 15; // Nach 15 Minuten nicht mehr in "running" Listen anzeigen

    $pdo = getDbConnection();
    $params = [];

    // SQL Query basierend auf Status Filter
    if ($status === 'waiting') {
        // Nur wartende Matches (für Lobby)
        $sql = "
            SELECT id, room_code, host_name, host_team, joiner_name, status, created_at, started_at
            FROM matches
            WHERE status = 'waiting'
            ORDER BY created_at DESC
            LIMIT 50
        ";
    } elseif ($status === 'running') {
        // Nur laufende Matches
        $sql = "
            SELECT id, room_code, host_name, host_team, joiner_name, status, created_at, started_at
            FROM matches
            WHERE status = 'running'
              AND COALESCE(started_at, created_at) >= DATE_SUB(NOW(), INTERVAL :run_limit MINUTE)
            ORDER BY started_at DESC
            LIMIT 50
        ";
        $params['run_limit'] = $runningLimitMinutes;
    } elseif ($status === 'finished') {
        // Beendete Matches (letzte 50)
        $sql = "
            SELECT id, room_code, host_name, host_team, joiner_name, status,
                   created_at, started_at, finished_at, winner_name, score_host, score_joiner
            FROM matches
            WHERE status = 'finished'
            ORDER BY finished_at DESC
            LIMIT 50
        ";
    } else {
        // Alle aktiven (waiting + running)
        $sql = "
            SELECT id, room_code, host_name, host_team, joiner_name, status, created_at, started_at
            FROM matches
            WHERE status = 'waiting'
               OR (status = 'running' AND COALESCE(started_at, created_at) >= DATE_SUB(NOW(), INTERVAL :run_limit MINUTE))
            ORDER BY
                CASE status WHEN 'waiting' THEN 0 ELSE 1 END,
                created_at DESC
            LIMIT 100
        ";
        $params['run_limit'] = $runningLimitMinutes;
    }

    if (empty($params)) {
        $stmt = $pdo->query($sql);
    } else {
        $stmt = $pdo->prepare($sql);
        $stmt->execute($params);
    }
    $matches = $stmt->fetchAll();

    // IDs als Integer casten
    foreach ($matches as &$match) {
        $match['id'] = (int)$match['id'];
        if (isset($match['score_host'])) {
            $match['score_host'] = (int)$match['score_host'];
            $match['score_joiner'] = (int)$match['score_joiner'];
        }
    }

    // Statistik hinzufügen
    $stats = [];
    $statsStmt = $pdo->query("
        SELECT status, COUNT(*) as count
        FROM matches
        GROUP BY status
    ");
    while ($row = $statsStmt->fetch()) {
        $stats[$row['status']] = (int)$row['count'];
    }

    // Erfolg zurückgeben
    jsonResponse([
        'success' => true,
        'matches' => $matches,
        'count' => count($matches),
        'stats' => $stats
    ]);

} catch (PDOException $e) {
    error_log("list_matches error: " . $e->getMessage());
    errorResponse('Database error', 500);
}

/**
 * Markiert laufende Spiele als beendet, wenn sie länger als 15 Minuten laufen.
 * Die Ergebnisse bleiben gespeichert, sie erscheinen nur nicht mehr als "running".
 */
function expire_long_running_matches(): void {
    try {
        $pdo = getDbConnection();
        $minutes = 15;

        $stmt = $pdo->prepare("
            UPDATE matches
            SET status = 'finished',
                finished_at = COALESCE(finished_at, NOW())
            WHERE status = 'running'
              AND COALESCE(started_at, created_at) < DATE_SUB(NOW(), INTERVAL :minutes MINUTE)
        ");
        $stmt->execute(['minutes' => $minutes]);
    } catch (PDOException $e) {
        error_log("expire_long_running_matches error: " . $e->getMessage());
    }
}
