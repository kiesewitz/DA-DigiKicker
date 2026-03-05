using Godot;
using System;

/// <summary>
/// Heads-Up-Display (HUD) für das Spiel.
/// Zeigt Spielstand, Timer, FPS-Counter, Tor-Benachrichtigungen, Countdown und Ping-Anzeige.
/// </summary>
public partial class HUD : Control
{
	// Referenzen zu UI-Nodes
	private Label _labelScoreRed;
	private Label _labelScoreBlue;
	private Label _labelTimer;
	private ColorRect _teamRedIcon;
	private ColorRect _teamBlueIcon;
	private Label _fpsCounter;
	private Control _goalNotification;
	private Label _labelGoal;

	// Online Multiplayer Ping Anzeige
	private Label _pingLabel;
	private Node _onlineManager;  // OnlineMultiplayerManager (GDScript)
	private bool _isOnlineGame = false;

	// Countdown Overlay
	private CenterContainer _countdownContainer;
	private Label _countdownLabel;
	private bool _countdownActive = false;

	// Manager-Referenzen und Einstellungen
	private GameManager _gameManager;
	private AudioManager _audioManager;
	private bool _showFps = false;

	/// <summary>
	/// Initialisiert das HUD: Holt Node-Referenzen, setzt Teamfarben,
	/// verbindet Signals und initialisiert alle Anzeigen.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// Alle UI-Node-Referenzen holen (GetNodeOrNull für optionale Nodes)
		_labelScoreRed = GetNode<Label>("TopBar/ScoreRed/LabelScoreRed");
		_labelScoreBlue = GetNode<Label>("TopBar/ScoreBlue/LabelScoreBlue");
		_labelTimer = GetNode<Label>("TopBar/LabelTimer");
		_teamRedIcon = GetNodeOrNull<ColorRect>("TopBar/ScoreRed/TeamRedIcon");
		_teamBlueIcon = GetNodeOrNull<ColorRect>("TopBar/ScoreBlue/TeamBlueIcon");
		_fpsCounter = GetNodeOrNull<Label>("TopBar/FPSCounter");
		_goalNotification = GetNodeOrNull<Control>("TopBar/GoalNotification");
		_labelGoal = GetNodeOrNull<Label>("TopBar/GoalNotification/LabelGoal");

		// Teamfarben setzen falls Icons existieren
		if (_teamRedIcon != null)
			_teamRedIcon.Color = new Color(1.0f, 0.27f, 0.27f); // #FF4444
		if (_teamBlueIcon != null)
			_teamBlueIcon.Color = new Color(0.27f, 0.27f, 1.0f); // #4444FF

		// Tor-Benachrichtigung initialisieren falls vorhanden
		if (_goalNotification != null)
		{
			_goalNotification.Visible = false;
			if (_labelGoal != null)
				_labelGoal.Text = "GOAL!";
		}

		// Countdown Overlay erstellen
		SetupCountdownOverlay();

		// GameManager holen und Signals verbinden
		_gameManager = GetNode<GameManager>("/root/GameManager");
		if (_gameManager != null)
		{
			_gameManager.GoalScored += OnGoalScored;
			_gameManager.TimeUpdated += OnTimeUpdated;
			_gameManager.FpsDisplayToggled += OnFpsDisplayToggled;
			_gameManager.CountdownRequested += OnCountdownRequested;
		}

		// Spielstand initialisieren
		if (_labelScoreRed != null) _labelScoreRed.Text = "0";
		if (_labelScoreBlue != null) _labelScoreBlue.Text = "0";

		// Timer mit aktueller Spielzeit vom GameManager initialisieren
		if (_labelTimer != null && _gameManager != null)
			_labelTimer.Text = _gameManager.GetTimeString();

		// FPS Counter Sichtbarkeit - mit GameManager synchronisieren
		_showFps = _gameManager != null ? _gameManager.ShowFps : false;
		if (_fpsCounter != null)
			_fpsCounter.Visible = _showFps;

		// Online Ping Label einrichten
		SetupPingLabel();
		CheckOnlineGame();
	}

	/// <summary>
	/// Wird beim Entfernen des Nodes aufgerufen.
	/// Trennt alle Signal-Verbindungen um Memory Leaks zu vermeiden.
	/// </summary>
	public override void _ExitTree()
	{
		// Signals trennen um Memory Leaks zu vermeiden
		if (_gameManager != null)
		{
			_gameManager.GoalScored -= OnGoalScored;
			_gameManager.TimeUpdated -= OnTimeUpdated;
			_gameManager.FpsDisplayToggled -= OnFpsDisplayToggled;
			_gameManager.CountdownRequested -= OnCountdownRequested;
		}
	}

	/// <summary>
	/// Wird jeden Frame aufgerufen.
	/// Aktualisiert FPS-Counter und Ping-Anzeige.
	/// </summary>
	public override void _Process(double delta)
	{
		// FPS Counter aktualisieren falls aktiviert
		if (_showFps && _fpsCounter != null && _fpsCounter.Visible)
		{
			_fpsCounter.Text = $"FPS: {Engine.GetFramesPerSecond()}";
		}

		// Ping-Anzeige für Online-Spiele aktualisieren
		UpdatePingDisplay();
	}

	/// <summary>
	/// Signal-Handler für GoalScored Event vom GameManager.
	/// Aktualisiert die Spielstand-Anzeige und zeigt Tor-Benachrichtigung.
	/// </summary>
	private void OnGoalScored(GameManager.Team team, int newScore)
	{
		// Spielstand-Label basierend auf dem Team aktualisieren
		if (team == GameManager.Team.Red)
		{
			if (_labelScoreRed != null)
				_labelScoreRed.Text = newScore.ToString();
		}
		else if (team == GameManager.Team.Blue)
		{
			if (_labelScoreBlue != null)
				_labelScoreBlue.Text = newScore.ToString();
		}

		// Tor-Benachrichtigung anzeigen
		ShowGoalNotification();
	}

	/// <summary>
	/// Signal-Handler für TimeUpdated Event vom GameManager.
	/// Aktualisiert die Timer-Anzeige mit verbleibender Spielzeit.
	/// </summary>
	private void OnTimeUpdated(float timeRemaining)
	{
		if (_labelTimer != null)
			_labelTimer.Text = FormatTime(timeRemaining);
	}

	/// <summary>
	/// Signal-Handler für FpsDisplayToggled Event vom GameManager.
	/// Schaltet die Sichtbarkeit des FPS-Counters um.
	/// </summary>
	private void OnFpsDisplayToggled(bool enabled)
	{
		SetFpsCounterVisible(enabled);
	}

	/// <summary>
	/// Zeigt eine animierte "GOAL!" Benachrichtigung.
	/// Verwendet Tween-Animationen für Fade-In, Scale-Up und Fade-Out Effekte.
	/// </summary>
	private void ShowGoalNotification()
	{
		// Prüfen ob Benachrichtigung existiert
		if (_goalNotification == null)
			return;

		// Benachrichtigung sichtbar machen
		_goalNotification.Visible = true;
		_goalNotification.Modulate = new Color(1, 1, 1, 0);
		_goalNotification.Scale = new Vector2(0.5f, 0.5f);

		// Tween Animation für Fade-In und Scale-Up erstellen
		Tween tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(_goalNotification, "modulate:a", 1.0f, 0.3f);
		tween.TweenProperty(_goalNotification, "scale", Vector2.One, 0.3f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);

		// Warten, dann Fade-Out
		tween.Chain().TweenInterval(1.5f);
		tween.Chain().TweenProperty(_goalNotification, "modulate:a", 0.0f, 0.3f);

		// Benachrichtigung verstecken wenn Animation fertig
		tween.Chain().TweenCallback(Callable.From(() =>
		{
			_goalNotification.Visible = false;
			_goalNotification.Modulate = new Color(1, 1, 1, 1);
			_goalNotification.Scale = Vector2.One;
		}));
	}

	/// <summary>
	/// Formatiert Sekunden in MM:SS Format.
	/// </summary>
	private string FormatTime(float seconds)
	{
		int minutes = Mathf.FloorToInt(seconds / 60.0f);
		int secs = Mathf.FloorToInt(seconds % 60.0f);
		return $"{minutes:D2}:{secs:D2}";
	}

	/// <summary>
	/// Setzt die Sichtbarkeit des FPS-Counters.
	/// </summary>
	public void SetFpsCounterVisible(bool visible)
	{
		_showFps = visible;
		if (_fpsCounter != null)
			_fpsCounter.Visible = visible;
	}

	/// <summary>
	/// Erstellt die Countdown Overlay UI-Elemente.
	/// Erzeugt einen zentrierten Container mit großem Label für die 3-2-1-GO Anzeige.
	/// </summary>
	private void SetupCountdownOverlay()
	{
		// Center Container für Countdown erstellen
		_countdownContainer = new CenterContainer();
		_countdownContainer.Name = "CountdownContainer";
		_countdownContainer.AnchorsPreset = (int)LayoutPreset.FullRect;
		_countdownContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_countdownContainer.Visible = false;
		_countdownContainer.MouseFilter = MouseFilterEnum.Ignore;
		AddChild(_countdownContainer);

		// Countdown Label erstellen
		_countdownLabel = new Label();
		_countdownLabel.Name = "CountdownLabel";
		_countdownLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_countdownLabel.VerticalAlignment = VerticalAlignment.Center;
		_countdownLabel.AddThemeFontSizeOverride("font_size", 120);
		_countdownLabel.Text = "3";

		// Outline/Shadow für bessere Sichtbarkeit hinzufügen
		_countdownLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 1));
		_countdownLabel.AddThemeConstantOverride("outline_size", 8);

		_countdownContainer.AddChild(_countdownLabel);
	}

	/// <summary>
	/// Signal-Handler für CountdownRequested Event vom GameManager.
	/// Startet den Countdown wenn vom GameManager angefordert.
	/// </summary>
	private void OnCountdownRequested()
	{
		StartCountdown();
	}

	/// <summary>
	/// Startet die 3-2-1-GO Countdown Animation.
	/// Zeigt nacheinander die Zahlen 3, 2, 1 und dann "GO!" mit Animationen.
	/// Benachrichtigt den GameManager wenn der Countdown abgeschlossen ist.
	/// </summary>
	public void StartCountdown()
	{
		if (_countdownActive)
			return;

		_countdownActive = true;
		_countdownContainer.Visible = true;
		_countdownLabel.Text = "3";
		_countdownLabel.Modulate = new Color(1, 1, 1, 1);
		_countdownLabel.Scale = Vector2.One;

		// Countdown animieren: 3 -> 2 -> 1 -> GO!
		AnimateCountdownNumber(3, () =>
		{
			AnimateCountdownNumber(2, () =>
			{
				AnimateCountdownNumber(1, () =>
				{
					// "GO!" kurz anzeigen
					_countdownLabel.Text = "GO!";
					_countdownLabel.Modulate = new Color(0.2f, 1.0f, 0.2f, 1); // Grün
					_countdownLabel.Scale = new Vector2(0.5f, 0.5f);

					// Pfiff-Sound für GO! abspielen
					_audioManager?.PlayWhistle();

					Tween goTween = CreateTween();
					goTween.TweenProperty(_countdownLabel, "scale", new Vector2(1.2f, 1.2f), 0.3f)
						.SetTrans(Tween.TransitionType.Back)
						.SetEase(Tween.EaseType.Out);
					goTween.TweenInterval(0.3f);
					goTween.TweenProperty(_countdownLabel, "modulate:a", 0.0f, 0.2f);
					goTween.TweenCallback(Callable.From(() =>
					{
						_countdownContainer.Visible = false;
						_countdownActive = false;

						// GameManager signalisieren dass Countdown abgeschlossen ist
						if (_gameManager != null)
						{
							_gameManager.OnCountdownComplete();
						}
					}));
				});
			});
		});
	}

	/// <summary>
	/// Animiert eine einzelne Countdown-Zahl mit Scale und Fade Effekten.
	/// Verwendet Bounce-Animation beim Einblenden und Fade-Out beim Ausblenden.
	/// </summary>
	private void AnimateCountdownNumber(int number, Action onComplete)
	{
		_countdownLabel.Text = number.ToString();
		_countdownLabel.Modulate = new Color(1, 1, 1, 1);
		_countdownLabel.Scale = new Vector2(0.3f, 0.3f);

		Tween tween = CreateTween();

		// Scale-Up mit Bounce-Effekt
		tween.TweenProperty(_countdownLabel, "scale", new Vector2(1.0f, 1.0f), 0.3f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);

		// Halten
		tween.TweenInterval(0.4f);

		// Fade-Out und leichtes Scale-Down
		tween.TweenProperty(_countdownLabel, "modulate:a", 0.3f, 0.2f);
		tween.Parallel().TweenProperty(_countdownLabel, "scale", new Vector2(0.8f, 0.8f), 0.2f);

		// Completion Callback aufrufen
		tween.TweenCallback(Callable.From(() => onComplete?.Invoke()));
	}

	/// <summary>
	/// Erstellt das Ping Label für Online Multiplayer Spiele.
	/// Positioniert in der oberen rechten Ecke des Bildschirms.
	/// </summary>
	private void SetupPingLabel()
	{
		// Ping Label erstellen
		_pingLabel = new Label();
		_pingLabel.Name = "PingLabel";
		_pingLabel.Text = "Ping: ---";
		_pingLabel.Visible = false;  // Versteckt bis Online-Spiel startet

		// In oberer rechter Ecke positionieren
		_pingLabel.AnchorsPreset = (int)LayoutPreset.TopRight;
		_pingLabel.SetAnchorsAndOffsetsPreset(LayoutPreset.TopRight, LayoutPresetMode.KeepSize);
		_pingLabel.Position = new Vector2(-100, 10);
		_pingLabel.CustomMinimumSize = new Vector2(90, 0);
		_pingLabel.HorizontalAlignment = HorizontalAlignment.Right;

		// Styling
		_pingLabel.AddThemeFontSizeOverride("font_size", 14);
		_pingLabel.AddThemeColorOverride("font_color", new Color(0.5f, 1.0f, 0.5f));  // Grün
		_pingLabel.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.7f));
		_pingLabel.AddThemeConstantOverride("outline_size", 2);

		AddChild(_pingLabel);
	}

	/// <summary>
	/// Prüft ob dies ein Online-Spiel ist und stellt Verbindung zum OnlineMultiplayerManager her.
	/// </summary>
	private void CheckOnlineGame()
	{
		// OnlineMultiplayerManager suchen
		_onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");

		if (_onlineManager != null)
		{
			// Verbindungsstatus prüfen (GDScript Methode aufrufen)
			var isConnected = _onlineManager.Call("is_connected_to_peer");
			_isOnlineGame = isConnected.AsBool();

			if (_isOnlineGame && _pingLabel != null)
			{
				_pingLabel.Visible = true;
			}
		}
	}

	/// <summary>
	/// Aktualisiert die Ping-Anzeige durch Auslesen vom OnlineMultiplayerManager.
	/// Wird jeden Frame in _Process aufgerufen.
	/// Zeigt Ping-Zeit in ms an und färbt den Text basierend auf der Verbindungsqualität.
	/// </summary>
	private void UpdatePingDisplay()
	{
		if (!_isOnlineGame || _onlineManager == null || _pingLabel == null)
			return;

		// Aktuellen Ping vom OnlineMultiplayerManager holen
		var pingResult = _onlineManager.Call("get_ping");
		int pingMs = pingResult.AsInt32();

		// Verbindungstyp holen (S für STUN, T für TURN)
		var connectionTypeResult = _onlineManager.Get("connection_type");
		string connectionType = connectionTypeResult.AsString();

		if (pingMs >= 0)
		{
			// Verbindungstyp-Präfix hinzufügen falls verfügbar
			string prefix = !string.IsNullOrEmpty(connectionType) ? connectionType + " " : "";
			_pingLabel.Text = $"{prefix}Ping: {pingMs} ms";

			// Farbe basierend auf Ping-Qualität
			Color pingColor;
			if (pingMs < 50)
				pingColor = new Color(0.2f, 1.0f, 0.2f);      // Grün - exzellent
			else if (pingMs < 100)
				pingColor = new Color(0.8f, 1.0f, 0.2f);      // Gelb-grün - gut
			else if (pingMs < 150)
				pingColor = new Color(1.0f, 0.8f, 0.2f);      // Gelb-orange - akzeptabel
			else
				pingColor = new Color(1.0f, 0.3f, 0.2f);      // Rot - schlecht

			_pingLabel.AddThemeColorOverride("font_color", pingColor);
		}
		else
		{
			_pingLabel.Text = "Ping: ---";
			_pingLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		}
	}

	/// <summary>
	/// Aktiviert den Online-Modus für die Anzeige (wird beim Start eines Online-Spiels aufgerufen).
	/// </summary>
	public void EnableOnlineMode()
	{
		_isOnlineGame = true;
		CheckOnlineGame();

		if (_pingLabel != null)
		{
			_pingLabel.Visible = true;
		}
	}
}
