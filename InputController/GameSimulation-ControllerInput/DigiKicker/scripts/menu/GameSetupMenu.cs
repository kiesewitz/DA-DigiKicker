using Godot;

/// <summary>
/// Spiel-Konfigurationsmenu für Einzel- und Mehrspieler-Modi.
/// Verwaltet Spieleinstellungen wie Spielzeit, Siegbedingungen, Team-Zuweisungen und Controller-Konfiguration.
/// </summary>
public partial class GameSetupMenu : Control
{
	// UI-Referenzen
	private Label _labelTitle;
	private Label _labelGameMode;
	private SpinBox _spinBoxGameDuration;
	private SpinBox _spinBoxWinScore;
	private OptionButton _optionWinCondition;
	private HBoxContainer _hboxPlayerTeam;
	private OptionButton _optionPlayerTeam;
	private HBoxContainer _hboxBotDifficulty;
	private OptionButton _optionBotDifficulty;
	private VBoxContainer _multiplayerSection;
	private OptionButton _optionRedTeam;
	private OptionButton _optionBlueTeam;
	private Button _btnBack;
	private Button _btnStartGame;

	// Properties
	public bool IsMultiplayer { get; set; } = false;

	// Manager-Referenzen
	private AudioManager _audioManager;

	// Basis-Pfad für Nodes
	private const string BASE_PATH = "CenterContainer/PanelContainer/MarginContainer/VBoxContainer";

	// Controller-Tracking
	private Godot.Collections.Array<int> _connectedJoypads = new();

	// Zähler für doppelte Controller-Namen (zur eindeutigen Benennung)
	private System.Collections.Generic.Dictionary<string, int> _controllerNameCounts = new();

	/// <summary>
	/// Initialisiert das Menu beim Laden.
	/// Holt alle Node-Referenzen, verbindet Signals und erkennt angeschlossene Controller.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// UI-Node-Referenzen holen (aktualisierte Pfade mit CenterContainer)
		_labelTitle = GetNode<Label>($"{BASE_PATH}/LabelTitle");
		_labelGameMode = GetNode<Label>($"{BASE_PATH}/LabelGameMode");

		_spinBoxGameDuration = GetNode<SpinBox>($"{BASE_PATH}/GameplaySection/HBoxDuration/SpinBoxGameDuration");
		_spinBoxWinScore = GetNode<SpinBox>($"{BASE_PATH}/GameplaySection/HBoxWinScore/SpinBoxWinScore");
		_optionWinCondition = GetNode<OptionButton>($"{BASE_PATH}/GameplaySection/HBoxWinCondition/OptionWinCondition");

		_hboxPlayerTeam = GetNode<HBoxContainer>($"{BASE_PATH}/GameplaySection/HBoxPlayerTeam");
		_optionPlayerTeam = GetNode<OptionButton>($"{BASE_PATH}/GameplaySection/HBoxPlayerTeam/OptionPlayerTeam");

		_hboxBotDifficulty = GetNode<HBoxContainer>($"{BASE_PATH}/GameplaySection/HBoxBotDifficulty");
		_optionBotDifficulty = GetNode<OptionButton>($"{BASE_PATH}/GameplaySection/HBoxBotDifficulty/OptionBotDifficulty");

		_multiplayerSection = GetNode<VBoxContainer>($"{BASE_PATH}/GameplaySection/MultiplayerSection");
		_optionRedTeam = GetNode<OptionButton>($"{BASE_PATH}/GameplaySection/MultiplayerSection/HBoxRedTeam/OptionRedTeam");
		_optionBlueTeam = GetNode<OptionButton>($"{BASE_PATH}/GameplaySection/MultiplayerSection/HBoxBlueTeam/OptionBlueTeam");

		_btnBack = GetNode<Button>($"{BASE_PATH}/ButtonContainer/BtnBack");
		_btnStartGame = GetNode<Button>($"{BASE_PATH}/ButtonContainer/BtnStartGame");

		// Prüfen, ob wir als eigenständige Scene laufen (vom ModeSelectionMenu)
		// Wenn Parent nicht SubMenuContainer ist, sind wir eigenständig und sollten im Multiplayer-Modus sein
		if (GetParent().Name != "SubMenuContainer")
		{
			IsMultiplayer = true;
		}

		// Button-Signals verbinden
		_btnBack.Pressed += OnBackPressed;
		_btnStartGame.Pressed += OnStartGamePressed;

		// SpinBox- und OptionButton-Signals für Sounds verbinden
		_spinBoxGameDuration.ValueChanged += OnSpinBoxChanged;
		_spinBoxWinScore.ValueChanged += OnSpinBoxChanged;
		_optionWinCondition.ItemSelected += OnOptionSelected;
		_optionPlayerTeam.ItemSelected += OnOptionSelected;
		_optionBotDifficulty.ItemSelected += OnOptionSelected;
		_optionRedTeam.ItemSelected += OnRedTeamSelected;
		_optionBlueTeam.ItemSelected += OnBlueTeamSelected;

		// Joypad-Verbindungs-/Trennungs-Signals verbinden
		Input.JoyConnectionChanged += OnJoyConnectionChanged;

		// Initiale Controller-Erkennung
		RefreshConnectedControllers();

		// Spielmodus-Anzeige aktualisieren
		UpdateGameModeLabel();
		UpdatePlayerTeamVisibility();
	}

	/// <summary>
	/// Wird beim Entfernen aus dem Scene-Tree aufgerufen.
	/// Trennt Joypad-Signals, um Memory-Leaks zu vermeiden.
	/// </summary>
	public override void _ExitTree()
	{
		// Von Joypad-Signals trennen, um Memory-Leaks zu vermeiden
		Input.JoyConnectionChanged -= OnJoyConnectionChanged;
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Joypad verbunden oder getrennt wird.
	/// Aktualisiert die Liste der verfügbaren Controller.
	/// </summary>
	private void OnJoyConnectionChanged(long device, bool connected)
	{
		GD.Print($"Joypad {device} {(connected ? "verbunden" : "getrennt")}");
		RefreshConnectedControllers();
	}

	/// <summary>
	/// Aktualisiert die Liste der angeschlossenen Controller und die Multiplayer-Team-Optionen.
	/// </summary>
	private void RefreshConnectedControllers()
	{
		_connectedJoypads = Input.GetConnectedJoypads();
		GD.Print($"Angeschlossene Joypads: {_connectedJoypads.Count}");

		// Multiplayer-Team-Optionsbuttons mit verfügbaren Controllern aktualisieren
		UpdateMultiplayerTeamOptions();
	}

	/// <summary>
	/// Wandelt rohe Controller-Namen in benutzerfreundliche Namen um.
	/// Erkennt gängige Controller-Typen wie Xbox, PlayStation, Nintendo Switch.
	/// </summary>
	/// <param name="rawName">Roher Controller-Name vom System</param>
	/// <returns>Benutzerfreundlicher Controller-Name</returns>
	private string GetFriendlyControllerName(string rawName)
	{
		string nameLower = rawName.ToLower();

		// Xbox-Controller
		if (nameLower.Contains("xbox") || nameLower.Contains("xinput"))
		{
			if (nameLower.Contains("360"))
				return "Xbox 360 Controller";
			else if (nameLower.Contains("one"))
				return "Xbox One Controller";
			else if (nameLower.Contains("series"))
				return "Xbox Series Controller";
			else
				return "Xbox Controller";
		}

		// PlayStation-Controller
		if (nameLower.Contains("playstation") || nameLower.Contains("ps") ||
			nameLower.Contains("dualshock") || nameLower.Contains("dualsense"))
		{
			if (nameLower.Contains("5") || nameLower.Contains("dualsense"))
				return "PlayStation 5 Controller";
			else if (nameLower.Contains("4") || nameLower.Contains("dualshock 4"))
				return "PlayStation 4 Controller";
			else if (nameLower.Contains("3") || nameLower.Contains("dualshock 3"))
				return "PlayStation 3 Controller";
			else
				return "PlayStation Controller";
		}

		// Nintendo-Controller
		if (nameLower.Contains("nintendo") || nameLower.Contains("switch"))
		{
			if (nameLower.Contains("pro"))
				return "Nintendo Switch Pro Controller";
			else if (nameLower.Contains("joycon") || nameLower.Contains("joy-con"))
				return "Nintendo Joy-Con";
			else
				return "Nintendo Controller";
		}

		// Steam Controller
		if (nameLower.Contains("steam"))
			return "Steam Controller";

		// Generischer Fallback
		return "Generic Controller";
	}

	/// <summary>
	/// Aktualisiert die Multiplayer-Team-Optionsbuttons mit verfügbaren Eingabegeräten.
	/// Im Multiplayer-Modus werden nur Keyboard 1, Keyboard 2 und angeschlossene Controller angezeigt (keine Bots).
	/// Bei mehreren identischen Controllern werden Nummern zur Unterscheidung hinzugefügt.
	/// </summary>
	private void UpdateMultiplayerTeamOptions()
	{
		if (_optionRedTeam == null || _optionBlueTeam == null)
			return;

		// Aktuelle Auswahl speichern
		int redSelection = _optionRedTeam.Selected;
		int blueSelection = _optionBlueTeam.Selected;

		// Alle Items löschen
		_optionRedTeam.Clear();
		_optionBlueTeam.Clear();

		// Keyboard-Optionen immer hinzufügen
		_optionRedTeam.AddItem("Keyboard 1", 0);
		_optionBlueTeam.AddItem("Keyboard 1", 0);

		_optionRedTeam.AddItem("Keyboard 2", 1);
		_optionBlueTeam.AddItem("Keyboard 2", 1);

		// Namen-Zähler für Duplikatserkennung zurücksetzen
		_controllerNameCounts.Clear();

		// Erster Durchlauf: Zählen, wie oft jeder benutzerfreundliche Name vorkommt
		var friendlyNames = new System.Collections.Generic.List<string>();
		foreach (int joypadId in _connectedJoypads)
		{
			string rawName = Input.GetJoyName(joypadId);
			string friendlyName = GetFriendlyControllerName(rawName);
			friendlyNames.Add(friendlyName);

			if (_controllerNameCounts.ContainsKey(friendlyName))
				_controllerNameCounts[friendlyName]++;
			else
				_controllerNameCounts[friendlyName] = 1;
		}

		// Zweiter Durchlauf: Items mit eindeutigen Namen hinzufügen
		var nameCounters = new System.Collections.Generic.Dictionary<string, int>();
		int itemId = 2; // Nach Keyboard-Optionen beginnen
		for (int i = 0; i < _connectedJoypads.Count; i++)
		{
			string friendlyName = friendlyNames[i];
			string displayName = friendlyName;

			// Wenn mehrere Controller den gleichen benutzerfreundlichen Namen haben, Suffix hinzufügen
			if (_controllerNameCounts[friendlyName] > 1)
			{
				if (!nameCounters.ContainsKey(friendlyName))
					nameCounters[friendlyName] = 1;
				else
					nameCounters[friendlyName]++;

				displayName = $"{friendlyName} ({nameCounters[friendlyName]})";
			}

			_optionRedTeam.AddItem(displayName, itemId);
			_optionBlueTeam.AddItem(displayName, itemId);
			itemId++;
		}

		// Auswahl wiederherstellen, falls möglich, sonst sinnvolle Standardwerte
		if (redSelection < _optionRedTeam.ItemCount)
		{
			_optionRedTeam.Selected = redSelection;
		}
		else
		{
			_optionRedTeam.Selected = 0; // Standard: Keyboard 1
		}

		if (blueSelection < _optionBlueTeam.ItemCount)
		{
			_optionBlueTeam.Selected = blueSelection;
		}
		else
		{
			// Standard: Controller 1 falls verfügbar, sonst Keyboard 2
			_optionBlueTeam.Selected = _connectedJoypads.Count > 0 ? 2 : 1;
		}
	}

	/// <summary>
	/// Wird bei SpinBox-Wertänderung aufgerufen.
	/// Spielt bewusst keinen Sound, da dies zu häufig ausgelöst wird.
	/// </summary>
	private void OnSpinBoxChanged(double value)
	{
		// Kein Sound für SpinBox - wird zu oft ausgelöst
	}

	/// <summary>
	/// Wird bei OptionButton-Auswahl aufgerufen.
	/// Spielt einen höheren Click-Sound.
	/// </summary>
	private void OnOptionSelected(long index)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Höherer Pitch für Auswahl
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Controller für das rote Team im Dropdown ausgewählt wird.
	/// Lässt den ausgewählten Controller vibrieren zur Bestätigung.
	/// </summary>
	private void OnRedTeamSelected(long index)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Höherer Pitch für Auswahl
		VibrateSelectedController(_optionRedTeam, (int)index);
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Controller für das blaue Team im Dropdown ausgewählt wird.
	/// Lässt den ausgewählten Controller vibrieren zur Bestätigung.
	/// </summary>
	private void OnBlueTeamSelected(long index)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Höherer Pitch für Auswahl
		VibrateSelectedController(_optionBlueTeam, (int)index);
	}

	/// <summary>
	/// Lässt den ausgewählten Controller vibrieren (falls ein Controller und kein Keyboard ausgewählt wurde).
	/// Gibt dem Spieler haptisches Feedback, welcher Controller ausgewählt wurde.
	/// </summary>
	/// <param name="optionButton">Der OptionButton mit der Auswahl</param>
	/// <param name="selectedIndex">Der ausgewählte Index</param>
	private void VibrateSelectedController(OptionButton optionButton, int selectedIndex)
	{
		if (optionButton == null || selectedIndex >= optionButton.ItemCount)
			return;

		// Ausgewählte Item-ID holen (nicht den Index)
		int itemId = optionButton.GetItemId(selectedIndex);

		// Item-IDs 0 und 1 sind Keyboards, Controller beginnen bei 2
		if (itemId < 2)
			return;

		// ItemId auf Controller-Index mappen: itemId 2 -> Controller0, itemId 3 -> Controller1, etc.
		int controllerIndex = itemId - 2;

		if (controllerIndex >= 0 && controllerIndex < _connectedJoypads.Count)
		{
			int joypadId = _connectedJoypads[controllerIndex];

			// Controller vibrieren lassen (moderate Vibration für 0.3 Sekunden)
			Input.StartJoyVibration(joypadId, 0.4f, 0.6f, 0.3f);

			GD.Print($"Controller {joypadId} vibriert für Team-Auswahl");
		}
	}

	/// <summary>
	/// Aktualisiert das Spielmodus-Label (Singleplayer/Multiplayer).
	/// </summary>
	private void UpdateGameModeLabel()
	{
		_labelGameMode.Text = IsMultiplayer ? "Multiplayer" : "Singleplayer";
	}

	/// <summary>
	/// Zeigt/versteckt UI-Elemente basierend auf dem aktuellen Spielmodus.
	/// Im Singleplayer: Team-Auswahl und Bot-Schwierigkeit sichtbar.
	/// Im Multiplayer: Multiplayer-Sektion mit Controller-Zuweisungen sichtbar.
	/// </summary>
	private void UpdatePlayerTeamVisibility()
	{
		// Team-Auswahl und Bot-Schwierigkeit nur im Singleplayer-Modus anzeigen
		if (_hboxPlayerTeam != null)
		{
			_hboxPlayerTeam.Visible = !IsMultiplayer;
		}

		if (_hboxBotDifficulty != null)
		{
			_hboxBotDifficulty.Visible = !IsMultiplayer;
		}

		// Multiplayer-Sektion nur im Multiplayer-Modus anzeigen
		if (_multiplayerSection != null)
		{
			_multiplayerSection.Visible = IsMultiplayer;
		}
	}

	/// <summary>
	/// Wird jeden Frame aufgerufen.
	/// Aktualisiert Spielmodus-Label und UI-Sichtbarkeit, wenn das Menu sichtbar ist.
	/// </summary>
	public override void _Process(double delta)
	{
		// Spielmodus-Label aktualisieren, wenn sichtbar
		if (Visible)
		{
			UpdateGameModeLabel();
			UpdatePlayerTeamVisibility();
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Zurück-Button gedrückt wird.
	/// Versteckt das Menu (wenn SubMenu) oder navigiert zum ModeSelectionMenu (wenn eigenständig).
	/// </summary>
	private void OnBackPressed()
	{
		_audioManager?.PlaySFX("button_click");

		// Prüfen, ob wir als eigenständige Scene oder als SubMenu laufen
		// Wenn Parent SubMenuContainer vom MainMenu ist, verstecken; sonst zurück navigieren
		if (GetParent().Name == "SubMenuContainer")
		{
			// Dieses Menu verstecken (läuft als SubMenu im MainMenu)
			Hide();
		}
		else
		{
			// Zurück zum ModeSelectionMenu navigieren (läuft als eigenständige Scene)
			GetTree().ChangeSceneToFile("res://scenes/menu/ModeSelectionMenu.tscn");
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Start-Game-Button gedrückt wird.
	/// Konfiguriert den GameManager mit allen Einstellungen und lädt die Spiel-Scene.
	/// </summary>
	private void OnStartGamePressed()
	{
		_audioManager?.PlaySFX("button_click");
		_audioManager?.PlayWhistle(); // Pfiff beim Spielstart

		// GameManager Singleton holen
		var gameManager = GetNode<GameManager>("/root/GameManager");

		// Spiel-Konfiguration setzen
		gameManager.IsMultiplayer = IsMultiplayer;
		gameManager.GameDuration = (int)_spinBoxGameDuration.Value;
		gameManager.WinScore = (int)_spinBoxWinScore.Value;

		// Siegbedingung basierend auf Auswahl setzen
		gameManager.CurrentWinCondition = _optionWinCondition.Selected switch
		{
			0 => GameManager.WinCondition.TimeLimit,
			1 => GameManager.WinCondition.FirstToScore,
			2 => GameManager.WinCondition.Both,
			_ => GameManager.WinCondition.Both
		};

		// Spieler-Team und Bot-Schwierigkeit für Singleplayer-Modus setzen
		if (!IsMultiplayer)
		{
			gameManager.PlayerTeam = _optionPlayerTeam.Selected switch
			{
				0 => GameManager.Team.Red,
				1 => GameManager.Team.Blue,
				_ => GameManager.Team.Red
			};

			// Prüfen, ob Trained AI ausgewählt ist (Index 3)
			if (_optionBotDifficulty.Selected == 3)
			{
				gameManager.CurrentBotType = GameManager.BotType.TrainedAI;
				gameManager.BotDifficulty = BotController.BotDifficulty.Hard; // Fallback falls benötigt
				GD.Print($"Starte Singleplayer-Spiel: Spieler steuert {gameManager.PlayerTeam}, Bot: TRAINED AI (ONNX)");
			}
			else
			{
				gameManager.CurrentBotType = GameManager.BotType.RuleBot;
				gameManager.BotDifficulty = _optionBotDifficulty.Selected switch
				{
					0 => BotController.BotDifficulty.Easy,
					1 => BotController.BotDifficulty.Medium,
					2 => BotController.BotDifficulty.Hard,
					_ => BotController.BotDifficulty.Medium
				};
				GD.Print($"Starte Singleplayer-Spiel: Spieler steuert {gameManager.PlayerTeam}, Bot steuert {(gameManager.PlayerTeam == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red)}, Bot-Schwierigkeit: {gameManager.BotDifficulty}");
			}
		}
		else
		{
			// Multiplayer-Modus - Team-Steuerungen konfigurieren
			ConfigureMultiplayerTeamControls(gameManager);
		}

		GD.Print($"Starte Spiel: Dauer={gameManager.GameDuration}min, Siegpunktzahl={gameManager.WinScore}, Bedingung={gameManager.CurrentWinCondition}");

		// Spiel-Scene laden
		GetTree().ChangeSceneToFile("res://scenes/game/Game.tscn");
	}

	/// <summary>
	/// Konfiguriert die Multiplayer-Team-Steuerungen basierend auf UI-Auswahl.
	/// Im Multiplayer-Modus sind keine Bots verfügbar - nur Keyboard und angeschlossene Controller.
	/// Mappt Item-IDs zu Eingabegeräten: 0=Keyboard1, 1=Keyboard2, 2+=Controller.
	/// </summary>
	/// <param name="gameManager">Der GameManager, der konfiguriert werden soll</param>
	private void ConfigureMultiplayerTeamControls(GameManager gameManager)
	{
		// Ausgewählte Item-IDs holen (nicht Indizes!)
		int redItemId = _optionRedTeam.GetItemId(_optionRedTeam.Selected);
		int blueItemId = _optionBlueTeam.GetItemId(_optionBlueTeam.Selected);

		// Rotes Team konfigurieren
		gameManager.RedTeamIsBot = false; // Im Multiplayer nie ein Bot
		if (redItemId == 0) // Keyboard 1
		{
			gameManager.RedTeamController = InputManager.InputDevice.Keyboard;
			GD.Print("Team Rot: Keyboard 1 (WASD + QE)");
		}
		else if (redItemId == 1) // Keyboard 2
		{
			gameManager.RedTeamController = InputManager.InputDevice.Keyboard2;
			GD.Print("Team Rot: Keyboard 2 (Pfeiltasten + Numpad)");
		}
		else // Controller (ID >= 2)
		{
			// ItemId auf Controller-Index mappen: itemId 2 -> Controller0, itemId 3 -> Controller1, etc.
			int controllerIndex = redItemId - 2;
			if (controllerIndex < _connectedJoypads.Count)
			{
				int joypadId = _connectedJoypads[controllerIndex];
				gameManager.RedTeamController = joypadId switch
				{
					0 => InputManager.InputDevice.Controller0,
					1 => InputManager.InputDevice.Controller1,
					2 => InputManager.InputDevice.Controller2,
					3 => InputManager.InputDevice.Controller3,
					_ => InputManager.InputDevice.Controller0
				};
				string controllerName = Input.GetJoyName(joypadId);
				if (string.IsNullOrWhiteSpace(controllerName))
					controllerName = $"Controller {joypadId + 1}";
				GD.Print($"Team Rot: {controllerName} (Joypad-ID: {joypadId})");
			}
			else
			{
				// Fallback falls etwas schief geht
				gameManager.RedTeamController = InputManager.InputDevice.Keyboard;
				GD.PrintErr("Warnung: Controller-Auswahl Team Rot ungültig, Fallback auf Keyboard 1");
			}
		}

		// Blaues Team konfigurieren
		gameManager.BlueTeamIsBot = false; // Im Multiplayer nie ein Bot
		if (blueItemId == 0) // Keyboard 1
		{
			gameManager.BlueTeamController = InputManager.InputDevice.Keyboard;
			GD.Print("Team Blau: Keyboard 1 (WASD + QE)");
		}
		else if (blueItemId == 1) // Keyboard 2
		{
			gameManager.BlueTeamController = InputManager.InputDevice.Keyboard2;
			GD.Print("Team Blau: Keyboard 2 (Pfeiltasten + Numpad)");
		}
		else // Controller (ID >= 2)
		{
			// ItemId auf Controller-Index mappen: itemId 2 -> Controller0, itemId 3 -> Controller1, etc.
			int controllerIndex = blueItemId - 2;
			if (controllerIndex < _connectedJoypads.Count)
			{
				int joypadId = _connectedJoypads[controllerIndex];
				gameManager.BlueTeamController = joypadId switch
				{
					0 => InputManager.InputDevice.Controller0,
					1 => InputManager.InputDevice.Controller1,
					2 => InputManager.InputDevice.Controller2,
					3 => InputManager.InputDevice.Controller3,
					_ => InputManager.InputDevice.Controller0
				};
				string controllerName = Input.GetJoyName(joypadId);
				if (string.IsNullOrWhiteSpace(controllerName))
					controllerName = $"Controller {joypadId + 1}";
				GD.Print($"Team Blau: {controllerName} (Joypad-ID: {joypadId})");
			}
			else
			{
				// Fallback falls etwas schief geht
				gameManager.BlueTeamController = InputManager.InputDevice.Controller0;
				GD.PrintErr("Warnung: Controller-Auswahl Team Blau ungültig, Fallback auf Controller 0");
			}
		}
	}
}
