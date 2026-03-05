using Godot;
using System;

/// <summary>
/// Pause-Menü das während des Spiels mit ESC aufgerufen werden kann.
/// Bietet Optionen zum Fortsetzen, Neustarten oder Zurückkehren zum Hauptmenü.
/// </summary>
public partial class PauseMenu : Control
{
	// Referenzen zu UI-Nodes
	private ColorRect _dimBackground;
	private Label _labelPaused;
	private Button _btnResume;
	private Button _btnRestart;
	private Button _btnMainMenu;

	// Manager-Referenzen
	private GameManager _gameManager;
	private AudioManager _audioManager;

	/// <summary>
	/// Initialisiert das Pause-Menü: Holt Node-Referenzen und verbindet Button-Signals.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// Node-Referenzen holen - passend zur Struktur in Game.tscn
		_dimBackground = GetNode<ColorRect>("DimBackground");
		_labelPaused = GetNode<Label>("CenterContainer/VBoxContainer/LabelPaused");
		_btnResume = GetNode<Button>("CenterContainer/VBoxContainer/BtnResume");
		_btnRestart = GetNode<Button>("CenterContainer/VBoxContainer/BtnRestart");
		_btnMainMenu = GetNode<Button>("CenterContainer/VBoxContainer/BtnMainMenu");

		// Button Signals verbinden
		_btnResume.Pressed += OnResume;
		_btnRestart.Pressed += OnRestart;
		_btnMainMenu.Pressed += OnMainMenu;

		// GameManager Referenz holen
		_gameManager = GetNode<GameManager>("/root/GameManager");
	}

	/// <summary>
	/// Wird beim Entfernen des Nodes aufgerufen.
	/// Trennt alle Button Signal-Verbindungen.
	/// </summary>
	public override void _ExitTree()
	{
		// Signals trennen
		_btnResume.Pressed -= OnResume;
		_btnRestart.Pressed -= OnRestart;
		_btnMainMenu.Pressed -= OnMainMenu;
	}

	/// <summary>
	/// Verarbeitet Input-Events.
	/// Schließt das Pause-Menü wenn ESC (ui_cancel) gedrückt wird.
	/// </summary>
	public override void _Input(InputEvent @event)
	{
		// Pause-Menü mit ESC-Taste schließen
		if (@event.IsActionPressed("ui_cancel") && Visible)
		{
			OnResume();
			GetViewport().SetInputAsHandled();
		}
	}

	/// <summary>
	/// Button-Handler für "Resume" Button.
	/// Setzt das Spiel fort und schließt das Pause-Menü.
	/// </summary>
	private void OnResume()
	{
		_audioManager?.PlaySFX("button_click");

		if (_gameManager != null)
		{
			_gameManager.ResumeGame();
		}
		else
		{
			// Fallback falls GameManager nicht verfügbar
			GetTree().Paused = false;
		}

		Visible = false;
	}

	/// <summary>
	/// Button-Handler für "Restart" Button.
	/// Lädt die aktuelle Szene neu um das Spiel neu zu starten.
	/// </summary>
	private void OnRestart()
	{
		_audioManager?.PlaySFX("button_click");
		_audioManager?.PlayWhistle(); // Pfiff für Neustart

		// Pause aufheben vor dem Neuladen
		GetTree().Paused = false;

		// Aktuelle Szene neu laden
		GetTree().ReloadCurrentScene();
	}

	/// <summary>
	/// Button-Handler für "Main Menu" Button.
	/// Trennt Online-Verbindung falls vorhanden und kehrt zum Hauptmenü zurück.
	/// </summary>
	private void OnMainMenu()
	{
		_audioManager?.PlaySFX("button_click");

		// Pause aufheben vor Szenenwechsel
		GetTree().Paused = false;

		// KRITISCH: Von Online-Match trennen falls dies ein Online-Spiel ist
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		if (onlineManager != null && onlineManager.HasMethod("is_connected_to_peer"))
		{
			bool isConnected = (bool)onlineManager.Call("is_connected_to_peer");
			if (isConnected)
			{
				GD.Print("[PauseMenu] Disconnecting from online match...");
				// Disconnect mit cleanup_backend = true aufrufen um Server zu benachrichtigen
				onlineManager.Call("disconnect_from_match", true);
			}
		}

		// Zurück zum Hauptmenü
		GetTree().ChangeSceneToFile("res://scenes/main/Main.tscn");
	}

	/// <summary>
	/// Zeigt das Pause-Menü an und setzt den Fokus auf den Resume Button.
	/// </summary>
	public void ShowMenu()
	{
		Visible = true;
		_btnResume.GrabFocus();
	}

	/// <summary>
	/// Versteckt das Pause-Menü.
	/// </summary>
	public void HideMenu()
	{
		Visible = false;
	}
}
