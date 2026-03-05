# =============================================================================
# NetworkGameSync.gd - Spielzustand Synchronisation für Online Multiplayer
# =============================================================================
# Synchronisiert die 8 Stangen (Rods) und den Ball zwischen zwei Peers.
# Host sendet authoritative State, Joiner interpoliert für Smoothness.
#
# DATEN-FORMAT (kompakt für niedrige Latenz):
# - 8 Rods: lateral_offset (float), rotation (float)
# - Ball: position_x, position_z, velocity_x, velocity_z
# - Zeitstempel für Interpolation
# =============================================================================

extends Node
class_name NetworkGameSync

# ========== SIGNALE ==========
signal state_applied(state: Dictionary)
signal match_event_occurred(event_type: String, data: Dictionary)

# ========== KONSTANTEN ==========
# Tickrate: 30 Hz State Updates (alle 33ms) - erhöht für besseres Response
const TICK_RATE := 30.0
const TICK_INTERVAL := 1.0 / TICK_RATE

# Interpolation Buffer (in Sekunden) - reduziert für schnelleres Response
const INTERPOLATION_DELAY := 0.03  # 30ms Buffer (reduziert von 100ms)

# Maximum Extrapolation (bei Packet Loss)
const MAX_EXTRAPOLATION_TIME := 0.15  # 150ms

# ========== MEMBER VARIABLEN ==========
# Referenzen
var online_manager: OnlineMultiplayerManager = null
var game_manager: Node = null  # GameManager (C# Singleton)

# Rod References (werden beim Start gesetzt)
var red_rods: Array = []  # [Goalkeeper, Defense, Midfield, Attack]
var blue_rods: Array = []
var ball: Node3D = null

# Team Assignment (wird von OnlineGameIntegration gesetzt)
var local_team: int = 0  # 1 = Red, 2 = Blue
var own_rods: Array = []  # Unsere eigenen Rods (die wir senden)
var opponent_rods: Array = []  # Gegnerische Rods (die wir empfangen)

# Timing
var tick_timer: float = 0.0
var last_received_time: float = 0.0

# State Buffer für Interpolation (Joiner)
var state_buffer: Array = []  # Array von {time: float, state: Dictionary}
const STATE_BUFFER_SIZE := 20

# Letzte gesendete State (für Delta Compression)
var last_sent_state: Dictionary = {}

# Match Events Queue
var pending_events: Array = []


# ========== LIFECYCLE ==========
func _ready() -> void:
	# OnlineMultiplayerManager finden
	online_manager = get_node_or_null("/root/OnlineMultiplayerManager")
	if online_manager == null:
		# Falls als Child hinzugefügt
		online_manager = get_parent() as OnlineMultiplayerManager

	if online_manager:
		online_manager.game_state_received.connect(_on_game_state_received)
		online_manager.match_event_received.connect(_on_match_event_received)

	# GameManager finden
	game_manager = get_node_or_null("/root/GameManager")


func _physics_process(delta: float) -> void:
	if online_manager == null or not online_manager.is_connected_to_peer():
		return

	# Tick Timer für State Sending
	tick_timer += delta

	if tick_timer >= TICK_INTERVAL:
		tick_timer -= TICK_INTERVAL

		# Sende immer den eigenen State (Host: mit Ball, Joiner: nur eigene Rods)
		_send_game_state()

	# Joiner wendet interpolierten State an
	if not online_manager.is_host():
		_apply_interpolated_state()


# ========== PUBLIC API ==========

## Setzt die Rod und Ball Referenzen (aufrufen nach Scene-Load)
## team: 1 = Red, 2 = Blue (welches Team wir steuern)
func setup_references(red_rod_array: Array, blue_rod_array: Array, ball_node: Node3D, team: int = 1) -> void:
	red_rods = red_rod_array
	blue_rods = blue_rod_array
	ball = ball_node
	local_team = team

	# Bestimme welche Rods wir senden/empfangen
	if local_team == 1:  # Wir sind Red
		own_rods = red_rods
		opponent_rods = blue_rods
		print("[NetworkSync] Wir sind RED - senden Red, empfangen Blue")
	else:  # Wir sind Blue
		own_rods = blue_rods
		opponent_rods = red_rods
		print("[NetworkSync] Wir sind BLUE - senden Blue, empfangen Red")

	print("[NetworkSync] Referenzen gesetzt: %d Red Rods, %d Blue Rods, Ball=%s" % [
		red_rods.size(), blue_rods.size(), ball != null
	])


## Sendet ein Match Event (Tor, Reset, Spielende)
func send_event(event_type: String, data: Dictionary = {}) -> void:
	if online_manager and online_manager.is_connected_to_peer():
		online_manager.send_match_event(event_type, data)


## Verarbeitet empfangene Events in der Queue
func process_pending_events() -> Array:
	var events := pending_events.duplicate()
	pending_events.clear()
	return events


# ========== PRIVATE: STATE SENDING (Host) ==========

func _send_game_state() -> void:
	if online_manager == null or not online_manager.is_connected_to_peer():
		return

	if red_rods.is_empty() or blue_rods.is_empty() or own_rods.is_empty():
		return

	var state := _collect_game_state()

	# Optional: Delta Compression (nur geänderte Werte senden)
	# Für einfache Implementierung senden wir den kompletten State

	online_manager.send_game_state(state)


## Sammelt den aktuellen Spielzustand
func _collect_game_state() -> Dictionary:
	var sender_role := "host"
	if online_manager != null and not online_manager.is_host():
		sender_role = "joiner"

	var state := {
		"t": Time.get_ticks_msec(),  # Timestamp
		"role": sender_role,  # Wer sendet (host/joiner)
		"team": local_team,
		"rods": [],  # Eigene Rods (die wir senden)
		"ball": {}
	}

	# Sende nur unsere eigenen Rods
	for rod in own_rods:
		if rod != null and is_instance_valid(rod):
			var rod_pos_obj = rod.get("position")
			var rod_rot_obj = rod.get("rotation")
			if rod.has_method("GetNetworkPosition"):
				rod_pos_obj = rod.call("GetNetworkPosition")
			if rod.has_method("GetNetworkRotation"):
				rod_rot_obj = rod.call("GetNetworkRotation")

			var rod_pos: Vector3 = Vector3.ZERO
			var rod_rot: Vector3 = Vector3.ZERO

			if rod_pos_obj is Vector3:
				rod_pos = rod_pos_obj
			if rod_rot_obj is Vector3:
				rod_rot = rod_rot_obj
			state["rods"].append({
				"l": _round_float(rod_pos.z, 3),  # Lateral offset (Position.Z)
				"rot": _round_float(rod_rot.z, 3)  # Rotation um Z-Achse
			})
		else:
			state["rods"].append({"l": 0.0, "rot": 0.0})

	# Ball Position und Velocity (nur vom Host relevant)
	if sender_role == "host":
		if ball != null and is_instance_valid(ball):
			state["ball"] = {
				"x": _round_float(ball.global_position.x, 3),
				"z": _round_float(ball.global_position.z, 3),
				"vx": _round_float(ball.linear_velocity.x, 2),
				"vz": _round_float(ball.linear_velocity.z, 2)
			}
		else:
			state["ball"] = {"x": 0.0, "z": 0.0, "vx": 0.0, "vz": 0.0}
	else:
		state["ball"] = {}

	return state


## Rundet Float auf n Dezimalstellen (für kompaktere Daten)
func _round_float(value: float, decimals: int) -> float:
	var mult := pow(10, decimals)
	return round(value * mult) / mult


# ========== PRIVATE: STATE RECEIVING (Joiner) ==========

func _on_game_state_received(state: Dictionary) -> void:
	var sender_role: String = state.get("role", "host")

	# Host verarbeitet nur die Joiner-Rods direkt (kein Ball-Update vom Joiner)
	if online_manager != null and online_manager.is_host():
		if sender_role != "joiner":
			return

		_apply_state_direct(state, false)
		return

	# Joiner verarbeitet nur Host-States
	if sender_role != "host":
		return

	# State in Buffer speichern mit aktuellem Zeitstempel
	var buffered_state := {
		"recv_time": Time.get_ticks_msec() / 1000.0,
		"game_time": state.get("t", 0) / 1000.0,
		"state": state
	}

	state_buffer.append(buffered_state)

	# Buffer Size limitieren
	while state_buffer.size() > STATE_BUFFER_SIZE:
		state_buffer.pop_front()

	last_received_time = buffered_state["recv_time"]


## Wendet interpolierten State an (Joiner)
func _apply_interpolated_state() -> void:
	if state_buffer.size() < 2:
		# Nicht genug Daten für Interpolation
		if state_buffer.size() == 1:
			# Direkter Apply des letzten States
			_apply_state_direct(state_buffer[0]["state"])
		return

	# Render Time = aktuelle Zeit - Interpolation Delay
	var current_time := Time.get_ticks_msec() / 1000.0
	var render_time := current_time - INTERPOLATION_DELAY

	# Finde zwei States für Interpolation
	var before_state: Dictionary = {}
	var after_state: Dictionary = {}
	var before_time: float = 0.0
	var after_time: float = 0.0

	for i in range(state_buffer.size() - 1):
		var s1 = state_buffer[i]
		var s2 = state_buffer[i + 1]

		if s1["recv_time"] <= render_time and s2["recv_time"] >= render_time:
			before_state = s1["state"]
			after_state = s2["state"]
			before_time = s1["recv_time"]
			after_time = s2["recv_time"]
			break

	# Falls keine passenden States gefunden: Neuesten verwenden
	if before_state.is_empty():
		var latest = state_buffer.back()
		_apply_state_direct(latest["state"])
		return

	# Interpolationsfaktor berechnen
	var time_diff := after_time - before_time
	var t: float = 0.5
	if time_diff > 0.001:
		t = (render_time - before_time) / time_diff
		t = clamp(t, 0.0, 1.0)

	# Interpolierten State berechnen und anwenden
	_apply_interpolated_state_data(before_state, after_state, t)


func _apply_state_direct(state: Dictionary, apply_ball: bool = true) -> void:
	# Wende empfangenen State auf gegnerische Rods an
	var rods_data: Array = state.get("rods", [])
	for i in range(min(opponent_rods.size(), rods_data.size())):
		var rod = opponent_rods[i]
		var data = rods_data[i]
		if rod != null and is_instance_valid(rod):
			# Position und Rotation setzen
			var lat: float = data.get("l", 0.0)
			var rot_z: float = data.get("rot", 0.0)

			if rod.has_method("ApplyNetworkState"):
				rod.call("ApplyNetworkState", lat, rot_z)
			else:
				var current_pos = rod.get("position")
				var current_rot = rod.get("rotation")
				rod.set("position", Vector3(current_pos.x, current_pos.y, lat))
				rod.set("rotation", Vector3(current_rot.x, current_rot.y, rot_z))

	# Ball Position (autoritativ vom Host)
	if apply_ball:
		var ball_data: Dictionary = state.get("ball", {})
		if ball != null and is_instance_valid(ball):
			var target_pos := Vector3(
				ball_data.get("x", ball.global_position.x),
				ball.global_position.y,
				ball_data.get("z", ball.global_position.z)
			)
			ball.global_position = target_pos
			ball.linear_velocity = Vector3(
				ball_data.get("vx", 0.0),
				0,
				ball_data.get("vz", 0.0)
			)

	emit_signal("state_applied", state)


func _apply_interpolated_state_data(before: Dictionary, after: Dictionary, t: float) -> void:
	# Interpoliere gegnerische Rods
	var rods_before: Array = before.get("rods", [])
	var rods_after: Array = after.get("rods", [])

	for i in range(min(opponent_rods.size(), min(rods_before.size(), rods_after.size()))):
		var rod = opponent_rods[i]
		var data_before = rods_before[i]
		var data_after = rods_after[i]

		if rod != null and is_instance_valid(rod):
			# Lineare Interpolation
			var lat: float = lerpf(data_before.get("l", 0.0), data_after.get("l", 0.0), t)
			var rot: float = lerp_angle(data_before.get("rot", 0.0), data_after.get("rot", 0.0), t)

			if rod.has_method("ApplyNetworkState"):
				rod.call("ApplyNetworkState", lat, rot)
			else:
				var current_pos = rod.get("position")
				var current_rot = rod.get("rotation")
				rod.set("position", Vector3(current_pos.x, current_pos.y, lat))
				rod.set("rotation", Vector3(current_rot.x, current_rot.y, rot))

	# Ball interpolieren - schnellere Interpolation für besseres Response
	var ball_before: Dictionary = before.get("ball", {})
	var ball_after: Dictionary = after.get("ball", {})

	if ball != null and is_instance_valid(ball):
		var x := lerpf(ball_before.get("x", 0.0), ball_after.get("x", 0.0), t)
		var z := lerpf(ball_before.get("z", 0.0), ball_after.get("z", 0.0), t)

		# Schnellere Interpolation für responsiveres Gefühl (0.9 statt 0.5)
		var target := Vector3(x, ball.global_position.y, z)
		ball.global_position = ball.global_position.lerp(target, 0.9)

		# Velocity interpolieren
		var vx := lerpf(ball_before.get("vx", 0.0), ball_after.get("vx", 0.0), t)
		var vz := lerpf(ball_before.get("vz", 0.0), ball_after.get("vz", 0.0), t)
		ball.linear_velocity = Vector3(vx, 0, vz)

	emit_signal("state_applied", after)


# ========== PRIVATE: MATCH EVENTS ==========

func _on_match_event_received(event_type: String, data: Dictionary) -> void:
	print("[NetworkSync] Event empfangen: %s" % event_type)

	# Event in Queue speichern
	pending_events.append({
		"type": event_type,
		"data": data
	})

	emit_signal("match_event_occurred", event_type, data)


# ========== HILFSMETHODEN ==========

## Hilfsfunktion für Winkel-Interpolation
func lerp_angle(from: float, to: float, weight: float) -> float:
	var diff := fmod(to - from + PI, TAU) - PI
	return from + diff * weight
