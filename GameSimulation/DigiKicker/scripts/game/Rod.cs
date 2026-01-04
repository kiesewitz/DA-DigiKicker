using Godot;
using System;

/// <summary>
/// Stange-Controller zur Verwaltung von Rotation und lateraler Bewegung.
/// Verarbeitet Eingaben und spawnt Spielerfiguren.
///
/// WICHTIG: Im echten Tischfußball rotiert die STANGE SELBST um ihre Längsachse,
/// und die Figuren sind darauf montiert und rotieren mit der Stange.
/// - Rod (dieser Node): Rotiert um lokale Z-Achse (Stange erstreckt sich entlang Z über Tischbreite)
/// - RodMesh: Visuelle Darstellung der Stange (Kind von Rod, -90° um X rotiert für Z-Ausrichtung)
/// - Figures: Montiert auf FigureSlots Node, rotieren passiv mit der Stange
/// </summary>
public partial class Rod : Node3D
{
	private const float SCALE = 5.0f;
	private const float FIGURE_SCALE = 0.45f;
	private const float TABLE_HEIGHT = 0.1f * SCALE;

	/// <summary>
	/// Stangen-Typ bestimmt Position und Figurenanzahl.
	/// </summary>
	public enum RodType
	{
		Goalkeeper,
		Defense,
		Midfield,
		Attack
	}

	// Exportierte Properties (sichtbar im Godot Editor)
	[Export] public GameManager.Team Team { get; set; }
	[Export] public RodType Type { get; set; }
	[Export] public int FigureCount { get; set; }
	[Export] public int PlayerIndex { get; set; } = 1;

	// Bewegungskonstanten
	private const float ROTATION_SPEED = 10.0f;
	private const float LATERAL_SPEED = 3.5f;  // Erhöht von 2.0 für schnellere Auf/Ab-Bewegung
	private const float WALL_MARGIN = 0.03f * SCALE; // Kleiner Rand um Wandkollisionen zu vermeiden

	// Figuren-Spawn-Konstanten - Figuren verteilt über Tischbreite
	private const float TABLE_WIDTH = 1.2f * SCALE;
	private const float ROD_LENGTH = TABLE_WIDTH + 0.6f * SCALE; // Erweitert über Kanten hinaus
	private const float USABLE_WIDTH = TABLE_WIDTH * 0.85f; // Standard nutzbare Breite (für Mittelfeld-Stangen)

	// Dynamisch berechneter maximaler lateraler Offset (gesetzt in _Ready basierend auf Figurenanzahl)
	private float _maxLateralOffset = 0.3f;

	// Scene-Referenz
	private PackedScene _figureScene;

	// Node-Referenzen
	private MeshInstance3D _rodMesh;       // Visuelle Darstellung der Stange
	private Node3D _figureSlots;           // Container für Figuren-Instanzen
	private Mesh _baseCircleMesh;
	private System.Collections.Generic.List<(Figure figure, MeshInstance3D circle)> _indicatorPairs = new();

	// InputManager-Referenz
	private InputManager _inputManager;

	// Aktueller lateraler Offset
	private float _currentOffset = 0.0f;

	// Bot-Steuerung
	private bool _isBotControlled = false;
	private Vector2 _botInput = Vector2.Zero;

	// Netzwerk-Steuerung - für gegnerische Stangen über Netzwerk gesteuert
	private bool _isNetworkControlled = false;
	private float _networkTargetLateral = 0.0f;
	private float _networkTargetRotation = 0.0f;
	private const float NETWORK_LATERAL_SPEED = 15.0f;  // Schnelle Interpolation für responsive Reaktion
	private const float NETWORK_ROTATION_SPEED = 25.0f; // Schnelle Rotations-Interpolation

	// Rotationsgeschwindigkeits-Tracking (für Schussstärke-Berechnung)
	private float _currentRotationVelocity = 0.0f;

	/// <summary>
	/// Aktuelle Rotationsgeschwindigkeit in Radiant pro Sekunde.
	/// Wird von Figure verwendet, um Schussstärke basierend auf Schwunggeschwindigkeit zu berechnen.
	/// </summary>
	public float CurrentRotationVelocity => _currentRotationVelocity;

	/// <summary>
	/// Maximal erlaubter lateraler Offset für diese Stange.
	/// Wird vom BotController verwendet, um Bewegungslimits einzuhalten.
	/// </summary>
	public float MaxLateralOffset => _maxLateralOffset;

	/// <summary>
	/// Aktuelle laterale Offset-Position der Stange.
	/// Wird vom BotController verwendet, um aktuelle Position relativ zu Limits zu kennen.
	/// </summary>
	public float CurrentLateralOffset => _currentOffset;

	// Material-Referenzen für Drei-Stufen-Highlighting
	private StandardMaterial3D _normalMaterial;      // Standard grau - nicht ausgewählt
	private StandardMaterial3D _inPairMaterial;      // Dezentes Leuchten - im ausgewählten Paar aber nicht aktiv
	private StandardMaterial3D _activeMaterial;      // Starkes Leuchten - empfängt aktiv Input

	// Auswahl-Status
	public enum SelectionState
	{
		Normal,      // Überhaupt nicht ausgewählt
		InPair,      // Teil des ausgewählten Paars aber nicht die aktive Stange
		Active       // Aktuell aktive Stange die Input empfängt
	}

	private SelectionState _selectionState = SelectionState.Normal;

	/// <summary>
	/// Initialisiert die Stange beim Laden der Scene.
	/// Lädt Figuren-Scene, richtet Mesh ein und spawnt Figuren.
	/// </summary>
	public override void _Ready()
	{
		GD.Print($"===== Rod._Ready() START for Team {Team}, Type {Type} =====");

		// Figuren-Scene laden
		_figureScene = GD.Load<PackedScene>("res://scenes/game/Figure.tscn");

		if (_figureScene == null)
		{
			GD.PrintErr("Failed to load Figure.tscn!");
			return;
		}
		else
		{
			GD.Print("Successfully loaded Figure.tscn");
		}

		// InputManager abrufen
		_inputManager = GetNodeOrNull<InputManager>("/root/InputManager");
		GD.Print($"InputManager found: {_inputManager != null}");

		// Node-Referenzen abrufen
		_rodMesh = GetNode<MeshInstance3D>("RodMesh");
		_figureSlots = GetNode<Node3D>("FigureSlots");

		GD.Print($"Node references - RodMesh: {_rodMesh != null}, FigureSlots: {_figureSlots != null}");
		GD.Print($"Rod GlobalPosition: {GlobalPosition}");

		// Stangen-Mesh einrichten
		SetupRodMesh();
		SetupBaseCircleMesh();

		// Figuren spawnen
		SpawnFigures();

		GD.Print($"[ROD CONFIG] Team={Team}, Type={Type}, PlayerIndex={PlayerIndex}, IsBotControlled={_isBotControlled}, IsNetworkControlled={_isNetworkControlled}");
		GD.Print($"===== Rod._Ready() END for Team {Team}, Type {Type} =====");
	}

	/// <summary>
	/// Erstellt das Basis-Kreis-Mesh für Bodenindikatoren unter den Figuren.
	/// </summary>
	private void SetupBaseCircleMesh()
	{
		var circle = new CylinderMesh
		{
			TopRadius = 0.04f * SCALE,
			BottomRadius = 0.04f * SCALE,
			Height = 0.0025f * SCALE,
			RadialSegments = 24
		};
		_baseCircleMesh = circle;
	}

	private int _debugInputCounter = 0;

	/// <summary>
	/// Physics-Update: Verarbeitet Input und aktualisiert Stangen-Position/Rotation.
	/// Netzwerk-gesteuerte Stangen verwenden Interpolation zur Zielposition.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		// Netzwerk-gesteuerte Stangen verwenden Interpolation zur Zielposition
		if (_isNetworkControlled)
		{
			HandleNetworkInterpolation((float)delta);
			UpdateIndicators();
			return;
		}

		// Input für diese Stange abrufen
		var input = GetRodInput();

		// Debug: Erste nicht-Null Inputs loggen um zu sehen ob Input funktioniert
		if (_debugInputCounter < 3 && (Mathf.Abs(input.Rotation) > 0.01f || Mathf.Abs(input.Lateral) > 0.01f))
		{
			GD.Print($"[ROD INPUT] Team={Team}, Type={Type}, PlayerIndex={PlayerIndex}, Rotation={input.Rotation:F2}, Lateral={input.Lateral:F2}");
			_debugInputCounter++;
		}

		// Rotation und laterale Bewegung verarbeiten
		HandleRotation(input.Rotation, (float)delta);
		HandleLateralMovement(input.Lateral, (float)delta);

		// Indikatoren flach auf Tisch halten und Figuren folgen lassen
		UpdateIndicators();
	}

	/// <summary>
	/// Richtet das Erscheinungsbild des Stangen-Mesh ein.
	/// Das Stangen-Mesh ist in Rod.tscn um -90° um X rotiert, um entlang der Z-Achse ausgerichtet zu sein.
	/// </summary>
	private void SetupRodMesh()
	{
		if (_rodMesh == null)
			return;

		// Zylinder-Mesh für Stange erstellen
		var cylinderMesh = new CylinderMesh();
		cylinderMesh.TopRadius = 0.014f;  // Etwas dickere Stange für bessere Sichtbarkeit
		cylinderMesh.BottomRadius = 0.014f;
		cylinderMesh.Height = ROD_LENGTH;
		cylinderMesh.RadialSegments = 8;

		_rodMesh.Mesh = cylinderMesh;

		// HINWEIS: RodMesh-Rotation ist in Rod.tscn Scene-Datei gesetzt (90 Grad um X-Achse)
		// Dies richtet den Zylinder entlang der Z-Achse aus (Tischbreite)

		// Team-Farbe für Highlights abrufen
		Color teamColor = Team == GameManager.Team.Red
			? new Color(1.0f, 0.2f, 0.2f)  // Rot
			: new Color(0.2f, 0.4f, 1.0f); // Blau

		// Normal-Material erstellen (metallisches Grau)
		_normalMaterial = new StandardMaterial3D();
		_normalMaterial.AlbedoColor = new Color(0.6f, 0.6f, 0.65f);
		_normalMaterial.Metallic = 0.8f;
		_normalMaterial.Roughness = 0.3f;

		// InPair-Material erstellen (dezentes Team-Farb-Leuchten)
		_inPairMaterial = new StandardMaterial3D();
		_inPairMaterial.AlbedoColor = new Color(0.7f, 0.7f, 0.75f); // Etwas helleres Grau
		_inPairMaterial.Metallic = 0.7f;
		_inPairMaterial.Roughness = 0.4f;
		_inPairMaterial.EmissionEnabled = true;
		_inPairMaterial.Emission = teamColor * 0.3f; // Dezentes Team-Farb-Leuchten
		_inPairMaterial.EmissionEnergyMultiplier = 0.5f; // Niedrige Intensität

		// Aktiv-Material erstellen (starkes Team-Farb-Leuchten)
		_activeMaterial = new StandardMaterial3D();
		_activeMaterial.AlbedoColor = teamColor * 0.6f; // Team-Farb-Tönung
		_activeMaterial.Metallic = 0.6f;
		_activeMaterial.Roughness = 0.4f;
		_activeMaterial.EmissionEnabled = true;
		_activeMaterial.Emission = teamColor; // Helle Team-Farb-Emission
		_activeMaterial.EmissionEnergyMultiplier = 2.0f; // Hohe Intensität

		// Normal-Material standardmäßig anwenden
		_rodMesh.SetSurfaceOverrideMaterial(0, _normalMaterial);
	}

	/// <summary>
	/// Spawnt Spielerfiguren entlang der Stange basierend auf Figurenanzahl.
	/// Verteilt Figuren gleichmäßig über die nutzbare Tischbreite.
	/// </summary>
	private void SpawnFigures()
	{
		GD.Print($"SpawnFigures() called - FigureCount: {FigureCount}");

		if (_figureScene == null || _figureSlots == null)
		{
			GD.PrintErr($"Cannot spawn figures - FigureScene: {_figureScene != null}, FigureSlots: {_figureSlots != null}");
			return;
		}

		// Dynamischen Abstand basierend auf Figurenanzahl berechnen
		float usableWidth = FigureCount switch
		{
			1 => 0.0f,
			2 => TABLE_WIDTH * 0.35f, // Verteidigungs-Paar enger zusammen
			3 => TABLE_WIDTH * 0.55f, // Angriffs-Trio moderater Abstand
			5 => TABLE_WIDTH * 0.6f,  // Mittelfeld leicht enger
			_ => USABLE_WIDTH        // Standard
		};

		float spacing;
		float startZ;

		if (FigureCount == 1)
		{
			// Einzelne Figur (Torwart) - zentriert
			spacing = 0;
			startZ = 0;
		}
		else
		{
			// Mehrere Figuren - gleichmäßig über nutzbare Breite verteilen
			spacing = usableWidth / (FigureCount - 1);
			startZ = -usableWidth / 2.0f;
		}

		GD.Print($"Spawning {FigureCount} figures - Spacing: {spacing}, StartZ: {startZ}");

		// Figuren gleichmäßig entlang der Stange spawnen
		for (int i = 0; i < FigureCount; i++)
		{
			var figure = _figureScene.Instantiate<Figure>();

			if (figure == null)
			{
				GD.PrintErr($"Failed to instantiate Figure {i}!");
				continue;
			}

			// Figur konfigurieren
			figure.Team = Team;

			// Figur entlang Stange positionieren (Z-Achse)
			// Y-Offset positioniert die Figur so, dass die Stange durch den Torsobereich geht
			// GLB-Modell-Ursprung ist oben, daher Offset nach unten für Stange auf Brusthöhe
			// Positives Y = Figur hängt unter Stange (Stange durch Oberkörper)
			float zPos = (FigureCount == 1) ? 0 : startZ + (i * spacing);
			figure.Position = new Vector3(0, 0.04f * SCALE, zPos);
			figure.Scale = new Vector3(FIGURE_SCALE, FIGURE_SCALE, FIGURE_SCALE);

			// Boden-Indikator-Kreis für Positionsklarheit hinzufügen (bleibt flach auf Tisch)
			var baseCircle = new MeshInstance3D
			{
				Mesh = _baseCircleMesh
			};

			// Nach Team einfärben
			var baseMat = new StandardMaterial3D();
			baseMat.AlbedoColor = Team == GameManager.Team.Red ? new Color(1.0f, 0.2f, 0.2f, 0.18f) : new Color(0.2f, 0.4f, 1.0f, 0.18f);
			baseMat.EmissionEnabled = true;
			baseMat.Emission = baseMat.AlbedoColor * 0.12f;
			baseMat.Metallic = 0.2f;
			baseMat.Roughness = 0.92f;
			baseCircle.SetSurfaceOverrideMaterial(0, baseMat);

			// An diese Stange binden, damit es sich mit Stange bewegt (X/Y) aber flach bleibt
			AddChild(baseCircle);
			_indicatorPairs.Add((figure, baseCircle));

			GD.Print($"Adding Figure {i} at position {figure.Position} (Team: {Team})");

			// Zu Figuren-Slots hinzufügen
			_figureSlots.AddChild(figure);

			// Hinzufügen verifizieren
			GD.Print($"Figure {i} added. Global position: {figure.GlobalPosition}");
		}

		GD.Print($"Finished spawning {FigureCount} figures. FigureSlots child count: {_figureSlots.GetChildCount()}");

		// Maximalen lateralen Offset basierend auf äußerster Figurenposition berechnen
		// Wand ist bei TABLE_WIDTH / 2, äußerste Figur bestimmt wie weit wir uns bewegen können
		if (Type == RodType.Goalkeeper)
		{
			// Torwart hat praktisch unbegrenzte Bewegung - nur winziger Rand um Wandkollision zu vermeiden
			_maxLateralOffset = (TABLE_WIDTH / 2.0f) - 0.01f;
			GD.Print($"Rod {Team} {Type}: Max lateral offset = {_maxLateralOffset} (GOALKEEPER - full range)");
		}
		else
		{
			float outermostFigureZ = (FigureCount == 1) ? 0 : usableWidth / 2.0f;
			_maxLateralOffset = (TABLE_WIDTH / 2.0f) - outermostFigureZ - WALL_MARGIN;
			_maxLateralOffset = Mathf.Max(_maxLateralOffset, 0.02f); // Minimale Bewegung sicherstellen
			GD.Print($"Rod {Team} {Type}: Max lateral offset = {_maxLateralOffset} (outermost figure at Z={outermostFigureZ})");
		}
	}

	/// <summary>
	/// Aktualisiert Positions-Indikatoren unter den Figuren.
	/// Hält Indikatoren flach auf dem Tisch und lässt sie den Figuren folgen.
	/// </summary>
	private void UpdateIndicators()
	{
		if (_indicatorPairs.Count == 0)
			return;

		foreach (var pair in _indicatorPairs)
		{
			if (pair.figure == null || !GodotObject.IsInstanceValid(pair.figure) || pair.circle == null || !GodotObject.IsInstanceValid(pair.circle))
				continue;

			var pos = pair.figure.GlobalTransform.Origin;
			// Indikator knapp über Tischhöhe platzieren, flach auf XZ-Ebene
			pos.Y = (TABLE_HEIGHT / 2f) + 0.003f * SCALE;
			// Auf Stangen-X-Position festnageln, Figur-Z folgen
			pos.X = GlobalTransform.Origin.X;
			pair.circle.GlobalTransform = new Transform3D(Basis.Identity, pos);
		}
	}

	private int _inputDebugCounter = 0;

	/// <summary>
	/// Ruft Input für diese Stange vom InputManager oder Bot-Controller ab.
	/// Gibt Rotations- und laterale Bewegungswerte zurück.
	/// </summary>
	private (float Rotation, float Lateral) GetRodInput()
	{
		// Wenn diese Stange bot-gesteuert ist, Bot-Input verwenden
		if (_isBotControlled)
		{
			return (_botInput.Y, _botInput.X);
		}

		// Sonst Spieler-Input verwenden
		if (_inputManager == null)
		{
			if (_inputDebugCounter < 1)
			{
				GD.Print($"[ROD INPUT DEBUG] Team={Team}, Type={Type} - InputManager is NULL!");
				_inputDebugCounter++;
			}
			return (0, 0);
		}

		// Stangen-Index basierend auf Typ abrufen
		int rodIndex = Type switch
		{
			RodType.Goalkeeper => 0,
			RodType.Defense => 1,
			RodType.Midfield => 2,
			RodType.Attack => 3,
			_ => 0
		};

		// Input vom InputManager abrufen
		// InputManager.GetRodInput gibt Vector2 zurück wobei X=lateral, Y=rotation
		Vector2 input = _inputManager.GetRodInput(PlayerIndex, rodIndex);

		return (input.Y, input.X);
	}

	/// <summary>
	/// Setzt Bot-Input für diese Stange (aufgerufen vom BotController).
	/// </summary>
	public void SetBotInput(Vector2 input)
	{
		_isBotControlled = true;
		_botInput = input;
	}

	/// <summary>
	/// Verarbeitet Stangen-Rotation um ihre Längsachse (Schussbewegung).
	/// WICHTIG: Im echten Tischfußball rotiert die gesamte STANGE, und die Figuren
	/// sind darauf montiert und rotieren mit.
	/// Die Stange erstreckt sich entlang der Z-Achse (über Tischbreite), daher rotieren wir um Z.
	/// </summary>
	private void HandleRotation(float input, float delta)
	{
		// Rotationsgeschwindigkeit tracken (Radiant pro Sekunde) für Schussstärke-Berechnung
		// Ermöglicht Figuren zu wissen wie schnell die Stange rotiert wenn sie den Ball treffen
		_currentRotationVelocity = input * ROTATION_SPEED;

		// Rotation auf die Stange selbst um ihre Z-Achse anwenden (Längsachse)
		// Das Stangen-Mesh erstreckt sich entlang Z, daher lässt Rotation um Z die Figuren korrekt schwingen
		float rotationAmount = input * ROTATION_SPEED * delta;
		RotateObjectLocal(Vector3.Back, rotationAmount);

		// Optional: Rotation begrenzen um volle 360-Grad-Drehung zu verhindern
		// Erlaubt ungefähr 180 Grad Rotation vorwärts/rückwärts
		var currentRotation = Rotation;
		currentRotation.Z = Mathf.Clamp(currentRotation.Z, -Mathf.Pi, Mathf.Pi);
		Rotation = currentRotation;
	}

	/// <summary>
	/// Verarbeitet laterale Bewegung entlang Z-Achse (Gleiten entlang Stange).
	/// Bewegung ist basierend auf Figurenanzahl begrenzt um Wandkollisionen zu verhindern.
	/// Torwart hat KEINE laterale Einschränkung um Bälle in Ecken zu erreichen.
	/// </summary>
	private void HandleLateralMovement(float input, float delta)
	{
		// Offset basierend auf Input aktualisieren
		_currentOffset += input * LATERAL_SPEED * delta;

		// Torwart hat praktisch KEINE Bewegungseinschränkung - kann überall hin
		// Nur winziger Rand um Z-Fighting mit Wänden zu verhindern
		// Andere Stangen werden basierend auf Figurenpositionen begrenzt um Kollisionen zu verhindern
		if (Type != RodType.Goalkeeper)
		{
			_currentOffset = Mathf.Clamp(_currentOffset, -_maxLateralOffset, _maxLateralOffset);
		}
		else
		{
			// Torwart: minimale Einschränkung, nur Durchdringen von Wänden verhindern
			// TABLE_WIDTH/2 = 3.0 (skaliert), nur 0.01 Rand verwenden
			float maxGoalkeeperOffset = (TABLE_WIDTH / 2.0f) - 0.01f;
			_currentOffset = Mathf.Clamp(_currentOffset, -maxGoalkeeperOffset, maxGoalkeeperOffset);
		}

		// Offset auf Stangenposition anwenden
		var pos = Position;
		pos.Z = _currentOffset;
		Position = pos;
	}

	/// <summary>
	/// Verarbeitet interpolierte Bewegung für netzwerk-gesteuerte Stangen.
	/// Bewegt sich sanft zur Zielposition/-rotation um korrekte Kollisionserkennung sicherzustellen.
	/// </summary>
	private void HandleNetworkInterpolation(float delta)
	{
		// Laterale Position interpolieren
		float lateralDiff = _networkTargetLateral - _currentOffset;
		float lateralStep = NETWORK_LATERAL_SPEED * delta;

		if (Mathf.Abs(lateralDiff) <= lateralStep)
		{
			_currentOffset = _networkTargetLateral;
		}
		else
		{
			_currentOffset += Mathf.Sign(lateralDiff) * lateralStep;
		}

		// Laterale Position anwenden
		var pos = Position;
		pos.Z = _currentOffset;
		Position = pos;

		// Rotation mit Winkel-Wrapping interpolieren
		var currentRot = Rotation;
		float rotDiff = _networkTargetRotation - currentRot.Z;

		// Rotationsdifferenz auf -PI bis PI normalisieren
		while (rotDiff > Mathf.Pi) rotDiff -= Mathf.Tau;
		while (rotDiff < -Mathf.Pi) rotDiff += Mathf.Tau;

		float rotStep = NETWORK_ROTATION_SPEED * delta;

		// Rotationsgeschwindigkeit für Schussstärke berechnen (wichtig für Figuren-Kollision)
		_currentRotationVelocity = rotDiff / delta; // Ungefähre Geschwindigkeit basierend auf Differenz
		_currentRotationVelocity = Mathf.Clamp(_currentRotationVelocity, -15.0f, 15.0f);

		if (Mathf.Abs(rotDiff) <= rotStep)
		{
			currentRot.Z = _networkTargetRotation;
		}
		else
		{
			currentRot.Z += Mathf.Sign(rotDiff) * rotStep;
		}

		// Rotation auf gültigen Bereich begrenzen
		currentRot.Z = Mathf.Clamp(currentRot.Z, -Mathf.Pi, Mathf.Pi);
		Rotation = currentRot;
	}

	/// <summary>
	/// Setzt Stange auf neutrale Position zurück (Mitte, keine Rotation).
	/// </summary>
	public void ResetPosition()
	{
		_currentOffset = 0.0f;
		Position = new Vector3(Position.X, Position.Y, 0);

		// Stangen-Rotation auf neutral zurücksetzen
		Rotation = Vector3.Zero;
	}

	/// <summary>
	/// Setzt den Auswahl-Status der Stange mit Drei-Stufen-Highlighting.
	/// </summary>
	/// <param name="state">Der neue Auswahl-Status (Normal, InPair oder Active)</param>
	public void SetSelectionState(SelectionState state)
	{
		if (_rodMesh == null || !GodotObject.IsInstanceValid(_rodMesh) || _normalMaterial == null || _inPairMaterial == null || _activeMaterial == null)
			return;

		_selectionState = state;

		// Passendes Material basierend auf Auswahl-Status anwenden
		StandardMaterial3D material = state switch
		{
			SelectionState.Normal => _normalMaterial,
			SelectionState.InPair => _inPairMaterial,
			SelectionState.Active => _activeMaterial,
			_ => _normalMaterial
		};

		_rodMesh.SetSurfaceOverrideMaterial(0, material);
		GD.Print($"Rod {Team} {Type} selection state changed to {state}");
	}

	/// <summary>
	/// Gibt den aktuellen Auswahl-Status der Stange zurück.
	/// </summary>
	public SelectionState GetSelectionState()
	{
		return _selectionState;
	}

	/// <summary>
	/// Legacy-Methode für Rückwärtskompatibilität. Setzt Auswahl auf Active wenn true, Normal wenn false.
	/// </summary>
	/// <param name="selected">True um als Active zu setzen, false für Normal</param>
	public void SetSelected(bool selected)
	{
		SetSelectionState(selected ? SelectionState.Active : SelectionState.Normal);
	}

	/// <summary>
	/// Legacy-Methode für Rückwärtskompatibilität. Gibt true zurück wenn Status Active oder InPair ist.
	/// </summary>
	public bool IsSelected()
	{
		return _selectionState != SelectionState.Normal;
	}

	/// <summary>
	/// Gibt die Z-Positionen aller Figuren auf dieser Stange zurück (relativ zur Stangenmitte).
	/// Wird vom BotController für präzise Figuren-Ausrichtung verwendet.
	/// </summary>
	public float[] GetFigureZPositions()
	{
		// Figurenpositionen mit gleicher Logik wie SpawnFigures berechnen
		float usableWidth = FigureCount switch
		{
			1 => 0.0f,
			2 => TABLE_WIDTH * 0.35f,
			3 => TABLE_WIDTH * 0.55f,
			5 => TABLE_WIDTH * 0.6f,
			_ => USABLE_WIDTH
		};

		float[] positions = new float[FigureCount];

		if (FigureCount == 1)
		{
			positions[0] = 0f;
		}
		else
		{
			float spacing = usableWidth / (FigureCount - 1);
			float startZ = -usableWidth / 2.0f;
			for (int i = 0; i < FigureCount; i++)
			{
				positions[i] = startZ + (i * spacing);
			}
		}

		return positions;
	}

	/// <summary>
	/// Gibt den Abstand zwischen benachbarten Figuren auf dieser Stange zurück.
	/// Gibt 0 für Ein-Figuren-Stangen (Torwart) zurück.
	/// </summary>
	public float GetFigureSpacing()
	{
		if (FigureCount <= 1) return 0f;

		float usableWidth = FigureCount switch
		{
			2 => TABLE_WIDTH * 0.35f,
			3 => TABLE_WIDTH * 0.55f,
			5 => TABLE_WIDTH * 0.6f,
			_ => USABLE_WIDTH
		};

		return usableWidth / (FigureCount - 1);
	}

	/// <summary>
	/// Gibt Stangenposition für GDScript Network Sync zurück ohne auf C# Property Casing angewiesen zu sein.
	/// </summary>
	public Vector3 GetNetworkPosition() => Position;

	/// <summary>
	/// Gibt Stangenrotation für GDScript Network Sync zurück.
	/// </summary>
	public Vector3 GetNetworkRotation() => Rotation;

	/// <summary>
	/// Gibt den aktuellen lateralen Offset (Z) für Netzwerk-Replikation zurück.
	/// </summary>
	public float GetNetworkLateralOffset() => Position.Z;

	/// <summary>
	/// Wendet netzwerkten Stangen-Status mit Interpolation für sanfte Bewegung an.
	/// Setzt Zielwerte zu denen die Stange in _PhysicsProcess interpoliert.
	/// Stellt korrekte Kollisionserkennung mit dem Ball sicher.
	/// </summary>
	public void ApplyNetworkState(float lateralOffset, float rotationZ)
	{
		// Netzwerk-Steuerungsmodus aktivieren falls noch nicht aktiv
		if (!_isNetworkControlled)
		{
			SetNetworkControlled(true);
		}

		// Zielwerte für Interpolation setzen
		_networkTargetLateral = lateralOffset;
		_networkTargetRotation = rotationZ;
	}

	/// <summary>
	/// Aktiviert oder deaktiviert Netzwerk-Steuerungsmodus für diese Stange.
	/// Wenn aktiviert, interpoliert die Stange zu Netzwerk-Zielpositionen.
	/// </summary>
	public void SetNetworkControlled(bool enabled)
	{
		_isNetworkControlled = enabled;
		_isBotControlled = false; // Netzwerk-Steuerung hat Priorität

		if (enabled)
		{
			// Ziele auf aktuelle Position initialisieren
			_networkTargetLateral = _currentOffset;
			_networkTargetRotation = Rotation.Z;
			GD.Print($"Rod {Team} {Type}: Network control ENABLED");
		}
		else
		{
			GD.Print($"Rod {Team} {Type}: Network control DISABLED");
		}
	}

	/// <summary>
	/// Gibt zurück ob diese Stange aktuell netzwerk-gesteuert ist.
	/// </summary>
	public bool IsNetworkControlled => _isNetworkControlled;
}
