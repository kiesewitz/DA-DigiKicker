using Godot;
using System;

/// <summary>
/// Hauptsteuerung der 3D-Tischfußball Spielszene.
/// Verwaltet Camera-Setup, Pause-Funktionalität und Scene-Initialisierung.
/// Rod-Highlighting wird ausschließlich von Table.cs über InputManager Signals gehandhabt.
/// </summary>
public partial class Game : Node3D
{
	private Camera3D _mainCamera;
	private CanvasLayer _uiLayer;
	private Control _pauseMenu;
	private GameManager _gameManager;

	// Online Multiplayer Integration
	private Node _onlineIntegration;
	private bool _isOnlineGame = false;

	/// <summary>
	/// Initialisiert die Spielszene beim Start.
	/// Holt Manager-Referenzen, richtet Camera ein, lädt UI-Elemente und startet das Spiel.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_gameManager = GetNode<GameManager>("/root/GameManager");

		// Camera einrichten
		SetupCamera();

		// UI-Referenzen holen
		_uiLayer = GetNode<CanvasLayer>("UILayer");
		_pauseMenu = _uiLayer.GetNode<Control>("PauseMenu");

		// Prüfen ob Online Multiplayer aktiv ist und Integration einrichten
		SetupOnlineMultiplayer();

		// Spielstatus mit Einstellungen vom GameManager initialisieren
		_gameManager.StartGame(_gameManager.GameDuration, _gameManager.WinScore, _gameManager.CurrentWinCondition);

		// Pause-Menü initial verstecken
		if (_pauseMenu != null)
		{
			_pauseMenu.Visible = false;
		}
	}

	/// <summary>
	/// Prüft ob dies ein Online-Spiel ist und lädt das OnlineGameIntegration Script.
	/// Verbindet sich mit dem OnlineMultiplayerManager falls vorhanden.
	/// </summary>
	private void SetupOnlineMultiplayer()
	{
		// Prüfen ob OnlineMultiplayerManager existiert und verbunden ist
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");

		if (onlineManager == null)
		{
			GD.Print("Game: No OnlineMultiplayerManager found - local game");
			return;
		}

		// GDScript-Methode aufrufen um Verbindungsstatus zu prüfen
		var isConnected = onlineManager.Call("is_connected_to_peer");
		_isOnlineGame = isConnected.AsBool();

		if (!_isOnlineGame)
		{
			GD.Print("Game: OnlineMultiplayerManager not connected - local game");
			return;
		}

		GD.Print("Game: Online game detected - loading OnlineGameIntegration");

		// OnlineGameIntegration Script laden und instanziieren
		var integrationScript = GD.Load<GodotObject>("res://scripts/online/OnlineGameIntegration.gd") as Script;

		if (integrationScript != null)
		{
			_onlineIntegration = new Node();
			_onlineIntegration.SetScript(integrationScript);
			_onlineIntegration.Name = "OnlineGameIntegration";
			AddChild(_onlineIntegration);
			GD.Print("Game: OnlineGameIntegration loaded successfully");
		}
		else
		{
			GD.PrintErr("Game: Failed to load OnlineGameIntegration.gd");
		}
	}

	/// <summary>
	/// Verarbeitet Input-Events, insbesondere ESC-Taste für Pause-Menü.
	/// </summary>
	public override void _Input(InputEvent @event)
	{
		// ESC-Taste für Pause-Menü verarbeiten
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}

	/// <summary>
	/// Holt Referenz zur Haupt-Camera. Camera-Positionierung wird von CameraController.cs gehandhabt.
	/// </summary>
	private void SetupCamera()
	{
		_mainCamera = GetNode<Camera3D>("MainCamera");

		if (_mainCamera == null)
		{
			GD.PrintErr("MainCamera node not found in Game scene!");
		}
		// Camera-Position und -Rotation werden vom CameraController Script gesetzt (am MainCamera Node)
	}

	/// <summary>
	/// Wechselt zwischen Pause und Fortsetzen des Spiels.
	/// Schaltet Pause-Status und Menü-Sichtbarkeit um.
	/// </summary>
	private void TogglePause()
	{
		if (_gameManager == null || _pauseMenu == null)
			return;

		bool isPaused = GetTree().Paused;

		// Pause-Status umschalten
		GetTree().Paused = !isPaused;

		// Pause-Menü Sichtbarkeit umschalten
		_pauseMenu.Visible = !isPaused;

		// GameManager-Status aktualisieren
		if (!isPaused)
		{
			_gameManager.PauseGame();
		}
		else
		{
			_gameManager.ResumeGame();
		}
	}

	/// <summary>
	/// Wird vom GameManager aufgerufen wenn das Spiel endet.
	/// Zeigt den Game-Over Screen an.
	/// </summary>
	public void OnGameEnded()
	{
		// Game-Over Screen anzeigen
		var gameOverScreen = _uiLayer.GetNode<Control>("GameOverScreen");
		if (gameOverScreen != null)
		{
			gameOverScreen.Visible = true;
		}
	}
}
