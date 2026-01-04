using Godot;
using System;

// Zentrale Verwaltung des Spielzustands, der Punkte, des Timers und der Siegbedingungen
public partial class GameManager : Node
{
	// Enums für Spielzustand, Teams, Siegbedingungen und Bot-Typen
	public enum GameState { Menu, Playing, Paused, GameOver }
	public enum Team { None, Red, Blue }
	public enum WinCondition { TimeLimit, FirstToScore, Both }
	public enum BotType { RuleBot, TrainedAI }

	// Signals für Spielereignisse
	[Signal] public delegate void GameStartedEventHandler();
	[Signal] public delegate void GamePausedEventHandler();
	[Signal] public delegate void GameResumedEventHandler();
	[Signal] public delegate void GameEndedEventHandler(Team winner);
	[Signal] public delegate void GoalScoredEventHandler(Team team, int newScore);
	[Signal] public delegate void TimeUpdatedEventHandler(float timeLeft);
	[Signal] public delegate void BallResetRequestedEventHandler();
	[Signal] public delegate void BallKickoffRequestedEventHandler();
	[Signal] public delegate void RodsResetRequestedEventHandler();
	[Signal] public delegate void FpsDisplayToggledEventHandler(bool enabled);
	[Signal] public delegate void CountdownRequestedEventHandler();

	// Öffentliche Properties für Spielzustand und Einstellungen
	public int ScoreRed { get; private set; } = 0;
	public int ScoreBlue { get; private set; } = 0;
	public float TimeRemaining { get; private set; } = 0;
	public int GameDuration { get; set; } = 3;
	public int WinScore { get; set; } = 5;
	public GameState CurrentState { get; private set; } = GameState.Menu;
	public WinCondition CurrentWinCondition { get; set; } = WinCondition.Both;
	public bool IsMultiplayer { get; set; } = false;
	public Team PlayerTeam { get; set; } = Team.Red;
	public BotController.BotDifficulty BotDifficulty { get; set; } = BotController.BotDifficulty.Medium;
	public BotType CurrentBotType { get; set; } = BotType.RuleBot;
	public bool ShowFps { get; private set; } = false;
	public bool IsCountdownActive => _countdownActive;
	public bool IsWaitingForCountdown => _waitingForCountdown;

	// Multiplayer Controller-Zuweisungen
	public InputManager.InputDevice RedTeamController { get; set; } = InputManager.InputDevice.Keyboard;
	public InputManager.InputDevice BlueTeamController { get; set; } = InputManager.InputDevice.Controller0;
	public bool RedTeamIsBot { get; set; } = false;
	public bool BlueTeamIsBot { get; set; } = false;
	public BotController.BotDifficulty RedTeamBotDifficulty { get; set; } = BotController.BotDifficulty.Medium;
	public BotController.BotDifficulty BlueTeamBotDifficulty { get; set; } = BotController.BotDifficulty.Medium;

	// Private Felder für interne Zustandsverwaltung
	private float _totalGameTime = 0;
	private const float GOAL_RESET_DELAY = 1.5f;
	private float _goalResetTimer = 0;
	private bool _waitingForCountdown = false;
	private bool _countdownActive = false;
	private bool _isGameStartCountdown = false;

	/// <summary>
	/// Initialisiert den GameManager beim Start der Szene
	/// </summary>
	public override void _Ready()
	{
		GD.Print("GameManager initialized");
		SetProcess(true);
		SetPhysicsProcess(false);
		CurrentState = GameState.Menu;
	}

	/// <summary>
	/// Verarbeitet den Spiel-Timer und Countdown-Verzögerungen in jedem Frame
	/// </summary>
	public override void _Process(double delta)
	{
		// Spiel-Timer herunterzählen während des Spielens
		if (CurrentState == GameState.Playing)
		{
			TimeRemaining -= (float)delta;

			if (TimeRemaining <= 0)
			{
				TimeRemaining = 0;
				EmitSignal(SignalName.TimeUpdated, TimeRemaining);
				CheckWinCondition();
			}
			else
			{
				// Zeit-Update Signal für flüssige UI-Aktualisierung
				EmitSignal(SignalName.TimeUpdated, TimeRemaining);
			}
		}

		// Countdown-Verzögerung nach Tor verarbeiten
		if (_waitingForCountdown)
		{
			_goalResetTimer -= (float)delta;
			if (_goalResetTimer <= 0)
			{
				_waitingForCountdown = false;
				// Ball vor Countdown zentrieren (verhindert Kamera-Sprung)
				EmitSignal(SignalName.BallResetRequested);
				StartCountdown(false);
			}
		}
	}

	/// <summary>
	/// Startet ein neues Spiel mit den angegebenen Einstellungen
	/// </summary>
	public void StartGame(int duration, int winScore, WinCondition condition)
	{
		GD.Print($"Starting game: Duration={duration}min, WinScore={winScore}, Condition={condition}");

		GameDuration = duration;
		WinScore = winScore;
		CurrentWinCondition = condition;

		// Spielzustand zurücksetzen
		ScoreRed = 0;
		ScoreBlue = 0;
		TimeRemaining = GameDuration * 60.0f;
		_totalGameTime = TimeRemaining;
		_waitingForCountdown = false;

		// Start-Signal senden
		EmitSignal(SignalName.GameStarted);
		EmitSignal(SignalName.TimeUpdated, TimeRemaining);

		// Ball in Spielfeldmitte positionieren
		EmitSignal(SignalName.BallResetRequested);

		// Countdown starten - Spiel beginnt nach Countdown
		StartCountdown(true);
	}

	/// <summary>
	/// Startet die Countdown-Sequenz vor Spielbeginn oder nach einem Tor
	/// </summary>
	private void StartCountdown(bool isGameStart)
	{
		_isGameStartCountdown = isGameStart;
		_countdownActive = true;

		// Spiel-Timer während Countdown pausieren
		CurrentState = GameState.Paused;

		GD.Print($"Starting countdown (isGameStart={isGameStart})");
		EmitSignal(SignalName.CountdownRequested);
	}

	/// <summary>
	/// Wird vom HUD aufgerufen, wenn die Countdown-Animation abgeschlossen ist
	/// </summary>
	public void OnCountdownComplete()
	{
		GD.Print("Countdown complete - starting play");
		_countdownActive = false;
		CurrentState = GameState.Playing;

		// Anstoß-Impuls anfordern
		EmitSignal(SignalName.BallKickoffRequested);
	}

	/// <summary>
	/// Pausiert das laufende Spiel
	/// </summary>
	public void PauseGame()
	{
		if (CurrentState == GameState.Playing)
		{
			CurrentState = GameState.Paused;
			GetTree().Paused = true;
			EmitSignal(SignalName.GamePaused);
			GD.Print("Game paused");
		}
	}

	/// <summary>
	/// Setzt das Spiel nach einer Pause fort
	/// </summary>
	public void ResumeGame()
	{
		if (CurrentState == GameState.Paused)
		{
			CurrentState = GameState.Playing;
			GetTree().Paused = false;
			EmitSignal(SignalName.GameResumed);
			GD.Print("Game resumed");
		}
	}

	/// <summary>
	/// Beendet das Spiel und gibt optional einen Gewinner bekannt
	/// </summary>
	public void EndGame(Team winner = Team.None)
	{
		if (CurrentState == GameState.GameOver)
			return;

		GD.Print($"Game ended. Winner: {winner}");

		CurrentState = GameState.GameOver;
		GetTree().Paused = false;

		// Statistiken aufzeichnen
		if (GetNode("/root/StatsManager") is StatsManager statsManager)
		{
			statsManager.RecordGameResult(winner == Team.Red);
		}

		EmitSignal(SignalName.GameEnded, (int)winner);
	}

	/// <summary>
	/// Registriert ein Tor für das angegebene Team und aktualisiert den Spielstand
	/// </summary>
	public void ScoreGoal(Team team)
	{
		if (CurrentState != GameState.Playing || team == Team.None)
			return;

		// Punktestand aktualisieren
		if (team == Team.Red)
		{
			ScoreRed++;
			GD.Print($"Goal for Red! Score: Red {ScoreRed} - Blue {ScoreBlue}");
			EmitSignal(SignalName.GoalScored, (int)Team.Red, ScoreRed);

			// Statistiken aufzeichnen
			if (GetNode("/root/StatsManager") is StatsManager statsManager)
			{
				statsManager.RecordGoal(true);
			}
		}
		else if (team == Team.Blue)
		{
			ScoreBlue++;
			GD.Print($"Goal for Blue! Score: Red {ScoreRed} - Blue {ScoreBlue}");
			EmitSignal(SignalName.GoalScored, (int)Team.Blue, ScoreBlue);

			// Statistiken aufzeichnen
			if (GetNode("/root/StatsManager") is StatsManager statsManager)
			{
				statsManager.RecordGoal(false);
			}
		}

		// Tor-Sound abspielen
		if (GetNode("/root/AudioManager") is AudioManager audioManager)
		{
			audioManager.PlaySFX("goal");
		}

		// Siegbedingung prüfen
		bool gameEnded = CheckWinConditionInternal();

		// Falls Spiel nicht beendet, Countdown für nächste Runde planen
		if (!gameEnded)
		{
			_waitingForCountdown = true;
			_goalResetTimer = GOAL_RESET_DELAY;

			// Stangen in Startpositionen zurücksetzen
			EmitSignal(SignalName.RodsResetRequested);
		}
	}

	/// <summary>
	/// Setzt den Ball in die Spielfeldmitte zurück
	/// </summary>
	public void ResetBall()
	{
		GD.Print("Requesting ball reset");
		EmitSignal(SignalName.BallResetRequested);
	}

	/// <summary>
	/// Kehrt zum Hauptmenü zurück
	/// </summary>
	public void ReturnToMenu()
	{
		CurrentState = GameState.Menu;
		GetTree().Paused = false;
		GD.Print("Returned to menu");
	}

	/// <summary>
	/// Gibt den aktuellen Spielstand als formatierten String zurück
	/// </summary>
	public string GetScoreString()
	{
		return $"{ScoreRed} : {ScoreBlue}";
	}

	/// <summary>
	/// Gibt die verbleibende Zeit als formatierten String zurück (MM:SS)
	/// </summary>
	public string GetTimeString()
	{
		int minutes = (int)(TimeRemaining / 60);
		int seconds = (int)(TimeRemaining % 60);
		return $"{minutes:D2}:{seconds:D2}";
	}

	/// <summary>
	/// Schaltet die FPS-Anzeige ein oder aus
	/// </summary>
	public void SetFpsDisplay(bool enabled)
	{
		ShowFps = enabled;
		EmitSignal(SignalName.FpsDisplayToggled, enabled);
		GD.Print($"FPS display {(enabled ? "enabled" : "disabled")}");
	}

	/// <summary>
	/// Prüft, ob eine Siegbedingung erfüllt wurde (wird vom Timer aufgerufen)
	/// </summary>
	private void CheckWinCondition()
	{
		CheckWinConditionInternal();
	}

	/// <summary>
	/// Prüft alle Siegbedingungen und gibt zurück, ob das Spiel beendet wurde
	/// </summary>
	private bool CheckWinConditionInternal()
	{
		Team winner = Team.None;
		bool gameEnded = false;

		switch (CurrentWinCondition)
		{
			case WinCondition.TimeLimit:
				// Spiel endet wenn Zeit abgelaufen
				if (TimeRemaining <= 0)
				{
					gameEnded = true;
					// Gewinner ist Team mit höherem Score, oder keiner bei Unentschieden
					if (ScoreRed > ScoreBlue)
						winner = Team.Red;
					else if (ScoreBlue > ScoreRed)
						winner = Team.Blue;
					else
						winner = Team.None;
				}
				break;

			case WinCondition.FirstToScore:
				// Spiel endet wenn ein Team die Zielpunktzahl erreicht
				if (ScoreRed >= WinScore)
				{
					gameEnded = true;
					winner = Team.Red;
				}
				else if (ScoreBlue >= WinScore)
				{
					gameEnded = true;
					winner = Team.Blue;
				}
				break;

			case WinCondition.Both:
				// Spiel endet wenn Zeit abläuft ODER ein Team die Zielpunktzahl erreicht
				if (TimeRemaining <= 0)
				{
					gameEnded = true;
					if (ScoreRed > ScoreBlue)
						winner = Team.Red;
					else if (ScoreBlue > ScoreRed)
						winner = Team.Blue;
					else
						winner = Team.None;
				}
				else if (ScoreRed >= WinScore)
				{
					gameEnded = true;
					winner = Team.Red;
				}
				else if (ScoreBlue >= WinScore)
				{
					gameEnded = true;
					winner = Team.Blue;
				}
				break;
		}

		if (gameEnded)
		{
			EndGame(winner);
		}

		return gameEnded;
	}
}
