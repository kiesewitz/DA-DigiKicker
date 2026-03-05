using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// AI Controller für bot-gesteuerte Stangen im DigiKicker Tischfußball.
/// Verwendet ein vereinheitlichtes Aktions-Modell: Bewege ZU Schuss-Position, dann schieße sofort wenn ausgerichtet.
///
/// Zentrale Design-Prinzipien:
/// 1. Lateral + Rotation als EINE Aktion koordinieren (nicht unabhängig)
/// 2. Einfache 5-Zustands-Maschine: Idle -> Tracking -> ReadyToStrike -> Striking -> RescueClear
/// 3. Priorität: "Niemals den Ball verfehlen" - enge Ausrichtung vor Schuss
/// 4. Läuft mit Physics Rate (50Hz) mit minimalem Smoothing
/// 5. Nur EINE Stange kontrolliert den Ball aktiv zur gleichen Zeit (Claiming System)
/// 6. Anti-Stuck-Erkennung löst RescueClear nach 0.35s aus
/// </summary>
public partial class BotController : Node
{
	#region Enums and Constants

	public enum BotDifficulty { Easy, Medium, Hard }

	/// <summary>
	/// Vereinheitlichter Aktions-Zustand für jede Stange.
	/// </summary>
	private enum RodAction
	{
		Idle,           // Ball weit weg, Stange kehrt zur Mitte zurück
		Tracking,       // Bewegt sich zu Abfang-Position
		ReadyToStrike,  // Ausgerichtet und wartet dass Ball in Schuss-Reichweite kommt
		Striking,       // Führt Schuss-Bewegung aus
		RescueClear     // Notfall-Unstick-Verhalten
	}

	// Physics-Konstanten (aus Rod.cs)
	private const float ROTATION_SPEED = 10.0f;
	private const float LATERAL_SPEED = 3.5f;

	// Schuss-Timing
	private const float STRIKE_DURATION = 0.12f;  // Wie lange ein Schuss dauert
	private const float STRIKE_RECOVERY = 0.08f;  // Kurze Pause nach Schuss
	private const float RESCUE_CLEAR_DURATION = 0.18f;  // Dauer des Rescue Clear

	// Ausrichtungs-Schwellen (skaliert nach Schwierigkeit)
	private const float BASE_ALIGNMENT_TOLERANCE = 0.045f;  // Medium Schwierigkeit Baseline
	private const float STRIKE_RANGE_X = 0.18f;  // Wie nah Ball auf X sein muss um zu schießen
	private const float STRIKE_RANGE_X_SLOW = 0.35f;  // Erweiterte Reichweite für langsame Bälle
	// Schuss-Geometrie (Torso vs. Füße)
	private const float LEG_BASE_OFFSET_Z = 0.02f;           // Torso zu Midfeet Offset (in Tisch Z)
	private const float LEG_SWING_AMPLITUDE_Z = 0.11f;       // Wie weit Füße seitlich mit Stangenrotation verschieben
	private const float LEG_CONTACT_HALF_WIDTH = 0.07f;      // Effektive halbe Breite der Schuss-Oberfläche
	private const float LEG_SWEEP_PADDING = 0.015f;          // Extra Rand um Sweep während Schwung abzudecken
	private const float EXPECTED_STRIKE_ROTATION = 0.7f;     // Ungefähre Radiant die Stange vorwärts während Schuss zurücklegt

	// Anti-Stuck-Konstanten
	private const float STUCK_SPEED_THRESHOLD = 0.08f;
	private const float STUCK_MOVEMENT_THRESHOLD = 0.002f;
	private const float STUCK_TIME_THRESHOLD = 0.35f;

	// Figuren-Locking Hysterese
	private const float FIGURE_LOCK_BALL_MOVE_THRESHOLD = 0.08f;
	private const float FIGURE_LOCK_DISTANCE_MULT = 0.6f;

	// Debug-Ausgabe-Steuerung - auf false setzen für sauberere Trainings-Ausgabe
	private const bool DEBUG_VERBOSE = false;

	#endregion

	#region Properties and State

	[Export] public BotDifficulty Difficulty { get; set; } = BotDifficulty.Medium;
	[Export] public GameManager.Team ControlledTeam { get; set; } = GameManager.Team.Blue;

	/// <summary>
	/// Zustandscontainer pro Stange.
	/// </summary>
	private class RodState
	{
		public RodAction Action = RodAction.Idle;
		public float ActionTimer = 0f;
		public int TargetFigureIndex = -1;  // -1 = nicht initialisiert
		public float LastInputX = 0f;  // Für Smoothing
		public float LastBallZ = 0f;   // Für Figuren-Locking Hysterese
		public bool FigureLocked = false;
	}

	// Schwierigkeits-skalierte Parameter
	private float _reactionDelay = 0.08f;
	private float _predictionFactor = 0.5f;
	private float _alignmentTolerance = BASE_ALIGNMENT_TOLERANCE;
	private float _trackingSpeedMult = 1.0f;
	private float _randomMissChance = 0.05f;

	// Referenzen
	private List<Rod> _controlledRods = new List<Rod>();
	private Dictionary<Rod, RodState> _rodStates = new Dictionary<Rod, RodState>();
	private Node3D _ball;
	private RigidBody3D _ballRigidBody;
	private GameManager _gameManager;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	// Ball-Tracking
	private Vector3 _ballPos;
	private Vector3 _ballVelocity;
	private float _ballSpeed;

	// Anti-Stuck-Erkennung (global)
	private Vector3 _lastStuckCheckPos;
	private float _stuckTimer = 0f;
	private Rod _rescueRod = null;  // Die Stange die gerade Rescue Clear ausführt
	private float _rescueTimer = 0f;

	// Multi-Stangen-Claiming
	private Rod _claimedRod = null;  // Die Stange die aktuell den Ball "besitzt"

	#endregion

	#region Lifecycle

	public override void _Ready()
	{
		_rng.Randomize();
		ConfigureDifficulty();

		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (_gameManager == null)
			GD.PrintErr("BotController: GameManager not found!");

		CallDeferred(nameof(FindBall));
		GD.Print($"BotController initialized - Team: {ControlledTeam}, Difficulty: {Difficulty}");
	}

	private void ConfigureDifficulty()
	{
		switch (Difficulty)
		{
			case BotDifficulty.Easy:
				_reactionDelay = 0.15f;
				_predictionFactor = 0.2f;
				_alignmentTolerance = 0.06f;
				_trackingSpeedMult = 0.7f;
				_randomMissChance = 0.15f;
				break;

			case BotDifficulty.Medium:
				_reactionDelay = 0.08f;
				_predictionFactor = 0.5f;
				_alignmentTolerance = 0.045f;
				_trackingSpeedMult = 1.0f;
				_randomMissChance = 0.05f;
				break;

			case BotDifficulty.Hard:
				_reactionDelay = 0.02f;
				_predictionFactor = 0.9f;
				_alignmentTolerance = 0.03f;
				_trackingSpeedMult = 1.2f;
				_randomMissChance = 0f;
				break;
		}
	}

	private void FindBall()
	{
		_ball = GetTree().Root.FindChild("Ball", true, false) as Node3D;
		_ballRigidBody = _ball as RigidBody3D;

		if (_ball == null)
			GD.PrintErr("BotController: Ball not found!");
		else
		{
			GD.Print($"BotController: Ball found at {_ball.GlobalPosition}");
			_lastStuckCheckPos = _ball.GlobalPosition;
		}
	}

	#endregion

	#region Rod Management

	public void RegisterRod(Rod rod)
	{
		if (!_controlledRods.Contains(rod))
		{
			_controlledRods.Add(rod);
			_rodStates[rod] = new RodState();
			GD.Print($"BotController: Registered {rod.Team} {rod.Type}");
		}
	}

	public void UnregisterRod(Rod rod)
	{
		_controlledRods.Remove(rod);
		_rodStates.Remove(rod);
		if (_claimedRod == rod) _claimedRod = null;
		if (_rescueRod == rod) _rescueRod = null;
	}

	public List<Rod> GetControlledRods() => new List<Rod>(_controlledRods);

	public void ClearRods()
	{
		_controlledRods.Clear();
		_rodStates.Clear();
		_claimedRod = null;
		_rescueRod = null;
	}

	#endregion

	#region Main Physics Loop (50Hz)

	public override void _PhysicsProcess(double delta)
	{
		if (_ball == null || _controlledRods.Count == 0)
			return;

		// Nicht verarbeiten während Countdown, Pause oder Warten auf Countdown nach Tor
		bool shouldPause = _gameManager != null && (
			_gameManager.CurrentState != GameManager.GameState.Playing ||
			_gameManager.IsWaitingForCountdown ||
			_gameManager.IsCountdownActive
		);

		if (shouldPause)
		{
			foreach (var rod in _controlledRods)
			{
				rod?.SetBotInput(Vector2.Zero);
				// Smoothing-Zustand zurücksetzen um Bewegung beim Wiederaufnehmen zu verhindern
				if (rod != null && _rodStates.TryGetValue(rod, out var state))
				{
					state.LastInputX = 0f;
					state.Action = RodAction.Idle;
					state.ActionTimer = 0f;
				}
			}
			// Stuck-Erkennung zurücksetzen
			_stuckTimer = 0f;
			_rescueRod = null;
			_rescueTimer = 0f;
			_claimedRod = null;
			return;
		}

		float dt = (float)delta;

		// Ball-Zustand aktualisieren
		UpdateBallState();

		// Anti-Stuck-Erkennung aktualisieren
		UpdateStuckDetection(dt);

		// Bestimmen welche Stange den Ball beansprucht (Multi-Rod Contention Fix)
		UpdateRodClaiming();

		// Jede Stange verarbeiten
		foreach (var rod in _controlledRods)
		{
			if (rod == null) continue;

			var state = _rodStates[rod];
			Vector2 input;

			// Prüfen ob diese Stange im Rescue-Modus ist
			if (_rescueRod == rod && _rescueTimer > 0)
			{
				input = CalculateRescueClearInput(rod, state, dt);
			}
			// Prüfen ob diese Stange die beanspruchte Stange ist (oder kein Claiming aktiv)
			else if (IsBallInRodZone(rod) && (rod == _claimedRod || _claimedRod == null))
			{
				input = CalculateUnifiedAction(rod, state, dt);
			}
			else
			{
				// Ball nicht in Zone ODER andere Stange hat ihn beansprucht - zur Mitte zurückkehren
				input = CalculateIdleInput(rod, state);
				// Nur auf Idle zurücksetzen wenn nicht im Rescue-Modus
				if (state.Action != RodAction.RescueClear)
					state.Action = RodAction.Idle;
			}

			// Input anwenden mit minimalem lateralem Smoothing, KEIN Rotations-Smoothing
			float smoothedX = Mathf.Lerp(state.LastInputX, input.X, 0.7f);
			state.LastInputX = smoothedX;

			rod.SetBotInput(new Vector2(smoothedX, input.Y));
		}
	}

	private void UpdateBallState()
	{
		_ballPos = _ball.GlobalPosition;
		if (_ballRigidBody != null)
			_ballVelocity = _ballRigidBody.LinearVelocity;
		_ballSpeed = _ballVelocity.Length();
	}

	#endregion

	#region Anti-Stuck Detection

	private void UpdateStuckDetection(float delta)
	{
		// Rescue-Timer aktualisieren falls aktiv
		if (_rescueRod != null && _rescueTimer > 0)
		{
			_rescueTimer -= delta;
			if (_rescueTimer <= 0)
			{
				// Rescue abgeschlossen
				if (_rodStates.TryGetValue(_rescueRod, out var state))
				{
					state.Action = RodAction.Tracking;
					state.ActionTimer = 0f;
				}
				_rescueRod = null;
				_stuckTimer = 0f;  // Stuck-Timer nach Rescue zurücksetzen
			}
			return;  // Nicht nach neuem Stuck prüfen während Rescue läuft
		}

		// Prüfen ob Ball stecken geblieben ist
		float distMoved = _ballPos.DistanceTo(_lastStuckCheckPos);

		if (_ballSpeed < STUCK_SPEED_THRESHOLD && distMoved < STUCK_MOVEMENT_THRESHOLD)
		{
			_stuckTimer += delta;

			if (_stuckTimer > STUCK_TIME_THRESHOLD)
			{
				// Ball steckt fest! Nächste Stange für Rescue finden
				TriggerRescueClear();
			}
		}
		else
		{
			// Ball bewegt sich - Stuck-Erkennung zurücksetzen
			_stuckTimer = 0f;
			_lastStuckCheckPos = _ballPos;
		}
	}

	private void TriggerRescueClear()
	{
		// Die kontrollierte Stange finden die dem Ball am nächsten ist (nach X-Distanz)
		Rod nearestRod = null;
		float nearestDist = float.MaxValue;

		foreach (var rod in _controlledRods)
		{
			if (rod == null) continue;
			float dist = Mathf.Abs(_ballPos.X - rod.GlobalPosition.X);
			if (dist < nearestDist)
			{
				nearestDist = dist;
				nearestRod = rod;
			}
		}

		if (nearestRod != null && nearestDist < 1.5f)  // Nur wenn einigermaßen nah
		{
			_rescueRod = nearestRod;
			_rescueTimer = RESCUE_CLEAR_DURATION;

			if (_rodStates.TryGetValue(nearestRod, out var state))
			{
				state.Action = RodAction.RescueClear;
				state.ActionTimer = 0f;
			}

			if (DEBUG_VERBOSE)
				GD.Print($"BotController: Triggering RescueClear on {nearestRod.Type}");
		}
		else
		{
			// Keine Stange nah genug, nur Timer zurücksetzen und hoffen dass Physics es regelt
			_stuckTimer = 0f;
			_lastStuckCheckPos = _ballPos;
		}
	}

	private Vector2 CalculateRescueClearInput(Rod rod, RodState state, float delta)
	{
		Vector3 rodPos = rod.GlobalPosition;
		float ballRelativeZ = _ballPos.Z - rodPos.Z;
		float kickDir = ControlledTeam == GameManager.Team.Red ? 1.0f : -1.0f;

		Vector2 input = Vector2.Zero;

		// LATERAL: Sehr aggressive Ausrichtung mit entspannter Toleranz
		float currentOffset = rod.CurrentLateralOffset;
		float lateralError = ballRelativeZ - currentOffset;

		// Weitere Toleranz für Rescue verwenden - nur nah genug kommen
		const float RESCUE_TOLERANCE = 0.2f;
		if (Mathf.Abs(lateralError) > RESCUE_TOLERANCE)
		{
			// Aggressive Bewegung zum Ball
			input.X = Mathf.Sign(lateralError) * 1.0f;
		}
		else
		{
			// Kleine Kratz-Bewegung hinzufügen um Ball zu lösen
			float scrapePhase = state.ActionTimer * 25f;  // Schnelle Oszillation
			input.X = Mathf.Sin(scrapePhase) * 0.4f;
		}

		// ROTATION: Volle Kraft Schuss
		state.ActionTimer += delta;
		if (state.ActionTimer < STRIKE_DURATION)
		{
			input.Y = kickDir * 1.2f;  // Volle Kraft
		}
		else
		{
			// Kurze Erholung dann wieder schießen
			float cycleTime = (state.ActionTimer - STRIKE_DURATION) % (STRIKE_DURATION + 0.05f);
			if (cycleTime < STRIKE_DURATION)
				input.Y = kickDir * 1.2f;
			else
				input.Y = -kickDir * 0.3f;
		}

		return input;
	}

	#endregion

	#region Multi-Rod Claiming

	private void UpdateRodClaiming()
	{
		// Alle Stangen finden die potenziell den Ball kontrollieren könnten
		List<(Rod rod, float dist)> candidateRods = new List<(Rod, float)>();

		foreach (var rod in _controlledRods)
		{
			if (rod == null) continue;
			if (IsBallInRodZone(rod))
			{
				float dist = Mathf.Abs(_ballPos.X - rod.GlobalPosition.X);
				candidateRods.Add((rod, dist));
			}
		}

		if (candidateRods.Count == 0)
		{
			_claimedRod = null;
			return;
		}

		// Nach Distanz sortieren und nächste auswählen
		candidateRods.Sort((a, b) => a.dist.CompareTo(b.dist));
		_claimedRod = candidateRods[0].rod;
	}

	#endregion

	#region Zone Detection

	private bool IsBallInRodZone(Rod rod)
	{
		var (minX, maxX) = GetRodZoneBounds(rod);
		float ballX = _ballPos.X;

		// Kleinen Überlappungspuffer hinzufügen um tote Zonen zu verhindern
		const float OVERLAP = 0.15f;
		return ballX >= minX - OVERLAP && ballX <= maxX + OVERLAP;
	}

	private (float minX, float maxX) GetRodZoneBounds(Rod rod)
	{
		float rodX = rod.GlobalPosition.X;
		bool isRed = rod.Team == GameManager.Team.Red;

		// Zonengrenzen basierend auf Stangentyp und Team
		// ERWEITERTE Zonen um Abdeckung sicherzustellen, besonders für Torwart
		return rod.Type switch
		{
			// Torwart: Erweiterte Zone um Bälle nahe/neben Tor abzudecken
			Rod.RodType.Goalkeeper => isRed ? (-6.0f, -3.0f) : (3.0f, 6.0f),
			// Verteidigung: Weitere Zone für bessere Abdeckung
			Rod.RodType.Defense => isRed ? (-3.5f, -2.0f) : (2.0f, 3.5f),
			// Mittelfeld: Überlappende Zonen für Mittelkontrolle
			Rod.RodType.Midfield => isRed ? (-1.8f, 0.8f) : (-0.8f, 1.8f),
			// Angriff: Erweitert für Druck
			Rod.RodType.Attack => isRed ? (1.2f, 3.5f) : (-3.5f, -1.2f),
			_ => (-6.0f, 6.0f)
		};
	}

	#endregion

	#region Unified Action Calculation

	private Vector2 CalculateUnifiedAction(Rod rod, RodState state, float delta)
	{
		// Torwart hat spezielle Logik
		if (rod.Type == Rod.RodType.Goalkeeper)
			return CalculateGoalkeeperAction(rod, state, delta);

		Vector3 rodPos = rod.GlobalPosition;
		float ballRelativeZ = _ballPos.Z - rodPos.Z;
		float ballDistanceX = _ballPos.X - rodPos.X;
		float absBallDistanceX = Mathf.Abs(ballDistanceX);
		bool ballSlow = _ballSpeed < 0.5f;
		bool ballStationary = _ballSpeed < 0.15f;
		bool ballVerySlow = _ballSpeed < 0.2f;
		float kickDir = ControlledTeam == GameManager.Team.Red ? 1.0f : -1.0f;

		// Figurenpositionen holen und bestes Ziel MIT HYSTERESE finden
		float[] figureZs = rod.GetFigureZPositions();
		int bestFigure = FindBestFigureWithHysteresis(rod, state, figureZs, ballRelativeZ);
		state.TargetFigureIndex = bestFigure;
		float targetFigureZ = figureZs[bestFigure];

		// Aktueller Ausrichtungsfehler (wie weit ist Ball von nächster Figur entfernt)
		float currentRodOffset = rod.CurrentLateralOffset;
		var kickZone = CalculateKickZone(rod, targetFigureZ, kickDir);
		float kickCenterWorldZ = currentRodOffset + kickZone.centerLocal;
		float alignmentError = CalculateKickAlignmentError(ballRelativeZ, kickCenterWorldZ, kickZone.halfWidth);

		// Vorhersagen wo Ball sein wird
		float predictedZ = ballRelativeZ;
		if (IsBallApproaching(rod) && _ballSpeed > 0.5f)
		{
			float timeToRod = absBallDistanceX / Mathf.Max(Mathf.Abs(_ballVelocity.X), 0.1f);
			timeToRod = Mathf.Min(timeToRod, 1.0f);
			predictedZ = ballRelativeZ + _ballVelocity.Z * timeToRod * _predictionFactor;
		}

		// Für langsame/stationäre Bälle direkt verfolgen (keine Vorhersage)
		if (ballSlow && absBallDistanceX < 1.0f)
		{
			predictedZ = ballRelativeZ;
		}

		// Dynamische Schussreichweite und Ausrichtungstoleranz für langsame Bälle
		float effectiveStrikeRange = STRIKE_RANGE_X;
		float effectiveAlignTolerance = _alignmentTolerance;

		if (ballVerySlow)
		{
			effectiveStrikeRange = STRIKE_RANGE_X_SLOW;
			effectiveAlignTolerance = _alignmentTolerance * 3.5f;
		}
		else if (ballSlow)
		{
			effectiveStrikeRange = Mathf.Lerp(STRIKE_RANGE_X, STRIKE_RANGE_X_SLOW, 0.5f);
			effectiveAlignTolerance = _alignmentTolerance * 2.0f;
		}

		// Zustandsmaschine mit dynamischen Schwellenwerten aktualisieren
		UpdateRodState(rod, state, absBallDistanceX, alignmentError, delta, effectiveStrikeRange, effectiveAlignTolerance);

		// Input basierend auf Zustand generieren
		Vector2 input = Vector2.Zero;

		// LATERAL: Immer versuchen auszurichten (außer während Schuss)
		if (state.Action != RodAction.Striking)
		{
			float targetZ = predictedZ - kickZone.centerLocal;  // Füße (nicht Torso) zu vorhergesagtem Ball ausrichten

			// Auf maximal erreichbare Position begrenzen
			float maxOffset = rod.MaxLateralOffset;
			targetZ = Mathf.Clamp(targetZ, -maxOffset, maxOffset);

			float lateralError = targetZ - currentRodOffset;

			// Aggressiveres Tracking für langsame/stationäre Bälle
			float speedBoost = 1.0f;
			if (ballStationary && absBallDistanceX < 0.6f)
			{
				speedBoost = 1.5f;  // Sehr aggressiv
			}
			else if (ballSlow && absBallDistanceX < 0.8f)
			{
				speedBoost = 1.3f;  // Aggressiv
			}

			input.X = CalculateLateralInput(lateralError, state.Action) * speedBoost;
		}

		// ROTATION: Basierend auf aktuellem Zustand
		input.Y = CalculateRotationInput(rod, state, absBallDistanceX, ballDistanceX);

		// Extra: Für sehr langsame Bälle die nah sind, aggressiver mit Schüssen sein
		if (ballStationary && absBallDistanceX < 0.3f && alignmentError < effectiveAlignTolerance)
		{
			input.Y = kickDir * 1.0f;  // Schuss erzwingen
		}

		return input;
	}

	private int FindBestFigureWithHysteresis(Rod rod, RodState state, float[] figureZs, float ballZ)
	{
		if (figureZs.Length == 1)
		{
			state.FigureLocked = true;
			state.LastBallZ = ballZ;
			return 0;
		}

		float figureSpacing = rod.GetFigureSpacing();
		float currentOffset = rod.CurrentLateralOffset;

		// Prüfen ob wir die aktuell gelockte Figur behalten sollten
		if (state.FigureLocked && state.TargetFigureIndex >= 0 && state.TargetFigureIndex < figureZs.Length)
		{
			float ballMoved = Mathf.Abs(ballZ - state.LastBallZ);
			float lockedFigureWorldZ = currentOffset + figureZs[state.TargetFigureIndex];
			float distToLockedFigure = Mathf.Abs(ballZ - lockedFigureWorldZ);

			// Gelockte Figur behalten wenn:
			// (a) Ball hat sich nicht viel bewegt UND
			// (b) Ball ist noch einigermaßen nah an gelockter Figur
			bool ballDidntMoveEnough = ballMoved < FIGURE_LOCK_BALL_MOVE_THRESHOLD;
			bool ballStillClose = distToLockedFigure < figureSpacing * FIGURE_LOCK_DISTANCE_MULT;

			if (ballDidntMoveEnough && ballStillClose)
			{
				// Gelockte Figur behalten
				return state.TargetFigureIndex;
			}
		}

		// Neue Figur auswählen
		state.LastBallZ = ballZ;
		state.FigureLocked = true;

		// Nächste Figur finden
		int nearestIdx = 0;
		float nearestDist = float.MaxValue;

		for (int i = 0; i < figureZs.Length; i++)
		{
			float worldZ = currentOffset + figureZs[i];
			float dist = Mathf.Abs(ballZ - worldZ);
			if (dist < nearestDist)
			{
				nearestDist = dist;
				nearestIdx = i;
			}
		}

		// Wenn Ball zwischen Figuren ist, basierend auf Angriffsrichtung wählen
		if (figureSpacing > 0 && nearestDist > figureSpacing * 0.35f)
		{
			int lowerIdx = nearestIdx > 0 ? nearestIdx - 1 : nearestIdx;
			int upperIdx = nearestIdx < figureZs.Length - 1 ? nearestIdx + 1 : nearestIdx;

			if (ControlledTeam == GameManager.Team.Red)
			{
				return upperIdx;
			}
			else
			{
				return lowerIdx;
			}
		}

		return nearestIdx;
	}

	private void UpdateRodState(Rod rod, RodState state, float ballDistX, float alignError, float delta,
		float effectiveStrikeRange, float effectiveAlignTolerance)
	{
		bool isAligned = alignError < effectiveAlignTolerance;
		bool isInStrikeRange = ballDistX < effectiveStrikeRange;
		bool ballApproaching = IsBallApproaching(rod);

		switch (state.Action)
		{
			case RodAction.Idle:
				// Tracking starten wenn Ball einigermaßen nah ist
				if (ballDistX < 1.5f || (ballApproaching && ballDistX < 2.5f))
				{
					state.Action = RodAction.Tracking;
					state.ActionTimer = 0f;
				}
				break;

			case RodAction.Tracking:
				state.ActionTimer += delta;
				// Auf Reaktionsverzögerung warten bevor Schuss erlaubt wird
				if (state.ActionTimer >= _reactionDelay && isAligned)
				{
					state.Action = RodAction.ReadyToStrike;
					state.ActionTimer = 0f;
				}
				break;

			case RodAction.ReadyToStrike:
				// Prüfen ob wir schießen sollten
				if (isInStrikeRange && isAligned)
				{
					// Zufällige Fehlschuss-Chance für Easy/Medium
					if (_rng.Randf() >= _randomMissChance)
					{
						state.Action = RodAction.Striking;
						state.ActionTimer = 0f;
					}
				}
				else if (!isAligned)
				{
					// Ausrichtung verloren, zurück zu Tracking
					state.Action = RodAction.Tracking;
					state.ActionTimer = _reactionDelay * 0.5f;  // Teilweise Verzögerung
				}
				break;

			case RodAction.Striking:
				state.ActionTimer += delta;
				if (state.ActionTimer >= STRIKE_DURATION + STRIKE_RECOVERY)
				{
					state.Action = RodAction.Tracking;
					state.ActionTimer = 0f;
				}
				break;

			case RodAction.RescueClear:
				// Wird von Rescue-Timer in Main Loop behandelt
				break;
		}
	}

	private float CalculateLateralInput(float error, RodAction action)
	{
		float absError = Mathf.Abs(error);

		// Totzone um Zittern zu verhindern
		if (absError < 0.015f)
			return 0f;

		// Geschwindigkeit basierend auf Aktion und Fehlergröße
		float speedMult = action switch
		{
			RodAction.Idle => 0.4f,
			RodAction.Tracking => 1.0f,
			RodAction.ReadyToStrike => 0.6f,  // Nur sanfte Korrekturen
			_ => 0f
		};

		// Proportionale Steuerung mit Geschwindigkeitsbegrenzung
		float speed = Mathf.Min(absError * 8f, 1.0f) * speedMult * _trackingSpeedMult;
		return Mathf.Sign(error) * speed;
	}

	private float CalculateRotationInput(Rod rod, RodState state, float ballDistX, float signedBallDistX)
	{
		// Schussrichtung: Blue schießt links (-X), Red schießt rechts (+X)
		float kickDir = ControlledTeam == GameManager.Team.Red ? 1.0f : -1.0f;

		// Prüfen ob Ball vor der Stange ist (nicht dahinter)
		bool ballInFront = ControlledTeam == GameManager.Team.Blue
			? signedBallDistX <= 0.05f
			: signedBallDistX >= -0.05f;

		switch (state.Action)
		{
			case RodAction.Idle:
				// Neutrale Position
				return 0f;

			case RodAction.Tracking:
				// Leichte Bereitschaftsposition (Figuren leicht zurück)
				return -kickDir * 0.15f;

			case RodAction.ReadyToStrike:
				// Ausholposition (Figuren weiter zurück, bereit zum Schwung)
				if (ballDistX < 0.3f && ballInFront)
					return -kickDir * 0.4f;
				return -kickDir * 0.2f;

			case RodAction.Striking:
				// VOLLE KRAFT SCHUSS - kein Smoothing in Main Loop angewendet
				if (state.ActionTimer < STRIKE_DURATION)
					return kickDir * 1.2f;  // Leicht über 1.0 für aggressiven Schuss
				// Erholung: zur Neutralen zurückkehren
				return -kickDir * 0.3f;

			case RodAction.RescueClear:
				// Separat behandelt
				return 0f;

			default:
				return 0f;
		}
	}

	#endregion

	#region Goalkeeper Special Logic

	private Vector2 CalculateGoalkeeperAction(Rod rod, RodState state, float delta)
	{
		Vector3 rodPos = rod.GlobalPosition;
		float ballRelativeZ = _ballPos.Z - rodPos.Z;
		float ballDistanceX = Mathf.Abs(_ballPos.X - rodPos.X);
		bool ballApproaching = IsBallApproaching(rod);
		bool ballSlow = _ballSpeed < 0.5f;
		bool ballStationary = _ballSpeed < 0.15f;

		Vector2 input = Vector2.Zero;

		// Ballkreuzungspunkt für schnelle Schüsse vorhersagen
		float targetZ = ballRelativeZ;
		if (ballApproaching && _ballSpeed > 1.0f)
		{
			float timeToRod = ballDistanceX / Mathf.Max(Mathf.Abs(_ballVelocity.X), 0.1f);
			timeToRod = Mathf.Min(timeToRod, 0.8f);
			targetZ = ballRelativeZ + _ballVelocity.Z * timeToRod * _predictionFactor;
		}

		// Für langsame/stationäre Bälle nahe dem Tor direkt verfolgen
		if (ballSlow && ballDistanceX < 1.5f)
		{
			targetZ = ballRelativeZ;
		}

		// TORWART: Keine Begrenzung - Bot jede Position erreichen lassen
		// Die physischen Stangenlimits begrenzen die tatsächliche Bewegung natürlich
		// Dies stellt sicher dass Torwart sich immer zum Ball bewegt, auch in Ecken

		// LATERAL: Ball/Vorhersage verfolgen - AGGRESSIVER für langsame Bälle
		float lateralError = targetZ - rod.CurrentLateralOffset;

		// Dringlichkeit basierend auf Situation
		float urgency;
		if (ballStationary && ballDistanceX < 1.0f)
		{
			urgency = 1.5f;  // Sehr aggressiv für stationäre Bälle in der Nähe
		}
		else if (ballSlow && ballDistanceX < 1.5f)
		{
			urgency = 1.2f;  // Aggressiv für langsame Bälle
		}
		else if (ballApproaching)
		{
			urgency = Mathf.Clamp(_ballSpeed / 3.0f, 0.8f, 1.5f);
		}
		else
		{
			urgency = 0.8f;
		}

		if (Mathf.Abs(lateralError) > 0.015f)
		{
			input.X = Mathf.Sign(lateralError) * Mathf.Min(Mathf.Abs(lateralError) * 8f, 1.0f) * urgency;
		}

		// ROTATION: Blocken oder Klären
		float kickDir = ControlledTeam == GameManager.Team.Red ? 1.0f : -1.0f;
		bool ballClose = ballDistanceX < 0.5f;
		bool ballVeryClose = ballDistanceX < 0.25f;
		bool aligned = Mathf.Abs(lateralError) < 0.1f;

		if (ballVeryClose && aligned)
		{
			// Ball sehr nah und ausgerichtet - SOFORT KLÄREN
			input.Y = kickDir * 1.2f;
		}
		else if (ballClose && aligned && (ballStationary || !ballApproaching))
		{
			// Ball nah, ausgerichtet und langsam/stationär - KLÄREN
			input.Y = kickDir * 1.0f;
		}
		else if (ballClose && ballApproaching)
		{
			// Ball kommt schnell - BLOCKEN (Figuren in Blockposition halten)
			input.Y = 0f;
		}
		else if (ballDistanceX < 0.8f)
		{
			// Ball in der Nähe - Bereitschaftsposition
			input.Y = -kickDir * 0.25f;
		}
		else
		{
			// Standard-Bereitschaftsposition
			input.Y = -kickDir * 0.15f;
		}

		return input;
	}

	#endregion

	#region Helper Methods

	private bool IsBallApproaching(Rod rod)
	{
		float rodX = rod.GlobalPosition.X;
		float ballX = _ballPos.X;
		float velX = _ballVelocity.X;

		if (ControlledTeam == GameManager.Team.Blue)
		{
			// Blue-Stange ist auf rechter Seite, Ball nähert sich wenn er sich nach rechts bewegt (+X)
			return velX > 0.3f && ballX < rodX;
		}
		else
		{
			// Red-Stange ist auf linker Seite, Ball nähert sich wenn er sich nach links bewegt (-X)
			return velX < -0.3f && ballX > rodX;
		}
	}

	private Vector2 CalculateIdleInput(Rod rod, RodState state)
	{
		// Langsam zur Mitte zurückkehren
		float currentOffset = rod.CurrentLateralOffset;
		float lateralInput = 0f;

		if (Mathf.Abs(currentOffset) > 0.03f)
		{
			lateralInput = -Mathf.Sign(currentOffset) * 0.3f;
		}

		// Figuren-Lock zurücksetzen wenn im Idle
		state.FigureLocked = false;

		return new Vector2(lateralInput, 0f);
	}

	#endregion

	#region Kick Alignment Helpers

	private (float centerLocal, float halfWidth) CalculateKickZone(Rod rod, float figureLocalZ, float kickDir)
	{
		// Aktuelle Rotation plus geschätzten Vorwärtsschwung-Winkel verwenden um Schwungpfad zu erfassen
		float rotationNow = NormalizeAngle(rod.Rotation.Z);
		float rotationAtImpact = NormalizeAngle(rotationNow + kickDir * EXPECTED_STRIKE_ROTATION);

		float offsetNow = CalculateLegCenterOffset(rotationNow);
		float offsetImpact = CalculateLegCenterOffset(rotationAtImpact);

		float minOffset = Mathf.Min(offsetNow, offsetImpact);
		float maxOffset = Mathf.Max(offsetNow, offsetImpact);

		float sweepCenter = (minOffset + maxOffset) * 0.5f;
		float sweepHalfWidth = (maxOffset - minOffset) * 0.5f;

		float centerLocal = figureLocalZ + sweepCenter;
		float halfWidth = LEG_CONTACT_HALF_WIDTH + sweepHalfWidth + LEG_SWEEP_PADDING;

		return (centerLocal, halfWidth);
	}

	private float CalculateLegCenterOffset(float rotation)
	{
		float normalized = NormalizeAngle(rotation);
		// Füße schwingen seitlich mit Stangenrotation; Sinus erfasst lateralen Offset entlang Z
		return LEG_BASE_OFFSET_Z + Mathf.Sin(normalized) * LEG_SWING_AMPLITUDE_Z;
	}

	private float CalculateKickAlignmentError(float ballRelativeZ, float kickCenterWorldZ, float kickHalfWidth)
	{
		float delta = Mathf.Abs(ballRelativeZ - kickCenterWorldZ);
		return delta <= kickHalfWidth ? 0f : delta - kickHalfWidth;
	}

	private float NormalizeAngle(float angle)
	{
		// Winkel in [-pi, pi] halten um Drift zu vermeiden
		return Mathf.Wrap(angle + Mathf.Pi, 0.0f, Mathf.Tau) - Mathf.Pi;
	}

	#endregion
}
