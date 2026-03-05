extends Node3D
class_name FoosballAIController

## AI Controller für Godot RL Agents - Foosball Training
## Muss in der "AGENT" Gruppe sein damit sync.gd ihn findet
## Implementiert die erforderliche Interface: get_obs, get_reward, get_action_space, set_action
## Unterstützt Self-Play: Observations werden für Red Team gespiegelt damit beide das Spiel aus eigener Perspektive sehen

# Referenzen - werden vom Parent Scene gesetzt
var game_node: Node3D
var ball: RigidBody3D
var controlled_rods: Array[Node3D] = []  # Rod Nodes die dieser Agent steuert
var opponent_rods: Array[Node3D] = []  # Gegnerische Rod Nodes (für Observation)

# RL State
var reward: float = 0.0
var done: bool = false
var needs_reset: bool = false

# Heuristic Modus: "human" oder "model"
var heuristic: String = "human"

# Score Tracking für Reward-Berechnung
var _last_own_score: int = 0
var _last_opponent_score: int = 0
var _episode_steps: int = 0
const MAX_EPISODE_STEPS: int = 9000  # Ca. 180 Sekunden bei 50Hz (3 Min pro Episode - länger für strategisches Spiel)

# Idle Ball Penalty Tracking
var _ball_idle_timer: float = 0.0
var _last_ball_position: Vector3 = Vector3.ZERO
const BALL_IDLE_THRESHOLD: float = 0.1  # Ball-Bewegungs-Schwellwert
const BALL_IDLE_TIME: float = 5.0  # Sekunden vor Idle Penalty (erhöht für strategisches Spiel)
const BALL_IDLE_PENALTY: float = 0.005  # Penalty pro Idle-Check (reduziert um Erfolg über Geschwindigkeit zu priorisieren)

# Debug Output Control - auf false setzen für sauberes Training-Output
const DEBUG_VERBOSE: bool = false

# Team das dieser Agent steuert
@export var controlled_team: int = 1  # 0 = Red, 1 = Blue

# Mirror-Faktor: 1 für Blue (rechte Seite), -1 für Red (linke Seite)
# Bewirkt dass beide Agents das Spiel aus ihrer eigenen Perspektive sehen
var mirror: float = 1.0

# Action Storage
var rod_actions: Array[Vector2] = []  # [lateral, rotation] pro Rod

## Initialisiert den Controller und fügt ihn zur AGENT Gruppe hinzu
func _ready():
	# Zur AGENT Gruppe hinzufügen damit sync.gd uns findet
	add_to_group("AGENT")

	# Action Storage für 4 Rods initialisieren
	for i in range(4):
		rod_actions.append(Vector2.ZERO)

	# Mirror-Faktor basierend auf Team setzen
	# Red Team (linke Seite) sieht gespiegelte Observations
	mirror = 1.0 if controlled_team == 1 else -1.0

	print("FoosballAIController ready - Team: ", "Blue" if controlled_team == 1 else "Red", " Mirror: ", mirror)

## Initialisiert den Controller mit einer Game-Referenz und findet alle relevanten Spielobjekte
func init(game: Node3D):
	game_node = game

	# Ball finden
	ball = game.get_node_or_null("Ball")
	if ball == null:
		ball = get_tree().root.find_child("Ball", true, false)

	# Gesteuerte Rods finden
	_find_controlled_rods()

	print("FoosballAIController initialized with ", controlled_rods.size(), " rods")

## Findet alle Rods die zu unserem Team und zum Gegner-Team gehören
## Sortiert sie nach Rod Type für konsistente Observation-Reihenfolge
func _find_controlled_rods():
	controlled_rods.clear()
	opponent_rods.clear()

	var table = get_tree().root.find_child("Table", true, false)
	if table == null:
		push_error("FoosballAIController: Table not found!")
		return

	# Team Container finden
	var own_team_name = "TeamBlue" if controlled_team == 1 else "TeamRed"
	var opp_team_name = "TeamRed" if controlled_team == 1 else "TeamBlue"

	var own_team_node = table.get_node_or_null(own_team_name)
	var opp_team_node = table.get_node_or_null(opp_team_name)

	if own_team_node == null:
		push_error("FoosballAIController: ", own_team_name, " not found!")
		return

	# Alle eigenen Rod Children sammeln
	for child in own_team_node.get_children():
		if child.has_method("SetBotInput"):
			controlled_rods.append(child)

	# Alle Gegner Rod Children sammeln
	if opp_team_node != null:
		for child in opp_team_node.get_children():
			if child.has_method("SetBotInput"):
				opponent_rods.append(child)

	# Sortiere nach Rod Type (Goalkeeper, Defense, Midfield, Attack)
	controlled_rods.sort_custom(func(a, b): return a.get("Type") < b.get("Type"))
	opponent_rods.sort_custom(func(a, b): return a.get("Type") < b.get("Type"))

	print("Found ", controlled_rods.size(), " own rods and ", opponent_rods.size(), " opponent rods for ", own_team_name)

## ============ GODOT RL AGENTS INTERFACE ============

## Definiert den Observation Space für Python (Stable Baselines 3)
## Rückgabe: Dictionary mit Observation Space Definition
func get_obs_space() -> Dictionary:
	# Observations (gespiegelt für Red Team damit beide das Spiel aus eigener Perspektive sehen):
	# - Ball: Position (x, z), Velocity (vx, vz) = 4
	# - Eigene Rods (4 Rods): lateral_offset, rotation = 8
	# - Gegner Rods (4 Rods): lateral_offset, rotation = 8
	# Gesamt: 20 Floats
	return {
		"obs": {
			"size": [20],
			"space": "box",
			"low": -10.0,
			"high": 10.0
		}
	}

## Gibt aktuelle Observation mit Spiegelung für Red Team zurück
## Self-Play: Red Team sieht gespiegelte X-Koordinaten, sodass beide Teams das Spiel aus ihrer Perspektive erleben
## Rückgabe: Dictionary mit "obs" Array (20 Floats)
func get_obs() -> Dictionary:
	var obs: Array[float] = []

	# Ball Observation (normalisiert auf ~[-1, 1] Range)
	# X Position/Velocity gespiegelt für Red Team damit beide "vorwärts" als positives X sehen
	if ball != null and is_instance_valid(ball):
		var ball_pos = ball.global_position
		var ball_vel = ball.linear_velocity

		# NaN/Inf Schutz - wenn Ball invalid Position hat, nutze Nullen
		if not is_finite(ball_pos.x) or not is_finite(ball_pos.z) or \
		   not is_finite(ball_vel.x) or not is_finite(ball_vel.z):
			if DEBUG_VERBOSE:
				push_warning("FoosballAIController: Ball has NaN/Inf values, using zeros")
			obs.append_array([0.0, 0.0, 0.0, 0.0])
		else:
			# Normalisiere Positionen (Tisch ist ungefähr -6 bis 6 auf X, -3 bis 3 auf Z)
			# Wende Mirror-Faktor für Red Team an
			obs.append(_safe_float(mirror * ball_pos.x / 6.0))
			obs.append(_safe_float(ball_pos.z / 3.0))
			# Normalisiere Velocities (ebenfalls gespiegelt)
			obs.append(_safe_float(mirror * clampf(ball_vel.x / 10.0, -1.0, 1.0)))
			obs.append(_safe_float(clampf(ball_vel.z / 10.0, -1.0, 1.0)))
	else:
		obs.append_array([0.0, 0.0, 0.0, 0.0])

	# Eigene Rod Observations
	for rod in controlled_rods:
		if rod != null and is_instance_valid(rod):
			# Lateral Offset (normalisiert durch max offset ~3.0)
			var lateral = rod.get("CurrentLateralOffset")
			if lateral == null:
				lateral = 0.0
			obs.append(_safe_float(lateral / 3.0))

			# Rotation (Radians, typischerweise -PI bis PI)
			var rot = rod.rotation.z
			obs.append(_safe_float(rot / PI))
		else:
			obs.append_array([0.0, 0.0])

	# Eigene Rods auffüllen wenn wir weniger als 4 haben
	while obs.size() < 12:
		obs.append(0.0)

	# Gegnerische Rod Observations
	for rod in opponent_rods:
		if rod != null and is_instance_valid(rod):
			var lateral = rod.get("CurrentLateralOffset")
			if lateral == null:
				lateral = 0.0
			obs.append(_safe_float(lateral / 3.0))

			var rot = rod.rotation.z
			obs.append(_safe_float(rot / PI))
		else:
			obs.append_array([0.0, 0.0])

	# Gegner Rods auffüllen wenn wir weniger als 4 haben
	while obs.size() < 20:
		obs.append(0.0)

	return {"obs": obs}

## Gibt 0.0 zurück wenn Wert NaN oder Inf ist, sonst clampt auf [-10, 10]
## Schützt das neuronale Netz vor ungültigen Werten
func _safe_float(value: float) -> float:
	if is_nan(value) or is_inf(value):
		return 0.0
	return clampf(value, -10.0, 10.0)

## Gibt akkumulierten Reward seit letztem Aufruf zurück
## Wird von sync.gd nach jedem Step aufgerufen
func get_reward() -> float:
	return reward

## Setzt Reward-Akkumulator zurück (aufgerufen von sync.gd nach get_reward)
func zero_reward():
	reward = 0.0

## Definiert Action Space für Python (Stable Baselines 3)
## Rückgabe: Dictionary mit Action Space Definition (8 kontinuierliche Actions)
func get_action_space() -> Dictionary:
	# 4 Rods × 2 Actions jeweils (lateral, rotation) = 8 continuous Actions
	return {
		"rod_actions": {
			"size": 8,
			"action_type": "continuous"
		}
	}

## Wendet Actions vom neuronalen Netz an (aufgerufen von sync.gd vor jedem Step)
## action: Dictionary mit "rod_actions" Array (8 Floats im Range [-1, 1])
func set_action(action: Dictionary) -> void:
	var actions = action.get("rod_actions", [])

	# Parse Actions in per-Rod Vector2 (lateral, rotation)
	for i in range(mini(4, int(actions.size() / 2))):
		var lateral = clampf(actions[i * 2], -1.0, 1.0)
		var rotation = clampf(actions[i * 2 + 1], -1.0, 1.0)
		rod_actions[i] = Vector2(lateral, rotation)

## Prüft ob Episode fertig ist (von sync.gd aufgerufen)
## Rückgabe: true wenn Episode beendet werden soll
func get_done() -> bool:
	return done

## Setzt Done-Flag zurück (aufgerufen von sync.gd nach Episode-Reset)
func set_done_false():
	done = false

## Setzt Control Mode: "human" oder "model"
## "model" aktiviert AI-Steuerung, "human" deaktiviert sie
func set_heuristic(h: String):
	heuristic = h
	print("FoosballAIController heuristic set to: ", h)

## ============ GAME LOGIC ============

## Wendet AI-Actions auf Rods an und trackt Episode-Fortschritt
## Bestraft Idle-Ball-Situationen um dynamisches Spiel zu fördern
func _physics_process(delta):
	# Nur aktiv wenn im "model" Modus
	if heuristic != "model":
		return

	# Actions auf Rods anwenden
	for i in range(min(controlled_rods.size(), rod_actions.size())):
		var rod = controlled_rods[i]
		if rod != null and is_instance_valid(rod) and rod.has_method("SetBotInput"):
			rod.call("SetBotInput", rod_actions[i])

	# === IDLE BALL PENALTY ===
	# Prüfe ob Ball stuck/idle ist und bestrafe das Team das ihn spielen sollte
	if ball != null and is_instance_valid(ball):
		var ball_pos = ball.global_position
		var ball_movement = ball_pos.distance_to(_last_ball_position)
		if ball_movement < BALL_IDLE_THRESHOLD:
			_ball_idle_timer += delta

			# Wenn Ball zu lange idle war, bestrafe das verantwortliche Team
			if _ball_idle_timer >= BALL_IDLE_TIME:
				# Bestimme welches Team verantwortlich ist basierend auf nächster Rod zum Ball
				var responsible_team = _get_team_responsible_for_ball(ball_pos)

				# Bestrafe das Team das den Ball spielen sollte
				if responsible_team == controlled_team:
					reward -= BALL_IDLE_PENALTY

				# Timer zurücksetzen (wird erneut triggern wenn weiterhin idle)
				_ball_idle_timer = 0.0
		else:
			# Ball bewegt sich, idle Timer zurücksetzen
			_ball_idle_timer = 0.0

		_last_ball_position = ball_pos

	# Episode Step Counting
	_episode_steps += 1
	if _episode_steps >= MAX_EPISODE_STEPS:
		done = true
		needs_reset = true


## Bestimmt welches Team verantwortlich ist den Ball zu spielen.
## Nutzt die nächste Rod zur Ball-X-Position.
## Rückgabe: 0 = Red, 1 = Blue
##
## Rod X Positionen (SCALE = 5.0):
## - Red GK:      -5.0  (verteidigt linkes Tor)
## - Red Def:     -3.5
## - Blue Atk:    -2.0  (greift Red's Tor an)
## - Red Mid:     -0.75
## - Blue Mid:     0.75
## - Red Atk:      2.0  (greift Blue's Tor an)
## - Blue Def:     3.5
## - Blue GK:      5.0  (verteidigt rechtes Tor)
func _get_team_responsible_for_ball(ball_pos: Vector3) -> int:
	var ball_x = ball_pos.x

	# Finde nächste Rod und bestimme deren Team
	# Rod Positionen mit ihren Teams: [Position, Team (0=Red, 1=Blue)]
	var rod_positions = [
		[-5.0, 0],   # Red GK
		[-3.5, 0],   # Red Def
		[-2.0, 1],   # Blue Atk
		[-0.75, 0],  # Red Mid
		[0.75, 1],   # Blue Mid
		[2.0, 0],    # Red Atk
		[3.5, 1],    # Blue Def
		[5.0, 1],    # Blue GK
	]

	var nearest_team = 0
	var nearest_distance = 999.0

	for rod_data in rod_positions:
		var rod_x = rod_data[0]
		var rod_team = rod_data[1]
		var distance = abs(ball_x - rod_x)

		if distance < nearest_distance:
			nearest_distance = distance
			nearest_team = rod_team

	return nearest_team

## Wird aufgerufen wenn ein Tor erzielt wurde
## scoring_team: 1 = Red scored, 2 = Blue scored
## Vergibt positive/negative Rewards basierend darauf ob eigenes Team getroffen hat
func on_goal_scored(scoring_team: int):
	# scoring_team: 1 = Red scored (Ball in Blue Tor bei +X), 2 = Blue scored (Ball in Red Tor bei -X)
	# controlled_team: 0 = Red, 1 = Blue
	var our_team_scored = (scoring_team == 1 and controlled_team == 0) or \
						  (scoring_team == 2 and controlled_team == 1)

	if our_team_scored:
		reward += 3.0  # Erhöht von 1.5 um Scoring-Erfolg zu priorisieren
		if DEBUG_VERBOSE:
			print("AI REWARD: +3.0 (scored goal) - Team ", "Red" if controlled_team == 0 else "Blue")
	else:
		reward -= 2.0  # Erhöht von 1.0 um Defensive-Erfolg zu priorisieren
		if DEBUG_VERBOSE:
			print("AI REWARD: -2.0 (conceded goal) - Team ", "Red" if controlled_team == 0 else "Blue")

## Wird aufgerufen wenn eine Figure den Ball berührt
## Kleiner positiver Reward um Ball-Interaktion zu fördern
func on_ball_touched():
	reward += 0.01

## Setzt den Controller für eine neue Episode zurück
## Wird vom RLTrainingManager aufgerufen
func reset():
	reward = 0.0
	done = false
	needs_reset = false
	_episode_steps = 0
	_ball_idle_timer = 0.0
	_last_ball_position = Vector3.ZERO

	# Actions zurücksetzen
	for i in range(rod_actions.size()):
		rod_actions[i] = Vector2.ZERO

	# Rod Positionen zurücksetzen
	for rod in controlled_rods:
		if rod != null and is_instance_valid(rod) and rod.has_method("ResetPosition"):
			rod.call("ResetPosition")

	if DEBUG_VERBOSE:
		print("FoosballAIController reset")
