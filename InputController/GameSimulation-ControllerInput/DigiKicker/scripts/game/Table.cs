using Godot;
using System;

/// <summary>
/// Table Scene Controller verantwortlich für das Aufsetzen des Spielfelds,
/// der Wände, Tore und das Spawnen der Rods für beide Teams.
/// </summary>
public partial class Table : Node3D
{
	private const float SCALE = 5.0f;

	// Tisch-Dimensionen
	private const float TABLE_LENGTH = 2.4f * SCALE;
	private const float TABLE_WIDTH = 1.2f * SCALE;
	private const float TABLE_HEIGHT = 0.02f * SCALE;

	// Wand-Dimensionen
	private const float WALL_HEIGHT = 0.25f * SCALE;  // Höhere Wände für bessere Sichtbarkeit
	private const float WALL_THICKNESS = 0.07f * SCALE;

	// Tor-Dimensionen
	private const float GOAL_WIDTH = (TABLE_WIDTH - WALL_THICKNESS * 2) * 0.5f; // Schmalere Toröffnung
	private const float GOAL_HEIGHT = 0.2f * SCALE;      // Höhe der Toröffnung
	private const float GOAL_DEPTH = 0.15f * SCALE;       // Tiefe des Tors (wie weit es nach hinten geht)
	private const float GOAL_FRAME_THICKNESS = 0.05f * SCALE; // Dicke der Torpfosten
	private const float GOAL_BACK_THICKNESS = 0.02f * SCALE;  // Dicke der Rückwand

	// Rod-Dimensionen
	private const float ROD_RADIUS = 0.015f * SCALE;
	private const float ROD_LENGTH = TABLE_WIDTH + 0.6f * SCALE; // Verlängerte Rods (1.8m gesamt - steht über beide Kanten hinaus)
	private const float ROD_Y = TABLE_HEIGHT / 2 + 0.17f * SCALE; // Höhe über Tisch für Figuren

	// Rod-Positionen entlang X-Achse (absolute Positionen vom Zentrum)
	// Standard Tischfußball Layout: 8 Rods gleichmäßig verteilt
	// Layout von links (Red-Tor) nach rechts (Blue-Tor):
	// RedGK - RedDef - BlueAtk - RedMid - BlueMid - RedAtk - BlueDef - BlueGK
	private const float ROD_POS_1 = -1.00f * SCALE;  // Red Goalkeeper (am nächsten zu Red-Tor)
	private const float ROD_POS_2 = -0.70f * SCALE;  // Red Defense
	private const float ROD_POS_3 = -0.40f * SCALE;  // Blue Attack (greift Red-Tor an)
	private const float ROD_POS_4 = -0.15f * SCALE;  // Red Midfield
	private const float ROD_POS_5 = 0.15f * SCALE;   // Blue Midfield
	private const float ROD_POS_6 = 0.40f * SCALE;   // Red Attack (greift Blue-Tor an)
	private const float ROD_POS_7 = 0.70f * SCALE;   // Blue Defense
	private const float ROD_POS_8 = 1.00f * SCALE;   // Blue Goalkeeper (am nächsten zu Blue-Tor)

	// Figuren-Anzahl pro Rod-Typ
	private const int GOALKEEPER_FIGURES = 1;
	private const int DEFENSE_FIGURES = 2;
	private const int MIDFIELD_FIGURES = 5;
	private const int ATTACK_FIGURES = 3;

	// Rod-Anzahl pro Team
	private const int MAX_RODS_PER_TEAM = 4; // GK, Defense, Midfield, Attack

	// Scene-Referenzen
	private PackedScene _rodScene;

	// Node-Referenzen
	private Node3D _teamRed;
	private Node3D _teamBlue;
	private StaticBody3D _tableBody;
	private Node3D _walls;
	private Node3D _goals;

	// Bot Controller für Singleplayer-Modus
	private BotController _botController;
	private RLBotController _rlBotController; // Trainierter AI Controller
	// Bot Controller für Multiplayer-Modus (einer pro Team falls benötigt)
	private BotController _redTeamBot;
	private BotController _blueTeamBot;
	private GameManager _gameManager;
	private InputManager _inputManager;

	// Rod-Tracking für visuelles Feedback
	private Rod[] _teamRedRods = new Rod[MAX_RODS_PER_TEAM];
	private Rod[] _teamBlueRods = new Rod[MAX_RODS_PER_TEAM];

	// Physics Materials für lebhaftere Abpraller
	private PhysicsMaterial _wallMaterial;
	private PhysicsMaterial _goalMaterial;
	private StandardMaterial3D _wallMaterialVisual;

	/// <summary>
	/// Initialisiert die Tischszene beim Start.
	/// Lädt Manager-Referenzen, verbindet Signals, setzt Dimensionen, spawnt Rods und richtet Bot-Controller ein.
	/// </summary>
	public override void _Ready()
	{
		GD.Print("========== Table._Ready() START ==========");

		// Manager-Referenzen holen
		_gameManager = GetNode<GameManager>("/root/GameManager");
		_inputManager = GetNode<InputManager>("/root/InputManager");

		// GameManager Signals für Rod-Reset verbinden
		if (_gameManager != null)
		{
			_gameManager.RodsResetRequested += OnRodsResetRequested;
		}

		// InputManager Signals für Rod-Auswahl-Feedback verbinden
		if (_inputManager != null)
		{
			_inputManager.RodSelectionChanged += OnRodSelectionChanged;
			_inputManager.ControllerRodPairChanged += OnControllerRodPairChanged;
			_inputManager.InputDeviceChanged += OnInputDeviceChanged;
		}

		// Rod Scene laden
		_rodScene = GD.Load<PackedScene>("res://scenes/game/Rod.tscn");

		if (_rodScene == null)
		{
			GD.PrintErr("Failed to load Rod.tscn!");
			return;
		}
		else
		{
			GD.Print("Successfully loaded Rod.tscn");
		}

		// Physics Materials initialisieren
		InitializePhysicsMaterials();

		// Node-Referenzen holen
		_tableBody = GetNode<StaticBody3D>("TableBody");
		_walls = GetNode<Node3D>("Walls");
		_goals = GetNode<Node3D>("Goals");
		_teamRed = GetNode<Node3D>("TeamRed");
		_teamBlue = GetNode<Node3D>("TeamBlue");

		GD.Print($"Node references - TableBody: {_tableBody != null}, Walls: {_walls != null}, Goals: {_goals != null}, TeamRed: {_teamRed != null}, TeamBlue: {_teamBlue != null}");

		// Tisch-Komponenten einrichten
		SetupTableDimensions();
		SetupGoals();
		SpawnRods();

		// Rod-Auswahl visuelles Feedback initialisieren
		CallDeferred(nameof(InitializeRodSelection));

		GD.Print("========== Table._Ready() END ==========");
	}

	/// <summary>
	/// Trennt Signal-Verbindungen beim Entfernen aus dem Scene-Baum.
	/// </summary>
	public override void _ExitTree()
	{
		if (_gameManager != null)
		{
			_gameManager.RodsResetRequested -= OnRodsResetRequested;
		}

		if (_inputManager != null)
		{
			_inputManager.RodSelectionChanged -= OnRodSelectionChanged;
			_inputManager.ControllerRodPairChanged -= OnControllerRodPairChanged;
			_inputManager.InputDeviceChanged -= OnInputDeviceChanged;
		}
	}

	/// <summary>
	/// Erstellt gemeinsame Physics Materials für Tisch, Wände und Tore.
	/// Optimiert für realistisches Tischfußball-Abprallverhalten.
	/// </summary>
	private void InitializePhysicsMaterials()
	{
		// Wand-Material - guter Abprall mit niedriger Friction für saubere Rebounds
		_wallMaterial = new PhysicsMaterial
		{
			Bounce = 0.8f,      // Höherer Abprall für lebhafte Wand-Rebounds
			Friction = 0.05f,   // Niedrige Friction - Ball soll nicht an Wänden kleben
			Rough = false,
			Absorbent = false
		};

		// Tor-Material - leicht weicherer Abprall wenn Ball ins Tor geht
		_goalMaterial = new PhysicsMaterial
		{
			Bounce = 0.5f,
			Friction = 0.1f,
			Rough = false,      // Durchschnittliche Friction Combine (niedrige Friction hat mehr Einfluss)
			Absorbent = false   // Durchschnittliche Bounce Combine
		};

		_wallMaterialVisual = new StandardMaterial3D
		{
			AlbedoColor = new Color(0.4f, 0.3f, 0.2f),
			Metallic = 0.1f,
			Roughness = 0.8f
		};
	}

	/// <summary>
	/// Initialisiert visuelles Rod-Auswahl-Feedback nachdem alle Rods gespawnt wurden.
	/// Im Singleplayer: nur menschliches Team. Im Multiplayer: beide Teams falls menschlich gesteuert.
	/// </summary>
	private void InitializeRodSelection()
	{
		if (_inputManager == null || _gameManager == null)
			return;

		// Im Singleplayer: Nur das menschliche Spieler-Team initialisieren
		if (!_gameManager.IsMultiplayer)
		{
			// Player 1 (Mensch) kontrolliert sein gewähltes Team
			if (_inputManager.IsUsingController(1))
			{
				var (leftRod, rightRod) = _inputManager.GetControllerSelectedRods(1);
				int activeRod = _inputManager.GetActiveRod(1);
				OnControllerRodPairChanged(1, leftRod, rightRod, activeRod);
			}
			else
			{
				int selectedRod = _inputManager.GetSelectedRod(1);
				OnRodSelectionChanged(1, selectedRod);
			}
		}
		// Im Multiplayer: Beide Teams initialisieren falls menschlich gesteuert
		else
		{
			// Red-Team (Player 1) initialisieren falls nicht Bot-gesteuert
			if (!_gameManager.RedTeamIsBot)
			{
				if (_inputManager.IsUsingController(1))
				{
					var (leftRod, rightRod) = _inputManager.GetControllerSelectedRods(1);
					int activeRod = _inputManager.GetActiveRod(1);
					OnControllerRodPairChanged(1, leftRod, rightRod, activeRod);
				}
				else
				{
					int selectedRod = _inputManager.GetSelectedRod(1);
					OnRodSelectionChanged(1, selectedRod);
				}
			}

			// Blue-Team (Player 2) initialisieren falls nicht Bot-gesteuert
			if (!_gameManager.BlueTeamIsBot)
			{
				if (_inputManager.IsUsingController(2))
				{
					var (leftRod, rightRod) = _inputManager.GetControllerSelectedRods(2);
					int activeRod = _inputManager.GetActiveRod(2);
					OnControllerRodPairChanged(2, leftRod, rightRod, activeRod);
				}
				else
				{
					int selectedRod = _inputManager.GetSelectedRod(2);
					OnRodSelectionChanged(2, selectedRod);
				}
			}
		}

		GD.Print("Rod selection visual feedback initialized");
	}

	/// <summary>
	/// Richtet Tisch- und Wand-Mesh sowie Collider-Dimensionen mit sauberer Kanten-Ausrichtung ein.
	/// </summary>
	private void SetupTableDimensions()
	{
		// Haupt-Tischkörper einrichten
		var tableMesh = _tableBody.GetNode<MeshInstance3D>("TableMesh");


		if (tableMesh != null)
		{
			Vector3 meshSize = tableMesh.Mesh.GetAabb().Size;

			tableMesh.Scale = new Vector3(
				TABLE_LENGTH / meshSize.X,
				TABLE_HEIGHT / meshSize.Y,
				TABLE_WIDTH / meshSize.Z
			);

			tableMesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
		}
		


		// Tischkörper Physics konfigurieren
		if (_tableBody != null)
		{
			_tableBody.PhysicsMaterialOverride = _wallMaterial;

			// Collision Layers setzen - Tisch ist auf Layer 1
			_tableBody.CollisionLayer = 1;
			_tableBody.CollisionMask = 2;  // Kollidiert mit Ball
		}

		// Dimensionen für Tor-Seitenwände berechnen
		// Seitenwände füllen die Lücke zwischen Torpfosten und Haupt-Tischwänden
		// Torpfosten äußere Kante liegt bei: GOAL_WIDTH / 2 + GOAL_FRAME_THICKNESS
		// Hauptwand innere Kante liegt bei: TABLE_WIDTH / 2
		float goalPostOuterEdge = GOAL_WIDTH / 2 + GOAL_FRAME_THICKNESS;
		float sideWallLength = TABLE_WIDTH+0.7f;
		float sideWallZPosNorth = goalPostOuterEdge + sideWallLength / 2;
		float sideWallZPosSouth = -sideWallZPosNorth;

		// Hauptwand-Länge - spannt sich zwischen den Tor-Seitenwänden (ohne Ecken)
		float mainWallLength = TABLE_LENGTH+0.7f;
		float goalSideWallX = TABLE_LENGTH / 2 + WALL_THICKNESS / 2;
		float mainWallThickness = WALL_THICKNESS / 1.9f * 2.39018f;

		// Seitenwände einrichten (Nord/Süd) - horizontale Hauptwände entlang des Tisches
		// WallNorth (positive Z) ist spielerseitig (Camera schaut von positive Z) - diese niedriger machen
		SetupWall("WallNorth", new Vector3(0, (WALL_HEIGHT / 2)-0.5f, TABLE_WIDTH / 2 + WALL_THICKNESS / 2), mainWallLength, WALL_HEIGHT, WALL_THICKNESS);
		SetupWall("WallSouth", new Vector3(0, WALL_HEIGHT / 2, -TABLE_WIDTH / 2 - WALL_THICKNESS / 2), mainWallLength, WALL_HEIGHT, mainWallThickness);
		SetupWall("WallEast", new Vector3(-goalSideWallX, WALL_HEIGHT / 2, 0), WALL_THICKNESS, WALL_HEIGHT, sideWallLength);
		SetupWall("WallWest", new Vector3(goalSideWallX, WALL_HEIGHT / 2, 0), WALL_THICKNESS, WALL_HEIGHT, sideWallLength);

		// Tor-Seitenwände - braune Wände neben den Toröffnungen (gleich wie Tischrahmen)

		// West Tor-Seitenwände (Red-Tor) - braun wie der Rest des Tisches

		// East Tor-Seitenwände (Blue-Tor) - braun wie der Rest des Tisches

		// Eck-Blöcke - füllen die Lücken an den vier Ecken des Tischrahmens
		// Positioniert an der Kreuzung von Hauptwänden und Tor-Seitenwänden

		// Eckhöhen entsprechen den angrenzenden Wandhöhen
		
	}

	/// <summary>
	/// Richtet eine Wand mit korrekten Dimensionen und Collision ein.
	/// Erstellt Wand dynamisch falls sie noch nicht in der Scene existiert.
	/// </summary>
	private void SetupWall(string wallName, Vector3 position, float length, float height, float thickness)
	{
		var wall = _walls.GetNodeOrNull<StaticBody3D>(wallName);

		// Node-Namen in Table.tscn ist WallNorthMesh
		var wallMesh = wall.GetNodeOrNull<MeshInstance3D>($"{wallName}Mesh");

		wall.Position = position;
		wall.PhysicsMaterialOverride = _wallMaterial;

		// Collision Layers setzen - Wände sind auf Layer 1, kollidieren mit Ball (Layer 2)
		wall.CollisionLayer = 1;  // Layer 1 für Wände/Tisch
		wall.CollisionMask = 2;   // Kollidiert mit Ball

		if (wallMesh == null)
		{
			GD.PrintErr($"WallMesh for {wallName} not found!");
			return;
		}


		Vector3 meshSize=wallMesh.Mesh.GetAabb().Size;
		
		
		wallMesh.Scale = new Vector3(
			length / meshSize.X,
			($"{wallName}Mesh"!="WallNorthMesh") ? (height / meshSize.Y) : (meshSize.Y),
			thickness / meshSize.Z
		);


		GD.Print($"Wall {wallName} configured at position {position} with size ({length}, {height}, {thickness})");
	}

	/// <summary>
	/// Fügt einen Eck-Füllblock für nahtlose äußere Rahmenverbindungen hinzu.
	/// </summary>
	private void SetupCorner(string cornerName, Vector3 position, Vector3 size)
	{
		var corner = _walls.GetNodeOrNull<StaticBody3D>(cornerName);
		MeshInstance3D cornerMesh;
		CollisionShape3D cornerCollider;

		if (corner == null)
		{
			corner = new StaticBody3D();
			corner.Name = cornerName;
			_walls.AddChild(corner);
			corner.Owner = GetTree().EditedSceneRoot;

			cornerMesh = new MeshInstance3D();
			cornerMesh.Name = $"{cornerName}Mesh";
			corner.AddChild(cornerMesh);
			cornerMesh.Owner = GetTree().EditedSceneRoot;

			cornerCollider = new CollisionShape3D();
			cornerCollider.Name = $"{cornerName}Collider";
			corner.AddChild(cornerCollider);
			cornerCollider.Owner = GetTree().EditedSceneRoot;
		}
		else
		{
			cornerMesh = corner.GetNodeOrNull<MeshInstance3D>($"{cornerName}Mesh");
			cornerCollider = corner.GetNodeOrNull<CollisionShape3D>($"{cornerName}Collider");
		}

		corner.Position = position;
		corner.PhysicsMaterialOverride = _wallMaterial;

		// Collision Layers setzen - gleich wie Wände
		corner.CollisionLayer = 1;
		corner.CollisionMask = 2;

		var boxMesh = new BoxMesh();
		boxMesh.Size = size;
		cornerMesh.Mesh = boxMesh;
		if (_wallMaterialVisual != null)
			cornerMesh.SetSurfaceOverrideMaterial(0, _wallMaterialVisual);

		var boxShape = new BoxShape3D();
		boxShape.Size = size;
		cornerCollider.Shape = boxShape;
	}

	/// <summary>
	/// Richtet beide Tore mit korrekten Dimensionen, Positionen und Team-Farben ein.
	/// </summary>
	private void SetupGoals()
	{
		GD.Print("SetupGoals() - Configuring goal visibility and dimensions...");

		// Tor-Positionen - so positioniert dass Torrahmen bündig am Tischrand sitzt
		// Tor erstreckt sich nach außen vom Tischrand um GOAL_DEPTH
		float goalX = TABLE_LENGTH / 2;

		// Red-Tor einrichten (linke Seite, negative X)
		SetupGoal("GoalRed", new Vector3(-goalX, 0, 0), new Color(0.8f, 0.2f, 0.2f)); // Rot

		// Blue-Tor einrichten (rechte Seite, positive X)
		SetupGoal("GoalBlue", new Vector3(goalX, 0, 0), new Color(0.2f, 0.3f, 0.9f)); // Blau

		GD.Print("SetupGoals() - Complete");
	}

	/// <summary>
	/// Richtet ein einzelnes Tor mit U-förmigem Rahmen (Pfosten + Querlatte), Rückwand und Trigger-Bereich ein.
	/// </summary>
	private void SetupGoal(string goalName, Vector3 position, Color teamColor)
	{
		var goal = _goals.GetNodeOrNull<Node3D>(goalName);
		if (goal == null)
		{
			GD.PrintErr($"Goal {goalName} not found!");
			return;
		}

		goal.Position = position;
		bool isRedGoal = goalName == "GoalRed";
		float xDirection = isRedGoal ? -1f : 1f; // Richtung in die das Tor zeigt

		GD.Print($"Setting up {goalName} at position {position}");

		// Tor-Material mit Team-Farbe erstellen
		var goalMaterial = new StandardMaterial3D();
		goalMaterial.AlbedoColor = teamColor;
		goalMaterial.Metallic = 0.4f;
		goalMaterial.Roughness = 0.6f;

		// Torrahmen einrichten - Pfosten und Querlatte an der Toröffnung
		var goalFrame = goal.GetNodeOrNull<StaticBody3D>("GoalFrame");



		var frameMesh = goalFrame.GetNodeOrNull<MeshInstance3D>("GoalFrameMesh");
		Vector3 meshSize=frameMesh.Mesh.GetAabb().Size;
		
		frameMesh.Scale = new Vector3(
			GOAL_FRAME_THICKNESS/meshSize.X,
			GOAL_HEIGHT/meshSize.Y,
			GOAL_WIDTH/meshSize.Z
		);
		
		frameMesh.SetSurfaceOverrideMaterial(0, goalMaterial);


		goalFrame.Position = new Vector3(0, GOAL_HEIGHT / 2, 0);




		GD.Print($"{goalName} - Goal frame created");


		// Setup Goal Back - invisible collision only (to stop ball)
		var goalBack = goal.GetNodeOrNull<StaticBody3D>("GoalBack");
		if (goalBack != null)
		{
			goalBack.PhysicsMaterialOverride = _goalMaterial;

			// Position backboard at the back of the goal
			goalBack.Position = new Vector3(xDirection * (GOAL_DEPTH + GOAL_FRAME_THICKNESS), GOAL_HEIGHT / 2, 0);

			// Hide the visual mesh
			var backMesh = goalBack.GetNodeOrNull<MeshInstance3D>("GoalBackMesh");
			if (backMesh != null)
			{
				backMesh.Visible = false;
			}

			// Keep collision for ball stopping
			var backCollider = goalBack.GetNodeOrNull<CollisionShape3D>("GoalBackCollider");
			if (backCollider != null)
			{
				var boxShape = new BoxShape3D();
				boxShape.Size = new Vector3(GOAL_BACK_THICKNESS, GOAL_HEIGHT, GOAL_WIDTH);
				backCollider.Shape = boxShape;
			}

			GD.Print($"{goalName} - Backboard collision (invisible)");
		}

		// Setup Goal Trigger (Area3D for scoring detection)
		var goalTrigger = goal.GetNodeOrNull<Area3D>("GoalTrigger");
		if (goalTrigger != null)
		{
			// Position trigger inside the goal area
			goalTrigger.Position = new Vector3(xDirection * (GOAL_DEPTH / 2 + GOAL_FRAME_THICKNESS), GOAL_HEIGHT / 2, 0);

			var triggerCollider = goalTrigger.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
			if (triggerCollider != null)
			{
				var boxShape = new BoxShape3D();
				// Trigger zone fills the goal opening
				boxShape.Size = new Vector3(GOAL_DEPTH * 0.8f, GOAL_HEIGHT * 0.9f, GOAL_WIDTH * 0.8f);
				triggerCollider.Shape = boxShape;

				GD.Print($"{goalName} - Trigger configured with size {boxShape.Size}");
			}
		}
	}

	/// <summary>
	/// Spawns all rods for both teams in correct foosball order from left to right.
	/// Correct sequence: 1R(GK) - 2R(Def) - 2B(Def) - 5R(Mid) - 5B(Mid) - 3R(Atk) - 3B(Atk) - 1B(GK)
	/// TeamRed defends LEFT (negative X), TeamBlue defends RIGHT (positive X)
	/// </summary>
	private void SpawnRods()
	{
		GD.Print("SpawnRods() - Creating all rods in correct foosball order...");

		// Check if this is Online Multiplayer (needed later for rod PlayerIndex assignment)
		var onlineManager = GetNodeOrNull("/root/OnlineMultiplayerManager");
		bool isOnlineMultiplayer = false;

		if (onlineManager != null)
		{
			var isConnected = onlineManager.Call("is_connected_to_peer");
			isOnlineMultiplayer = isConnected.AsBool();
		}

		// Setup bot controller for singleplayer mode
		if (_gameManager != null && !_gameManager.IsMultiplayer)
		{
			// Determine which team the bot controls (opposite of player's team)
			var botTeam = _gameManager.PlayerTeam == GameManager.Team.Red
				? GameManager.Team.Blue
				: GameManager.Team.Red;

			// Check if we should use the Trained AI (ONNX) or Rule-based bot
			if (_gameManager.CurrentBotType == GameManager.BotType.TrainedAI)
			{
				GD.Print("Singleplayer mode - creating RLBotController (Trained AI)");
				_rlBotController = new RLBotController();
				_rlBotController.Name = "RLBotController";
				_rlBotController.ControlledTeam = botTeam;
				AddChild(_rlBotController);
				GD.Print($"RLBotController created - Trained AI controls {botTeam}, Player controls {_gameManager.PlayerTeam}");
			}
			else
			{
				GD.Print("Singleplayer mode - creating BotController (Rule-based)");
				_botController = new BotController();
				_botController.Name = "BotController";
				_botController.ControlledTeam = botTeam;
				_botController.Difficulty = _gameManager.BotDifficulty;
				AddChild(_botController);
				GD.Print($"BotController created - Bot controls {botTeam}, Player controls {_gameManager.PlayerTeam}, Difficulty: {_botController.Difficulty}");
			}
		}
		// Setup bot controllers for multiplayer mode
		else if (_gameManager != null && _gameManager.IsMultiplayer)
		{
			GD.Print("Multiplayer mode detected - checking for bot players");

			if (isOnlineMultiplayer)
			{
				GD.Print("Online Multiplayer detected - skipping BotController creation (using network control)");
				// In Online Multiplayer: keine BotController erstellen!
				// Das gegnerische Team wird über Netzwerk gesteuert (NetworkGameSync)
			}
			else
			{
				// Local Multiplayer (Splitscreen/Hotseat) - normale Bot-Logik

				// Create bot for Red team if needed
				if (_gameManager.RedTeamIsBot)
				{
					_redTeamBot = new BotController();
					_redTeamBot.Name = "RedTeamBot";
					_redTeamBot.ControlledTeam = GameManager.Team.Red;
					_redTeamBot.Difficulty = _gameManager.RedTeamBotDifficulty;
					AddChild(_redTeamBot);
					GD.Print($"Red Team Bot created - Difficulty: {_redTeamBot.Difficulty}");
				}

				// Create bot for Blue team if needed
				if (_gameManager.BlueTeamIsBot)
				{
					_blueTeamBot = new BotController();
					_blueTeamBot.Name = "BlueTeamBot";
					_blueTeamBot.ControlledTeam = GameManager.Team.Blue;
					_blueTeamBot.Difficulty = _gameManager.BlueTeamBotDifficulty;
					AddChild(_blueTeamBot);
					GD.Print($"Blue Team Bot created - Difficulty: {_blueTeamBot.Difficulty}");
				}
			}
		}

		// Standard foosball rod layout from left (Red goal) to right (Blue goal):
		// In Online Multiplayer: Lokaler Spieler ist immer Player 1, egal welches Team
		int redPlayerIndex = 1;
		int bluePlayerIndex = 2;

		if (isOnlineMultiplayer && _gameManager != null)
		{
			// PlayerIndex ALWAYS matches team color (InputManager expects this)
			// Red team = Player 1, Blue team = Player 2
			// Network control determines whether rods respond to local input
			redPlayerIndex = 1;
			bluePlayerIndex = 2;
		}

		// Position 1: Red Goalkeeper (1 figure) - closest to Red goal
		SpawnRod(_teamRed, GameManager.Team.Red, Rod.RodType.Goalkeeper, ROD_POS_1, GOALKEEPER_FIGURES, redPlayerIndex);

		// Position 2: Red Defense (2 figures)
		SpawnRod(_teamRed, GameManager.Team.Red, Rod.RodType.Defense, ROD_POS_2, DEFENSE_FIGURES, redPlayerIndex);

		// Position 3: Blue Attack (3 figures) - attacking Red's goal
		SpawnRod(_teamBlue, GameManager.Team.Blue, Rod.RodType.Attack, ROD_POS_3, ATTACK_FIGURES, bluePlayerIndex);

		// Position 4: Red Midfield (5 figures)
		SpawnRod(_teamRed, GameManager.Team.Red, Rod.RodType.Midfield, ROD_POS_4, MIDFIELD_FIGURES, redPlayerIndex);

		// Position 5: Blue Midfield (5 figures)
		SpawnRod(_teamBlue, GameManager.Team.Blue, Rod.RodType.Midfield, ROD_POS_5, MIDFIELD_FIGURES, bluePlayerIndex);

		// Position 6: Red Attack (3 figures) - attacking Blue's goal
		SpawnRod(_teamRed, GameManager.Team.Red, Rod.RodType.Attack, ROD_POS_6, ATTACK_FIGURES, redPlayerIndex);

		// Position 7: Blue Defense (2 figures)
		SpawnRod(_teamBlue, GameManager.Team.Blue, Rod.RodType.Defense, ROD_POS_7, DEFENSE_FIGURES, bluePlayerIndex);

		// Position 8: Blue Goalkeeper (1 figure) - closest to Blue goal
		SpawnRod(_teamBlue, GameManager.Team.Blue, Rod.RodType.Goalkeeper, ROD_POS_8, GOALKEEPER_FIGURES, bluePlayerIndex);

		GD.Print($"SpawnRods() - Finished. TeamRed children: {_teamRed.GetChildCount()}, TeamBlue children: {_teamBlue.GetChildCount()}");
	}

	/// <summary>
	/// Spawns a single rod with specified configuration.
	/// </summary>
	private void SpawnRod(Node3D parent, GameManager.Team team, Rod.RodType type, float xPos, int figureCount, int playerIndex)
	{
		GD.Print($"Creating rod: Team={team}, Type={type}, Pos=({xPos}, {ROD_Y}, 0), Figures={figureCount}");

		var rod = _rodScene.Instantiate<Rod>();

		if (rod == null)
		{
			GD.PrintErr("Failed to instantiate Rod!");
			return;
		}

		// Configure rod
		rod.Team = team;
		rod.Type = type;
		rod.FigureCount = figureCount;
		rod.PlayerIndex = playerIndex;

		// Position rod above table surface
		rod.Position = new Vector3(xPos, ROD_Y, 0);

		// Add to team parent
		parent.AddChild(rod);

		// Store rod reference in tracking arrays
		int rodIndex = type switch
		{
			Rod.RodType.Goalkeeper => 0,
			Rod.RodType.Defense => 1,
			Rod.RodType.Midfield => 2,
			Rod.RodType.Attack => 3,
			_ => 0
		};

		if (team == GameManager.Team.Red)
			_teamRedRods[rodIndex] = rod;
		else
			_teamBlueRods[rodIndex] = rod;

		// Register rod with bot controller if it's controlled by AI in singleplayer
		if (_botController != null && team == _botController.ControlledTeam)
		{
			// Defer registration to ensure rod is fully initialized
			CallDeferred(nameof(RegisterRodWithBot), rod);
		}

		// Register rod with RL bot controller (needs both own and opponent rods)
		if (_rlBotController != null)
		{
			CallDeferred(nameof(RegisterRodWithRLBot), rod, (int)team);
		}

		GD.Print($"Rod added to {parent.Name}. Global position: {rod.GlobalPosition}");
	}

	/// <summary>
	/// Registers a rod with the bot controller (deferred call).
	/// </summary>
	private void RegisterRodWithBot(Rod rod)
	{
		if (_botController != null && rod != null)
		{
			_botController.RegisterRod(rod);
		}
	}

	/// <summary>
	/// Registers a rod with the RL bot controller (deferred call).
	/// RL bot needs both own rods and opponent rods for observations.
	/// </summary>
	private void RegisterRodWithRLBot(Rod rod, int teamInt)
	{
		if (_rlBotController == null || rod == null)
			return;

		var team = (GameManager.Team)teamInt;
		if (team == _rlBotController.ControlledTeam)
		{
			_rlBotController.RegisterRod(rod);
		}
		else
		{
			_rlBotController.RegisterOpponentRod(rod);
		}
	}

	/// <summary>
	/// Handles keyboard rod selection changes - single rod at a time.
	/// In singleplayer: only Player 1's team gets highlighting
	/// In multiplayer: both teams get highlighting
	/// </summary>
	private void OnRodSelectionChanged(int player, int rodIndex)
	{
		GD.Print($"Table: Rod selection changed - Player {player}, Rod {rodIndex}");

		// Determine which team the player controls
		GameManager.Team playerTeam;
		Rod[] rods;

		if (_gameManager.IsMultiplayer)
		{
			// In multiplayer, use the player index from the signal (1=Red, 2=Blue)
			if (player == 1)
			{
				if (_gameManager.RedTeamIsBot)
					return;
				playerTeam = GameManager.Team.Red;
				rods = _teamRedRods;
			}
			else
			{
				if (_gameManager.BlueTeamIsBot)
					return;
				playerTeam = GameManager.Team.Blue;
				rods = _teamBlueRods;
			}
		}
		else
		{
			// In singleplayer, ignore Player 2 events (bot team shouldn't be highlighted)
			if (player == 2)
				return;

			playerTeam = _gameManager.PlayerTeam;
			rods = (playerTeam == GameManager.Team.Red) ? _teamRedRods : _teamBlueRods;
		}

		// Reset all rods of this team to normal state, then highlight selected
		for (int i = 0; i < MAX_RODS_PER_TEAM; i++)
		{
			if (rods[i] != null)
			{
				rods[i].SetSelectionState(Rod.SelectionState.Normal);
			}
		}

		// Highlight the selected rod as active
		if (rodIndex >= 0 && rodIndex < MAX_RODS_PER_TEAM && rods[rodIndex] != null)
		{
			rods[rodIndex].SetSelectionState(Rod.SelectionState.Active);
		}
	}

	/// <summary>
	/// Handles controller rod pair changes - highlights two rods (one from each pair).
	/// In singleplayer: only Player 1's team gets highlighting
	/// In multiplayer: both teams get highlighting
	/// </summary>
	private void OnControllerRodPairChanged(int player, int leftRod, int rightRod, int activeRod)
	{
		GD.Print($"Table: Controller rod pair changed - Player {player}, Left={leftRod}, Right={rightRod}");

		// Determine which team the player controls
		GameManager.Team playerTeam;
		Rod[] rods;

		if (_gameManager.IsMultiplayer)
		{
			// In multiplayer, use the player index from the signal (1=Red, 2=Blue)
			if (player == 1)
			{
				if (_gameManager.RedTeamIsBot)
					return;
				playerTeam = GameManager.Team.Red;
				rods = _teamRedRods;
			}
			else
			{
				if (_gameManager.BlueTeamIsBot)
					return;
				playerTeam = GameManager.Team.Blue;
				rods = _teamBlueRods;
			}
		}
		else
		{
			// In singleplayer, ignore Player 2 events (bot team shouldn't be highlighted)
			if (player == 2)
				return;

			playerTeam = _gameManager.PlayerTeam;
			rods = (playerTeam == GameManager.Team.Red) ? _teamRedRods : _teamBlueRods;
		}

		// Reset all rods of this team to normal state
		for (int i = 0; i < MAX_RODS_PER_TEAM; i++)
		{
			if (rods[i] != null)
			{
				rods[i].SetSelectionState(Rod.SelectionState.Normal);
			}
		}

		// Highlight the active rod from each pair
		if (leftRod >= 0 && leftRod < MAX_RODS_PER_TEAM && rods[leftRod] != null)
		{
			rods[leftRod].SetSelectionState(Rod.SelectionState.Active);
		}
		if (rightRod >= 0 && rightRod < MAX_RODS_PER_TEAM && rods[rightRod] != null)
		{
			rods[rightRod].SetSelectionState(Rod.SelectionState.Active);
		}
	}

	/// <summary>
	/// Handles input device changes (keyboard <-> controller switch).
	/// Updates rod highlighting based on new input mode.
	/// </summary>
	private void OnInputDeviceChanged(int player, int newDeviceInt)
	{
		var newDevice = (InputManager.InputDevice)newDeviceInt;
		GD.Print($"Table: Input device changed - Player {player} now using {newDevice}");

		// Determine which team the player controls
		GameManager.Team playerTeam;
		Rod[] rods;

		if (_gameManager.IsMultiplayer)
		{
			// In multiplayer, use the player index from the signal (1=Red, 2=Blue)
			if (player == 1)
			{
				if (_gameManager.RedTeamIsBot)
					return;
				playerTeam = GameManager.Team.Red;
				rods = _teamRedRods;
			}
			else
			{
				if (_gameManager.BlueTeamIsBot)
					return;
				playerTeam = GameManager.Team.Blue;
				rods = _teamBlueRods;
			}
		}
		else
		{
			// In singleplayer, only handle player 1
			if (player != 1)
				return;

			playerTeam = _gameManager.PlayerTeam;
			rods = playerTeam == GameManager.Team.Red ? _teamRedRods : _teamBlueRods;
		}

		// Reset all rods to normal first
		for (int i = 0; i < MAX_RODS_PER_TEAM; i++)
		{
			if (rods[i] != null)
			{
				rods[i].SetSelectionState(Rod.SelectionState.Normal);
			}
		}

		// Apply appropriate highlighting based on input mode
		if (newDevice == InputManager.InputDevice.Keyboard || newDevice == InputManager.InputDevice.Keyboard2)
		{
			// Keyboard: highlight selected rod
			int selectedRod = _inputManager.GetSelectedRod(player);
			if (selectedRod >= 0 && selectedRod < MAX_RODS_PER_TEAM && rods[selectedRod] != null)
			{
				rods[selectedRod].SetSelectionState(Rod.SelectionState.Active);
			}
		}
		else
		{
			// Controller: highlight both active rods from each pair
			var (leftRod, rightRod) = _inputManager.GetControllerSelectedRods(player);
			if (leftRod >= 0 && leftRod < MAX_RODS_PER_TEAM && rods[leftRod] != null)
			{
				rods[leftRod].SetSelectionState(Rod.SelectionState.Active);
			}
			if (rightRod >= 0 && rightRod < MAX_RODS_PER_TEAM && rods[rightRod] != null)
			{
				rods[rightRod].SetSelectionState(Rod.SelectionState.Active);
			}
		}
	}

	/// <summary>
	/// Gets the opponent team.
	/// </summary>
	private GameManager.Team GetOpponentTeam(GameManager.Team team)
	{
		return team == GameManager.Team.Red ? GameManager.Team.Blue : GameManager.Team.Red;
	}

	/// <summary>
	/// Resets all rods to their starting positions (called after a goal).
	/// </summary>
	private void OnRodsResetRequested()
	{
		GD.Print("Table: Resetting all rods to start positions...");

		// Reset all Red team rods
		for (int i = 0; i < MAX_RODS_PER_TEAM; i++)
		{
			if (_teamRedRods[i] != null)
			{
				_teamRedRods[i].ResetPosition();
			}
		}

		// Reset all Blue team rods
		for (int i = 0; i < MAX_RODS_PER_TEAM; i++)
		{
			if (_teamBlueRods[i] != null)
			{
				_teamBlueRods[i].ResetPosition();
			}
		}

		GD.Print("Table: All rods reset complete");
	}
}
