# =============================================================================
# ModeSelectionMenu.gd - Multiplayer Mode Selection Menu
# =============================================================================
# Verwaltet die Auswahl zwischen lokalem und Online-Multiplayer.
# Navigiert zu:
# - GameSetupMenu.tscn (Lokales Spiel)
# - OnlineMenu.tscn (Online Multiplayer)
# =============================================================================

extends Control
class_name ModeSelectionMenu

# ========== NODE REFERENCES ==========
@onready var btn_local: Button = $CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonContainer/BtnLocal
@onready var btn_online: Button = $CenterContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonContainer/BtnOnline
@onready var btn_back: Button = $CenterContainer/PanelContainer/MarginContainer/VBoxContainer/BtnBack

# ========== MEMBER VARIABLES ==========
var audio_manager: Node = null


# ========== LIFECYCLE ==========

## Initialisiert das Menu und verbindet alle Button Signale
func _ready() -> void:
	# AudioManager Referenz holen
	audio_manager = get_node_or_null("/root/AudioManager")

	# Button Signale verbinden
	btn_local.pressed.connect(_on_local_pressed)
	btn_online.pressed.connect(_on_online_pressed)
	btn_back.pressed.connect(_on_back_pressed)


# ========== BUTTON HANDLERS ==========

## Wird aufgerufen wenn Local Button gedrückt wird
## Navigiert zum GameSetupMenu für lokales Multiplayer
func _on_local_pressed() -> void:
	_play_button_sound()

	# Wechsel zu GameSetupMenu.tscn
	get_tree().change_scene_to_file("res://scenes/menu/GameSetupMenu.tscn")


## Wird aufgerufen wenn Online Button gedrückt wird
## Navigiert zum OnlineMenu für Online Multiplayer
func _on_online_pressed() -> void:
	_play_button_sound()

	# Wechsel zu OnlineMenu.tscn
	get_tree().change_scene_to_file("res://scenes/menu/OnlineMenu.tscn")


## Wird aufgerufen wenn Back Button gedrückt wird
## Navigiert zurück zum MainMenu
func _on_back_pressed() -> void:
	_play_button_sound()

	# Zurück zum MainMenu
	get_tree().change_scene_to_file("res://scenes/menu/MainMenu.tscn")


# ========== HELPERS ==========

## Spielt Button-Click Sound über AudioManager
func _play_button_sound() -> void:
	if audio_manager and audio_manager.has_method("play_sfx"):
		audio_manager.play_sfx("button_click")
