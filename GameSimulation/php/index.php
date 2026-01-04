<?php
// =============================================================================
// DigiKicker Online Multiplayer - Lobby Webseite
// =============================================================================
// Zeigt offene, laufende und beendete Matches an
// Auto-Refresh alle 5 Sekunden
// =============================================================================

require_once __DIR__ . '/config.php';

// Alte Matches aufraumen
cleanupOldMatches();

// Matches laden
$matches = ['waiting' => [], 'running' => [], 'finished' => []];
$stats = ['waiting' => 0, 'running' => 0, 'finished' => 0];

try {
    $pdo = getDbConnection();

    // Wartende Matches
    $stmt = $pdo->query("
        SELECT room_code, host_name, created_at
        FROM matches WHERE status = 'waiting'
        ORDER BY created_at DESC LIMIT 20
    ");
    $matches['waiting'] = $stmt->fetchAll();

    // Laufende Matches
    $stmt = $pdo->query("
        SELECT room_code, host_name, joiner_name, started_at
        FROM matches WHERE status = 'running'
        ORDER BY started_at DESC LIMIT 20
    ");
    $matches['running'] = $stmt->fetchAll();

    // Beendete Matches (letzte 10)
    $stmt = $pdo->query("
        SELECT room_code, host_name, joiner_name, winner_name, score_host, score_joiner, finished_at
        FROM matches WHERE status = 'finished'
        ORDER BY finished_at DESC LIMIT 10
    ");
    $matches['finished'] = $stmt->fetchAll();

    // Statistiken
    $stmt = $pdo->query("SELECT status, COUNT(*) as c FROM matches GROUP BY status");
    while ($row = $stmt->fetch()) {
        $stats[$row['status']] = (int)$row['c'];
    }

} catch (PDOException $e) {
    $error = "Datenbankfehler: " . $e->getMessage();
}

// Zeitformat-Helfer
function formatTime($datetime) {
    if (!$datetime) return '-';
    $dt = new DateTime($datetime);
    return $dt->format('H:i:s');
}

function formatDate($datetime) {
    if (!$datetime) return '-';
    $dt = new DateTime($datetime);
    return $dt->format('d.m.Y H:i');
}

function timeAgo($datetime) {
    if (!$datetime) return '-';
    $dt = new DateTime($datetime);
    $now = new DateTime();
    $diff = $now->diff($dt);

    if ($diff->d > 0) return $diff->d . ' Tag(e)';
    if ($diff->h > 0) return $diff->h . ' Std.';
    if ($diff->i > 0) return $diff->i . ' Min.';
    return 'gerade eben';
}
?>
<!DOCTYPE html>
<html lang="de">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="refresh" content="5">
    <title>DigiKicker - Online Lobby</title>
    <style>
        /* Basis Styling */
        * { box-sizing: border-box; margin: 0; padding: 0; }

        body {
            font-family: 'Segoe UI', Tahoma, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            color: #e8e8e8;
            min-height: 100vh;
            padding: 20px;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
        }

        /* Header */
        header {
            text-align: center;
            margin-bottom: 30px;
            padding: 20px;
            background: rgba(255,255,255,0.05);
            border-radius: 15px;
            border: 1px solid rgba(255,255,255,0.1);
        }

        h1 {
            font-size: 2.5rem;
            color: #00d9ff;
            text-shadow: 0 0 20px rgba(0,217,255,0.5);
            margin-bottom: 10px;
        }

        .subtitle {
            color: #888;
            font-size: 1.1rem;
        }

        /* Statistiken */
        .stats {
            display: flex;
            justify-content: center;
            gap: 30px;
            margin: 20px 0;
            flex-wrap: wrap;
        }

        .stat-box {
            background: rgba(255,255,255,0.1);
            padding: 15px 25px;
            border-radius: 10px;
            text-align: center;
            min-width: 120px;
        }

        .stat-box .number {
            font-size: 2rem;
            font-weight: bold;
        }

        .stat-box.waiting .number { color: #ffd700; }
        .stat-box.running .number { color: #00ff88; }
        .stat-box.finished .number { color: #888; }

        .stat-box .label {
            font-size: 0.9rem;
            color: #aaa;
            margin-top: 5px;
        }

        /* Sections */
        .section {
            margin-bottom: 30px;
            background: rgba(255,255,255,0.05);
            border-radius: 15px;
            padding: 20px;
            border: 1px solid rgba(255,255,255,0.1);
        }

        .section h2 {
            font-size: 1.4rem;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 1px solid rgba(255,255,255,0.1);
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .section.waiting h2 { color: #ffd700; }
        .section.running h2 { color: #00ff88; }
        .section.finished h2 { color: #aaa; }

        /* Tabellen */
        table {
            width: 100%;
            border-collapse: collapse;
        }

        th, td {
            padding: 12px 15px;
            text-align: left;
            border-bottom: 1px solid rgba(255,255,255,0.05);
        }

        th {
            color: #888;
            font-weight: 600;
            font-size: 0.85rem;
            text-transform: uppercase;
        }

        tr:hover { background: rgba(255,255,255,0.03); }

        .room-code {
            font-family: 'Consolas', monospace;
            font-size: 1.1rem;
            font-weight: bold;
            color: #00d9ff;
            background: rgba(0,217,255,0.1);
            padding: 4px 10px;
            border-radius: 5px;
        }

        .player-name {
            color: #fff;
            font-weight: 500;
        }

        .vs { color: #666; margin: 0 5px; }

        .winner {
            color: #ffd700;
            font-weight: bold;
        }

        .score {
            font-family: 'Consolas', monospace;
            font-size: 1.1rem;
            color: #00ff88;
        }

        .time-ago {
            color: #666;
            font-size: 0.9rem;
        }

        /* Empty State */
        .empty {
            text-align: center;
            color: #666;
            padding: 30px;
            font-style: italic;
        }

        /* Footer */
        footer {
            text-align: center;
            padding: 20px;
            color: #555;
            font-size: 0.85rem;
        }

        footer a { color: #00d9ff; text-decoration: none; }
        footer a:hover { text-decoration: underline; }

        /* Responsive */
        @media (max-width: 768px) {
            h1 { font-size: 1.8rem; }
            .stats { gap: 15px; }
            .stat-box { padding: 10px 15px; min-width: 100px; }
            th, td { padding: 8px 10px; font-size: 0.9rem; }
            .room-code { font-size: 0.95rem; }
        }

        /* Animation */
        @keyframes pulse {
            0%, 100% { opacity: 1; }
            50% { opacity: 0.7; }
        }

        .live-indicator {
            display: inline-block;
            width: 8px;
            height: 8px;
            background: #00ff88;
            border-radius: 50%;
            margin-right: 8px;
            animation: pulse 1.5s infinite;
        }
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>DigiKicker</h1>
            <p class="subtitle">Online Multiplayer Lobby</p>

            <!-- Statistiken -->
            <div class="stats">
                <div class="stat-box waiting">
                    <div class="number"><?= $stats['waiting'] ?></div>
                    <div class="label">Wartend</div>
                </div>
                <div class="stat-box running">
                    <div class="number"><?= $stats['running'] ?></div>
                    <div class="label">Laufend</div>
                </div>
                <div class="stat-box finished">
                    <div class="number"><?= $stats['finished'] ?></div>
                    <div class="label">Beendet</div>
                </div>
            </div>

            <!-- TURN Server Statistiken -->
            <div class="stats" style="margin-top: 20px;">
                <div class="stat-box" style="background: rgba(0,217,255,0.1);">
                    <div class="number" id="turn-memory" style="color: #00d9ff;">-</div>
                    <div class="label">TURN Memory (MB)</div>
                </div>
                <div class="stat-box" style="background: rgba(0,217,255,0.1);">
                    <div class="number" id="turn-cpu" style="color: #00d9ff;">-</div>
                    <div class="label">TURN CPU Load</div>
                </div>
                <div class="stat-box" style="background: rgba(0,217,255,0.1);">
                    <div class="number" id="turn-rx" style="color: #00d9ff;">-</div>
                    <div class="label">TURN RX (MB)</div>
                </div>
                <div class="stat-box" style="background: rgba(0,217,255,0.1);">
                    <div class="number" id="turn-tx" style="color: #00d9ff;">-</div>
                    <div class="label">TURN TX (MB)</div>
                </div>
            </div>
        </header>

        <!-- Offene Spiele -->
        <section class="section waiting">
            <h2>Offene Spiele (Warten auf Gegner)</h2>
            <?php if (empty($matches['waiting'])): ?>
                <p class="empty">Keine offenen Spiele verfuegbar. Starte ein neues Spiel im DigiKicker!</p>
            <?php else: ?>
                <table>
                    <thead>
                        <tr>
                            <th>Room Code</th>
                            <th>Host</th>
                            <th>Erstellt</th>
                            <th>Wartet seit</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($matches['waiting'] as $match): ?>
                        <tr>
                            <td><span class="room-code"><?= htmlspecialchars($match['room_code']) ?></span></td>
                            <td><span class="player-name"><?= htmlspecialchars($match['host_name']) ?></span></td>
                            <td><?= formatTime($match['created_at']) ?></td>
                            <td class="time-ago"><?= timeAgo($match['created_at']) ?></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            <?php endif; ?>
        </section>

        <!-- Laufende Spiele -->
        <section class="section running">
            <h2><span class="live-indicator"></span>Laufende Spiele</h2>
            <?php if (empty($matches['running'])): ?>
                <p class="empty">Aktuell keine laufenden Spiele.</p>
            <?php else: ?>
                <table>
                    <thead>
                        <tr>
                            <th>Room Code</th>
                            <th>Spieler</th>
                            <th>Gestartet</th>
                            <th>Laeuft seit</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($matches['running'] as $match): ?>
                        <tr>
                            <td><span class="room-code"><?= htmlspecialchars($match['room_code']) ?></span></td>
                            <td>
                                <span class="player-name"><?= htmlspecialchars($match['host_name']) ?></span>
                                <span class="vs">vs</span>
                                <span class="player-name"><?= htmlspecialchars($match['joiner_name']) ?></span>
                            </td>
                            <td><?= formatTime($match['started_at']) ?></td>
                            <td class="time-ago"><?= timeAgo($match['started_at']) ?></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            <?php endif; ?>
        </section>

        <!-- Beendete Spiele -->
        <section class="section finished">
            <h2>Letzte Ergebnisse</h2>
            <?php if (empty($matches['finished'])): ?>
                <p class="empty">Noch keine beendeten Spiele.</p>
            <?php else: ?>
                <table>
                    <thead>
                        <tr>
                            <th>Spieler</th>
                            <th>Ergebnis</th>
                            <th>Gewinner</th>
                            <th>Beendet</th>
                        </tr>
                    </thead>
                    <tbody>
                        <?php foreach ($matches['finished'] as $match): ?>
                        <tr>
                            <td>
                                <span class="player-name"><?= htmlspecialchars($match['host_name']) ?></span>
                                <span class="vs">vs</span>
                                <span class="player-name"><?= htmlspecialchars($match['joiner_name'] ?: '???') ?></span>
                            </td>
                            <td><span class="score"><?= $match['score_host'] ?> : <?= $match['score_joiner'] ?></span></td>
                            <td>
                                <?php if ($match['winner_name']): ?>
                                    <span class="winner"><?= htmlspecialchars($match['winner_name']) ?></span>
                                <?php else: ?>
                                    <span class="time-ago">Unentschieden</span>
                                <?php endif; ?>
                            </td>
                            <td class="time-ago"><?= timeAgo($match['finished_at']) ?></td>
                        </tr>
                        <?php endforeach; ?>
                    </tbody>
                </table>
            <?php endif; ?>
        </section>

        <footer>
            <p>DigiKicker Online Multiplayer - Auto-Refresh alle 5 Sekunden</p>
            <p><a href="api/list_matches.php">API: list_matches.php</a></p>
        </footer>
    </div>

    <script>
        // TURN Server Stats laden (über PHP-Proxy wegen Mixed-Content)
        function updateTurnStats() {
            fetch('api/turn_stats.php')
            .then(response => response.json())
            .then(data => {
                // Memory in MB
                document.getElementById('turn-memory').textContent = data.memory_mb_used || '-';

                // CPU Load
                document.getElementById('turn-cpu').textContent = data.cpu_load || '-';

                // RX in MB (bytes zu MB)
                const rxMB = data.rx_bytes ? (data.rx_bytes / (1024 * 1024)).toFixed(2) : '-';
                document.getElementById('turn-rx').textContent = rxMB;

                // TX in MB (bytes zu MB)
                const txMB = data.tx_bytes ? (data.tx_bytes / (1024 * 1024)).toFixed(2) : '-';
                document.getElementById('turn-tx').textContent = txMB;
            })
            .catch(error => {
                console.error('TURN Stats Fehler:', error);
                document.getElementById('turn-memory').textContent = 'Fehler';
                document.getElementById('turn-cpu').textContent = 'Fehler';
                document.getElementById('turn-rx').textContent = 'Fehler';
                document.getElementById('turn-tx').textContent = 'Fehler';
            });
        }

        // Beim Laden einmal ausführen
        updateTurnStats();

        // Dann jede Minute aktualisieren
        setInterval(updateTurnStats, 60000);
    </script>
</body>
</html>
