using Godot;
using System;

/// <summary>
/// Physik-Controller für den Ball mit realistischem Verhalten durch Godot Physics Engine
/// Verwendet ausschließlich Impulse, keine direkte Positionsmanipulation
/// </summary>
public partial class Ball : RigidBody3D
{
	private const float SCALE = 5.0f;

	// Physikalische Ball-Eigenschaften
	private const float BALL_RADIUS = 0.035f * SCALE;
	private const float BALL_MASS = 0.1f;

	// Physics Material Einstellungen
	private const float BALL_BOUNCE = 0.7f;
	private const float BALL_FRICTION = 0.08f;

	// Dämpfung für kontrollierte Bewegung
	private const float LINEAR_DAMP = 0.5f;
	private const float ANGULAR_DAMP = 0.6f;

	// Geschwindigkeitsgrenzen
	private const float MAX_VELOCITY = 15.0f;
	private const float MIN_VELOCITY_THRESHOLD = 0.02f;

	// Tisch-Dimensionen für Reset-Position und Grenzen
	private const float TABLE_HEIGHT = 0.1f * SCALE;
	private const float RESET_Y = TABLE_HEIGHT / 2f + BALL_RADIUS + 0.05f;
	private const float TABLE_LENGTH = 2.4f * SCALE;
	private const float TABLE_WIDTH = 1.2f * SCALE;

	// Node-Referenzen
	private MeshInstance3D _ballMesh;
	private CollisionShape3D _ballCollider;
	private GpuParticles3D _ballTrail;
	private GameManager _gameManager;
	private AudioManager _audioManager;

	// Zustandsverfolgung
	private bool _waitingForCountdown = false;
	private float _lastSpeed = 0.0f;

	// Schuss-Immunität nach Reset
	private const float KICK_IMMUNITY_DURATION = 0.5f;
	private float _kickImmunityTimer = 0.0f;

	// Sound Cooldown
	private const float SOUND_COOLDOWN = 0.05f;

	// Debug-Ausgabe
	private const bool DEBUG_VERBOSE = false;
	private float _soundCooldownTimer = 0.0f;

	// Automatischer Anstoß bei festgefahrenem Ball
	private const float STUCK_NUDGE_TIME = 10.0f;
	private const float STUCK_THRESHOLD = 0.05f;
	private float _stuckTimer = 0.0f;
	private Vector3 _lastPositionForStuck;
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	/// <summary>
	/// Initialisiert den Ball mit Physik-Setup und Signal-Verbindungen
	/// </summary>
	public override void _Ready()
	{
		// Node-Referenzen holen
		_ballMesh = GetNode<MeshInstance3D>("BallMesh");
		_ballCollider = GetNode<CollisionShape3D>("BallCollider");
		_ballTrail = GetNodeOrNull<GpuParticles3D>("BallTrail");
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// Ball-Komponenten einrichten
		SetupBallPhysics();
		SetupBallMesh();
		SetupBallCollider();

		// GameManager Signale verbinden
		if (_gameManager != null)
		{
			_gameManager.BallResetRequested += OnBallResetRequested;
			_gameManager.BallKickoffRequested += OnBallKickoffRequested;
		}

		// Kollisions-Signal für Sounds verbinden
		BodyEntered += OnBodyEntered;

		// Startposition in der Mitte
		ResetToCenter();

		// RNG für festgefahrenen Ball initialisieren
		_rng.Randomize();
		_lastPositionForStuck = GlobalPosition;

		GD.Print("Ball initialized with physics-first approach");
	}

	/// <summary>
	/// Trennt Signal-Verbindungen beim Entfernen aus der Szene
	/// </summary>
	public override void _ExitTree()
	{
		if (_gameManager != null)
		{
			_gameManager.BallResetRequested -= OnBallResetRequested;
			_gameManager.BallKickoffRequested -= OnBallKickoffRequested;
		}
	}

	/// <summary>
	/// Callback für Ball-Reset Signal vom GameManager
	/// </summary>
	private void OnBallResetRequested()
	{
		ResetToCenter();
	}

	/// <summary>
	/// Callback für Anstoß Signal vom GameManager
	/// </summary>
	private void OnBallKickoffRequested()
	{
		ApplyKickoffImpulse();
	}

	/// <summary>
	/// Verarbeitet Ball-Physik in jedem Frame mit minimalen Eingriffen in die Physics Engine
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// Minimale Intervention - Physics Engine übernimmt die Bewegung

		// Schuss-Immunitäts-Timer aktualisieren
		if (_kickImmunityTimer > 0)
		{
			_kickImmunityTimer -= (float)delta;
		}

		// Sound Cooldown Timer aktualisieren
		if (_soundCooldownTimer > 0)
		{
			_soundCooldownTimer -= (float)delta;
		}

		// Festgefahrenen Ball erkennen und automatisch anstoßen
		if (_gameManager != null && _gameManager.CurrentState == GameManager.GameState.Playing)
		{
			float movement = (GlobalPosition - _lastPositionForStuck).Length();
			if (movement < STUCK_THRESHOLD)
			{
				_stuckTimer += (float)delta;

				// Ball hat sich zu lange nicht bewegt - leichten Anstoß geben
				if (_stuckTimer >= STUCK_NUDGE_TIME)
				{
					ApplyStuckNudge();
					_stuckTimer = 0f;
					_lastPositionForStuck = GlobalPosition;
				}
			}
			else
			{
				_stuckTimer = 0f;
				_lastPositionForStuck = GlobalPosition;
			}
		}

		// Maximale Geschwindigkeit begrenzen
		float speed = LinearVelocity.Length();
		if (speed > MAX_VELOCITY)
		{
			LinearVelocity = LinearVelocity.Normalized() * MAX_VELOCITY;
		}

		// Mikro-Bewegungen stoppen
		if (speed > 0 && speed < MIN_VELOCITY_THRESHOLD)
		{
			LinearVelocity = Vector3.Zero;
			AngularVelocity = Vector3.Zero;
		}

		// Y-Position auf Tischhöhe halten
		var pos = GlobalPosition;
		if (Mathf.Abs(pos.Y - RESET_Y) > 0.1f)
		{
			// Ball vom Tisch abgedriftet - zurück zwingen
			pos.Y = RESET_Y;
			GlobalPosition = pos;
			// Y-Geschwindigkeit nullen
			var vel = LinearVelocity;
			vel.Y = 0;
			LinearVelocity = vel;
		}

		// Sicherheitsgrenzen prüfen - Ball zurücksetzen wenn er entkommen ist
		float maxX = TABLE_LENGTH / 2 + 0.5f;
		float maxZ = TABLE_WIDTH / 2 + 0.3f;

		bool escaped = false;
		if (Mathf.Abs(pos.X) > maxX)
		{
			GD.Print($"Ball escaped X bounds (X={pos.X}), resetting...");
			escaped = true;
		}
		if (Mathf.Abs(pos.Z) > maxZ)
		{
			GD.Print($"Ball escaped Z bounds (Z={pos.Z}), resetting...");
			escaped = true;
		}
		if (pos.Y < -1.0f || pos.Y > RESET_Y + 2.0f)
		{
			GD.Print($"Ball escaped Y bounds (Y={pos.Y}), resetting...");
			escaped = true;
		}

		if (escaped)
		{
			ResetToCenter();
		}
	}

	/// <summary>
	/// Konfiguriert die Ball-Physik mit optimierten Parametern
	/// </summary>
	private void SetupBallPhysics()
	{
		// Masse für responsives Spielgefühl
		Mass = BALL_MASS;

		// Physics Material mit optimierten Einstellungen
		var physicsMaterial = new PhysicsMaterial();
		physicsMaterial.Bounce = BALL_BOUNCE;
		physicsMaterial.Friction = BALL_FRICTION;
		// MIN Combine Mode - niedrigerer Wert gewinnt
		// Verhindert, dass Ball an Wänden klebt
		physicsMaterial.Rough = false;
		physicsMaterial.Absorbent = false;

		PhysicsMaterialOverride = physicsMaterial;

		// CCD für schnell bewegten Ball (verhindert Tunneling durch Wände)
		ContinuousCd = true;

		// Keine Gravitation - Ball bleibt auf Tischoberfläche
		GravityScale = 0.0f;

		// Niedrige Dämpfung für natürliches Rollverhalten
		LinearDamp = LINEAR_DAMP;
		AngularDamp = ANGULAR_DAMP;

		// Kollisions-Layer
		// Ball auf Layer 2, kollidiert mit Layer 1 (Tisch/Wände) und Layer 4 (Figuren)
		CollisionLayer = 2;
		CollisionMask = 1 | 4;

		// Y-Achse sperren (kein Springen)
		AxisLockLinearY = true;
		AxisLockLinearX = false;
		AxisLockLinearZ = false;

		// Kontakt-Monitoring aktivieren
		ContactMonitor = true;
		MaxContactsReported = 4;

		GD.Print($"Ball physics: Mass={Mass}, Bounce={BALL_BOUNCE}, Friction={BALL_FRICTION}, LinearDamp={LINEAR_DAMP}");
	}

	/// <summary>
	/// Erstellt das Ball-Mesh mit Kugel-Geometrie
	/// </summary>
	private void SetupBallMesh()
	{
		if (_ballMesh == null)
			return;

		var sphereMesh = new SphereMesh();
		sphereMesh.Radius = BALL_RADIUS;
		sphereMesh.Height = BALL_RADIUS * 2;
		sphereMesh.RadialSegments = 24;
		sphereMesh.Rings = 12;

		_ballMesh.Mesh = sphereMesh;

		// White ball material
		var material = new StandardMaterial3D();
		material.AlbedoColor = new Color(0.95f, 0.95f, 0.95f);
		material.Roughness = 0.2f;
		material.Metallic = 0.0f;

		_ballMesh.SetSurfaceOverrideMaterial(0, material);
	}

	/// <summary>
	/// Richtet die Kollisionsform als einfache Kugel ein
	/// </summary>
	private void SetupBallCollider()
	{
		if (_ballCollider == null)
			return;

		var sphereShape = new SphereShape3D();
		sphereShape.Radius = BALL_RADIUS;

		_ballCollider.Shape = sphereShape;
		// Skalierung auf 1,1,1 belassen - Größe wird über Radius gesetzt
		_ballCollider.Scale = Vector3.One;
	}

	/// <summary>
	/// Behandelt Kollisions-Events für Hit-Sounds bei Wand- und Figurenkontakt
	/// </summary>
	private void OnBodyEntered(Node body)
	{
		if (_audioManager == null)
			return;

		// Cooldown prüfen um Sound-Spam zu verhindern
		if (_soundCooldownTimer > 0)
			return;

		float speed = LinearVelocity.Length();

		// Mindestgeschwindigkeit für Sound
		if (speed < 0.5f)
			return;

		// Prüfen ob es sich um StaticBody3D oder RigidBody3D handelt
		if (body is StaticBody3D || body is RigidBody3D)
		{
			// Kollisions-Layer prüfen (Wand/Tisch oder Figur)
			uint bodyLayer = 0;

			if (body is StaticBody3D staticBody)
				bodyLayer = staticBody.CollisionLayer;
			else if (body is RigidBody3D rigidBody)
				bodyLayer = rigidBody.CollisionLayer;

			// Geschwindigkeitsbasierte Tonhöhenvariation
			float speedFactor = Mathf.Clamp(speed / MAX_VELOCITY, 0.0f, 1.0f);

			// Zufällige Variation für natürlichen Sound
			var random = new RandomNumberGenerator();
			random.Randomize();
			float randomVariation = random.RandfRange(-0.05f, 0.05f);

			bool soundPlayed = false;

			// Layer 1 = Tisch/Wände → tieferer Hit-Sound (0.8 - 1.0)
			// Layer 4 = Figuren → höherer Hit-Sound (1.1 - 1.3)
			if ((bodyLayer & 1) != 0)
			{
				// Wand- oder Tisch-Kollision - tieferer Sound
				float pitchBase = Mathf.Lerp(0.8f, 1.0f, speedFactor);
				float finalPitch = Mathf.Clamp(pitchBase + randomVariation, 0.75f, 1.05f);
				_audioManager.PlaySFX("hit", finalPitch);
				soundPlayed = true;
			}
			else if ((bodyLayer & 4) != 0)
			{
				// Figuren-Kollision - höherer, schärferer Sound
				float pitchBase = Mathf.Lerp(1.1f, 1.3f, speedFactor);
				float finalPitch = Mathf.Clamp(pitchBase + randomVariation, 1.05f, 1.35f);
				_audioManager.PlaySFX("hit", finalPitch);
				soundPlayed = true;
			}

			// Cooldown setzen wenn Sound abgespielt wurde
			if (soundPlayed)
			{
				_soundCooldownTimer = SOUND_COOLDOWN;
			}
		}
	}

	/// <summary>
	/// Setzt den Ball in die Spielfeldmitte zurück ohne Impuls anzuwenden
	/// </summary>
	public void ResetToCenter()
	{
		GD.Print("Ball reset to center");

		// Einzige Stelle wo Position direkt manipuliert wird
		GlobalPosition = new Vector3(0, RESET_Y, 0);

		// Alle Geschwindigkeiten auf Null setzen
		LinearVelocity = Vector3.Zero;
		AngularVelocity = Vector3.Zero;

		// Immunitäts-Flag setzen um sofortige Schüsse nach Reset zu verhindern
		_kickImmunityTimer = KICK_IMMUNITY_DURATION;

		// Start-Impuls wird separat durch ApplyKickoffImpulse() gehandhabt
		GD.Print("Ball positioned - waiting for kickoff");
	}

	/// <summary>
	/// Wendet Anstoß-Impuls an um Spiel zu starten (nach Countdown)
	/// </summary>
	public void ApplyKickoffImpulse()
	{
		if (!IsInstanceValid(this))
			return;

		// Nur anwenden wenn Spiel läuft
		if (_gameManager == null || _gameManager.CurrentState != GameManager.GameState.Playing)
		{
			GD.Print("Kickoff impulse skipped - game not playing");
			return;
		}

		ApplyStartImpulse();
	}

	/// <summary>
	/// Gibt dem Ball einen leichten Anstoß wenn er zu lange nicht bewegt wurde
	/// </summary>
	private void ApplyStuckNudge()
	{
		// Zufällige Richtung
		float angle = _rng.RandfRange(0, Mathf.Tau);
		var direction = new Vector3(
			Mathf.Cos(angle),
			0,
			Mathf.Sin(angle)
		).Normalized();

		// Leichter Anstoß-Impuls
		float nudgeStrength = 0.15f;
		ApplyCentralImpulse(direction * nudgeStrength);

		if (DEBUG_VERBOSE)
			GD.Print($"Ball stuck for {STUCK_NUDGE_TIME}s - applying nudge in direction {direction}");
	}

	/// <summary>
	/// Wendet initialen Impuls für Spielbeginn an
	/// </summary>
	private void ApplyStartImpulse()
	{
		if (!IsInstanceValid(this))
			return;

		var random = new RandomNumberGenerator();
		random.Randomize();

		// Zufällige horizontale Richtung (bevorzugt Z-Achse für Querbewegung)
		var direction = new Vector3(
			random.RandfRange(-0.3f, 0.3f),
			0,
			random.RandfRange(-1.0f, 1.0f)
		).Normalized();

		// Sanften Impuls anwenden
		float impulseStrength = 0.25f;
		ApplyCentralImpulse(direction * impulseStrength);

		GD.Print($"Start impulse applied: {direction * impulseStrength}");
	}

	/// <summary>
	/// Wendet Schuss-Impuls von Figuren-Kollision an (keine direkte Geschwindigkeitsänderung!)
	/// </summary>
	public void ApplyKick(Vector3 direction, float strength)
	{
		// Schuss-Immunität prüfen (verhindert Schüsse direkt nach Reset)
		if (_kickImmunityTimer > 0)
		{
			GD.Print("Kick ignored - ball has immunity after reset");
			return;
		}

		// Sicherstellen dass Richtung horizontal und normalisiert ist
		direction.Y = 0;
		if (direction.LengthSquared() < 0.001f)
			return;
		direction = direction.Normalized();

		// Impuls basierend auf aktuellem Zustand berechnen
		float currentSpeed = LinearVelocity.Length();
		float effectiveStrength = strength;

		// Stehende Bälle bekommen moderaten Boost
		if (currentSpeed < 0.5f)
		{
			effectiveStrength = strength * 1.8f;
		}
		// Langsam rollende Bälle bekommen kleinen Boost
		else if (currentSpeed < 2.0f)
		{
			effectiveStrength = strength * 1.3f;
		}

		// Impuls anwenden - Physics Engine übernimmt den Rest
		ApplyCentralImpulse(direction * effectiveStrength);

		// Spin für realistisches Rollen hinzufügen
		var spinAxis = direction.Cross(Vector3.Up);
		ApplyTorqueImpulse(spinAxis * effectiveStrength * 0.3f);

		if (DEBUG_VERBOSE)
			GD.Print($"Kick applied: dir={direction}, strength={effectiveStrength}");
	}

	/// <summary>
	/// Gibt die aktuelle Ball-Geschwindigkeit zurück
	/// </summary>
	public float GetSpeed()
	{
		return LinearVelocity.Length();
	}

	/// <summary>
	/// Prüft ob sich der Ball bewegt
	/// </summary>
	public bool IsMoving()
	{
		return LinearVelocity.LengthSquared() > MIN_VELOCITY_THRESHOLD * MIN_VELOCITY_THRESHOLD;
	}
}
