using Godot;
using System;

// Verwaltung und Persistierung von Spielerstatistiken
public partial class StatsManager : Node
{
	// Basis-Statistiken
	public int TotalGoalsScored { get; private set; } = 0;
	public int TotalGoalsConceded { get; private set; } = 0;
	public int GamesPlayed { get; private set; } = 0;
	public int GamesWon { get; private set; } = 0;
	public int GamesLost { get; private set; } = 0;
	public int GamesTied { get; private set; } = 0;

	// Aliase für Kompatibilität mit UI Scripts
	public int GoalsScored => TotalGoalsScored;
	public int GoalsConceded => TotalGoalsConceded;

	// Berechnete Statistiken
	public int TotalGoals => TotalGoalsScored + TotalGoalsConceded;
	public float WinRate => GamesPlayed > 0 ? (float)GamesWon / GamesPlayed : 0.0f;
	public float GoalsPerGame => GamesPlayed > 0 ? (float)TotalGoalsScored / GamesPlayed : 0.0f;
	public int GoalDifference => TotalGoalsScored - TotalGoalsConceded;

	// Pfad zur Statistik-Datei
	private const string STATS_PATH = "user://player_stats.json";

	/// <summary>
	/// Initialisiert den StatsManager und lädt gespeicherte Statistiken
	/// </summary>
	public override void _Ready()
	{
		GD.Print("StatsManager initialized");
		LoadStats();
	}

	/// <summary>
	/// Registriert ein erzieltes oder kassiertes Tor
	/// </summary>
	public void RecordGoal(bool scored)
	{
		if (scored)
		{
			TotalGoalsScored++;
			GD.Print($"Goal scored! Total: {TotalGoalsScored}");
		}
		else
		{
			TotalGoalsConceded++;
			GD.Print($"Goal conceded! Total: {TotalGoalsConceded}");
		}

		SaveStats();
	}

	/// <summary>
	/// Registriert das Ergebnis eines Spiels
	/// </summary>
	public void RecordGameResult(bool won, bool tied = false)
	{
		GamesPlayed++;

		if (tied)
		{
			GamesTied++;
			GD.Print($"Game tied! Total ties: {GamesTied}");
		}
		else if (won)
		{
			GamesWon++;
			GD.Print($"Game won! Total wins: {GamesWon}");
		}
		else
		{
			GamesLost++;
			GD.Print($"Game lost! Total losses: {GamesLost}");
		}

		GD.Print($"Games played: {GamesPlayed}, Win rate: {WinRate:P1}");
		SaveStats();
	}

	/// <summary>
	/// Setzt alle Statistiken auf Null zurück
	/// </summary>
	public void ResetAllStats()
	{
		TotalGoalsScored = 0;
		TotalGoalsConceded = 0;
		GamesPlayed = 0;
		GamesWon = 0;
		GamesLost = 0;
		GamesTied = 0;

		GD.Print("All statistics have been reset");
		SaveStats();
	}

	/// <summary>
	/// Alias für ResetAllStats (Kompatibilität mit UI Scripts)
	/// </summary>
	public void ResetStats()
	{
		ResetAllStats();
	}

	/// <summary>
	/// Speichert die Statistiken in eine Datei
	/// </summary>
	public void SaveStats()
	{
		var stats = new Godot.Collections.Dictionary
		{
			{ "total_goals_scored", TotalGoalsScored },
			{ "total_goals_conceded", TotalGoalsConceded },
			{ "games_played", GamesPlayed },
			{ "games_won", GamesWon },
			{ "games_lost", GamesLost },
			{ "games_tied", GamesTied },
			{ "last_updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
		};

		string json = Json.Stringify(stats, "\t");

		using var file = FileAccess.Open(STATS_PATH, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.StoreString(json);
			GD.Print($"Statistics saved to {STATS_PATH}");
		}
		else
		{
			GD.PrintErr($"Failed to save statistics to {STATS_PATH}");
		}
	}

	/// <summary>
	/// Lädt die Statistiken aus einer Datei
	/// </summary>
	public void LoadStats()
	{
		if (!FileAccess.FileExists(STATS_PATH))
		{
			GD.Print("No saved statistics found, starting fresh");
			return;
		}

		using var file = FileAccess.Open(STATS_PATH, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open statistics file: {STATS_PATH}");
			return;
		}

		string jsonString = file.GetAsText();
		var json = new Json();
		var parseResult = json.Parse(jsonString);

		if (parseResult != Error.Ok)
		{
			GD.PrintErr($"Failed to parse statistics JSON: {json.GetErrorMessage()}");
			return;
		}

		var stats = json.Data.AsGodotDictionary();

		// Statistikwerte aus Dictionary auslesen
		if (stats.ContainsKey("total_goals_scored"))
			TotalGoalsScored = (int)stats["total_goals_scored"];

		if (stats.ContainsKey("total_goals_conceded"))
			TotalGoalsConceded = (int)stats["total_goals_conceded"];

		if (stats.ContainsKey("games_played"))
			GamesPlayed = (int)stats["games_played"];

		if (stats.ContainsKey("games_won"))
			GamesWon = (int)stats["games_won"];

		if (stats.ContainsKey("games_lost"))
			GamesLost = (int)stats["games_lost"];

		if (stats.ContainsKey("games_tied"))
			GamesTied = (int)stats["games_tied"];

		GD.Print($"Statistics loaded - Games: {GamesPlayed}, Wins: {GamesWon}, Losses: {GamesLost}, Ties: {GamesTied}");
		GD.Print($"Goals: {TotalGoalsScored} scored, {TotalGoalsConceded} conceded, Difference: {GoalDifference}");
	}

	/// <summary>
	/// Gibt eine formatierte Statistik-Zusammenfassung zurück
	/// </summary>
	public string GetStatsSummary()
	{
		return $@"
=== Player Statistics ===
Games Played: {GamesPlayed}
Wins: {GamesWon} ({WinRate:P1})
Losses: {GamesLost}
Ties: {GamesTied}

Goals Scored: {TotalGoalsScored}
Goals Conceded: {TotalGoalsConceded}
Goal Difference: {GoalDifference:+#;-#;0}
Goals per Game: {GoalsPerGame:F2}
".Trim();
	}

	/// <summary>
	/// Exportiert die Statistiken als Godot Dictionary für UI-Anzeige
	/// </summary>
	public Godot.Collections.Dictionary GetStatsAsDictionary()
	{
		return new Godot.Collections.Dictionary
		{
			{ "games_played", GamesPlayed },
			{ "games_won", GamesWon },
			{ "games_lost", GamesLost },
			{ "games_tied", GamesTied },
			{ "win_rate", WinRate },
			{ "goals_scored", TotalGoalsScored },
			{ "goals_conceded", TotalGoalsConceded },
			{ "goal_difference", GoalDifference },
			{ "goals_per_game", GoalsPerGame }
		};
	}
}
