using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Zentrale Verwaltung aller Spieler-Eingaben mit Controller-Erkennung und Key-Rebinding
/// Verarbeitet Tastatur- und Controller-Eingaben, verwaltet Stangen-Auswahl und dynamisches Input-Switching
/// </summary>
public partial class InputManager : Node
{
	/// <summary>
	/// Verfügbare Eingabegeräte: Zwei Tastaturlayouts und bis zu vier Controller
	/// </summary>
	public enum InputDevice { Keyboard, Keyboard2, Controller0, Controller1, Controller2, Controller3 }

	// Tastenbelegungs-System
	private InputBindings _bindings;

	// Signals für Input-Events
	[Signal] public delegate void ControllerConnectedEventHandler(int id);
	[Signal] public delegate void ControllerDisconnectedEventHandler(int id);
	[Signal] public delegate void JoyConnectionChangedEventHandler(int device, bool connected);
	[Signal] public delegate void RodSelectionChangedEventHandler(int player, int rodIndex);
	[Signal] public delegate void ControllerRodPairChangedEventHandler(int player, int leftRod, int rightRod, int activeRod);
	[Signal] public delegate void InputDeviceChangedEventHandler(int player, int newDevice);
	[Signal] public delegate void RebindingStartedEventHandler(int player, string actionName);
	[Signal] public delegate void RebindingCompletedEventHandler(int player, string actionName, string inputName);
	[Signal] public delegate void RebindingCancelledEventHandler(int player);

	// Gerätezuweisungen für Spieler
	public InputDevice Player1Device { get; private set; } = InputDevice.Keyboard;
	public InputDevice Player2Device { get; private set; } = InputDevice.Controller0;

	// Stangen-Auswahl für Tastatur-Spieler (Index 0-3: Torwart, Verteidigung, Mittelfeld, Angriff)
	private int _player1SelectedRod = 0;
	private int _player2SelectedRod = 0;

	// Controller Stangen-Paare (zwei Stangen gleichzeitig steuerbar)
	// Paar 0: Torwart (0) + Verteidigung (1) - gesteuert durch LINKEN Stick/Schultertaste
	// Paar 1: Mittelfeld (2) + Angriff (3) - gesteuert durch RECHTEN Stick/Schultertaste
	private int _player1LeftPairRods = 0;
	private int _player1RightPairRods = 0;
	private int _player2LeftPairRods = 0;
	private int _player2RightPairRods = 0;

	// Aktuell aktive Stange für visuelle Hervorhebung
	private int _player1ActiveRod = 0;
	private int _player2ActiveRod = 0;

	private const int MAX_RODS_PER_TEAM = 4;

	// Stangen-Paar Definitionen: Links (Torwart/Verteidigung), Rechts (Mittelfeld/Angriff)
	private static readonly int[] LEFT_PAIR_RODS = { 0, 1 };
	private static readonly int[] RIGHT_PAIR_RODS = { 2, 3 };

	// GameManager Referenz für Team-Zuordnung
	private GameManager _gameManager;

	// Liste aller verbundenen Controller-IDs
	private List<int> _connectedControllers = new List<int>();

	// Deadzone-Schwellwerte für Analog-Eingaben
	private const float STICK_DEADZONE = 0.15f;
	private const float ROTATION_DEADZONE = 0.35f;
	private const float TRIGGER_DEADZONE = 0.1f;

	// Dynamisches Input-Switching zwischen Tastatur und Controller im Singleplayer
	private bool _dynamicInputEnabled = true;
	private float _lastKeyboardInputTime = 0;
	private float _lastControllerInputTime = 0;
	private const float INPUT_SWITCH_COOLDOWN = 0.1f;

	// Zustand des Rebinding-Prozesses
	private bool _isRebinding = false;
	private int _rebindingPlayer = 0;
	private InputBindings.GameAction _rebindingAction;
	private bool _rebindingIsController = false;

	/// <summary>
	/// Initialisiert den InputManager und lädt gespeicherte Tastenbelegungen
	/// </summary>
	public override void _Ready()
	{
		GD.Print("InputManager initialized");

		// Tastenbelegungen initialisieren und aus Datei laden
		_bindings = new InputBindings();
		_bindings.LoadFromFile();

		// GameManager-Referenz für Team-Zuordnung abrufen
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");

		// Controller-Verbindungs-Signal registrieren
		Input.Singleton.JoyConnectionChanged += OnJoyConnectionChanged;

		// Bereits verbundene Controller erkennen und registrieren
		DetectControllers();
	}

	/// <summary>
	/// Verarbeitet dynamisches Input-Switching zwischen Tastatur und Controller im Singleplayer
	/// </summary>
	public override void _Process(double delta)
	{
		// Dynamisches Input-Switching nur für Singleplayer und Online-Multiplayer aktiv
		// Bei lokalem Multiplayer (zwei Spieler lokal) bleibt jeder bei seinem Gerät
		if (_dynamicInputEnabled && _gameManager != null)
		{
			bool isLocalMultiplayer = _gameManager.IsMultiplayer && !IsOnlineGame();
			if (!isLocalMultiplayer)
			{
				DetectAndSwitchInputDevice();
			}
		}
	}

	/// <summary>
	/// Prüft ob es sich um ein Online-Multiplayer-Spiel handelt
	/// </summary>
	/// <returns>True wenn Verbindung zu Remote-Peer besteht, sonst False</returns>
	private bool IsOnlineGame()
	{
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		if (onlineManager != null && onlineManager.HasMethod("is_connected_to_peer"))
		{
			return (bool)onlineManager.Call("is_connected_to_peer");
		}
		return false;
	}

	/// <summary>
	/// Erkennt aktive Eingaben von Tastatur oder Controller und wechselt Player1Device entsprechend
	/// </summary>
	private void DetectAndSwitchInputDevice()
	{
		float currentTime = (float)Time.GetTicksMsec() / 1000.0f;

		// Tastatur-Eingaben prüfen: alle Bewegungs- und Rotations-Aktionen von Spieler 1
		bool keyboardActive = _bindings.IsActionPressed(InputBindings.GameAction.P1_MoveUp, 1, false) ||
							  _bindings.IsActionPressed(InputBindings.GameAction.P1_MoveDown, 1, false) ||
							  _bindings.IsActionPressed(InputBindings.GameAction.P1_RotateLeft, 1, false) ||
							  _bindings.IsActionPressed(InputBindings.GameAction.P1_RotateRight, 1, false) ||
							  _bindings.IsActionPressed(InputBindings.GameAction.P1_SwitchRodLeft, 1, false) ||
							  _bindings.IsActionPressed(InputBindings.GameAction.P1_SwitchRodRight, 1, false);

		if (keyboardActive)
		{
			_lastKeyboardInputTime = currentTime;
		}

		// Controller-Eingaben prüfen: alle verbundenen Controller durchsuchen
		bool controllerActive = false;
		int activeControllerId = -1;

		foreach (int controllerId in _connectedControllers)
		{
			// Analog-Sticks auslesen
			float leftX = Mathf.Abs(Input.GetJoyAxis(controllerId, JoyAxis.LeftX));
			float leftY = Mathf.Abs(Input.GetJoyAxis(controllerId, JoyAxis.LeftY));
			float rightX = Mathf.Abs(Input.GetJoyAxis(controllerId, JoyAxis.RightX));
			float rightY = Mathf.Abs(Input.GetJoyAxis(controllerId, JoyAxis.RightY));

			// Prüfen ob irgendein Stick die Deadzone überschreitet
			if (leftX > STICK_DEADZONE || leftY > STICK_DEADZONE ||
				rightX > STICK_DEADZONE || rightY > STICK_DEADZONE)
			{
				controllerActive = true;
				activeControllerId = controllerId;
				break;
			}

			// Schultertasten prüfen (LB/RB)
			if (Input.IsJoyButtonPressed(controllerId, JoyButton.LeftShoulder) ||
				Input.IsJoyButtonPressed(controllerId, JoyButton.RightShoulder))
			{
				controllerActive = true;
				activeControllerId = controllerId;
				break;
			}
		}

		if (controllerActive)
		{
			_lastControllerInputTime = currentTime;
		}

		// Zu Tastatur wechseln wenn diese zuletzt benutzt wurde und aktuell Controller aktiv ist
		if (Player1Device != InputDevice.Keyboard &&
			_lastKeyboardInputTime > _lastControllerInputTime &&
			currentTime - _lastKeyboardInputTime < INPUT_SWITCH_COOLDOWN)
		{
			GD.Print("Dynamic switch: Controller -> Keyboard");
			Player1Device = InputDevice.Keyboard;

			// GameManager für Online-Multiplayer aktualisieren
			UpdateGameManagerTeamController(InputDevice.Keyboard);

			EmitSignal(SignalName.InputDeviceChanged, 1, (int)InputDevice.Keyboard);
			EmitSignal(SignalName.RodSelectionChanged, 1, _player1SelectedRod);
		}
		// Zu Controller wechseln wenn dieser zuletzt benutzt wurde und aktuell Tastatur aktiv ist
		else if (Player1Device == InputDevice.Keyboard &&
				 _lastControllerInputTime > _lastKeyboardInputTime &&
				 currentTime - _lastControllerInputTime < INPUT_SWITCH_COOLDOWN &&
				 activeControllerId >= 0)
		{
			InputDevice newDevice = GetDeviceFromControllerId(activeControllerId);
			GD.Print($"Dynamic switch: Keyboard -> Controller {activeControllerId}");
			Player1Device = newDevice;

			// GameManager für Online-Multiplayer aktualisieren
			UpdateGameManagerTeamController(newDevice);

			EmitSignal(SignalName.InputDeviceChanged, 1, (int)newDevice);
			EmitRodPairChangedSignal(1);
		}
	}

	/// <summary>
	/// Verarbeitet Input-Events für Stangenwechsel und Rebinding
	/// </summary>
	public override void _Input(InputEvent @event)
	{
		// Bei aktivem Rebinding: nur Rebinding-Eingaben verarbeiten
		if (_isRebinding)
		{
			HandleRebindingInput(@event);
			return;
		}

		// Tastatur-Stangenwechsel für lokalen Spieler verarbeiten
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
		{
			// Welche Tasten wurden gedrückt?
			bool isP1SwitchLeft = _bindings.MatchesBinding(keyEvent, InputBindings.GameAction.P1_SwitchRodLeft, 1, false);
			bool isP1SwitchRight = _bindings.MatchesBinding(keyEvent, InputBindings.GameAction.P1_SwitchRodRight, 1, false);
			bool isP2SwitchLeft = _bindings.MatchesBinding(keyEvent, InputBindings.GameAction.P2_SwitchRodLeft, 2, false);
			bool isP2SwitchRight = _bindings.MatchesBinding(keyEvent, InputBindings.GameAction.P2_SwitchRodRight, 2, false);

			if (!isP1SwitchLeft && !isP1SwitchRight && !isP2SwitchLeft && !isP2SwitchRight)
				return;

			// Bestimmen: welcher Spieler (für Variablen), welches Team (für Richtung), welche Taste (links/rechts)
			int varPlayer = 0; // 1 oder 2 - welche Variablen verwendet werden
			bool isBlueTeam = false;
			bool isSwitchLeft = false;
			bool isSwitchRight = false;

			if (_gameManager != null && _gameManager.IsMultiplayer && !IsOnlineGame())
			{
				// Lokaler Multiplayer: Gerät bestimmt welche Tasten akzeptiert werden
				// Keyboard1 = P1 Tasten, Keyboard2 = P2 Tasten
				if (_gameManager.RedTeamController == InputDevice.Keyboard && (isP1SwitchLeft || isP1SwitchRight))
				{
					varPlayer = 1;
					isBlueTeam = false;
					isSwitchLeft = isP1SwitchLeft;
					isSwitchRight = isP1SwitchRight;
				}
				else if (_gameManager.RedTeamController == InputDevice.Keyboard2 && (isP2SwitchLeft || isP2SwitchRight))
				{
					varPlayer = 1;
					isBlueTeam = false;
					isSwitchLeft = isP2SwitchLeft;
					isSwitchRight = isP2SwitchRight;
				}
				else if (_gameManager.BlueTeamController == InputDevice.Keyboard && (isP1SwitchLeft || isP1SwitchRight))
				{
					varPlayer = 2;
					isBlueTeam = true;
					isSwitchLeft = isP1SwitchLeft;
					isSwitchRight = isP1SwitchRight;
				}
				else if (_gameManager.BlueTeamController == InputDevice.Keyboard2 && (isP2SwitchLeft || isP2SwitchRight))
				{
					varPlayer = 2;
					isBlueTeam = true;
					isSwitchLeft = isP2SwitchLeft;
					isSwitchRight = isP2SwitchRight;
				}
			}
			else if (_gameManager != null && _gameManager.IsMultiplayer && IsOnlineGame())
			{
				// Online Multiplayer: Immer P1 Tasten, unabhängig vom Hauptgerät
				// (Spieler kann Controller + Tastatur gleichzeitig nutzen)
				if (isP1SwitchLeft || isP1SwitchRight)
				{
					varPlayer = 1; // Im Online-MP immer Player 1 Variablen
					isBlueTeam = _gameManager.PlayerTeam == GameManager.Team.Blue;
					isSwitchLeft = isP1SwitchLeft;
					isSwitchRight = isP1SwitchRight;
				}
			}
			else
			{
				// Singleplayer
				if (Player1Device == InputDevice.Keyboard || Player1Device == InputDevice.Keyboard2)
				{
					if (isP1SwitchLeft || isP1SwitchRight)
					{
						varPlayer = 1;
						isBlueTeam = _gameManager != null && _gameManager.PlayerTeam == GameManager.Team.Blue;
						isSwitchLeft = isP1SwitchLeft;
						isSwitchRight = isP1SwitchRight;
					}
				}
			}

			if (varPlayer == 0)
				return;

			if (isSwitchLeft)
			{
				// Rotes Team: niedrigerer Index nach links, Blaues Team: höherer Index nach links
				// varPlayer bestimmt welche Variable, isBlueTeam bestimmt die Richtung
				if (varPlayer == 2)
				{
					if (isBlueTeam)
						_player2SelectedRod = Mathf.Min(MAX_RODS_PER_TEAM - 1, _player2SelectedRod + 1);
					else
						_player2SelectedRod = Mathf.Max(0, _player2SelectedRod - 1);
				}
				else
				{
					if (isBlueTeam)
						_player1SelectedRod = Mathf.Min(MAX_RODS_PER_TEAM - 1, _player1SelectedRod + 1);
					else
						_player1SelectedRod = Mathf.Max(0, _player1SelectedRod - 1);
				}

				int sel = varPlayer == 2 ? _player2SelectedRod : _player1SelectedRod;
				int signalPlayer = isBlueTeam ? 2 : 1; // Für UI-Farbmarkierung
				GD.Print($"Player {signalPlayer} switched to rod {sel + 1} ({GetRodTypeName(sel)})");
				EmitSignal(SignalName.RodSelectionChanged, signalPlayer, sel);
			}
			else if (isSwitchRight)
			{
				// Rotes Team: höherer Index nach rechts, Blaues Team: niedrigerer Index nach rechts
				if (varPlayer == 2)
				{
					if (isBlueTeam)
						_player2SelectedRod = Mathf.Max(0, _player2SelectedRod - 1);
					else
						_player2SelectedRod = Mathf.Min(MAX_RODS_PER_TEAM - 1, _player2SelectedRod + 1);
				}
				else
				{
					if (isBlueTeam)
						_player1SelectedRod = Mathf.Max(0, _player1SelectedRod - 1);
					else
						_player1SelectedRod = Mathf.Min(MAX_RODS_PER_TEAM - 1, _player1SelectedRod + 1);
				}

				int sel = varPlayer == 2 ? _player2SelectedRod : _player1SelectedRod;
				int signalPlayer = isBlueTeam ? 2 : 1; // Für UI-Farbmarkierung
				GD.Print($"Player {signalPlayer} switched to rod {sel + 1} ({GetRodTypeName(sel)})");
				EmitSignal(SignalName.RodSelectionChanged, signalPlayer, sel);
			}
		}

		// Controller-Stangenwechsel über Schultertasten verarbeiten
		HandleControllerRodSwitching(@event);
	}

	/// <summary>
	/// Liefert einen lesbaren Namen für einen Stangen-Typ basierend auf dem Index
	/// </summary>
	/// <param name="rodIndex">Index der Stange (0=Torwart, 1=Verteidigung, 2=Mittelfeld, 3=Angriff)</param>
	/// <returns>Bezeichnung der Stange</returns>
	private string GetRodTypeName(int rodIndex)
	{
		return rodIndex switch
		{
			0 => "Goalkeeper",
			1 => "Defense",
			2 => "Midfield",
			3 => "Attack",
			_ => "Unknown"
		};
	}

	/// <summary>
	/// Verarbeitet Controller-Schultertasten zum Wechseln der Stangen innerhalb eines Paares
	/// Rotes Team: LB schaltet linkes Paar (TW/Vert), RB schaltet rechtes Paar (Mit/Ang)
	/// Blaues Team: LB und RB sind vertauscht entsprechend der Perspektive
	/// </summary>
	private void HandleControllerRodSwitching(InputEvent @event)
	{
		if (@event is not InputEventJoypadButton joyEvent || !joyEvent.Pressed)
			return;

		int deviceId = joyEvent.Device;
		InputDevice device = GetDeviceFromControllerId(deviceId);

		// Bestimmen welchem Spieler dieser Controller gehört
		int player = 0;
		bool playerFound = false;

		if (_gameManager != null && _gameManager.IsMultiplayer)
		{
			if (IsOnlineGame())
			{
				// Online Multiplayer: Nur der lokale Spieler steuert
				if (_gameManager.PlayerTeam == GameManager.Team.Red)
				{
					player = 1;
					playerFound = true;
				}
				else
				{
					player = 2;
					playerFound = true;
				}
			}
			else
			{
				// Lokaler Multiplayer: Prüfen welchem Team dieser Controller gehört
				if (_gameManager.RedTeamController == device)
				{
					player = 1;
					playerFound = true;
				}
				else if (_gameManager.BlueTeamController == device)
				{
					player = 2;
					playerFound = true;
				}
			}
		}
		else
		{
			// Im Singleplayer: Player1Device verwenden
			if (Player1Device == device)
			{
				player = 1;
				playerFound = true;
			}
		}

		if (!playerFound)
			return;

		// Prüfen ob dieser Spieler im blauen Team ist (benötigt vertauschte Schultertasten)
		bool isBlueTeam = false;
		if (_gameManager != null)
		{
			if (_gameManager.IsMultiplayer)
			{
				isBlueTeam = (player == 2);
			}
			else
			{
				isBlueTeam = (player == 1 && _gameManager.PlayerTeam == GameManager.Team.Blue);
			}
		}

		// Gegen Controller-Tastenbelegungen prüfen
		bool isToggleLeftButton = _bindings.MatchesBinding(joyEvent, InputBindings.GameAction.Controller_ToggleLeftPair, 1, true);
		bool isToggleRightButton = _bindings.MatchesBinding(joyEvent, InputBindings.GameAction.Controller_ToggleRightPair, 1, true);

		// Für blaues Team: LB und RB Funktionalität tauschen
		// Rot: Linker Stick = linkes Paar, Rechter Stick = rechtes Paar
		// Blau: Linker Stick = rechtes Paar, Rechter Stick = linkes Paar
		bool toggleLeftPair = (isToggleLeftButton && !isBlueTeam) || (isToggleRightButton && isBlueTeam);
		bool toggleRightPair = (isToggleRightButton && !isBlueTeam) || (isToggleLeftButton && isBlueTeam);

		// Im Online-Multiplayer gibt es nur einen lokalen Spieler, der immer Player 1 Variablen verwendet
		// Aber für das Signal brauchen wir den echten Team-Player für die korrekte Farbmarkierung
		int internalPlayer = (_gameManager != null && _gameManager.IsMultiplayer && IsOnlineGame()) ? 1 : player;
		int signalPlayer = player; // Für UI-Farbmarkierung den echten Team-Player verwenden

		if (toggleLeftPair)
		{
			// Linkes Paar umschalten (Torwart <-> Verteidigung)
			if (internalPlayer == 1)
			{
				_player1LeftPairRods = (_player1LeftPairRods + 1) % 2;
				_player1ActiveRod = LEFT_PAIR_RODS[_player1LeftPairRods];
				GD.Print($"Player {signalPlayer} left pair - switched to {GetRodTypeName(_player1ActiveRod)} (rod {_player1ActiveRod})");
				EmitRodPairChangedSignalForPlayer(signalPlayer, _player1LeftPairRods, _player1RightPairRods, _player1ActiveRod);
			}
			else
			{
				_player2LeftPairRods = (_player2LeftPairRods + 1) % 2;
				_player2ActiveRod = LEFT_PAIR_RODS[_player2LeftPairRods];
				GD.Print($"Player {signalPlayer} left pair - switched to {GetRodTypeName(_player2ActiveRod)} (rod {_player2ActiveRod})");
				EmitRodPairChangedSignalForPlayer(signalPlayer, _player2LeftPairRods, _player2RightPairRods, _player2ActiveRod);
			}
		}
		else if (toggleRightPair)
		{
			// Rechtes Paar umschalten (Mittelfeld <-> Angriff)
			if (internalPlayer == 1)
			{
				_player1RightPairRods = (_player1RightPairRods + 1) % 2;
				_player1ActiveRod = RIGHT_PAIR_RODS[_player1RightPairRods];
				GD.Print($"Player {signalPlayer} right pair - switched to {GetRodTypeName(_player1ActiveRod)} (rod {_player1ActiveRod})");
				EmitRodPairChangedSignalForPlayer(signalPlayer, _player1LeftPairRods, _player1RightPairRods, _player1ActiveRod);
			}
			else
			{
				_player2RightPairRods = (_player2RightPairRods + 1) % 2;
				_player2ActiveRod = RIGHT_PAIR_RODS[_player2RightPairRods];
				GD.Print($"Player {signalPlayer} right pair - switched to {GetRodTypeName(_player2ActiveRod)} (rod {_player2ActiveRod})");
				EmitRodPairChangedSignalForPlayer(signalPlayer, _player2LeftPairRods, _player2RightPairRods, _player2ActiveRod);
			}
		}
	}

	/// <summary>
	/// Sendet Signal um Änderungen am Stangen-Paar und aktiver Stange zu kommunizieren
	/// </summary>
	private void EmitRodPairChangedSignal(int player)
	{
		if (player == 1)
		{
			int leftRod = LEFT_PAIR_RODS[_player1LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player1RightPairRods];
			EmitSignal(SignalName.ControllerRodPairChanged, player, leftRod, rightRod, _player1ActiveRod);
		}
		else
		{
			int leftRod = LEFT_PAIR_RODS[_player2LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player2RightPairRods];
			EmitSignal(SignalName.ControllerRodPairChanged, player, leftRod, rightRod, _player2ActiveRod);
		}
	}

	/// <summary>
	/// Sendet Signal mit expliziten Werten für Online-Multiplayer (korrekte Farbmarkierung)
	/// </summary>
	private void EmitRodPairChangedSignalForPlayer(int player, int leftPairIndex, int rightPairIndex, int activeRod)
	{
		int leftRod = LEFT_PAIR_RODS[leftPairIndex];
		int rightRod = RIGHT_PAIR_RODS[rightPairIndex];
		EmitSignal(SignalName.ControllerRodPairChanged, player, leftRod, rightRod, activeRod);
	}

	/// <summary>
	/// Liefert die Eingabe für eine bestimmte Stange eines Spielers
	/// </summary>
	/// <param name="player">Spieler-Index (1=Rot, 2=Blau)</param>
	/// <param name="rodIndex">Stangen-Index (0-3)</param>
	/// <returns>Vector2 mit x=Lateralbewegung, y=Rotation</returns>
	public Vector2 GetRodInput(int player, int rodIndex)
	{
		// Eingaben während Countdown blockieren
		if (_gameManager != null && (_gameManager.IsCountdownActive || _gameManager.IsWaitingForCountdown))
		{
			return Vector2.Zero;
		}

		// Eingaben blockieren wenn Spiel nicht im Playing-Zustand ist
		if (_gameManager != null && _gameManager.CurrentState != GameManager.GameState.Playing)
		{
			return Vector2.Zero;
		}

		// Korrektes Eingabegerät ermitteln
		InputDevice device;
		int inputPlayer;

		// Im Multiplayer: Unterscheidung zwischen Online und Lokal
		if (_gameManager != null && _gameManager.IsMultiplayer)
		{
			if (IsOnlineGame())
			{
				// Online-Multiplayer: Nur der lokale Spieler steuert sein Team
				int localTeamIndex = _gameManager.PlayerTeam == GameManager.Team.Red ? 1 : 2;
				if (player != localTeamIndex)
				{
					return Vector2.Zero; // Gegner-Stangen: keine lokale Eingabe
				}
				device = Player1Device;
				inputPlayer = 1; // Im Online-MP immer Player 1 Eingaben verwenden
			}
			else
			{
				// Lokaler Multiplayer: Beide Spieler steuern lokal
				// Spieler 1 (Red) mit RedTeamController, Spieler 2 (Blue) mit BlueTeamController
				if (player == 1)
				{
					device = _gameManager.RedTeamController;
					inputPlayer = 1;
				}
				else
				{
					device = _gameManager.BlueTeamController;
					inputPlayer = 2;
				}
			}
		}
		else
		{
			// Im Singleplayer: Prüfen ob diese Stange zum menschlichen Team gehört
			if (_gameManager != null)
			{
				bool isHumanRod = (_gameManager.PlayerTeam == GameManager.Team.Red && player == 1)
							   || (_gameManager.PlayerTeam == GameManager.Team.Blue && player == 2);

				if (!isHumanRod)
				{
					// Diese Stange wird vom Bot gesteuert
					return Vector2.Zero;
				}
			}

			device = Player1Device;
			inputPlayer = 1;  // Mensch ist immer "Player 1" im Singleplayer
		}

		// Bei Tastatur-Eingabe: nur Eingabe zurückgeben wenn diese Stange ausgewählt ist
		if (device == InputDevice.Keyboard || device == InputDevice.Keyboard2)
		{
			// Im Online-Multiplayer gibt es nur einen lokalen Spieler, der Player 1 Variablen verwendet
			bool usePlayer1Vars = (_gameManager == null || !_gameManager.IsMultiplayer || IsOnlineGame());
			int selectedRod = usePlayer1Vars ? _player1SelectedRod :
							  (inputPlayer == 2 ? _player2SelectedRod : _player1SelectedRod);
			if (rodIndex != selectedRod)
				return Vector2.Zero;
			return GetInputForDevice(device, inputPlayer);
		}

		// Bei Controller-Eingabe: zwei Stangen können gleichzeitig gesteuert werden
		return GetControllerRodInput(player, rodIndex);
	}

	/// <summary>
	/// Liefert Controller-Eingabe für eine bestimmte Stange (eine Stange pro Stick, wechselbar via Schultertasten)
	/// Für blaues Team sind die Sticks vertauscht: linker Stick steuert Mit+Ang Paar, rechter Stick steuert TW+Vert Paar
	/// </summary>
	private Vector2 GetControllerRodInput(int player, int rodIndex)
	{
		InputDevice device;
		bool isBlueTeam;

		if (_gameManager != null && !_gameManager.IsMultiplayer)
		{
			// Singleplayer: Mensch verwendet Player1Device, steuert PlayerTeam
			bool isHumanRod = (_gameManager.PlayerTeam == GameManager.Team.Red && player == 1)
						   || (_gameManager.PlayerTeam == GameManager.Team.Blue && player == 2);

			if (!isHumanRod)
			{
				// Diese Stange wird vom Bot gesteuert
				return Vector2.Zero;
			}

			device = Player1Device;
			isBlueTeam = (_gameManager.PlayerTeam == GameManager.Team.Blue);
		}
		else if (_gameManager != null && _gameManager.IsMultiplayer && IsOnlineGame())
		{
			// Online Multiplayer: Lokaler Spieler verwendet Player1Device
			device = Player1Device;
			isBlueTeam = (_gameManager.PlayerTeam == GameManager.Team.Blue);
		}
		else
		{
			// Lokaler Multiplayer: Gerät aus GameManager Team-Zuweisungen holen
			device = player == 1 ? _gameManager.RedTeamController : _gameManager.BlueTeamController;
			isBlueTeam = (player == 2);
		}

		int controllerId = GetControllerIdFromDevice(device);
		if (controllerId < 0)
			return Vector2.Zero;

		// Aktuell aktive Stange aus jedem Paar ermitteln
		int humanPlayerIndex;
		if (_gameManager != null && !_gameManager.IsMultiplayer)
		{
			humanPlayerIndex = 1; // Mensch ist immer Player 1 im Singleplayer
		}
		else if (_gameManager != null && _gameManager.IsMultiplayer && IsOnlineGame())
		{
			humanPlayerIndex = 1; // Im Online-Multiplayer gibt es nur einen lokalen Spieler
		}
		else
		{
			humanPlayerIndex = player; // Lokaler Multiplayer: Spieler-Index verwenden
		}

		int leftPairActiveRod = humanPlayerIndex == 1
			? LEFT_PAIR_RODS[_player1LeftPairRods]
			: LEFT_PAIR_RODS[_player2LeftPairRods];
		int rightPairActiveRod = humanPlayerIndex == 1
			? RIGHT_PAIR_RODS[_player1RightPairRods]
			: RIGHT_PAIR_RODS[_player2RightPairRods];

		Vector2 input = Vector2.Zero;

		// Für blaues Team: Stick-Zuordnungen vertauschen
		// - Linker Stick steuert RECHTES Paar (Mit+Ang, Indizes 2,3) - physisch LINKS von Kamera-Sicht
		// - Rechter Stick steuert LINKES Paar (TW+Vert, Indizes 0,1) - physisch RECHTS von Kamera-Sicht
		if (isBlueTeam)
		{
			// Blaues Team: Linker Stick -> aktive Stange aus rechtem Paar (Mittelfeld oder Angriff)
			if (rodIndex == rightPairActiveRod)
			{
				float lateralInput = Input.GetJoyAxis(controllerId, JoyAxis.LeftY);
				float rotationInput = Input.GetJoyAxis(controllerId, JoyAxis.LeftX);

				if (Mathf.Abs(lateralInput) > STICK_DEADZONE)
					input.X = lateralInput;
				if (Mathf.Abs(rotationInput) > ROTATION_DEADZONE)
					input.Y = rotationInput;
			}
			// Blaues Team: Rechter Stick -> aktive Stange aus linkem Paar (Torwart oder Verteidigung)
			else if (rodIndex == leftPairActiveRod)
			{
				float lateralInput = Input.GetJoyAxis(controllerId, JoyAxis.RightY);
				float rotationInput = Input.GetJoyAxis(controllerId, JoyAxis.RightX);

				if (Mathf.Abs(lateralInput) > STICK_DEADZONE)
					input.X = lateralInput;
				if (Mathf.Abs(rotationInput) > ROTATION_DEADZONE)
					input.Y = rotationInput;
			}
		}
		else
		{
			// Rotes Team: Linker Stick -> aktive Stange aus linkem Paar (Torwart oder Verteidigung)
			if (rodIndex == leftPairActiveRod)
			{
				float lateralInput = Input.GetJoyAxis(controllerId, JoyAxis.LeftY);
				float rotationInput = Input.GetJoyAxis(controllerId, JoyAxis.LeftX);

				if (Mathf.Abs(lateralInput) > STICK_DEADZONE)
					input.X = lateralInput;
				if (Mathf.Abs(rotationInput) > ROTATION_DEADZONE)
					input.Y = rotationInput;
			}
			// Rotes Team: Rechter Stick -> aktive Stange aus rechtem Paar (Mittelfeld oder Angriff)
			else if (rodIndex == rightPairActiveRod)
			{
				float lateralInput = Input.GetJoyAxis(controllerId, JoyAxis.RightY);
				float rotationInput = Input.GetJoyAxis(controllerId, JoyAxis.RightX);

				if (Mathf.Abs(lateralInput) > STICK_DEADZONE)
					input.X = lateralInput;
				if (Mathf.Abs(rotationInput) > ROTATION_DEADZONE)
					input.Y = rotationInput;
			}
		}

		return input;
	}

	/// <summary>
	/// Aktualisiert welche Stange aktuell aktiv ist (Eingaben erhält) für visuelles Feedback
	/// </summary>
	private void UpdateActiveRod(int player, int rodIndex)
	{
		if (player == 1)
		{
			if (_player1ActiveRod != rodIndex)
			{
				_player1ActiveRod = rodIndex;
				EmitRodPairChangedSignal(player);
			}
		}
		else
		{
			if (_player2ActiveRod != rodIndex)
			{
				_player2ActiveRod = rodIndex;
				EmitRodPairChangedSignal(player);
			}
		}
	}

	/// <summary>
	/// Liefert die aktuell ausgewählten Stangen im Controller-Modus
	/// </summary>
	/// <param name="player">Spieler-Index</param>
	/// <returns>Tuple mit linker und rechter Paar-Stange</returns>
	public (int leftPairRod, int rightPairRod) GetControllerSelectedRods(int player)
	{
		if (player == 1)
		{
			int leftRod = LEFT_PAIR_RODS[_player1LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player1RightPairRods];
			return (leftRod, rightRod);
		}
		else
		{
			int leftRod = LEFT_PAIR_RODS[_player2LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player2RightPairRods];
			return (leftRod, rightRod);
		}
	}

	/// <summary>
	/// Liefert alle Stangen der aktuellen Auswahl (beide Paare, 4 Stangen gesamt für Hervorhebung)
	/// </summary>
	/// <param name="player">Spieler-Index</param>
	/// <returns>Tuple mit Array der aktiven Stangen und der aktuell aktiven Stange</returns>
	public (int[] activePair, int activeRod) GetControllerRodPairInfo(int player)
	{
		if (player == 1)
		{
			int leftRod = LEFT_PAIR_RODS[_player1LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player1RightPairRods];
			return (new int[] { leftRod, rightRod }, _player1ActiveRod);
		}
		else
		{
			int leftRod = LEFT_PAIR_RODS[_player2LeftPairRods];
			int rightRod = RIGHT_PAIR_RODS[_player2RightPairRods];
			return (new int[] { leftRod, rightRod }, _player2ActiveRod);
		}
	}

	/// <summary>
	/// Liefert die aktuell aktive Stange eines Spielers (die Eingaben erhält)
	/// </summary>
	public int GetActiveRod(int player)
	{
		return player == 1 ? _player1ActiveRod : _player2ActiveRod;
	}

	/// <summary>
	/// Prüft ob ein Spieler einen Controller verwendet
	/// </summary>
	public bool IsUsingController(int player)
	{
		InputDevice device;

		// Im Multiplayer: teamspezifische Controller-Zuweisungen verwenden
		if (_gameManager != null && _gameManager.IsMultiplayer)
		{
			if (player == 1)
			{
				device = _gameManager.RedTeamController;
			}
			else
			{
				device = _gameManager.BlueTeamController;
			}
		}
		else
		{
			// Im Singleplayer: Player1Device für Spieler 1 verwenden
			device = player == 1 ? Player1Device : Player2Device;
		}

		return device != InputDevice.Keyboard && device != InputDevice.Keyboard2;
	}

	/// <summary>
	/// Liefert Roh-Eingaben von einem bestimmten Eingabegerät
	/// </summary>
	private Vector2 GetInputForDevice(InputDevice device, int player)
	{
		Vector2 input = Vector2.Zero;

		switch (device)
		{
			case InputDevice.Keyboard:
				// Keyboard1 verwendet IMMER P1 Tasten (WASD + QE), unabhängig vom Team
				if (_bindings.IsActionPressed(InputBindings.GameAction.P1_MoveUp, 1, false))
					input.X -= 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P1_MoveDown, 1, false))
					input.X += 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P1_RotateLeft, 1, false))
					input.Y -= 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P1_RotateRight, 1, false))
					input.Y += 1.0f;
				break;

			case InputDevice.Keyboard2:
				// Keyboard2 verwendet IMMER P2 Tasten (Pfeiltasten + Numpad), unabhängig vom Team
				if (_bindings.IsActionPressed(InputBindings.GameAction.P2_MoveUp, 2, false))
					input.X -= 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P2_MoveDown, 2, false))
					input.X += 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P2_RotateLeft, 2, false))
					input.Y -= 1.0f;
				if (_bindings.IsActionPressed(InputBindings.GameAction.P2_RotateRight, 2, false))
					input.Y += 1.0f;
				break;

			case InputDevice.Controller0:
			case InputDevice.Controller1:
			case InputDevice.Controller2:
			case InputDevice.Controller3:
				int controllerId = GetControllerIdFromDevice(device);
				if (controllerId >= 0)
				{
					input = GetControllerInput(controllerId);
				}
				break;
		}

		return input;
	}

	/// <summary>
	/// Liefert Eingaben von einem Controller (Sticks und Trigger)
	/// </summary>
	private Vector2 GetControllerInput(int controllerId)
	{
		Vector2 input = Vector2.Zero;

		// Linker Stick X für Lateralbewegung
		float lateralInput = Input.GetJoyAxis(controllerId, JoyAxis.LeftX);
		if (Mathf.Abs(lateralInput) > STICK_DEADZONE)
		{
			input.X = lateralInput;
		}

		// Rechter Stick X für Rotation
		float rotationInput = Input.GetJoyAxis(controllerId, JoyAxis.RightX);
		if (Mathf.Abs(rotationInput) > STICK_DEADZONE)
		{
			input.Y = rotationInput;
		}
		else
		{
			// Alternative: Trigger für Rotation verwenden (L2/R2)
			float leftTrigger = Input.GetJoyAxis(controllerId, JoyAxis.TriggerLeft);
			float rightTrigger = Input.GetJoyAxis(controllerId, JoyAxis.TriggerRight);

			if (leftTrigger > TRIGGER_DEADZONE)
			{
				input.Y -= leftTrigger;
			}
			if (rightTrigger > TRIGGER_DEADZONE)
			{
				input.Y += rightTrigger;
			}
		}

		return input;
	}

	/// <summary>
	/// Liefert die aktuell ausgewählte Stange eines Spielers (nur Tastatur-Modus)
	/// </summary>
	public int GetSelectedRod(int player)
	{
		return player == 1 ? _player1SelectedRod : _player2SelectedRod;
	}

	/// <summary>
	/// Setzt die ausgewählte Stange eines Spielers (nur Tastatur-Modus)
	/// </summary>
	public void SetSelectedRod(int player, int rodIndex)
	{
		rodIndex = Mathf.Clamp(rodIndex, 0, MAX_RODS_PER_TEAM - 1);
		if (player == 1)
		{
			_player1SelectedRod = rodIndex;
		}
		else if (player == 2)
		{
			_player2SelectedRod = rodIndex;
		}
	}

	/// <summary>
	/// Erkennt alle verbundenen Controller und registriert sie
	/// </summary>
	public void DetectControllers()
	{
		_connectedControllers.Clear();
		var connectedJoypads = Input.GetConnectedJoypads();

		foreach (int joypadId in connectedJoypads)
		{
			_connectedControllers.Add(joypadId);
			GD.Print($"Controller detected: {Input.GetJoyName(joypadId)} (ID: {joypadId})");
		}

		GD.Print($"Total controllers connected: {_connectedControllers.Count}");
	}

	/// <summary>
	/// Liefert die Anzahl der verbundenen Controller
	/// </summary>
	public int GetConnectedControllerCount()
	{
		return _connectedControllers.Count;
	}

	/// <summary>
	/// Liefert die Liste aller verbundenen Controller-IDs
	/// </summary>
	public List<int> GetConnectedControllers()
	{
		return new List<int>(_connectedControllers);
	}

	/// <summary>
	/// Konvertiert InputDevice-Enum zu Controller-ID
	/// </summary>
	private int GetControllerIdFromDevice(InputDevice device)
	{
		switch (device)
		{
			case InputDevice.Controller0: return 0;
			case InputDevice.Controller1: return 1;
			case InputDevice.Controller2: return 2;
			case InputDevice.Controller3: return 3;
			default: return -1;
		}
	}

	/// <summary>
	/// Konvertiert Controller-ID zu InputDevice-Enum
	/// </summary>
	public InputDevice GetDeviceFromControllerId(int controllerId)
	{
		switch (controllerId)
		{
			case 0: return InputDevice.Controller0;
			case 1: return InputDevice.Controller1;
			case 2: return InputDevice.Controller2;
			case 3: return InputDevice.Controller3;
			default: return InputDevice.Keyboard;
		}
	}

	/// <summary>
	/// Prüft ob die Pause-Taste gedrückt wurde (Tastatur oder Controller)
	/// </summary>
	public bool IsPausePressed()
	{
		// Tastatur: Belegung prüfen
		if (_bindings.IsActionPressed(InputBindings.GameAction.Pause, 1, false))
			return true;

		// Controller: Belegung prüfen
		if (_bindings.IsActionPressed(InputBindings.GameAction.Pause, 1, true))
			return true;

		return false;
	}

	/// <summary>
	/// Callback-Funktion für Controller-Verbindungsänderungen
	/// </summary>
	private void OnJoyConnectionChanged(long device, bool connected)
	{
		int deviceId = (int)device;

		if (connected)
		{
			if (!_connectedControllers.Contains(deviceId))
			{
				_connectedControllers.Add(deviceId);
				GD.Print($"Controller connected: {Input.GetJoyName(deviceId)} (ID: {deviceId})");
				EmitSignal(SignalName.ControllerConnected, deviceId);
				EmitSignal(SignalName.JoyConnectionChanged, deviceId, connected);
			}
		}
		else
		{
			if (_connectedControllers.Contains(deviceId))
			{
				_connectedControllers.Remove(deviceId);
				GD.Print($"Controller disconnected: ID {deviceId}");
				EmitSignal(SignalName.ControllerDisconnected, deviceId);
				EmitSignal(SignalName.JoyConnectionChanged, deviceId, connected);
			}
		}
	}

	/// <summary>
	/// Liefert einen lesbaren Namen für ein Eingabegerät
	/// </summary>
	public string GetDeviceName(InputDevice device)
	{
		switch (device)
		{
			case InputDevice.Keyboard:
				return "Keyboard (WASD + QE)";
			case InputDevice.Keyboard2:
				return "Keyboard 2 (Arrows + Numpad)";
			case InputDevice.Controller0:
			case InputDevice.Controller1:
			case InputDevice.Controller2:
			case InputDevice.Controller3:
				int controllerId = GetControllerIdFromDevice(device);
				if (_connectedControllers.Contains(controllerId))
				{
					return Input.GetJoyName(controllerId);
				}
				return $"Controller {controllerId} (Not Connected)";
			default:
				return "Unknown";
		}
	}

	/// <summary>
	/// Setzt das Eingabegerät für Spieler 1
	/// </summary>
	public void SetPlayer1Device(InputDevice device)
	{
		Player1Device = device;
		GD.Print($"Player 1 device set to: {GetDeviceName(device)}");
	}

	/// <summary>
	/// Setzt das Eingabegerät für Spieler 2
	/// </summary>
	public void SetPlayer2Device(InputDevice device)
	{
		Player2Device = device;
		GD.Print($"Player 2 device set to: {GetDeviceName(device)}");
	}

	/// <summary>
	/// Startet den Rebinding-Prozess für eine bestimmte Aktion
	/// </summary>
	public void StartRebinding(int player, InputBindings.GameAction action, bool isController = false)
	{
		_isRebinding = true;
		_rebindingPlayer = player;
		_rebindingAction = action;
		_rebindingIsController = isController;

		string actionName = GetActionDisplayName(action);
		GD.Print($"Waiting for input to rebind '{actionName}' for Player {player}...");
		EmitSignal(SignalName.RebindingStarted, player, actionName);
	}

	/// <summary>
	/// Bricht den aktuellen Rebinding-Prozess ab
	/// </summary>
	public void CancelRebinding()
	{
		if (_isRebinding)
		{
			_isRebinding = false;
			GD.Print("Rebinding cancelled");
			EmitSignal(SignalName.RebindingCancelled, _rebindingPlayer);
		}
	}

	/// <summary>
	/// Verarbeitet Eingabe-Events während des Rebinding-Prozesses
	/// </summary>
	private void HandleRebindingInput(InputEvent @event)
	{
		// ESC erlaubt Abbruch des Rebindings
		if (@event is InputEventKey escKey && escKey.Keycode == Key.Escape && escKey.Pressed)
		{
			CancelRebinding();
			return;
		}

		// Tastatur-Eingabe erfassen
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && !_rebindingIsController)
		{
			// Prüfen ob diese Taste bereits von anderer Aktion verwendet wird
			if (IsKeyAlreadyBound(keyEvent.Keycode, _rebindingPlayer, _rebindingAction))
			{
				GD.Print($"Key {OS.GetKeycodeString(keyEvent.Keycode)} is already bound to another action");
			}

			// Neue Belegung setzen
			_bindings.SetBinding(_rebindingAction, keyEvent, _rebindingPlayer, false);
			_bindings.SaveToFile();

			string inputName = InputBindings.GetInputEventDisplayName(keyEvent);
			string actionName = GetActionDisplayName(_rebindingAction);
			GD.Print($"Rebound '{actionName}' to {inputName} for Player {_rebindingPlayer}");

			EmitSignal(SignalName.RebindingCompleted, _rebindingPlayer, actionName, inputName);
			_isRebinding = false;
		}
		// Controller-Button-Eingabe erfassen
		else if (@event is InputEventJoypadButton joyEvent && joyEvent.Pressed && _rebindingIsController)
		{
			// Neue Belegung setzen
			_bindings.SetBinding(_rebindingAction, joyEvent, _rebindingPlayer, true);
			_bindings.SaveToFile();

			string inputName = InputBindings.GetInputEventDisplayName(joyEvent);
			string actionName = GetActionDisplayName(_rebindingAction);
			GD.Print($"Rebound '{actionName}' to {inputName}");

			EmitSignal(SignalName.RebindingCompleted, _rebindingPlayer, actionName, inputName);
			_isRebinding = false;
		}
	}

	/// <summary>
	/// Prüft ob eine Taste bereits an eine andere Aktion für denselben Spieler gebunden ist
	/// </summary>
	private bool IsKeyAlreadyBound(Key keycode, int player, InputBindings.GameAction exceptAction)
	{
		var allBindings = _bindings.GetAllBindings(player, false);

		foreach (var kvp in allBindings)
		{
			if (kvp.Key == exceptAction)
				continue;

			if (kvp.Value is InputEventKey keyEvent && keyEvent.Keycode == keycode)
				return true;
		}

		return false;
	}

	/// <summary>
	/// Setzt alle Tastenbelegungen auf Standard-Werte zurück
	/// </summary>
	public void ResetBindingsToDefault()
	{
		_bindings.SetDefaultBindings();
		_bindings.SaveToFile();
		GD.Print("Input bindings reset to defaults");
	}

	/// <summary>
	/// Liefert einen lesbaren Namen für eine Spiel-Aktion
	/// </summary>
	private string GetActionDisplayName(InputBindings.GameAction action)
	{
		return action switch
		{
			InputBindings.GameAction.P1_MoveUp => "Move Up",
			InputBindings.GameAction.P1_MoveDown => "Move Down",
			InputBindings.GameAction.P1_RotateLeft => "Rotate Left",
			InputBindings.GameAction.P1_RotateRight => "Rotate Right",
			InputBindings.GameAction.P1_SwitchRodLeft => "Switch Rod Left",
			InputBindings.GameAction.P1_SwitchRodRight => "Switch Rod Right",
			InputBindings.GameAction.P2_MoveUp => "Move Up",
			InputBindings.GameAction.P2_MoveDown => "Move Down",
			InputBindings.GameAction.P2_RotateLeft => "Rotate Left",
			InputBindings.GameAction.P2_RotateRight => "Rotate Right",
			InputBindings.GameAction.P2_SwitchRodLeft => "Switch Rod Left",
			InputBindings.GameAction.P2_SwitchRodRight => "Switch Rod Right",
			InputBindings.GameAction.Controller_ToggleLeftPair => "Toggle Left Pair",
			InputBindings.GameAction.Controller_ToggleRightPair => "Toggle Right Pair",
			InputBindings.GameAction.Pause => "Pause",
			_ => action.ToString()
		};
	}

	/// <summary>
	/// Liefert die InputBindings-Instanz (für UI-Anzeige)
	/// </summary>
	public InputBindings GetBindings()
	{
		return _bindings;
	}

	/// <summary>
	/// Liefert die aktuelle Tastenbelegung für eine Aktion als Anzeige-String
	/// </summary>
	public string GetBindingDisplayName(InputBindings.GameAction action, int player = 1, bool isController = false)
	{
		var binding = _bindings.GetBinding(action, player, isController);
		if (binding == null)
			return "None";
		return InputBindings.GetInputEventDisplayName(binding);
	}

	/// <summary>
	/// Prüft ob Rebinding aktuell aktiv ist
	/// </summary>
	public bool IsRebinding()
	{
		return _isRebinding;
	}

	/// <summary>
	/// Aktualisiert den Team-Controller im GameManager wenn Player1Device im Online-Multiplayer wechselt
	/// Stellt sicher dass GetControllerRodInput() das korrekte Gerät verwendet
	/// </summary>
	private void UpdateGameManagerTeamController(InputDevice newDevice)
	{
		if (_gameManager == null || !_gameManager.IsMultiplayer)
			return;

		// Prüfen ob es sich um Online-Multiplayer handelt (nicht lokaler Multiplayer)
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		if (onlineManager == null)
			return;

		var isConnected = onlineManager.Call("is_connected_to_peer");
		if (!isConnected.AsBool())
			return;

		// Team-Controller für unser Team aktualisieren
		if (_gameManager.PlayerTeam == GameManager.Team.Red)
		{
			_gameManager.RedTeamController = newDevice;
			GD.Print($"[InputManager] Updated RedTeamController to {newDevice}");
		}
		else
		{
			_gameManager.BlueTeamController = newDevice;
			GD.Print($"[InputManager] Updated BlueTeamController to {newDevice}");
		}
	}
}
