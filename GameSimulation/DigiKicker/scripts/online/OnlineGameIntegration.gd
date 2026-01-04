# =============================================================================
# OnlineGameIntegration.gd - Verbindet Online-Multiplayer mit dem Spiel
# =============================================================================
# Dieses Script wird zur Game Scene hinzugefügt wenn ein Online-Spiel gestartet wird.
# Es verknüpft den OnlineMultiplayerManager mit den Rods und dem Ball.
#
# Hinzufügen: In Game.tscn oder dynamisch beim Scene-Wechsel
# =============================================================================

extends Node
class_name OnlineGameIntegration

# ========== KONSTANTEN ==========
# Rod-Typen Mapping (muss mit Rod.cs enum übereinstimmen)
const ROD_TYPE_GOALKEEPER := 0
const ROD_TYPE_DEFENSE := 1
const ROD_TYPE_MIDFIELD := 2
const ROD_TYPE_ATTACK := 3

# ========== MEMBER VARIABLEN ==========
var online_manager: Node = null
var network_sync: Node = null
var game_manager: Node = null

# Referenzen zu den Spielobjekten
var red_rods: Array = []
var blue_rods: Array = []
var ball: Node3D = null
var table: Node3D = null

# Online State
var is_online_game: bool = false
var is_host: bool = false

# Input Override für Online-Spieler
var local_team: int = 0  # 1 = Red, 2 = Blue


# ========== LIFECYCLE ==========
func _ready() -> void:
	# Prüfe ob dies ein Online-Spiel ist
	online_manager = get_node_or_null("/root/OnlineMultiplayerManager")

	if online_manager == null:
		print("[OnlineIntegration] Kein OnlineMultiplayerManager - lokales Spiel")
		queue_free()  # Entferne dieses Script wenn nicht online
		return

	if not online_manager.is_connected_to_peer():
		print("[OnlineIntegration] Nicht verbunden - lokales Spiel")
		queue_free()
		return

	print("[OnlineIntegration] Online-Spiel erkannt!")
	is_online_game = true
	is_host = online_manager.is_host()

	# GameManager
	game_manager = get_node_or_null("/root/GameManager")
	_connect_game_manager_signals()

	# Bestimme welches Team wir steuern basierend auf der Auswahl
	# Host wählt Team, Joiner bekommt das andere
	var own_team_str: String = online_manager.get_own_team()
	local_team = 1 if own_team_str == "red" else 2
	print("[OnlineIntegration] Lokales Team: ", own_team_str, " (", local_team, ")")

	# NetworkGameSync erstellen
	network_sync = preload("res://scripts/online/NetworkGameSync.gd").new()
	network_sync.name = "NetworkGameSync"
	add_child(network_sync)

	# Warte mit call_deferred statt await (await funktioniert nicht zuverlässig in _ready)
	print("[OnlineIntegration DEBUG] Warte auf deferred call...")
	call_deferred("_deferred_setup")


func _deferred_setup() -> void:
	print("[OnlineIntegration DEBUG] _deferred_setup() aufgerufen")

	# Spielobjekte finden
	_find_game_objects()

	print("[OnlineIntegration DEBUG] _find_game_objects() abgeschlossen, verbinde Signale")
	# Signale verbinden
	if online_manager:
		online_manager.opponent_disconnected.connect(_on_opponent_disconnected)
		online_manager.match_event_received.connect(_on_match_event_received)
	print("[OnlineIntegration DEBUG] Setup abgeschlossen")


func _find_game_objects() -> void:
	print("[OnlineIntegration DEBUG] _find_game_objects() gestartet")

	# Table finden
	print("[OnlineIntegration DEBUG] Suche Table unter /root/Main/CurrentScene/Game/Table")
	table = get_node_or_null("/root/Main/CurrentScene/Game/Table") as Node3D
	if table == null:
		print("[OnlineIntegration DEBUG] Nicht gefunden, versuche /root/Game/Table")
		table = get_node_or_null("/root/Game/Table") as Node3D

	if table == null:
		print("[OnlineIntegration DEBUG] Table immer noch null, versuche relative Suche")
		# Versuche relative Suche vom aktuellen Node aus
		var parent = get_parent()
		if parent:
			print("[OnlineIntegration DEBUG] Parent: ", parent.name)
			table = parent.get_node_or_null("Table") as Node3D

	if table == null:
		push_error("[OnlineIntegration] Table nicht gefunden!")
		return
	else:
		print("[OnlineIntegration DEBUG] Table gefunden: ", table.get_path())

	# Rods finden
	print("[OnlineIntegration DEBUG] Suche TeamRed und TeamBlue")
	var team_red = table.get_node_or_null("TeamRed")
	var team_blue = table.get_node_or_null("TeamBlue")

	if team_red == null or team_blue == null:
		push_error("[OnlineIntegration] Team Nodes nicht gefunden! TeamRed=%s, TeamBlue=%s" % [team_red, team_blue])
		return
	else:
		print("[OnlineIntegration DEBUG] Teams gefunden: TeamRed=%s, TeamBlue=%s" % [team_red.name, team_blue.name])

	# Red Rods (sortiert nach Type: GK, Def, Mid, Att)
	print("[OnlineIntegration DEBUG] Sammle Red Rods")
	red_rods = _get_sorted_rods(team_red)

	# Blue Rods
	print("[OnlineIntegration DEBUG] Sammle Blue Rods")
	blue_rods = _get_sorted_rods(team_blue)

	print("[OnlineIntegration] Gefunden: %d Red Rods, %d Blue Rods" % [red_rods.size(), blue_rods.size()])

	# Ball finden
	print("[OnlineIntegration DEBUG] Suche Ball")
	ball = get_node_or_null("/root/Main/CurrentScene/Game/Ball") as Node3D
	if ball == null:
		print("[OnlineIntegration DEBUG] Nicht unter /root/Main/CurrentScene/Game/Ball, versuche /root/Game/Ball")
		ball = get_node_or_null("/root/Game/Ball") as Node3D

	if ball == null:
		print("[OnlineIntegration DEBUG] Ball immer noch null, versuche relative Suche")
		var parent = get_parent()
		if parent:
			ball = parent.get_node_or_null("Ball") as Node3D

	if ball == null:
		push_error("[OnlineIntegration] Ball nicht gefunden!")
		return
	else:
		print("[OnlineIntegration DEBUG] Ball gefunden: ", ball.get_path())

	print("[OnlineIntegration] Ball gefunden: ", ball.name)

	# Nur Host lässt die Tore triggern, Joiner deaktiviert lokale Treffer
	_configure_goal_triggers(is_host)

	# NetworkSync mit Referenzen versorgen (inkl. lokalem Team)
	if network_sync != null:
		network_sync.setup_references(red_rods, blue_rods, ball, local_team)

	# Gegnerische Rods deaktivieren (nur eigene Rods steuerbar)
	_disable_opponent_rods()

	# HUD Ping aktivieren
	_enable_hud_online_mode()


func _get_sorted_rods(team_node: Node) -> Array:
	var rods: Array = []

	# Alle Rods des Teams sammeln
	for child in team_node.get_children():
		if child.has_method("get_class") or child.get("Type") != null:
			rods.append(child)

	# Nach Rod Type sortieren (GK=0, Def=1, Mid=2, Att=3)
	rods.sort_custom(func(a, b):
		var type_a = a.get("Type") if a.get("Type") != null else 0
		var type_b = b.get("Type") if b.get("Type") != null else 0
		return type_a < type_b
	)

	return rods


func _disable_opponent_rods() -> void:
	print("[OnlineIntegration DEBUG] _disable_opponent_rods() gestartet")
	print("[OnlineIntegration DEBUG] local_team=%d, red_rods.size()=%d, blue_rods.size()=%d" % [local_team, red_rods.size(), blue_rods.size()])

	# Aktiviere Netzwerk-Kontrolle für gegnerische Rods (sie werden durch NetworkSync gesteuert)
	var opponent_rods: Array = []
	var own_rods: Array = []

	if local_team == 1:
		# Wir sind Red -> Blue Rods werden vom Netzwerk gesteuert
		opponent_rods = blue_rods
		own_rods = red_rods
		print("[OnlineIntegration] Wir sind RED - Blue Rods = Netzwerk, Red Rods = Lokal")
	else:
		# Wir sind Blue -> Red Rods werden vom Netzwerk gesteuert
		opponent_rods = red_rods
		own_rods = blue_rods
		print("[OnlineIntegration] Wir sind BLUE - Red Rods = Netzwerk, Blue Rods = Lokal")

	# Aktiviere Network Control NUR für Gegner-Rods
	for rod in opponent_rods:
		if rod != null and is_instance_valid(rod):
			if rod.has_method("SetNetworkControlled"):
				rod.call("SetNetworkControlled", true)
				var rod_type = rod.get("Type") if rod.get("Type") != null else "?"
				print("[OnlineIntegration] → Rod ", rod_type, " als NETWORK-CONTROLLED markiert")
			else:
				print("[OnlineIntegration] WARNUNG: Rod hat keine SetNetworkControlled Methode!")

	# Stelle sicher, dass eigene Rods NICHT network-controlled sind
	for rod in own_rods:
		if rod != null and is_instance_valid(rod):
			if rod.has_method("SetNetworkControlled"):
				rod.call("SetNetworkControlled", false)
				var rod_type = rod.get("Type") if rod.get("Type") != null else "?"
				print("[OnlineIntegration] → Rod ", rod_type, " als LOKAL-CONTROLLED markiert")


func _enable_hud_online_mode() -> void:
	# HUD finden und Online-Modus aktivieren
	var hud = get_node_or_null("/root/Main/CurrentScene/Game/UILayer/HUD")
	if hud == null:
		hud = get_node_or_null("/root/Game/UILayer/HUD")

	if hud != null and hud.has_method("EnableOnlineMode"):
		hud.EnableOnlineMode()


func _connect_game_manager_signals() -> void:
	if game_manager == null or online_manager == null:
		return

	# Nur Host sendet Ereignisse an den Gegner
	if not online_manager.is_host():
		return

	if not game_manager.is_connected("GoalScored", Callable(self, "_on_goal_scored")):
		game_manager.connect("GoalScored", Callable(self, "_on_goal_scored"))

	if not game_manager.is_connected("GameEnded", Callable(self, "_on_game_ended")):
		game_manager.connect("GameEnded", Callable(self, "_on_game_ended"))


func _configure_goal_triggers(enable_scoring: bool) -> void:
	if table == null:
		return

	var goals_node = table.get_node_or_null("Goals")
	if goals_node == null:
		return

	var triggers := [
		goals_node.get_node_or_null("GoalRed/GoalTrigger"),
		goals_node.get_node_or_null("GoalBlue/GoalTrigger")
	]

	for trigger in triggers:
		if trigger != null and is_instance_valid(trigger):
			trigger.set("monitoring", enable_scoring)
			trigger.set("monitorable", enable_scoring)
			trigger.set_process(enable_scoring)
			trigger.set_physics_process(enable_scoring)
			print("[OnlineIntegration] GoalTrigger %s aktiv=%s" % [trigger.name, enable_scoring])


# ========== CALLBACKS ==========
func _on_opponent_disconnected() -> void:
	print("[OnlineIntegration] Gegner getrennt!")

	# Match als beendet melden an Server
	_finish_match_on_server()

	# Zur�ck zum Hauptmenü
	if game_manager:
		game_manager.EndGame(local_team)  # Lokaler Spieler gewinnt durch Disconnect


func _on_goal_scored(team: int, new_score: int) -> void:
	if online_manager == null or not online_manager.is_host():
		return

	online_manager.send_match_event("goal", {"team": team, "score": new_score})


func _on_game_ended(winner: int) -> void:
	if online_manager == null or not online_manager.is_host():
		return

	online_manager.send_match_event("game_end", {"winner": winner})
	_finish_match_on_server()


func _on_match_event_received(event_type: String, data: Dictionary) -> void:
	match event_type:
		"goal":
			# Tor vom Gegner empfangen
			var scoring_team: int = data.get("team", 0)
			if game_manager:
				game_manager.call("ScoreGoal", scoring_team)

		"game_end":
			# Spielende empfangen
			var winner_team: int = data.get("winner", 0)
			_finish_match_on_server()
			if game_manager:
				game_manager.call("EndGame", winner_team)

		"reset":
			# Ball Reset empfangen
			if game_manager:
				game_manager.call("ResetBall")


# ========== SERVER KOMMUNIKATION ==========
func _finish_match_on_server() -> void:
	if online_manager == null:
		return

	var room_code: String = online_manager.room_code
	if room_code.is_empty():
		return

	# Ergebnis vom GameManager holen
	var score_red: int = 0
	var score_blue: int = 0
	var winner_name: String = ""

	if game_manager:
		score_red = game_manager.get("ScoreRed")
		score_blue = game_manager.get("ScoreBlue")

		if score_red > score_blue:
			winner_name = online_manager.player_name if is_host else online_manager.opponent_name
		elif score_blue > score_red:
			winner_name = online_manager.opponent_name if is_host else online_manager.player_name

	# HTTP Request an finish_match.php
	var http := HTTPRequest.new()
	add_child(http)
	http.request_completed.connect(func(r, c, h, b): http.queue_free())

	var body := JSON.stringify({
		"room_code": room_code,
		"winner_name": winner_name,
		"score_host": score_red if is_host else score_blue,
		"score_joiner": score_blue if is_host else score_red
	})

	var headers := ["Content-Type: application/json"]
	http.request("https://diplomarbeit.rath-net.at/api/finish_match.php", headers, HTTPClient.METHOD_POST, body)


# ========== HILFSMETHODEN ==========

## Sendet ein Match Event an den Gegner
func send_event(event_type: String, data: Dictionary = {}) -> void:
	if online_manager and online_manager.is_connected_to_peer():
		online_manager.send_match_event(event_type, data)


## Prüft ob wir der Host sind
func am_i_host() -> bool:
	return is_host


## Gibt das lokale Team zurück (1=Red, 2=Blue)
func get_local_team() -> int:
	return local_team
