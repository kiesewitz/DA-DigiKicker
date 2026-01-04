using Godot;
using System;

/// <summary>
/// Game-Over-Bildschirm am Spielende.
/// Zeigt Gewinner und Endstand und bietet Neustart oder Rueckkehr ins Hauptmenue.
/// </summary>
public partial class GameOverScreen : Control
{
	// Referenzen zu UI-Nodes
	private ColorRect _dimBackground;
	private Label _labelGameOver;
	private Label _labelWinner;
	private Label _labelFinalScoreRed;
	private Label _labelVS;
	private Label _labelFinalScoreBlue;
	private Button _btnPlayAgain;
	private Button _btnMainMenu;

	// Manager-Referenzen
	private GameManager _gameManager;
	private StatsManager _statsManager;

	/// <summary>
	/// Initialisiert den Game-Over-Bildschirm: Node-Referenzen holen, statische Texte setzen,
	/// Signals verbinden und den Bildschirm initial verstecken.
	/// </summary>
	public override void _Ready()
	{
		// Node-Referenzen holen (Pfade beinhalten CenterContainer fuer zentriertes Layout)
		_dimBackground = GetNode<ColorRect>("DimBackground");
		_labelGameOver = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/LabelGameOver");
		_labelWinner = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/LabelWinner");
		_labelFinalScoreRed = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/FinalScore/LabelFinalScoreRed");
		_labelVS = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/FinalScore/LabelVS");
		_labelFinalScoreBlue = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/FinalScore/LabelFinalScoreBlue");
		_btnPlayAgain = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/BtnPlayAgain");
		_btnMainMenu = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/BtnMainMenu");

		// Statische Text-Labels setzen
		_labelGameOver.Text = "GAME OVER";
		_labelVS.Text = "vs";
		_btnPlayAgain.Text = "Play Again";
		_btnMainMenu.Text = "Main Menu";

		// Dimm-Hintergrund konfigurieren
		_dimBackground.Color = new Color(0, 0, 0, 0.6f);

		// Initial versteckt
		Visible = false;

		// Button Signals verbinden
		_btnPlayAgain.Pressed += OnPlayAgain;
		_btnMainMenu.Pressed += OnMainMenu;

		// Manager-Referenzen holen
		_gameManager = GetNode<GameManager>("/root/GameManager");
		_statsManager = GetNode<StatsManager>("/root/StatsManager");

		// Mit GameManager's GameEnded Signal verbinden
		if (_gameManager != null)
		{
			_gameManager.GameEnded += OnGameEnded;
		}
	}

	/// <summary>
	/// Wird beim Entfernen des Nodes aufgerufen und trennt alle Signal-Verbindungen.
	/// </summary>
	public override void _ExitTree()
	{
		// Signals trennen
		_btnPlayAgain.Pressed -= OnPlayAgain;
		_btnMainMenu.Pressed -= OnMainMenu;

		if (_gameManager != null)
		{
			_gameManager.GameEnded -= OnGameEnded;
		}
	}

	/// <summary>
	/// Signal-Handler fuer GameEnded-Event vom GameManager.
	/// Zeigt Gewinner, Endstand und animiert das Erscheinen.
	/// Versteckt den "Play Again" Button fuer Online-Spiele und zeichnet lokale Statistiken auf.
	/// </summary>
	private void OnGameEnded(GameManager.Team winner)
	{
		// Bildschirm sichtbar machen
		Visible = true;

		// Pruefen, ob dies ein Online-Spiel ist
		bool isOnlineGame = IsOnlineGame();

		// Play Again Button im Online-Modus verstecken (Online-Matches koennen nicht neugestartet werden)
		_btnPlayAgain.Visible = !isOnlineGame;

		// Gewinner-Text mit Farbe setzen
		switch (winner)
		{
			case GameManager.Team.Red:
				_labelWinner.Text = "RED TEAM WINS!";
				_labelWinner.AddThemeColorOverride("font_color", new Color(1.0f, 0.27f, 0.27f));
				break;
			case GameManager.Team.Blue:
				_labelWinner.Text = "BLUE TEAM WINS!";
				_labelWinner.AddThemeColorOverride("font_color", new Color(0.27f, 0.27f, 1.0f));
				break;
			case GameManager.Team.None:
				_labelWinner.Text = "DRAW!";
				_labelWinner.AddThemeColorOverride("font_color", new Color(1.0f, 1.0f, 1.0f));
				break;
		}

		// Endstand vom GameManager holen
		int redScore = _gameManager.ScoreRed;
		int blueScore = _gameManager.ScoreBlue;

		// Endstand aktualisieren
		_labelFinalScoreRed.Text = redScore.ToString();
		_labelFinalScoreBlue.Text = blueScore.ToString();

		// Spielstand-Labels einfaerben
		_labelFinalScoreRed.AddThemeColorOverride("font_color", new Color(1.0f, 0.27f, 0.27f));
		_labelFinalScoreBlue.AddThemeColorOverride("font_color", new Color(0.27f, 0.27f, 1.0f));

		// Spielergebnis in Statistiken aufzeichnen (nur fuer lokale Spiele)
		if (_statsManager != null && !isOnlineGame)
		{
			bool won = (winner == GameManager.Team.Red);
			_statsManager.RecordGameResult(won);
		}

		// Bildschirm-Erscheinen animieren
		Modulate = new Color(1, 1, 1, 0);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 1.0f, 0.5f);

		// Passenden Button fokussieren
		if (isOnlineGame)
		{
			_btnMainMenu.GrabFocus();
		}
		else
		{
			_btnPlayAgain.GrabFocus();
		}
	}

	/// <summary>
	/// Prueft, ob es sich um ein Online-Multiplayer-Spiel handelt.
	/// </summary>
	private bool IsOnlineGame()
	{
		// Pruefen, ob OnlineMultiplayerManager existiert und verbunden ist
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		if (onlineManager != null && onlineManager.HasMethod("is_connected_to_peer"))
		{
			return (bool)onlineManager.Call("is_connected_to_peer");
		}
		return false;
	}

	private void OnPlayAgain()
	{
		GD.Print("OnPlayAgain button pressed");

		// Szene zuerst entpausieren (Spiel koennte pausiert sein)
		GetTree().Paused = false;

		// Bildschirm ausblenden
		Visible = false;

		// Aktuelle Szene neu laden fuer ein neues Spiel
		GetTree().ReloadCurrentScene();
	}

	private void OnMainMenu()
	{
		GD.Print("OnMainMenu button pressed");

		// Szene zuerst entpausieren (Spiel koennte pausiert sein)
		GetTree().Paused = false;

		// Bildschirm ausblenden
		Visible = false;

		// Wichtig: Bei Online-Spiel die Verbindung trennen
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		if (onlineManager != null && onlineManager.HasMethod("is_connected_to_peer"))
		{
			bool isConnected = (bool)onlineManager.Call("is_connected_to_peer");
			if (isConnected)
			{
				GD.Print("[GameOverScreen] Disconnecting from online match...");
				// disconnect mit cleanup_backend=true rufen, damit Server informiert wird
				onlineManager.Call("disconnect_from_match", true);
			}
		}

		// Zurueck zum Hauptmenue wechseln
		GetTree().ChangeSceneToFile("res://scenes/main/Main.tscn");
	}

	public void ShowScreen(GameManager.Team winner)
	{
		// Alternative Aufruf, falls nicht ueber Signal genutzt
		OnGameEnded(winner);
	}
}
