using Godot;
using System;

/// <summary>
/// Tor Trigger-Zone die erkennt wenn der Ball eintritt.
/// Meldet Tore an den GameManager und löst Tor-Feier aus.
/// </summary>
public partial class Goal : Area3D
{
	[Export] public GameManager.Team DefendingTeam { get; set; }

	private GameManager _gameManager;
	private AudioManager _audioManager;
	private bool _goalScored = false;  // Verhindert mehrfache Trigger

	/// <summary>
	/// Initialisiert das Tor beim Laden der Scene.
	/// Verbindet Signals und konfiguriert die Trigger-Area.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen abrufen
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// Body Entered Signal verbinden
		BodyEntered += OnBodyEntered;

		// Tor Trigger Area einrichten
		SetupGoalTrigger();

		GD.Print($"Goal {Name} initialized - DefendingTeam: {DefendingTeam}");
	}

	/// <summary>
	/// Richtet die Tor Trigger Collision Area ein.
	/// Konfiguriert Layers und Masken für zuverlässige Ball-Erkennung.
	/// </summary>
	private void SetupGoalTrigger()
	{
		// Area Properties für zuverlässige Erkennung konfigurieren
		Monitorable = true;
		Monitoring = true;

		// Tor Trigger hat keinen physischen Layer, erkennt nur Ball (Layer 2)
		CollisionLayer = 0;
		CollisionMask = 2;  // Ball ist auf Layer 2

		GD.Print($"Goal {Name} trigger configured - CollisionMask: {CollisionMask}");
	}

	/// <summary>
	/// Wird aufgerufen wenn ein Body die Tor Trigger Area betritt.
	/// Erkennt den Ball und meldet das Tor an den GameManager.
	/// </summary>
	private void OnBodyEntered(Node3D body)
	{
		GD.Print($"Goal {Name}: Body entered - {body.Name} (Type: {body.GetType().Name})");

		// Mehrfache Tor-Trigger verhindern
		if (_goalScored)
		{
			GD.Print($"Goal {Name}: Already scored, ignoring");
			return;
		}

		// Prüfen ob der Body der Ball ist
		if (body is Ball ball)
		{
			_goalScored = true;

			// Bestimmen welches Team getroffen hat (Gegenteil des verteidigenden Teams)
			GameManager.Team scoringTeam = DefendingTeam == GameManager.Team.Red
				? GameManager.Team.Blue
				: GameManager.Team.Red;

			GD.Print($"=== GOAL! {scoringTeam} scored in {DefendingTeam}'s goal! ===");

			// Ball kurz stoppen für visuelles Feedback
			ball.LinearVelocity = ball.LinearVelocity * 0.1f;

			// Tor an GameManager melden (der Ball-Reset und Score-Update übernimmt)
			if (_gameManager != null)
			{
				_gameManager.ScoreGoal(scoringTeam);
			}
			else
			{
				GD.PrintErr("GameManager not found! Cannot register goal.");
			}

			// Goal-Scored Flag nach Verzögerung zurücksetzen (um neue Tore nach Reset zu erlauben)
			GetTree().CreateTimer(2.5f).Timeout += () => {
				_goalScored = false;
				GD.Print($"Goal {Name}: Ready for next goal");
			};
		}
	}

	/// <summary>
	/// Wird jeden Physics Frame aufgerufen um Ball-Nähe zu prüfen (optionale Verbesserung).
	/// Erhöht Zuverlässigkeit durch manuelles Prüfen überlappender Bodies.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// Überlappende Bodies manuell prüfen für zusätzliche Zuverlässigkeit
		var bodies = GetOverlappingBodies();
		foreach (var body in bodies)
		{
			if (body is Ball && !_goalScored)
			{
				OnBodyEntered(body);
				break;
			}
		}
	}
}
