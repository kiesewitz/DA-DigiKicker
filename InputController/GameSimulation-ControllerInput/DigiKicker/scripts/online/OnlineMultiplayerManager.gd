# =============================================================================
# OnlineMultiplayerManager.gd - WebRTC P2P Multiplayer Manager
# =============================================================================
# Verwaltet die WebRTC Verbindung für Online-Multiplayer via PHP Signaling.
# Nutzt WebRTCPeerConnection + DataChannel für direkte P2P Kommunikation.
# =============================================================================

extends Node
class_name OnlineMultiplayerManager

# ========== SIGNALE ==========
# Verbindungsstatus
signal connection_state_changed(state: ConnectionState)
signal status_message(message: String)
signal error_occurred(message: String)

# Match Events
signal match_created(room_code: String)
signal match_joined(room_code: String, host_name: String)
signal opponent_joined(joiner_name: String)
signal opponent_disconnected()
signal match_started()

# Spieldaten
signal game_state_received(state: Dictionary)
signal match_event_received(event_type: String, data: Dictionary)

# Ping
signal ping_updated(ping_ms: int, connection_type: String)

# ========== ENUMS ==========
enum ConnectionState {
	DISCONNECTED,      # Nicht verbunden
	CREATING_MATCH,    # Match wird erstellt (Host)
	WAITING_FOR_JOINER,# Host wartet auf Joiner
	JOINING_MATCH,     # Tritt Match bei (Joiner)
	SIGNALING,         # WebRTC Signaling läuft
	CONNECTING,        # ICE Verbindung wird hergestellt
	CONNECTED,         # P2P Verbindung steht
	ERROR              # Fehler aufgetreten
}

enum Role {
	NONE,
	HOST,
	JOINER
}

# ========== KONSTANTEN ==========
# API Endpoint URL
const API_BASE_URL := "https://diplomarbeit.rath-net.at/api/"

# Signaling Polling Intervall (nur während Verbindungsaufbau)
const SIGNALING_POLL_INTERVAL := 0.5  # Sekunden

# Verbindungs-Timeout
const CONNECTION_TIMEOUT := 120.0  # Sekunden (verlängert, damit Host-Lobby nicht früh schließt)

# STUN Status Logging Intervall (zur Sichtbarkeit, dass weiter versucht wird)
const STUN_STATUS_INTERVAL := 15.0  # Sekunden, periodischer Hinweis (entschärft Spam)
const STUN_STATUS_MAX_PINGS := 8    # Danach verstummen die Logs

# Lobby Auto-Cleanup nach 10 Minuten
const LOBBY_TIMEOUT := 600.0  # Sekunden (10 Minuten)

# Ping Intervall (im Spiel)
const PING_INTERVAL := 1.0  # Sekunden

# STUN/TURN Server (Alte Konfiguration - auskommentiert)
# const STUN_SERVERS := [
# 	"stun:stun.relay.metered.ca:80",
# 	"stun:stun.l.google.com:19302"
# ]
#
# const TURN_SERVERS := [
# 	{
# 		"urls": [
# 			"turn:europe.relay.metered.ca:80?transport=udp",
# 			"turn:europe.relay.metered.ca:443?transport=udp"
# 		],
# 		"username": "d15e8a8920a96d22450bf226",
# 		"credential": "/Ieq/ndrpJrUanLw"
# 	}
# ]

# STUN/TURN Server (Neue Konfiguration - DADigiKicker Server)
const STUN_SERVERS := [
	"stun:185.119.117.112:3478"
]

const TURN_SERVERS := [
	{
		"urls": "turn:185.119.117.112:3478?transport=udp",
		"username": "DADigiKicker",
		"credential": "xR9!mF7Zp@3KQeL#V2N8A5cH",
		"credentialType": "password"
	}
]

# ========== MEMBER VARIABLEN ==========
# Verbindungsstatus
var current_state: ConnectionState = ConnectionState.DISCONNECTED
var current_role: Role = Role.NONE
var room_code: String = ""
var player_name: String = ""
var opponent_name: String = ""
var match_id: int = 0
var offer_created: bool = false
var answer_created: bool = false

# Team-Auswahl (nur für Host relevant)
var host_team: String = "red"  # "red" oder "blue"

# WebRTC Objekte
var peer_connection: WebRTCPeerConnection = null
var data_channel: WebRTCDataChannel = null

# HTTP Request Objekte (für Signaling)
var http_request: HTTPRequest = null

# Signaling State
var signaling_poll_timer: Timer = null
var connection_timeout_timer: Timer = null
var lobby_timeout_timer: Timer = null
var last_signal_id: int = 0
var ice_candidates_queue: Array = []  # Lokale ICE Candidates zum Senden
var pending_remote_candidates: Array = []  # ICE Candidates vom Gegner warten auf Remote Description
var remote_description_set: bool = false
var stun_status_timer: Timer = null
var stun_status_counter: int = 0
var stun_last_ice_state: String = ""
var force_fetch_counter: int = 0  # Limitierte Zahl an erzwungenen Full-Fetches (alt)

# Ping Tracking
var ping_timer: Timer = null
var ping_id: int = 0
var ping_send_time: int = 0
var ping_history: Array = []  # Letzte 5 Ping-Werte für Mittelwert
var current_ping_ms: int = -1
var connection_type: String = ""  # "S" für STUN, "T" für TURN, "" wenn unbekannt

# ========== LIFECYCLE ==========
func _ready() -> void:
	# HTTP Request Node erstellen
	http_request = HTTPRequest.new()
	add_child(http_request)
	http_request.request_completed.connect(_on_http_request_completed)

	# Timer für Signaling Polling
	signaling_poll_timer = Timer.new()
	signaling_poll_timer.wait_time = SIGNALING_POLL_INTERVAL
	signaling_poll_timer.timeout.connect(_on_signaling_poll_timeout)
	add_child(signaling_poll_timer)

	# Timer für Connection Timeout
	connection_timeout_timer = Timer.new()
	connection_timeout_timer.one_shot = true
	connection_timeout_timer.wait_time = CONNECTION_TIMEOUT
	connection_timeout_timer.timeout.connect(_on_connection_timeout)
	add_child(connection_timeout_timer)

	# Timer für Ping
	ping_timer = Timer.new()
	ping_timer.wait_time = PING_INTERVAL
	ping_timer.timeout.connect(_send_ping)
	add_child(ping_timer)

	# Timer für Lobby Timeout (10 Minuten Auto-Cleanup)
	lobby_timeout_timer = Timer.new()
	lobby_timeout_timer.one_shot = true
	lobby_timeout_timer.wait_time = LOBBY_TIMEOUT
	lobby_timeout_timer.timeout.connect(_on_lobby_timeout)
	add_child(lobby_timeout_timer)

	# Timer für STUN Status Logs
	stun_status_timer = Timer.new()
	stun_status_timer.wait_time = STUN_STATUS_INTERVAL
	stun_status_timer.timeout.connect(_on_stun_status_ping)
	add_child(stun_status_timer)


func _notification(what: int) -> void:
	# Application wird geschlossen oder crashed
	if what == NOTIFICATION_WM_CLOSE_REQUEST or what == NOTIFICATION_CRASH:
		_handle_application_exit()


func _exit_tree() -> void:
	# Beim Exit immer Backend Cleanup durchführen wenn Host mit offener Lobby
	_handle_application_exit()


func _handle_application_exit() -> void:
	# Cleanup beim Exit nur wenn Host mit offener Lobby
	var should_cleanup_backend := false
	if current_role == Role.HOST and not room_code.is_empty():
		# Host hat eine Lobby offen -> Backend informieren
		should_cleanup_backend = true

	disconnect_from_match(should_cleanup_backend)


# ========== PUBLIC API ==========

## Erstellt ein neues Match als Host
## host_player_name: Name des Host-Spielers
## selected_team: "red" oder "blue" (Default: "red")
func create_match(host_player_name: String, selected_team: String = "red") -> void:
	if current_state != ConnectionState.DISCONNECTED:
		push_warning("Already in a match or connecting")
		return

	player_name = host_player_name
	host_team = selected_team.to_lower()
	current_role = Role.HOST
	_set_state(ConnectionState.CREATING_MATCH)
	emit_signal("status_message", "Erstelle Match...")

	# HTTP Request an create_match.php
	var body := JSON.stringify({
		"host_name": player_name,
		"host_team": host_team
	})
	var headers := ["Content-Type: application/json"]
	var url := API_BASE_URL + "create_match.php"

	var error := http_request.request(url, headers, HTTPClient.METHOD_POST, body)
	if error != OK:
		_handle_error("HTTP Request fehlgeschlagen: " + str(error))


## Tritt einem Match bei als Joiner
func join_match(join_room_code: String, joiner_player_name: String) -> void:
	if current_state != ConnectionState.DISCONNECTED:
		push_warning("Already in a match or connecting")
		return

	player_name = joiner_player_name
	room_code = join_room_code.to_upper()
	current_role = Role.JOINER
	_set_state(ConnectionState.JOINING_MATCH)
	emit_signal("status_message", "Trete Match bei...")

	# HTTP Request an join_match.php
	var body := JSON.stringify({
		"room_code": room_code,
		"joiner_name": player_name
	})
	var headers := ["Content-Type: application/json"]
	var url := API_BASE_URL + "join_match.php"

	var error := http_request.request(url, headers, HTTPClient.METHOD_POST, body)
	if error != OK:
		_handle_error("HTTP Request fehlgeschlagen: " + str(error))


## Beendet die aktuelle Verbindung
## cleanup_backend: Wenn true, wird auch ein DELETE Request an das Backend geschickt
func disconnect_from_match(cleanup_backend: bool = false) -> void:
	# Backend Cleanup (wenn angefordert UND wir haben einen Room Code)
	if cleanup_backend and not room_code.is_empty():
		_send_delete_match_request(room_code)

	# Timer stoppen
	signaling_poll_timer.stop()
	connection_timeout_timer.stop()
	ping_timer.stop()
	lobby_timeout_timer.stop()
	stun_status_timer.stop()

	# WebRTC aufräumen
	if data_channel != null:
		data_channel.close()
		data_channel = null

	if peer_connection != null:
		peer_connection.close()
		peer_connection = null

	# State zurücksetzen
	room_code = ""
	opponent_name = ""
	match_id = 0
	offer_created = false
	answer_created = false
	last_signal_id = 0
	ice_candidates_queue.clear()
	pending_remote_candidates.clear()
	remote_description_set = false
	ping_history.clear()
	current_ping_ms = -1
	connection_type = ""
	force_fetch_counter = 0
	current_role = Role.NONE
	host_team = "red"  # Zurücksetzen auf Default

	_set_state(ConnectionState.DISCONNECTED)


## Sendet Spielzustand über DataChannel
## state: Dictionary mit Rod-Positionen und Ball-Daten
func send_game_state(state: Dictionary) -> void:
	if data_channel == null or data_channel.get_ready_state() != WebRTCDataChannel.STATE_OPEN:
		return

	# Kompaktes Nachrichtenformat
	var msg := {
		"t": "state",  # type: state
		"d": state     # data
	}
	_send_data(msg)


## Sendet Match Event (Tor, Reset, Spielende)
func send_match_event(event_type: String, data: Dictionary = {}) -> void:
	if data_channel == null or data_channel.get_ready_state() != WebRTCDataChannel.STATE_OPEN:
		return

	var msg := {
		"t": "event",
		"e": event_type,
		"d": data
	}
	_send_data(msg)


## Gibt aktuelle Ping-Zeit in ms zurück (-1 wenn nicht verbunden)
func get_ping() -> int:
	return current_ping_ms


## Gibt Liste offener Matches zurück (async via Signal)
func request_open_matches() -> void:
	var url := API_BASE_URL + "list_matches.php?status=waiting"
	# Neuen HTTPRequest für diese Anfrage erstellen (da der Hauptrequest belegt sein könnte)
	var req := HTTPRequest.new()
	add_child(req)
	req.request_completed.connect(func(result, code, _headers, body):
		req.queue_free()
		if result == HTTPRequest.RESULT_SUCCESS and code == 200:
			var json = JSON.parse_string(body.get_string_from_utf8())
			if json and json.get("success", false):
				# Signal mit Match-Liste
				pass  # Könnte ein zusätzliches Signal sein
	)
	req.request(url)


# ========== PRIVATE: STATE MANAGEMENT ==========

func _set_state(new_state: ConnectionState) -> void:
	if current_state == new_state:
		return

	current_state = new_state
	emit_signal("connection_state_changed", new_state)

	# Debug Log
	var state_names := ["DISCONNECTED", "CREATING_MATCH", "WAITING_FOR_JOINER",
						"JOINING_MATCH", "SIGNALING", "CONNECTING", "CONNECTED", "ERROR"]
	print("[Online] State: ", state_names[new_state])


func _handle_error(message: String) -> void:
	push_error("[OnlineMultiplayer] " + message)
	emit_signal("error_occurred", message)
	_set_state(ConnectionState.ERROR)
	disconnect_from_match()


# ========== PRIVATE: HTTP HANDLING ==========

func _on_http_request_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray) -> void:
	if result != HTTPRequest.RESULT_SUCCESS:
		_handle_error("HTTP Fehler: " + str(result))
		return

	if response_code != 200:
		_handle_error("Server Fehler: " + str(response_code))
		return

	var json = JSON.parse_string(body.get_string_from_utf8())
	if json == null:
		_handle_error("Ungültige JSON Antwort")
		return

	if not json.get("success", false):
		_handle_error("API Fehler: " + json.get("error", "Unbekannt"))
		return

	# Response je nach aktuellem State verarbeiten
	match current_state:
		ConnectionState.CREATING_MATCH:
			_handle_match_created(json)
		ConnectionState.JOINING_MATCH:
			_handle_match_joined(json)
		ConnectionState.SIGNALING, ConnectionState.WAITING_FOR_JOINER:
			_handle_signaling_response(json)
		_:
			pass


func _handle_match_created(response: Dictionary) -> void:
	room_code = response.get("room_code", "")
	match_id = response.get("match_id", 0)

	if room_code.is_empty():
		_handle_error("Kein Room Code erhalten")
		return

	emit_signal("match_created", room_code)
	emit_signal("status_message", "Match erstellt: " + room_code + "\nWarte auf Gegner...")
	_set_state(ConnectionState.WAITING_FOR_JOINER)

	# Host initialisiert WebRTC und erstellt Offer
	_init_webrtc_as_host()

	# Starte Signaling Polling (um zu erfahren wenn Joiner beitritt)
	last_signal_id = 0
	signaling_poll_timer.start()
	# Kein frühes Abbrechen mehr, Lobby bleibt länger offen
	connection_timeout_timer.start()

	# Starte Lobby Timeout (10 Minuten Auto-Cleanup)
	lobby_timeout_timer.start()


func _handle_match_joined(response: Dictionary) -> void:
	match_id = response.get("match_id", 0)
	opponent_name = response.get("host_name", "Gegner")
	host_team = response.get("host_team", "red")  # Host's Team vom Server speichern

	emit_signal("match_joined", room_code, opponent_name)
	emit_signal("status_message", "Verbinde mit " + opponent_name + "...")
	_set_state(ConnectionState.SIGNALING)

	# Joiner initialisiert WebRTC
	_init_webrtc_as_joiner()

	# Starte Signaling Polling (um Offer vom Host zu holen)
	last_signal_id = 0
	# Kurzes Delay (1s) reicht, damit der Host sein Offer senden kann
	signaling_poll_timer.start(1.0)
	connection_timeout_timer.start()


# ========== PRIVATE: WEBRTC ==========

func _init_webrtc_as_host() -> void:
	# Peer Connection mit STUN Servern erstellen
	peer_connection = WebRTCPeerConnection.new()
	remote_description_set = false
	pending_remote_candidates.clear()
	offer_created = false
	answer_created = false

	# ICE Server konfigurieren
	var config := {
		"iceServers": []
	}
	for stun_url in STUN_SERVERS:
		config["iceServers"].append({"urls": [stun_url]})
	for turn_cfg in TURN_SERVERS:
		config["iceServers"].append(turn_cfg)

	print("[Online][ICE] Host init mit ", config["iceServers"].size(), " ICE-Server-Einträgen (STUN ", STUN_SERVERS.size(), " / TURN ", TURN_SERVERS.size(), ")")

	var err := peer_connection.initialize(config)
	if err != OK:
		_handle_error("WebRTC Init fehlgeschlagen: " + str(err))
		return

	# Signale verbinden
	peer_connection.session_description_created.connect(_on_session_description_created)
	peer_connection.ice_candidate_created.connect(_on_ice_candidate_created)

	# DataChannel erstellen (Host erstellt den Channel)
	var channel_config := {
		"negotiated": true,
		"id": 1,
		"ordered": false,  # Unordered für niedrigere Latenz
		"maxRetransmits": 0  # Unreliable für State Updates
	}
	data_channel = peer_connection.create_data_channel("game", channel_config)
	_setup_data_channel()

	# Offer wird NICHT sofort erstellt - erst wenn Joiner beitritt!
	# Siehe _on_signaling_poll_completed() Zeile ~640
	_start_stun_status_logging()


func _init_webrtc_as_joiner() -> void:
	# Peer Connection mit STUN Servern erstellen
	peer_connection = WebRTCPeerConnection.new()
	remote_description_set = false
	pending_remote_candidates.clear()
	offer_created = false
	answer_created = false

	var config := {
		"iceServers": []
	}
	for stun_url in STUN_SERVERS:
		config["iceServers"].append({"urls": [stun_url]})
	for turn_cfg in TURN_SERVERS:
		config["iceServers"].append(turn_cfg)

	print("[Online][ICE] Joiner init mit ", config["iceServers"].size(), " ICE-Server-Einträgen (STUN ", STUN_SERVERS.size(), " / TURN ", TURN_SERVERS.size(), ")")

	var err := peer_connection.initialize(config)
	if err != OK:
		_handle_error("WebRTC Init fehlgeschlagen: " + str(err))
		return

	# Signale verbinden
	peer_connection.session_description_created.connect(_on_session_description_created)
	peer_connection.ice_candidate_created.connect(_on_ice_candidate_created)

	# DataChannel erstellen (gleiche Config wie Host für negotiated channel)
	var channel_config := {
		"negotiated": true,
		"id": 1,
		"ordered": false,
		"maxRetransmits": 0
	}
	data_channel = peer_connection.create_data_channel("game", channel_config)
	_setup_data_channel()
	_start_stun_status_logging()


func _setup_data_channel() -> void:
	if data_channel == null:
		return

	# DataChannel Callbacks werden in _process gepollt


func _on_session_description_created(type: String, sdp: String) -> void:
	print("[Online][SDP] session_description_created type=", type, " len=", sdp.length())
	# Lokale Description setzen
	peer_connection.set_local_description(type, sdp)

	# An Server senden
	var msg_type := "offer" if type == "offer" else "answer"
	var sender_role := "host" if current_role == Role.HOST else "joiner"

	if msg_type == "offer":
		offer_created = true
	else:
		answer_created = true

	_push_signaling_message(msg_type, sender_role, {"type": type, "sdp": sdp})


func _on_ice_candidate_created(media: String, index: int, candidate_name: String) -> void:
	# ICE Candidate Typ erkennen für bessere Diagnose
	var candidate_type := "unknown"
	if "typ host" in candidate_name:
		candidate_type = "host (lokal)"
	elif "typ srflx" in candidate_name:
		candidate_type = "srflx (STUN)"
		connection_type = "S"  # STUN wird verwendet
	elif "typ relay" in candidate_name:
		candidate_type = "relay (TURN)"
		connection_type = "T"  # TURN wird verwendet
	elif "typ prflx" in candidate_name:
		candidate_type = "prflx (peer-reflexive)"

	print("[Online][ICE] Candidate erstellt: ", candidate_type, " | media=", media, " index=", index)
	print("[Online][ICE]   -> ", candidate_name)

	# ICE Candidate an Server senden
	var sender_role := "host" if current_role == Role.HOST else "joiner"

	_push_signaling_message("candidate", sender_role, {
		"media": media,
		"index": index,
		"name": candidate_name
	})


# ========== PRIVATE: SIGNALING HTTP ==========

func _push_signaling_message(msg_type: String, sender_role: String, payload: Dictionary) -> void:
	print("[Online][Push] Sende: type=", msg_type, " role=", sender_role, " room=", room_code)

	var body := JSON.stringify({
		"room_code": room_code,
		"sender_role": sender_role,
		"msg_type": msg_type,
		"payload": payload
	})

	# Neuen HTTPRequest für Push (damit Poll weiter funktioniert)
	var req := HTTPRequest.new()
	add_child(req)
	req.request_completed.connect(func(result: int, code: int, _headers: PackedStringArray, body_data: PackedByteArray):
		var response_str: String = body_data.get_string_from_utf8()
		if result != HTTPRequest.RESULT_SUCCESS or code != 200:
			print("[Online][Push] FEHLER: result=", result, " code=", code, " response=", response_str.substr(0, 200))
		else:
			print("[Online][Push] OK: ", msg_type, " gesendet -> ", response_str.substr(0, 100))
		req.queue_free()
	)

	var headers := ["Content-Type: application/json"]
	req.request(API_BASE_URL + "push_signal.php", headers, HTTPClient.METHOD_POST, body)


func _on_signaling_poll_timeout() -> void:
	# Signaling Messages vom Server abrufen
	if current_state != ConnectionState.WAITING_FOR_JOINER and current_state != ConnectionState.SIGNALING:
		signaling_poll_timer.stop()
		return

	var role_param := "host" if current_role == Role.HOST else "joiner"
	var url := API_BASE_URL + "pull_signal.php?room_code=" + room_code + "&role=" + role_param + "&last_id=" + str(last_signal_id)

	print("[Online][Poll] ===== POLLING START =====")
	print("[Online][Poll] URL: ", url)
	print("[Online][Poll] role=", role_param, " room=", room_code, " last_id=", last_signal_id, " state=", current_state)

	var req := HTTPRequest.new()
	add_child(req)
	req.request_completed.connect(_on_signaling_poll_completed.bind(req))
	req.request(url)


func _on_signaling_poll_completed(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray, req: HTTPRequest) -> void:
	req.queue_free()

	print("[Online][Poll] ===== POLL RESPONSE =====")
	print("[Online][Poll] HTTP result=", result, " code=", response_code)

	if result != HTTPRequest.RESULT_SUCCESS or response_code != 200:
		print("[Online][Poll] HTTP Fehler!")
		return  # Ignorieren, nächster Poll versucht es erneut

	var body_str := body.get_string_from_utf8()
	print("[Online][Poll] RAW Body (first 500 chars): ", body_str.substr(0, 500))

	var json = JSON.parse_string(body_str)
	if json == null:
		print("[Online][Poll] JSON PARSE FEHLER!")
		return
	if not json.get("success", false):
		print("[Online][Poll] success=false, error=", json.get("error", "unknown"))
		return

	# Debug: Anzahl empfangener Nachrichten
	var messages: Array = json.get("messages", [])
	var msg_count: int = messages.size()
	print("[Online][Poll] Parsed: messages=", msg_count, " status=", json.get("match_status", "?"), " joiner=", json.get("joiner_name", "null"), " host=", json.get("host_name", "null"))

	# Debug-Info vom Server anzeigen
	var debug_info = json.get("debug", {})
	if debug_info:
		print("[Online][Poll] SERVER DEBUG:")
		print("[Online][Poll]   requested_room_code=", debug_info.get("requested_room_code", "?"))
		print("[Online][Poll]   requested_role=", debug_info.get("requested_role", "?"))
		print("[Online][Poll]   queried_other_role=", debug_info.get("queried_other_role", "?"))
		print("[Online][Poll]   queried_match_id=", debug_info.get("queried_match_id", "?"))
		print("[Online][Poll]   queried_last_id=", debug_info.get("queried_last_id", "?"))
		print("[Online][Poll]   total_messages_in_db=", debug_info.get("total_messages_in_db", "?"))
		# Falls Server meldet, dass Nachrichten existieren, wir aber keine bekommen, forciere schnellen Re-Poll
		var total_from_other := 0
		var expected_role := "host" if current_role == Role.JOINER else "joiner"
		for entry in debug_info.get("total_messages_in_db", []):
			if entry.get("sender_role", "") == expected_role:
				total_from_other = int(entry.get("total", 0))
				break
		if messages.is_empty() and total_from_other > 0:
			print("[Online][Poll] WARN: Server hat ", total_from_other, " Nachrichten von ", expected_role, " aber response liefert keine -> Re-Poll mit last_id=0")
			last_signal_id = 0
			# Sofortigen Re-Poll anstoßen
			signaling_poll_timer.start(0.2)
	print("[Online][Poll] ===== POLL END =====")

	# WICHTIG: Nach erstem 10s-Poll (Joiner) -> zurück auf normales 0.5s Intervall
	if current_role == Role.JOINER and signaling_poll_timer.wait_time > SIGNALING_POLL_INTERVAL:
		print("[Online][Poll] Setze Poll-Intervall zurück auf ", SIGNALING_POLL_INTERVAL, "s")
		signaling_poll_timer.wait_time = SIGNALING_POLL_INTERVAL
		# KRITISCH: Timer nach wait_time Änderung neu starten!
		signaling_poll_timer.start()

	# Match Status prüfen (für Host: Joiner beigetreten?)
	if current_state == ConnectionState.WAITING_FOR_JOINER:
		var joiner_name = json.get("joiner_name", "")
		var match_status: String = json.get("match_status", "")
		var joined_detected: bool = (joiner_name is String and not joiner_name.is_empty()) or match_status == "running"

		if joined_detected and not offer_created:
			if joiner_name is String and not joiner_name.is_empty():
				opponent_name = joiner_name
			else:
				opponent_name = "Gegner"

			emit_signal("opponent_joined", opponent_name)
			emit_signal("status_message", opponent_name + " ist beigetreten!\nVerbinde...")
			_set_state(ConnectionState.SIGNALING)

			# JETZT erst Offer erstellen (nachdem Joiner confirmed ist)
			print("[Online] Joiner beigetreten (Status=", match_status, ") -> erstelle WebRTC Offer...")
			var offer_err := peer_connection.create_offer()
			if offer_err != OK:
				_handle_error("create_offer fehlgeschlagen: " + str(offer_err))
			else:
				print("[Online] create_offer gestartet")
	# Fallback: Wenn wir schon in SIGNALING sind, aber noch kein Offer erzeugt wurde und der Server running meldet, trotzdem versuchen
	elif current_role == Role.HOST and not offer_created and json.get("match_status", "") == "running":
		print("[Online] Fallback: Host sieht match_status=running aber offer_created=false -> versuche create_offer erneut")
		var offer_err2 := peer_connection.create_offer()
		if offer_err2 != OK:
			_handle_error("create_offer fehlgeschlagen (Fallback): " + str(offer_err2))
		else:
			print("[Online] create_offer (Fallback) gestartet")

	# Signaling Messages verarbeiten
	_process_signaling_messages(messages)

	# Falls der Server meldet, dass Nachrichten existieren, wir aber keine erhalten haben, forcieren wir einen Full-Refetch.
	if messages.is_empty():
		var total_from_other := 0
		var expected_role := "host" if current_role == Role.JOINER else "joiner"
		for entry in debug_info.get("total_messages_in_db", []):
			if entry.get("sender_role", "") == expected_role:
				total_from_other = int(entry.get("total", 0))
				break

		if total_from_other > 0:
			print("[Online][Poll] WARN: Server hat ", total_from_other, " Nachrichten von ", expected_role, " aber response liefert keine -> Force refetch")
			last_signal_id = 0
			_force_full_signaling_fetch()
		# Zusätzlicher Fallback speziell für Joiner: wenn running aber keine Messages, trotzdem Full-Fetch versuchen (max 3x)
		elif current_role == Role.JOINER and json.get("match_status", "") == "running" and force_fetch_counter < 3:
			force_fetch_counter += 1
			print("[Online][Poll] Fallback: Joiner sieht running aber messages=0 -> Full-Fetch Versuch ", force_fetch_counter)
			last_signal_id = 0
			_force_full_signaling_fetch()


func _handle_remote_offer(payload: Dictionary) -> void:
	# Joiner empfängt Offer vom Host
	print("[Online][Offer] _handle_remote_offer aufgerufen, current_role=", current_role)
	if current_role != Role.JOINER:
		print("[Online][Offer] SKIP: Nicht Joiner (role=", current_role, ")")
		return

	# GUARD: Skip wenn bereits verbunden oder getrennt
	if current_state == ConnectionState.CONNECTED or current_state == ConnectionState.DISCONNECTED:
		print("[Online][Offer] SKIP: Bereits verbunden oder getrennt (state=", current_state, ")")
		return

	# GUARD: Skip wenn peer_connection null ist
	if peer_connection == null:
		print("[Online][Offer] SKIP: peer_connection ist null")
		return

	# GUARD: Skip wenn DataChannel bereits offen ist (Verbindung steht)
	if data_channel != null and data_channel.get_ready_state() == WebRTCDataChannel.STATE_OPEN:
		print("[Online][Offer] SKIP: DataChannel bereits offen, Verbindung steht")
		return

	# GUARD: Skip wenn WebRTC Connection bereits verbunden/geschlossen
	var conn_state = peer_connection.get_connection_state()
	if conn_state == WebRTCPeerConnection.STATE_CONNECTED or conn_state == WebRTCPeerConnection.STATE_CLOSED:
		print("[Online][Offer] SKIP: WebRTC bereits verbunden/geschlossen (conn_state=", conn_state, ")")
		return

	var sdp: String = payload.get("sdp", "")
	if sdp.is_empty():
		print("[Online][Offer] FEHLER: SDP ist leer! payload=", payload)
		return

	print("[Online] Offer erhalten (", sdp.length(), " bytes), erstelle Answer...")
	peer_connection.set_remote_description("offer", sdp)
	remote_description_set = true
	_flush_pending_remote_candidates()

	# Answer erstellen (wird automatisch durch session_description_created gesendet)
	if peer_connection.has_method("create_answer"):
		var err: int = peer_connection.create_answer()
		if err != OK:
			_handle_error("create_answer failed: " + str(err))
	else:
		push_warning("create_answer() nicht verfügbar im WebRTC Plugin - bitte Plugin aktualisieren")


func _handle_remote_answer(payload: Dictionary) -> void:
	# Host empfängt Answer vom Joiner
	if current_role != Role.HOST:
		return

	# GUARD: Skip wenn bereits verbunden oder getrennt
	if current_state == ConnectionState.CONNECTED or current_state == ConnectionState.DISCONNECTED:
		print("[Online][Answer] SKIP: Bereits verbunden oder getrennt (state=", current_state, ")")
		return

	# GUARD: Skip wenn peer_connection null ist
	if peer_connection == null:
		print("[Online][Answer] SKIP: peer_connection ist null")
		return

	# GUARD: Skip wenn DataChannel bereits offen ist (Verbindung steht)
	if data_channel != null and data_channel.get_ready_state() == WebRTCDataChannel.STATE_OPEN:
		print("[Online][Answer] SKIP: DataChannel bereits offen, Verbindung steht")
		return

	# GUARD: Skip wenn WebRTC Connection bereits verbunden/geschlossen
	var conn_state = peer_connection.get_connection_state()
	if conn_state == WebRTCPeerConnection.STATE_CONNECTED or conn_state == WebRTCPeerConnection.STATE_CLOSED:
		print("[Online][Answer] SKIP: WebRTC bereits verbunden/geschlossen (conn_state=", conn_state, ")")
		return

	var sdp: String = payload.get("sdp", "")
	if sdp.is_empty():
		return

	print("[Online] Answer erhalten")
	peer_connection.set_remote_description("answer", sdp)
	remote_description_set = true
	_flush_pending_remote_candidates()


func _handle_remote_ice_candidate(payload: Dictionary) -> void:
	var media: String = payload.get("media", "")
	var index: int = payload.get("index", 0)
	var candidate_name: String = payload.get("name", "")

	if candidate_name.is_empty():
		return

	# ICE Candidate Typ erkennen für bessere Diagnose
	var candidate_type := "unknown"
	if "typ host" in candidate_name:
		candidate_type = "host (lokal)"
	elif "typ srflx" in candidate_name:
		candidate_type = "srflx (STUN)"
	elif "typ relay" in candidate_name:
		candidate_type = "relay (TURN)"
	elif "typ prflx" in candidate_name:
		candidate_type = "prflx (peer-reflexive)"

	print("[Online][ICE] Candidate erhalten: ", candidate_type, " | media=", media, " index=", index)
	print("[Online][ICE]   -> ", candidate_name)

	if not remote_description_set:
		pending_remote_candidates.append(payload)
		return

	peer_connection.add_ice_candidate(media, index, candidate_name)


func _flush_pending_remote_candidates() -> void:
	if not remote_description_set:
		return

	for cand in pending_remote_candidates:
		var media: String = cand.get("media", "")
		var index: int = cand.get("index", 0)
		var candidate_name: String = cand.get("name", "")
		if candidate_name.is_empty():
			continue
		peer_connection.add_ice_candidate(media, index, candidate_name)

	pending_remote_candidates.clear()


func _handle_signaling_response(_response: Dictionary) -> void:
	# Allgemeine Signaling Response Verarbeitung (falls über Haupt-HTTPRequest)
	pass


func _process_signaling_messages(messages: Array) -> void:
	for msg in messages:
		var msg_id: int = msg.get("id", 0)
		if msg_id > last_signal_id:
			last_signal_id = msg_id

		var msg_type: String = msg.get("msg_type", "")
		var payload = msg.get("payload", {})

		print("[Online][Poll] Verarbeite msg_type=", msg_type, " id=", msg_id)

		match msg_type:
			"offer":
				print("[Online][Poll] -> Rufe _handle_remote_offer auf")
				_handle_remote_offer(payload)
			"answer":
				print("[Online][Poll] -> Rufe _handle_remote_answer auf")
				_handle_remote_answer(payload)
			"candidate":
				_handle_remote_ice_candidate(payload)


func _force_full_signaling_fetch() -> void:
	# Zusätzlicher Request mit last_id=0, falls Server Messages meldet aber response leer ist
	if room_code.is_empty():
		return

	var role_param := "host" if current_role == Role.HOST else "joiner"
	var url := API_BASE_URL + "pull_signal.php?room_code=" + room_code + "&role=" + role_param + "&last_id=0"

	var req := HTTPRequest.new()
	add_child(req)
	req.request_completed.connect(func(result: int, code: int, _headers: PackedStringArray, body: PackedByteArray):
		var body_str := body.get_string_from_utf8()
		print("[Online][ForceFetch] HTTP result=", result, " code=", code, " len=", body_str.length())
		if result != HTTPRequest.RESULT_SUCCESS or code != 200:
			req.queue_free()
			return

		var json = JSON.parse_string(body_str)
		if json == null:
			req.queue_free()
			return

		var msgs: Array = json.get("messages", [])
		print("[Online][ForceFetch] messages=", msgs.size(), " (last_id reset)")
		_process_signaling_messages(msgs)
		req.queue_free()
	)

	req.request(url)


# ========== PRIVATE: DATA CHANNEL ==========

func _process(_delta: float) -> void:
	# WebRTC Polling
	if peer_connection != null:
		peer_connection.poll()

		# Connection State prüfen
		var connection_state = peer_connection.get_connection_state()
		if connection_state == WebRTCPeerConnection.STATE_CONNECTED:
			if current_state == ConnectionState.SIGNALING or current_state == ConnectionState.CONNECTING:
				_on_peer_connected()

	# DataChannel Messages lesen
	if data_channel != null:
		var channel_state = data_channel.get_ready_state()

		if channel_state == WebRTCDataChannel.STATE_OPEN:
			# Verbindung steht
			if current_state != ConnectionState.CONNECTED:
				_on_data_channel_open()

			# Messages lesen
			while data_channel.get_available_packet_count() > 0:
				var packet := data_channel.get_packet()
				_handle_received_data(packet)

		elif channel_state == WebRTCDataChannel.STATE_CLOSED:
			if current_state == ConnectionState.CONNECTED:
				_on_peer_disconnected()


func _on_peer_connected() -> void:
	print("[Online] Peer verbunden!")
	_set_state(ConnectionState.CONNECTING)


func _on_data_channel_open() -> void:
	print("[Online] DataChannel offen - Verbindung hergestellt!")
	_set_state(ConnectionState.CONNECTED)

	# Timer stoppen
	signaling_poll_timer.stop()
	connection_timeout_timer.stop()
	lobby_timeout_timer.stop()  # Verbindung steht, kein Auto-Cleanup mehr
	stun_status_timer.stop()

	# Ping starten
	ping_timer.start()

	emit_signal("status_message", "Verbunden mit " + opponent_name)
	emit_signal("match_started")


func _on_peer_disconnected() -> void:
	print("[Online] Peer getrennt")
	emit_signal("opponent_disconnected")
	disconnect_from_match()


func _on_connection_timeout() -> void:
	print("[Online] Verbindung dauert länger als " + str(CONNECTION_TIMEOUT) + "s - weiter versuchen")
	emit_signal("status_message", "Verbindung dauert länger... versuche weiter")
	# Nicht abbrechen, sondern weiter pollen
	signaling_poll_timer.start()


func _on_lobby_timeout() -> void:
	print("[Online] Lobby Timeout nach 10 Minuten - Auto-Cleanup")
	emit_signal("status_message", "Lobby wurde nach 10 Minuten automatisch geschlossen")

	# Backend Cleanup durchführen
	disconnect_from_match(true)


# ========== PRIVATE: DATA TRANSFER ==========

func _send_data(data: Dictionary) -> void:
	if data_channel == null or data_channel.get_ready_state() != WebRTCDataChannel.STATE_OPEN:
		return

	# JSON zu Bytes
	var json_str := JSON.stringify(data)
	var bytes := json_str.to_utf8_buffer()

	data_channel.put_packet(bytes)


func _handle_received_data(packet: PackedByteArray) -> void:
	var json_str := packet.get_string_from_utf8()
	var data = JSON.parse_string(json_str)

	if data == null:
		return

	var msg_type: String = data.get("t", "")

	match msg_type:
		"state":
			# Spielzustand empfangen
			var state_data: Dictionary = data.get("d", {})
			emit_signal("game_state_received", state_data)

		"event":
			# Match Event empfangen
			var event_type: String = data.get("e", "")
			var event_data: Dictionary = data.get("d", {})
			emit_signal("match_event_received", event_type, event_data)

		"ping":
			# Ping empfangen -> Pong senden
			_send_data({
				"t": "pong",
				"id": data.get("id", 0),
				"ts": data.get("ts", 0)
			})

		"pong":
			# Pong empfangen -> Ping berechnen
			_handle_pong(data)


# ========== PRIVATE: PING ==========

func _send_ping() -> void:
	if data_channel == null or data_channel.get_ready_state() != WebRTCDataChannel.STATE_OPEN:
		return

	ping_id += 1
	ping_send_time = Time.get_ticks_msec()

	_send_data({
		"t": "ping",
		"id": ping_id,
		"ts": ping_send_time
	})


func _handle_pong(data: Dictionary) -> void:
	var recv_id: int = data.get("id", 0)
	var recv_ts: int = data.get("ts", 0)

	# Nur wenn ID passt
	if recv_id != ping_id:
		return

	# RTT berechnen
	var now := Time.get_ticks_msec()
	var rtt := now - recv_ts

	# In History speichern (max 5 Werte)
	ping_history.append(rtt)
	if ping_history.size() > 5:
		ping_history.pop_front()

	# Gleitender Mittelwert
	var sum := 0.0
	for p in ping_history:
		sum += p
	current_ping_ms = int(sum / ping_history.size())

	emit_signal("ping_updated", current_ping_ms, connection_type)


# ========== STUN STATUS LOGGING ==========

func _start_stun_status_logging() -> void:
	stun_status_counter = 0
	stun_last_ice_state = ""
	if stun_status_timer != null:
		stun_status_timer.start()


func _on_stun_status_ping() -> void:
	# Nur während Signaling/Connecting loggen
	if current_state == ConnectionState.CONNECTED or current_state == ConnectionState.DISCONNECTED or current_state == ConnectionState.ERROR:
		stun_status_timer.stop()
		return

	stun_status_counter += 1
	if stun_status_counter > STUN_STATUS_MAX_PINGS:
		stun_status_timer.stop()
		return

	var ice_state := "unknown"
	if peer_connection != null:
		ice_state = str(peer_connection.get_connection_state())

	# Log nur bei Änderung oder jede 3. Runde
	if stun_status_counter == 1 or ice_state != stun_last_ice_state or stun_status_counter % 3 == 0:
		print("\n[Online][ICE] Versuch ", stun_status_counter, " - ICE-State: ", ice_state, " (versuche weiter)\n")
		stun_last_ice_state = ice_state


# Hilfslogik für ICE-Server-Fehler
func _log_ice_server_issue(msg: String) -> void:
	print("[Online][ICE] Hinweis: ", msg)


# ========== ÖFFENTLICHE HILFSMETHODEN ==========

## Gibt den aktuellen Verbindungsstatus als String zurück
func get_state_string() -> String:
	match current_state:
		ConnectionState.DISCONNECTED:
			return "Getrennt"
		ConnectionState.CREATING_MATCH:
			return "Erstelle Match..."
		ConnectionState.WAITING_FOR_JOINER:
			return "Warte auf Gegner..."
		ConnectionState.JOINING_MATCH:
			return "Trete bei..."
		ConnectionState.SIGNALING:
			return "Verbinde..."
		ConnectionState.CONNECTING:
			return "Verbindung wird hergestellt..."
		ConnectionState.CONNECTED:
			return "Verbunden"
		ConnectionState.ERROR:
			return "Fehler"
		_:
			return "Unbekannt"


## Prüft ob eine aktive Verbindung besteht
func is_connected_to_peer() -> bool:
	return current_state == ConnectionState.CONNECTED


## Gibt zurück ob wir Host sind
func is_host() -> bool:
	return current_role == Role.HOST


## Gibt das eigene Team zurück ("red" oder "blue")
## Host: gibt host_team zurück
## Joiner: gibt das gegenteilige Team vom Host zurück
func get_own_team() -> String:
	if current_role == Role.HOST:
		return host_team
	else:
		# Joiner spielt das andere Team
		return "blue" if host_team == "red" else "red"


## Gibt das gegnerische Team zurück ("red" oder "blue")
func get_opponent_team() -> String:
	return "blue" if get_own_team() == "red" else "red"


# ========== PRIVATE: BACKEND CLEANUP ==========

## Sendet DELETE Request an Backend um Match zu löschen
func _send_delete_match_request(code: String) -> void:
	if code.is_empty():
		return

	print("[Online] Sende DELETE Request für Match: ", code)

	var body := JSON.stringify({"room_code": code})
	var headers := ["Content-Type: application/json"]
	var url := API_BASE_URL + "delete_match.php"

	# Neuen HTTPRequest für Cleanup (damit andere Requests nicht blockiert werden)
	var req := HTTPRequest.new()
	add_child(req)
	req.request_completed.connect(func(result, response_code, _headers, _body):
		req.queue_free()
		if result == HTTPRequest.RESULT_SUCCESS and response_code == 200:
			print("[Online] Match erfolgreich vom Backend gelöscht")
		else:
			push_warning("[Online] DELETE Request fehlgeschlagen (Code: " + str(response_code) + ")")
	)

	req.request(url, headers, HTTPClient.METHOD_POST, body)
