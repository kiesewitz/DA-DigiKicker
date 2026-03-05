# =============================================================================
# OnlineMenu.gd - Online Multiplayer Menu UI Controller
# =============================================================================
# Verwaltet das Online-Multiplayer Menü:
# - Spielername Eingabe
# - Host/Join Buttons
# - Room Code Eingabe
# - Liste offener Spiele
# - Verbindungsstatus Anzeige
# =============================================================================

extends Control
class_name OnlineMenu

# ========== NODE REFERENCES ==========
# Base path to VBoxContainer
const VBOX_PATH := "CenterContainer/PanelContainer/MarginContainer/VBoxContainer"

@onready var name_input: LineEdit = get_node(VBOX_PATH + "/NameSection/LineEditPlayerName")
@onready var team_section: VBoxContainer = get_node(VBOX_PATH + "/TeamSection")
@onready var btn_team_red: Button = get_node(VBOX_PATH + "/TeamSection/TeamButtonContainer/BtnTeamRed")
@onready var btn_team_blue: Button = get_node(VBOX_PATH + "/TeamSection/TeamButtonContainer/BtnTeamBlue")
@onready var btn_host: Button = get_node(VBOX_PATH + "/ActionSection/BtnHost")
@onready var btn_join: Button = get_node(VBOX_PATH + "/ActionSection/BtnJoin")
@onready var room_code_input: LineEdit = get_node(VBOX_PATH + "/JoinSection/LineEditRoomCode")
@onready var join_section: VBoxContainer = get_node(VBOX_PATH + "/JoinSection")
@onready var status_label: Label = get_node(VBOX_PATH + "/StatusSection/LabelStatus")
@onready var room_code_display: Label = get_node(VBOX_PATH + "/StatusSection/LabelRoomCode")
@onready var btn_cancel: Button = get_node(VBOX_PATH + "/StatusSection/BtnCancel")
@onready var btn_back: Button = get_node(VBOX_PATH + "/ButtonContainer/BtnBack")
@onready var btn_start: Button = get_node(VBOX_PATH + "/ButtonContainer/BtnStartOnline")
@onready var open_games_container: VBoxContainer = get_node(VBOX_PATH + "/OpenGamesSection/ScrollContainer/OpenGamesList")
@onready var ping_label: Label = get_node(VBOX_PATH + "/StatusSection/LabelPing")

# ========== MEMBER VARIABLES ==========
var online_manager: OnlineMultiplayerManager = null
var audio_manager: Node = null
var connection_established: bool = false
var update_games_timer: Timer = null
var start_game_host_team: String = ""


# ========== LIFECYCLE ==========

## Initialisiert das Online Menu, erstellt OnlineMultiplayerManager und verbindet Signale
func _ready() -> void:
	# AudioManager Referenz holen
	audio_manager = get_node_or_null("/root/AudioManager")

	# OnlineMultiplayerManager erstellen oder finden
	online_manager = get_node_or_null("/root/OnlineMultiplayerManager")
	if online_manager == null:
		online_manager = OnlineMultiplayerManager.new()
		online_manager.name = "OnlineMultiplayerManager"
		get_tree().root.call_deferred("add_child", online_manager)

	# Signale verbinden
	online_manager.connection_state_changed.connect(_on_connection_state_changed)
	online_manager.status_message.connect(_on_status_message)
	online_manager.error_occurred.connect(_on_error_occurred)
	online_manager.match_created.connect(_on_match_created)
	online_manager.match_joined.connect(_on_match_joined)
	online_manager.opponent_joined.connect(_on_opponent_joined)
	online_manager.match_started.connect(_on_match_started)
	online_manager.ping_updated.connect(_on_ping_updated)
	online_manager.match_event_received.connect(_on_match_event_received)

	# Button Signale
	btn_host.pressed.connect(_on_host_pressed)
	btn_join.pressed.connect(_on_join_pressed)
	btn_cancel.pressed.connect(_on_cancel_pressed)
	btn_back.pressed.connect(_on_back_pressed)
	btn_start.pressed.connect(_on_start_pressed)

	# Room Code Input: Auto-Uppercase
	room_code_input.text_changed.connect(func(text):
		room_code_input.text = text.to_upper()
		room_code_input.caret_column = room_code_input.text.length()
	)

	# Initiale UI Einstellung
	_update_ui_for_state(OnlineMultiplayerManager.ConnectionState.DISCONNECTED)

	# Default Spielername
	if name_input.text.is_empty():
		name_input.text = "Spieler"

	# Timer für offene Spiele Updates
	update_games_timer = Timer.new()
	update_games_timer.wait_time = 5.0
	update_games_timer.timeout.connect(_refresh_open_games)
	add_child(update_games_timer)
	if visible:
		_refresh_open_games()
		update_games_timer.start()


## Wird aufgerufen bei Visibility-Änderung des Menus
## Startet/Stoppt den Timer für offene Spiele und führt Cleanup durch
func _notification(what: int) -> void:
	if what == NOTIFICATION_VISIBILITY_CHANGED:
		if update_games_timer == null:
			return
		if visible:
			# Bei Sichtbarkeit: Offene Spiele laden
			_refresh_open_games()
			update_games_timer.start()
		else:
			# Menu wird versteckt - Cleanup durchführen
			update_games_timer.stop()
			_cleanup_on_menu_exit()


# ========== BUTTON HANDLERS ==========

## Wird aufgerufen wenn Host Button gedrückt wird
## Erstellt ein neues Match als Host mit ausgewähltem Team
func _on_host_pressed() -> void:
	_play_button_sound()

	var player_name := name_input.text.strip_edges()
	if player_name.is_empty():
		_show_error("Bitte Spielername eingeben")
		return

	# Team-Auswahl ermitteln
	var selected_team := "red"
	if btn_team_blue.button_pressed:
		selected_team = "blue"

	online_manager.create_match(player_name, selected_team)


## Wird aufgerufen wenn Join Button gedrückt wird
## Tritt einem existierenden Match mit Room Code bei
func _on_join_pressed() -> void:
	_play_button_sound()

	var player_name := name_input.text.strip_edges()
	if player_name.is_empty():
		_show_error("Bitte Spielername eingeben")
		return

	var room_code := room_code_input.text.strip_edges()
	if room_code.length() != 6:
		_show_error("Room Code muss 6 Zeichen haben")
		return

	online_manager.join_match(room_code, player_name)


func _on_cancel_pressed() -> void:
	_play_button_sound()

	# Cleanup mit Backend-Request (löscht Lobby aus Datenbank)
	# Nur wenn Host und Lobby noch offen (waiting oder signaling)
	var should_cleanup_backend := false
	if online_manager.is_host():
		var state = online_manager.current_state
		if state == OnlineMultiplayerManager.ConnectionState.WAITING_FOR_JOINER or \
		   state == OnlineMultiplayerManager.ConnectionState.SIGNALING:
			should_cleanup_backend = true

	online_manager.disconnect_from_match(should_cleanup_backend)
	_update_ui_for_state(OnlineMultiplayerManager.ConnectionState.DISCONNECTED)


func _on_back_pressed() -> void:
	_play_button_sound()

	# Verbindung trennen falls verbunden
	# Mit Backend-Cleanup wenn Host und Lobby noch offen
	if online_manager:
		var should_cleanup_backend := false
		if online_manager.is_host():
			var state = online_manager.current_state
			if state == OnlineMultiplayerManager.ConnectionState.WAITING_FOR_JOINER or \
			   state == OnlineMultiplayerManager.ConnectionState.SIGNALING or \
			   state == OnlineMultiplayerManager.ConnectionState.CREATING_MATCH:
				should_cleanup_backend = true

		online_manager.disconnect_from_match(should_cleanup_backend)

	# Check if we're running as a standalone scene or as a submenu
	# If parent is MainMenu's SubMenuContainer, we hide; otherwise we navigate back
	if get_parent().name == "SubMenuContainer":
		# Hide this menu (running as submenu in MainMenu)
		hide()
	else:
		# Navigate back to ModeSelectionMenu (running as standalone scene)
		get_tree().change_scene_to_file("res://scenes/menu/ModeSelectionMenu.tscn")


func _on_start_pressed() -> void:
	_play_button_sound()

	if not connection_established:
		_show_error("Noch nicht verbunden")
		return

	if not online_manager.is_host():
		_show_error("Nur der Host kann das Spiel starten")
		return

	# Wenn Host: START_GAME Event an Joiner senden
	if online_manager.is_host():
		var own_team := online_manager.get_own_team()
		start_game_host_team = own_team
		var payload := {"host_team": own_team}
		online_manager.send_match_event("start_game", payload)
		print("[OnlineMenu] Host sendet start_game Event (Team: ", own_team, ")")

	# Beide: Spiel starten (Online Multiplayer Modus)
	_start_online_game()


# ========== ONLINE MANAGER CALLBACKS ==========

func _on_connection_state_changed(state: int) -> void:
	_update_ui_for_state(state)


func _on_status_message(message: String) -> void:
	status_label.text = message


func _on_error_occurred(message: String) -> void:
	_show_error(message)


func _on_match_created(room_code: String) -> void:
	room_code_display.text = "Room Code: " + room_code
	room_code_display.visible = true


func _on_match_joined(_room_code: String, host_name: String) -> void:
	status_label.text = "Verbinde mit " + host_name + "..."


func _on_opponent_joined(joiner_name: String) -> void:
	status_label.text = joiner_name + " ist beigetreten!\nVerbinde..."


func _on_match_started() -> void:
	connection_established = true
	status_label.text = "Verbunden!"
	btn_start.disabled = false


func _on_ping_updated(ping_ms: int, connection_type: String) -> void:
	if ping_label:
		var type_prefix := connection_type if not connection_type.is_empty() else ""
		if not type_prefix.is_empty():
			ping_label.text = type_prefix + " Ping: " + str(ping_ms) + " ms"
		else:
			ping_label.text = "Ping: " + str(ping_ms) + " ms"
		ping_label.visible = true


func _on_match_event_received(event_type: String, data: Dictionary) -> void:
	# START_GAME Event empfangen (nur für Joiner relevant)
	if event_type.to_lower() == "start_game":
		# Host Team aus Payload übernehmen falls vorhanden (redundant, aber hilfreich)
		if data.has("host_team") and online_manager:
			start_game_host_team = str(data.get("host_team", online_manager.host_team)).to_lower()
			online_manager.host_team = start_game_host_team

		if not online_manager.is_host():
			print("[OnlineMenu] Joiner empfängt start_game Event - starte Spiel")
			_start_online_game()


# ========== UI HELPERS ==========

func _update_ui_for_state(state: int) -> void:
	match state:
		OnlineMultiplayerManager.ConnectionState.DISCONNECTED:
			# Eingaben aktivieren
			name_input.editable = true
			team_section.visible = true
			btn_team_red.disabled = false
			btn_team_blue.disabled = false
			btn_host.disabled = false
			btn_join.disabled = false
			join_section.visible = true
			room_code_input.editable = true
			btn_cancel.visible = false
			btn_start.disabled = true
			room_code_display.visible = false
			ping_label.visible = false
			status_label.text = "Nicht verbunden"
			connection_established = false

		OnlineMultiplayerManager.ConnectionState.CREATING_MATCH, \
		OnlineMultiplayerManager.ConnectionState.WAITING_FOR_JOINER, \
		OnlineMultiplayerManager.ConnectionState.JOINING_MATCH, \
		OnlineMultiplayerManager.ConnectionState.SIGNALING, \
		OnlineMultiplayerManager.ConnectionState.CONNECTING:
			# Eingaben deaktivieren
			name_input.editable = false
			team_section.visible = false
			btn_host.disabled = true
			btn_join.disabled = true
			join_section.visible = false
			btn_cancel.visible = true
			btn_start.disabled = true

		OnlineMultiplayerManager.ConnectionState.CONNECTED:
			# Verbunden - Start Button aktivieren
			name_input.editable = false
			team_section.visible = false
			btn_host.disabled = true
			btn_join.disabled = true
			join_section.visible = false
			btn_cancel.visible = true
			btn_start.disabled = not online_manager.is_host()
			connection_established = true
			status_label.text = "Verbunden mit " + online_manager.opponent_name

		OnlineMultiplayerManager.ConnectionState.ERROR:
			# Fehler - zurück zu Disconnected UI
			call_deferred("_update_ui_for_state", OnlineMultiplayerManager.ConnectionState.DISCONNECTED)


func _show_error(message: String) -> void:
	status_label.text = "Fehler: " + message
	status_label.add_theme_color_override("font_color", Color.RED)

	# Nach 3 Sekunden Farbe zurücksetzen
	await get_tree().create_timer(3.0).timeout
	status_label.remove_theme_color_override("font_color")


func _play_button_sound() -> void:
	if audio_manager and audio_manager.has_method("play_sfx"):
		audio_manager.play_sfx("button_click")


# ========== OPEN GAMES LIST ==========

func _refresh_open_games() -> void:
	if not visible:
		print("[OnlineMenu] _refresh_open_games: Menu nicht sichtbar - skip")
		return

	print("[OnlineMenu] Lade offene Spiele...")
	print("[OnlineMenu] Request -> https://diplomarbeit.rath-net.at/api/list_matches.php?status=waiting")

	# HTTP Request für offene Spiele
	var http := HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(_on_open_games_received.bind(http))
	var err := http.request("https://diplomarbeit.rath-net.at/api/list_matches.php?status=waiting")
	if err != OK:
		print("[OnlineMenu] HTTP Request fehlgeschlagen: ", err)


func _on_open_games_received(result: int, response_code: int, _headers: PackedStringArray, body: PackedByteArray, http: HTTPRequest) -> void:
	http.queue_free()

	print("[OnlineMenu] Open Games Response - Result: ", result, " Code: ", response_code)

	if result != HTTPRequest.RESULT_SUCCESS or response_code != 200:
		print("[OnlineMenu] Fehler beim Laden der offenen Spiele")
		return

	var json_str := body.get_string_from_utf8()
	print("[OnlineMenu] Response Body: ", json_str)

	var json = JSON.parse_string(json_str)
	if json == null or not json.get("success", false):
		print("[OnlineMenu] JSON parsing fehlgeschlagen oder success=false")
		return

	# Liste leeren
	for child in open_games_container.get_children():
		child.queue_free()

	# Matches hinzufügen
	var matches: Array = json.get("matches", [])
	print("[OnlineMenu] Gefundene Matches: ", matches.size())

	if matches.is_empty():
		var label := Label.new()
		label.text = "Keine offenen Spiele"
		label.add_theme_color_override("font_color", Color(0.5, 0.5, 0.5))
		open_games_container.add_child(label)
		print("[OnlineMenu] Keine offenen Spiele - Label hinzugefügt")
		return

	for match_data in matches:
		var item := _create_game_list_item(match_data)
		open_games_container.add_child(item)
		print("[OnlineMenu] Match hinzugefügt: ", match_data.get("host_name", "???"))


func _create_game_list_item(match_data: Dictionary) -> HBoxContainer:
	var container := HBoxContainer.new()

	# Host Name
	var name_label := Label.new()
	name_label.text = match_data.get("host_name", "???")
	name_label.custom_minimum_size.x = 120
	container.add_child(name_label)

	# Room Code
	var code_label := Label.new()
	code_label.text = match_data.get("room_code", "------")
	code_label.add_theme_color_override("font_color", Color(0, 0.85, 1))
	code_label.custom_minimum_size.x = 80
	container.add_child(code_label)

	# Join Button
	var join_btn := Button.new()
	join_btn.text = "Beitreten"
	join_btn.custom_minimum_size.x = 80
	join_btn.pressed.connect(func():
		room_code_input.text = match_data.get("room_code", "")
		_on_join_pressed()
	)
	container.add_child(join_btn)

	return container


# ========== CLEANUP ==========

func _cleanup_on_menu_exit() -> void:
	# Nur cleanup wenn noch nicht verbunden (Connected State kann weiter laufen)
	if not online_manager:
		return

	# Wenn Host und Lobby noch offen -> Backend Cleanup
	var should_cleanup_backend := false
	if online_manager.is_host():
		var state = online_manager.current_state
		if state == OnlineMultiplayerManager.ConnectionState.WAITING_FOR_JOINER or \
		   state == OnlineMultiplayerManager.ConnectionState.SIGNALING or \
		   state == OnlineMultiplayerManager.ConnectionState.CREATING_MATCH:
			should_cleanup_backend = true
			online_manager.disconnect_from_match(should_cleanup_backend)


# ========== GAME START ==========

func _start_online_game() -> void:
	# GameManager konfigurieren für Online Multiplayer
	var game_manager := get_node_or_null("/root/GameManager")
	if game_manager == null:
		_show_error("GameManager nicht gefunden")
		return

	# Online Modus setzen
	game_manager.IsMultiplayer = true

	# Team basierend auf eigener Team-Auswahl zuweisen
	var host_team_choice := start_game_host_team if not start_game_host_team.is_empty() else online_manager.host_team
	if host_team_choice.is_empty():
		host_team_choice = "red"

	var own_team := online_manager.get_own_team()
	# Sicherheit: falls Start-Event bekannt, force Auswahl basierend auf host_team_choice
	if online_manager.is_host():
		own_team = host_team_choice
	else:
		own_team = "blue" if host_team_choice == "red" else "red"

	if own_team == "red":
		game_manager.PlayerTeam = 1  # GameManager.Team.Red
	else:
		game_manager.PlayerTeam = 2  # GameManager.Team.Blue

	# Eingabe-Zuweisung: eigenes Team bekommt das aktuelle Device (Keyboard oder Controller)
	# Gegner-Team bekommt Keyboard2 (wird eh ignoriert, da network-controlled)
	var input_manager = get_node_or_null("/root/InputManager")
	var player_device := 0  # Default: Keyboard

	if input_manager != null:
		# Hole das aktuelle Device von Player 1 (könnte Keyboard oder Controller sein)
		player_device = input_manager.get("Player1Device")
		print("[OnlineMenu] Player1Device: ", player_device)

	var KEYBOARD2 := 1  # Für Gegner (wird nicht benutzt)

	if own_team == "red":
		game_manager.RedTeamController = player_device  # Verwende aktuelles Device!
		game_manager.BlueTeamController = KEYBOARD2
		print("[OnlineMenu] Red Team Controller: ", player_device, " (Player's Device)")
	else:
		game_manager.RedTeamController = KEYBOARD2
		game_manager.BlueTeamController = player_device  # Verwende aktuelles Device!
		print("[OnlineMenu] Blue Team Controller: ", player_device, " (Player's Device)")

	# Nur eigenes Team erhält lokale Eingaben; Gegner wird als Bot markiert (Input blockiert, aber Netzwerk steuert)
	game_manager.RedTeamIsBot = own_team != "red"
	game_manager.BlueTeamIsBot = own_team != "blue"

	# Spieleinstellungen (Standard)
	game_manager.GameDuration = 3
	game_manager.WinScore = 5

	# Debug-Info
	print("[OnlineMenu] Starte Online-Spiel - Eigenes Team: ", own_team, " (PlayerTeam: ", game_manager.PlayerTeam, ")")

	# Scene wechseln
	get_tree().change_scene_to_file("res://scenes/game/Game.tscn")
