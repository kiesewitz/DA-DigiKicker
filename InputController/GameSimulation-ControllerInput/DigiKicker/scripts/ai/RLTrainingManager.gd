extends Node3D
class_name RLTrainingManager

## RL Training Manager für DigiKicker Foosball
## Arbeitet mit bestehendem GameManager Autoload für korrekte Table/Rod Initialisierung
## Handhabt AI Controller Setup, Reward Signals und Episode Management für Self-Play Training

@onready var ball: RigidBody3D = $Ball
@onready var table: Node3D = $Table
@onready var ai_controller_blue: Node3D = $AIControllerBlue
@onready var ai_controller_red: Node3D = $AIControllerRed
var ai_controllers: Array[Node3D] = []

# Autoload Referenzen
var game_manager: Node
var audio_manager: Node

# Game State Tracking (spiegelt GameManager)
var score_red: int = 0
var score_blue: int = 0
var _last_score_red: int = 0
var _last_score_blue: int = 0

# Episode Management
const MAX_EPISODE_TIME: float = 60.0  # Maximale Episode-Länge in Sekunden
var _episode_time: float = 0.0

# Debug Output Control - auf false setzen für sauberes Training-Output
const DEBUG_VERBOSE: bool = false

# Ball Tracking
const BALL_RESET_Y: float = 0.425
const BALL_RESET_POS: Vector3 = Vector3(0, 0.425, 0)

# Tor-Erkennung Schwellwerte (direkte Ball-Position Überwachung)
# Tisch ist 12 Units lang (X: -6 bis +6), Tore bei X = ±6.0
# Tor-Öffnung ist ca. 1.82 Units breit (GOAL_WIDTH), zentriert bei Z=0
const GOAL_X_THRESHOLD: float = 5.9  # Ball über dieses X = in Tor-Bereich (leicht vor Tor-Linie)
const GOAL_Z_WIDTH: float = 1.5  # Ball muss innerhalb dieser Z-Range sein um als Tor zu zählen (großzügig)
const BALL_OUT_Y_THRESHOLD: float = -0.5  # Ball fiel unter den Tisch
var _goal_cooldown: float = 0.0  # Verhindert Doppel-Erkennung
var _debug_frame_counter: int = 0  # Für periodisches Debug-Output

## Initialisiert den Training Manager und konfiguriert die Scene für RL Training
func _ready():
	print("==================================================")
	print("RLTrainingManager starting...")
	print("==================================================")

	# Autoloads holen
	game_manager = get_node_or_null("/root/GameManager")
	audio_manager = get_node_or_null("/root/AudioManager")

	# Audio während Training stummschalten für Performance
	_mute_audio_for_training()

	if game_manager == null:
		push_error("GameManager not found! Training requires GameManager autoload.")
		push_error("Make sure to run the RLTraining scene with GameManager enabled.")
		return

	# GameManager für Training-Modus konfigurieren
	_configure_game_manager_for_training()

	# Warte bis Scene Tree bereit ist
	await get_tree().process_frame
	await get_tree().process_frame
	await get_tree().create_timer(0.5).timeout  # Warte bis Table Rods gespawnt hat

	# AI Controllers Liste aufbauen
	ai_controllers.clear()
	if ai_controller_blue:
		ai_controllers.append(ai_controller_blue)
	if ai_controller_red:
		ai_controllers.append(ai_controller_red)

	# Alle AI Controllers mit Referenzen initialisieren
	for controller in ai_controllers:
		if controller and controller.has_method("init"):
			controller.init(self)

	print("Initialized ", ai_controllers.size(), " AI controllers for Self-Play")

	# Mit GameManager Goal Signal verbinden
	if game_manager.has_signal("GoalScored"):
		game_manager.GoalScored.connect(_on_game_manager_goal_scored)
		print("Connected to GameManager.GoalScored signal")

	print("RLTrainingManager ready!")
	print("==================================================")

## Konfiguriert GameManager Einstellungen für RL Training (Self-Play)
## Setzt beide Teams auf AI-Kontrolle und startet ein unendliches Spiel
func _configure_game_manager_for_training():
	if game_manager == null:
		return

	# Als Bot vs Bot einrichten - beide Teams von AI Controllern gesteuert
	if "IsMultiplayer" in game_manager:
		game_manager.IsMultiplayer = true
	if "RedTeamIsBot" in game_manager:
		game_manager.RedTeamIsBot = false  # AI Controller handhabt Red
	if "BlueTeamIsBot" in game_manager:
		game_manager.BlueTeamIsBot = false  # AI Controller handhabt Blue

	# Spiel starten damit CurrentState = Playing und Tore registriert werden können
	# Nutze sehr lange Duration und hohen Win Score um Spielende zu verhindern
	if game_manager.has_method("StartGame"):
		# StartGame(duration_minutes, win_score, condition)
		# WinCondition: TimeLimit=0, FirstToScore=1, Both=2
		game_manager.StartGame(999, 9999, 1)  # 999 Minuten, 9999 Tore benötigt, FirstToScore
		print("GameManager.StartGame() called for training")

		# Countdown überspringen - direkt zu Playing State gehen
		# StartGame setzt State auf Paused und fordert Countdown an, also rufen wir OnCountdownComplete auf
		if game_manager.has_method("OnCountdownComplete"):
			game_manager.OnCountdownComplete()
			print("Countdown skipped - game now in Playing state")
	else:
		push_warning("GameManager.StartGame() not found - goals may not register!")

	print("GameManager configured for Self-Play training mode")

## Physics Update Loop - prüft Episode-Status, Tor-Erkennung und Ball-Position
func _physics_process(delta):
	if game_manager == null:
		return

	# Episode Zeit tracken
	_episode_time += delta

	# Goal Cooldown reduzieren
	if _goal_cooldown > 0:
		_goal_cooldown -= delta

	# Prüfen ob ein AI Controller einen vollen Episode-Reset benötigt
	var any_needs_reset = false
	for controller in ai_controllers:
		if controller and controller.needs_reset:
			any_needs_reset = true
			break

	if any_needs_reset:
		_full_episode_reset()
		return

	# Direkte Ball-Position Tor-Erkennung (zuverlässiger als GameManager Signals)
	_check_ball_position_for_goal()

	# Episode Timeout - nach MAX_EPISODE_TIME Episode beenden
	if _episode_time >= MAX_EPISODE_TIME:
		if DEBUG_VERBOSE:
			print("Episode timeout - resetting")
		for controller in ai_controllers:
			if controller:
				controller.done = true
				controller.needs_reset = true

## Direkte Ball-Position Überwachung für Tor-Erkennung
## Prüft ob Ball in Tor-Bereich ist oder unter den Tisch gefallen ist
func _check_ball_position_for_goal():
	if ball == null or _goal_cooldown > 0:
		return

	var ball_pos = ball.global_position

	# Debug: Ball-Position alle 100 Frames ausgeben (nur wenn verbose)
	_debug_frame_counter += 1
	if DEBUG_VERBOSE and _debug_frame_counter % 100 == 0:
		print("Ball pos: X=", "%.2f" % ball_pos.x, " Y=", "%.2f" % ball_pos.y, " Z=", "%.2f" % ball_pos.z)

	# Prüfen ob Ball aus dem Spiel gefallen ist (unter Tisch)
	if ball_pos.y < BALL_OUT_Y_THRESHOLD:
		if DEBUG_VERBOSE:
			print("Ball fell out of play at Y=", ball_pos.y)
		_reset_ball_to_center()
		_goal_cooldown = 0.5
		return

	# Prüfen ob Ball innerhalb Tor-Öffnung ist (Z Position Check)
	var in_goal_z_range = abs(ball_pos.z) < GOAL_Z_WIDTH

	# Debug: Loggen wenn Ball nahe Tor-Bereich ist (nur wenn verbose)
	if DEBUG_VERBOSE and abs(ball_pos.x) > 5.5:
		print("Ball near goal: X=", "%.2f" % ball_pos.x, " Z=", "%.2f" % ball_pos.z, " in_z_range=", in_goal_z_range)

	# Prüfen auf Tor auf Red Seite (Ball ging zu negativem X = Blue scored)
	if ball_pos.x < -GOAL_X_THRESHOLD and in_goal_z_range:
		if DEBUG_VERBOSE:
			print("GOAL detected at X=", ball_pos.x, " Z=", ball_pos.z, " - Blue scored!")
		_on_goal_detected(2)  # Blue scored (Team.Blue = 2)
		_goal_cooldown = 0.5
		return

	# Prüfen auf Tor auf Blue Seite (Ball ging zu positivem X = Red scored)
	if ball_pos.x > GOAL_X_THRESHOLD and in_goal_z_range:
		if DEBUG_VERBOSE:
			print("GOAL detected at X=", ball_pos.x, " Z=", ball_pos.z, " - Red scored!")
		_on_goal_detected(1)  # Red scored (Team.Red = 1)
		_goal_cooldown = 0.5
		return

	# Ball steckt im Tor-Bereich aber außerhalb Tor-Öffnung (an Seitenwand) - zurücksetzen
	if abs(ball_pos.x) > GOAL_X_THRESHOLD and not in_goal_z_range:
		if DEBUG_VERBOSE:
			print("Ball stuck at X=", ball_pos.x, " Z=", ball_pos.z, " (outside goal opening) - resetting")
		_reset_ball_to_center()
		_goal_cooldown = 0.5
		return

## Wird aufgerufen wenn GameManager GoalScored Signal emittiert
## Leitet Tor-Event an beide AI Controllers weiter
func _on_game_manager_goal_scored(scoring_team, _new_score):
	# Team Enum: None=0, Red=1, Blue=2
	if DEBUG_VERBOSE:
		print("GameManager goal signal: Team ", scoring_team, " Score: ", _new_score)
	_on_goal_detected(int(scoring_team))

## Handhabt Tor-Erkennung und triggert AI Rewards für beide Teams
## scoring_team: 1 = Red scored, 2 = Blue scored
func _on_goal_detected(scoring_team: int):
	if DEBUG_VERBOSE:
		print("GOAL! Team ", scoring_team, " scored!")

	# Alle AI Controllers benachrichtigen (jeder berechnet eigenen Reward basierend auf scoring_team)
	for controller in ai_controllers:
		if controller and controller.has_method("on_goal_scored"):
			controller.on_goal_scored(scoring_team)

	# Sofort Ball zur Mitte zurücksetzen für schnelles Training
	_reset_ball_to_center()

## Setzt Ball zur Mitte zurück mit zufälligem Impuls
## Instant-Reset ohne Cooldown für schnelles Training
func _reset_ball_to_center():
	if ball == null:
		return

	# Position und Velocity zurücksetzen
	ball.global_position = BALL_RESET_POS
	ball.linear_velocity = Vector3.ZERO
	ball.angular_velocity = Vector3.ZERO

	# Zufälligen Impuls anwenden um Spiel zu starten
	var rng = RandomNumberGenerator.new()
	rng.randomize()
	var dir = Vector3(rng.randf_range(-1.0, 1.0), 0, rng.randf_range(-1.0, 1.0)).normalized()
	ball.apply_central_impulse(dir * 0.5)

	if DEBUG_VERBOSE:
		print("Ball reset to center")

## Voller Episode-Reset für Self-Play
## Setzt beide AI Controllers, Ball und alle Rods zurück
func _full_episode_reset():
	if DEBUG_VERBOSE:
		print("Full episode reset")

	_episode_time = 0.0
	_last_score_red = 0
	_last_score_blue = 0

	# Alle AI Controllers zurücksetzen
	for controller in ai_controllers:
		if controller and controller.has_method("reset"):
			controller.reset()

	# Spiel durch GameManager zurücksetzen falls möglich
	if game_manager and game_manager.has_method("ResetGame"):
		game_manager.ResetGame()
	else:
		# Manueller Reset falls GameManager nicht verfügbar
		_reset_all_rods()
		_reset_ball_to_center()

## Setzt alle Rods beider Teams zur Mittelposition zurück
func _reset_all_rods():
	if table == null:
		return

	for team_name in ["TeamRed", "TeamBlue"]:
		var team_node = table.find_child(team_name, true, false)
		if team_node:
			for rod in team_node.get_children():
				if rod.has_method("ResetPosition"):
					rod.call("ResetPosition")

## Schaltet alle Audio Busse während Training stumm für Performance (auch weils nervig ist)
## Deaktiviert Master, Music und SFX Busse
func _mute_audio_for_training():
	# Mute Master, Music und SFX Busse
	var master_idx = AudioServer.get_bus_index("Master")
	var music_idx = AudioServer.get_bus_index("Music")
	var sfx_idx = AudioServer.get_bus_index("SFX")

	if master_idx >= 0:
		AudioServer.set_bus_mute(master_idx, true)
	if music_idx >= 0:
		AudioServer.set_bus_mute(music_idx, true)
	if sfx_idx >= 0:
		AudioServer.set_bus_mute(sfx_idx, true)

	print("Audio muted for training")
