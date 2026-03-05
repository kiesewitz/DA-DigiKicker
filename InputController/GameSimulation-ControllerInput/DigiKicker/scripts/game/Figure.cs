using Godot;
using System;

/// <summary>
/// Spielerfigur montiert auf Stangen.
/// Verwaltet Aussehen, Kollision und Team-basierte Materialien.
/// </summary>
public partial class Figure : Node3D
{
	[Export] public GameManager.Team Team { get; set; }
	[Export] public Texture2D RedTexture;
	[Export] public Texture2D BlueTexture;

	// Figuren-Dimensionen
	private const float FIGURE_SCALE = 0.45f;
	private const float FIGURE_RADIUS = 0.04f * FIGURE_SCALE;
	private const float FIGURE_HEIGHT = 0.15f * FIGURE_SCALE;

	// Node-Referenzen
	private MeshInstance3D _characterMesh;
	private StaticBody3D _staticBody;
	private CollisionShape3D _collisionShape;

	// Schuss-Erkennung
	private Area3D _kickTrigger;
	private Rod _parentRod;

	// Debug-Ausgabe-Steuerung - auf false setzen für sauberere Trainings-Ausgabe
	private const bool DEBUG_VERBOSE = false;

	/// <summary>
	/// Initialisiert die Figur beim Laden der Scene.
	/// Lädt Nodes und richtet Komponenten ein.
	/// </summary>
	public override void _Ready()
	{
		GD.Print($"Figure._Ready() called for Team {Team}");

		// Node-Referenzen abrufen
		_characterMesh = GetNode<MeshInstance3D>("Character");
		_staticBody = GetNode<StaticBody3D>("StaticBody3D");
		_collisionShape = _staticBody.GetNode<CollisionShape3D>("CollisionShape3D");

		GD.Print($"Figure nodes found - Mesh: {_characterMesh != null}, StaticBody: {_staticBody != null}, Collision: {_collisionShape != null}");

		// Komponenten einrichten
		// HINWEIS: SetupMesh() und SetupCollision() sind deaktiviert da Figure.tscn bereits ein GLB-Modell hat
		// Um stattdessen Platzhalter-Meshes zu verwenden, diese auskommentieren:
		// SetupMesh();
		// SetupCollision();

		SetupPhysicsMaterial();
		SetMaterial();
		SetupOrientation();
		SetupKickTrigger();

		GD.Print($"Figure ready at position: {GlobalPosition}");
	}

	/// <summary>
	/// Erstellt das Figuren-Mesh als Kapsel-Platzhalter.
	/// </summary>
	private void SetupMesh()
	{
		if (_characterMesh == null)
			return;

		// Kapsel-Mesh für Figur erstellen
		var capsuleMesh = new CapsuleMesh();
		capsuleMesh.Radius = FIGURE_RADIUS;
		capsuleMesh.Height = FIGURE_HEIGHT;
		capsuleMesh.RadialSegments = 8;
		capsuleMesh.Rings = 4;

		_characterMesh.Mesh = capsuleMesh;

		// Mesh leicht über Drehpunkt positionieren
		_characterMesh.Position = new Vector3(0, FIGURE_HEIGHT / 2, 0);
	}

	/// <summary>
	/// Richtet Collision Shape für Figur ein.
	/// </summary>
	private void SetupCollision()
	{
		if (_collisionShape == null)
			return;

		// Kapsel-Collision Shape passend zum Mesh erstellen
		var capsuleShape = new CapsuleShape3D();
		capsuleShape.Radius = FIGURE_RADIUS;
		capsuleShape.Height = FIGURE_HEIGHT;

		_collisionShape.Shape = capsuleShape;

		// Collider passend zum Mesh positionieren
		_collisionShape.Position = new Vector3(0, FIGURE_HEIGHT / 2, 0);
	}

	/// <summary>
	/// Fügt ein Physics Material hinzu damit der Ball responsiver von Figuren abprallt.
	/// Konfiguriert auch Collision Layers für korrekte Ball-Interaktion.
	/// </summary>
	private void SetupPhysicsMaterial()
	{
		if (_staticBody == null)
			return;

		// Physics Material für knackige, responsive Schüsse
		// HOHER Bounce überträgt Energie sauber, NIEDRIGE Friktion verhindert Rutschen
		var material = new PhysicsMaterial
		{
			Bounce = 0.95f,    // Sehr hoher Bounce - Ball schießt von Figuren ab
			Friction = 0.02f,  // Fast keine Friktion - verhindert Greifen/Rutschen
			Rough = false,     // Durchschnitt verwenden (niedrigere Friktion gewinnt)
			Absorbent = false  // Durchschnitt verwenden (höherer Bounce gewinnt)
		};

		_staticBody.PhysicsMaterialOverride = material;

		// Collision Layers setzen
		// Figuren sind auf Layer 3 (Wert 4), kollidieren mit Ball Layer 2 (Wert 2)
		_staticBody.CollisionLayer = 4;  // Layer 3
		_staticBody.CollisionMask = 2;   // Ball erkennen (Layer 2)
	}

	/// <summary>
	/// Richtet Figuren-Ausrichtung basierend auf Team ein.
	/// TeamRed (linke Seite) schaut RECHTS (positiv X) zum gegnerischen Tor.
	/// TeamBlue (rechte Seite) schaut LINKS (negativ X) zum gegnerischen Tor.
	/// </summary>
	private void SetupOrientation()
	{
		// Figur drehen um zum gegnerischen Tor zu schauen
		// Standard GLB-Modell-Ausrichtung muss 90 Grad um Y-Achse gedreht werden
		// TeamRed: Zu positiv X drehen (rechts)
		// TeamBlue: Zu negativ X drehen (links) - zusätzliche 180 Grad

		switch (Team)
		{
			case GameManager.Team.Red:
				// Rotes Team auf linker Seite, schaut RECHTS (positiv X)
				RotationDegrees = new Vector3(0, 90, 0);
				GD.Print("Setting RED figure orientation: facing positive X (right)");
				break;

			case GameManager.Team.Blue:
				// Blaues Team auf rechter Seite, schaut LINKS (negativ X)
				RotationDegrees = new Vector3(0, -90, 0);
				GD.Print("Setting BLUE figure orientation: facing negative X (left)");
				break;

			default:
				// Fallback: Unbekanntes/None als Red behandeln um ungesetzte Zustände zu vermeiden
				Team = GameManager.Team.Red;
				RotationDegrees = new Vector3(0, 90, 0);
				GD.Print("Unknown team, defaulting to RED orientation");
				break;
		}
	}

	/// <summary>
	/// Richtet einen Area3D Trigger für Schuss-Erkennung ein.
	/// StaticBody3D emittiert keine BodyEntered Signals, daher brauchen wir Area3D.
	/// Der Area3D dupliziert die Collision Shape und erkennt Ball-Kontakt.
	/// </summary>
	private void SetupKickTrigger()
	{
		if (_collisionShape?.Shape == null)
		{
			GD.PrintErr("Figure.SetupKickTrigger() - No collision shape available!");
			return;
		}

		// Eltern-Stange für Rotationsgeschwindigkeit finden
		_parentRod = GetParentRod();

		// Area3D für Schuss-Erkennung erstellen
		_kickTrigger = new Area3D();
		_kickTrigger.Name = "KickTrigger";

		// Collision Shape vom StaticBody3D duplizieren
		var triggerCollider = new CollisionShape3D();
		triggerCollider.Shape = _collisionShape.Shape.Duplicate() as Shape3D;
		triggerCollider.Position = _collisionShape.Position;

		_kickTrigger.AddChild(triggerCollider);

		// Area3D konfigurieren - nicht erkennbar, aber überwacht Ball
		_kickTrigger.Monitorable = false;  // Andere Areas müssen dies nicht erkennen
		_kickTrigger.Monitoring = true;    // Diese Area erkennt Bodies
		_kickTrigger.CollisionLayer = 0;   // Auf keinem Layer
		_kickTrigger.CollisionMask = 2;    // Ball erkennen (Layer 2)

		// Zur Figur hinzufügen
		AddChild(_kickTrigger);

		// Signal verbinden
		_kickTrigger.BodyEntered += OnBodyEntered;

		GD.Print($"Figure kick trigger set up, parent rod: {(_parentRod != null ? _parentRod.Type.ToString() : "null")}");
	}

	/// <summary>
	/// Findet den Eltern-Rod Node in der Scene-Hierarchie.
	/// Figuren-Hierarchie: Rod -> FigureSlots -> Figure
	/// </summary>
	private Rod GetParentRod()
	{
		// Figur ist Kind von FigureSlots, welcher Kind von Rod ist
		var figureSlots = GetParent();
		if (figureSlots?.GetParent() is Rod rod)
		{
			return rod;
		}
		GD.PrintErr("Figure.GetParentRod() - Could not find parent Rod!");
		return null;
	}

	/// <summary>
	/// Setzt Figuren-Material basierend auf Team-Farbe.
	/// Rotes Team: Rotes Material
	/// Blaues Team: Blaues Material
	/// </summary>
	private void SetMaterial()
	{
		if (_characterMesh == null)
		{
			GD.PrintErr("Figure.SetMaterial() - _characterMesh is null!");
			return;
		}

		var material = new StandardMaterial3D();

		// Farbe basierend auf Team setzen
		switch (Team)
		{
			case GameManager.Team.Red:
				material.AlbedoTexture = RedTexture;
				GD.Print("Setting RED material");
				break;

			case GameManager.Team.Blue:
				material.AlbedoTexture = BlueTexture;
				GD.Print("Setting BLUE material");
				break;

			default:
				material.AlbedoColor = new Color(0.5f, 0.5f, 0.5f); // Grauer Fallback
				GD.Print("Setting GRAY fallback material");
				break;
		}

		// Material-Eigenschaften setzen
		material.Roughness = 0.7f;
		material.Metallic = 0.1f;

		// Material auf alle Oberflächen des Mesh anwenden
		var mesh = _characterMesh.Mesh;
		if (mesh != null)
		{
			int surfaceCount = mesh.GetSurfaceCount();
			GD.Print($"Figure mesh has {surfaceCount} surface(s)");

			for (int i = 0; i < surfaceCount; i++)
			{
				_characterMesh.SetSurfaceOverrideMaterial(i, material);
			}
		}
		else
		{
			GD.PrintErr("Figure._characterMesh.Mesh is null!");
		}
	}

	/// <summary>
	/// Wird aufgerufen wenn Figur mit Ball via Area3D Trigger kollidiert.
	/// Wendet nur Impuls an wenn die Stange aktiv rotiert (schwingt).
	/// Wenn nicht rotierend, lässt StaticBody3D Physics die Kollision natürlich handhaben.
	/// </summary>
	private void OnBodyEntered(Node3D body)
	{
		if (body is Ball ball)
		{
			// AI Controller benachrichtigen welches Team den Ball berührt hat (für Eigentor-Tracking)
			NotifyAIControllersBallTouched();

			// Rotationsgeschwindigkeit von Eltern-Stange abrufen
			float rotationSpeed = _parentRod?.CurrentRotationVelocity ?? 0f;
			float absRotation = Mathf.Abs(rotationSpeed);

			// WICHTIG: Schuss-Impuls nur anwenden wenn Stange aktiv schwingt!
			// Minimale Rotations-Schwelle - darunter lässt Physics es natürlich handhaben
			const float MIN_ROTATION_FOR_KICK = 2.0f; // rad/sec (etwa 20% der Max-Geschwindigkeit)

			if (absRotation < MIN_ROTATION_FOR_KICK)
			{
				// Stange schwingt nicht hart genug - StaticBody3D Physics übernimmt Kollision
				// Kein Impuls angewandt, nur natürlicher Bounce vom PhysicsMaterial
				return;
			}

			// Basis-Schuss-Richtung von Figur zu Ball berechnen
			var kickDirection = (ball.GlobalPosition - GlobalPosition).Normalized();
			kickDirection.Y = 0; // Horizontal halten

			// Schuss-Stärke basierend auf Rotationsgeschwindigkeit berechnen
			// rotationSpeed ist in rad/sec, Max ist ~10 rad/sec (ROTATION_SPEED in Rod.cs)
			// Skaliert von 0.15 (leichter Schuss) bis 0.6 (kraftvoller Schuss)
			// Hinweis: Ball.ApplyKick multipliziert bis zu 2x für stehende Bälle
			const float MIN_KICK = 0.15f;
			const float MAX_KICK = 0.6f;
			const float MAX_ROTATION_SPEED = 10.0f;

			// Rotation von Schwelle zu Max mappen
			float effectiveRotation = absRotation - MIN_ROTATION_FOR_KICK;
			float rotationRange = MAX_ROTATION_SPEED - MIN_ROTATION_FOR_KICK;
			float rotationFactor = Mathf.Clamp(effectiveRotation / rotationRange, 0.0f, 1.0f);
			float kickStrength = MIN_KICK + (MAX_KICK - MIN_KICK) * rotationFactor;

			// Richtungs-Bias basierend auf Schwung-Richtung hinzufügen
			// Lässt den Ball in Richtung der Figuren-Schwingung gehen
			float rotDir = Mathf.Sign(rotationSpeed);
			// Rotes Team (linke Seite) kickt rechts bei positiver Rotation
			// Blaues Team (rechte Seite) kickt links bei positiver Rotation
			if (Team == GameManager.Team.Red)
				kickDirection.X += rotDir * 0.4f;
			else
				kickDirection.X -= rotDir * 0.4f;
			kickDirection = kickDirection.Normalized();

			ball.ApplyKick(kickDirection, kickStrength);

			// Hinweis: Schuss-Sound wird jetzt automatisch in Ball.cs OnBodyEntered abgespielt

			if (DEBUG_VERBOSE)
				GD.Print($"Kick! Rotation: {rotationSpeed:F2} rad/s, Strength: {kickStrength:F1}");
		}
	}

	/// <summary>
	/// Benachrichtigt alle AI Controller dass dieses Team den Ball berührt hat.
	/// Wird für Eigentor-Erkennung im RL Training verwendet.
	/// </summary>
	private void NotifyAIControllersBallTouched()
	{
		// Team Enum zu int konvertieren (Red=0, Blue=1)
		int teamIndex = Team == GameManager.Team.Red ? 0 : 1;

		// Alle AI Controller in AGENT Gruppe finden
		var agents = GetTree().GetNodesInGroup("AGENT");
		foreach (var agent in agents)
		{
			// GDScript-Methode auf jedem AI Controller aufrufen
			if (agent.HasMethod("on_ball_touched_by_team"))
			{
				agent.Call("on_ball_touched_by_team", teamIndex);
			}
		}
	}
}
