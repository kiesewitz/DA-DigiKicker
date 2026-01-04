-- Matches Tabelle: Speichert alle Spielsitzungen
CREATE TABLE IF NOT EXISTS matches (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- Room Code: 6 Zeichen alphanumerisch (z.B. "ABC123")
    room_code VARCHAR(6) NOT NULL UNIQUE,

    -- Spieler Namen (frei wählbar, keine Uniqueness)
    host_name VARCHAR(50) NOT NULL,
    joiner_name VARCHAR(50) DEFAULT NULL,

    -- Team-Auswahl: red oder blue
    host_team ENUM('red', 'blue') NOT NULL DEFAULT 'red',

    -- Status: waiting = Host wartet, running = Spiel läuft, finished = beendet
    status ENUM('waiting', 'running', 'finished') NOT NULL DEFAULT 'waiting',

    -- Zeitstempel
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    started_at DATETIME DEFAULT NULL,
    finished_at DATETIME DEFAULT NULL,

    -- Ergebnis (nur bei finished relevant)
    winner_name VARCHAR(50) DEFAULT NULL,
    score_host INT DEFAULT 0,
    score_joiner INT DEFAULT 0,

    -- Index für schnelle Abfragen nach Status
    INDEX idx_status (status),
    INDEX idx_room_code (room_code),
    INDEX idx_created_at (created_at)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Signaling Messages Tabelle: WebRTC Signaling Daten (Offer/Answer/ICE Candidates)
CREATE TABLE IF NOT EXISTS signaling_messages (
    id INT AUTO_INCREMENT PRIMARY KEY,

    -- Referenz zum Match
    match_id INT NOT NULL,

    -- Absender: host oder joiner
    sender_role ENUM('host', 'joiner') NOT NULL,

    -- Nachrichtentyp: offer (SDP), answer (SDP), candidate (ICE)
    msg_type ENUM('offer', 'answer', 'candidate') NOT NULL,

    -- Payload: JSON String mit SDP oder ICE Candidate Daten
    payload TEXT NOT NULL,

    -- Zeitstempel für Polling (Client fragt: "gib mir alles seit ID X")
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    -- Foreign Key zu matches
    FOREIGN KEY (match_id) REFERENCES matches(id) ON DELETE CASCADE,

    -- Index für effizientes Polling nach match_id und id
    INDEX idx_match_sender (match_id, sender_role),
    INDEX idx_match_id_created (match_id, id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

