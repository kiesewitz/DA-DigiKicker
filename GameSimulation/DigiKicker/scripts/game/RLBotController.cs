using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

/// <summary>
/// RL Bot Controller der ein trainiertes ONNX-Modell für Inferenz verwendet.
/// Spiegelt Observations für das rote Team, sodass beide Seiten dasselbe Modell nutzen können.
/// </summary>
public partial class RLBotController : Node
{
	[Export] public GameManager.Team ControlledTeam { get; set; } = GameManager.Team.Blue;
	[Export] public string ModelPath { get; set; } = "res://models/foosball_ai.onnx";

	// ONNX Runtime Komponenten
	private InferenceSession _session;
	private bool _modelLoaded = false;

	// Referenzen zu Spielobjekten
	private List<Rod> _controlledRods = new List<Rod>();
	private List<Rod> _opponentRods = new List<Rod>();
	private Node3D _ball;
	private RigidBody3D _ballRigidBody;
	private GameManager _gameManager;

	// Spiegelfaktor: 1 für Blue (rechte Seite), -1 für Red (linke Seite)
	private float _mirror = 1.0f;

	// Observation Buffer (20 Floats)
	private float[] _observations = new float[20];

	// Action Smoothing um Zittern zu reduzieren
	private float[] _currentActions = new float[8];
	private float[] _targetActions = new float[8];
	private const float ACTION_SMOOTHING = 0.15f; // Niedriger = glatter aber langsamere Reaktion

	// Action Repeat - Inferenz nur alle N Frames ausführen
	private int _frameCounter = 0;
	private const int ACTION_REPEAT = 4; // Inferenz alle 4 Frames (~12.5 Hz statt 50 Hz)

	/// <summary>
	/// Initialisiert den RL Bot Controller beim Laden der Scene.
	/// </summary>
	public override void _Ready()
	{
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		_mirror = ControlledTeam == GameManager.Team.Blue ? 1.0f : -1.0f;

		LoadModel();
		CallDeferred(nameof(FindBall));

		GD.Print($"RLBotController initialized - Team: {ControlledTeam}, Mirror: {_mirror}");
	}

	/// <summary>
	/// Lädt das ONNX-Modell für die Inferenz.
	/// Konvertiert Godot res:// Pfad in absoluten Pfad und prüft auf externe Datendateien.
	/// </summary>
	private void LoadModel()
	{
		try
		{
			// Godot res:// Pfad in absoluten Pfad konvertieren
			string absolutePath = ProjectSettings.GlobalizePath(ModelPath);
			GD.Print($"Loading ONNX model from: {absolutePath}");

			// Prüfen ob externe Datendatei existiert (für Modelle mit separaten Gewichten)
			string dataPath = absolutePath + ".data";
			if (System.IO.File.Exists(dataPath))
			{
				GD.Print($"Found external data file: {dataPath}");
			}

			var sessionOptions = new SessionOptions();
			_session = new InferenceSession(absolutePath, sessionOptions);
			_modelLoaded = true;

			GD.Print("ONNX model loaded successfully!");
			GD.Print($"  Inputs: {string.Join(", ", _session.InputMetadata.Keys)}");
			GD.Print($"  Outputs: {string.Join(", ", _session.OutputMetadata.Keys)}");
		}
		catch (Exception e)
		{
			GD.PrintErr($"Failed to load ONNX model: {e.Message}");
			_modelLoaded = false;
		}
	}

	/// <summary>
	/// Findet den Ball im Scene Tree.
	/// Wird verzögert aufgerufen um sicherzustellen dass alle Nodes bereit sind.
	/// </summary>
	private void FindBall()
	{
		_ball = GetTree().Root.FindChild("Ball", true, false) as Node3D;
		_ballRigidBody = _ball as RigidBody3D;

		if (_ball == null)
			GD.PrintErr("RLBotController: Ball not found!");
		else
			GD.Print($"RLBotController: Ball found at {_ball.GlobalPosition}");
	}

	#region Rod Management

	/// <summary>
	/// Registriert eine kontrollierte Stange (eigenes Team).
	/// Stangen werden nach Typ sortiert (Goalkeeper=0, Defense=1, Midfield=2, Attack=3).
	/// </summary>
	public void RegisterRod(Rod rod)
	{
		if (!_controlledRods.Contains(rod))
		{
			_controlledRods.Add(rod);
			// Nach Stangentyp sortieren (Goalkeeper=0, Defense=1, Midfield=2, Attack=3)
			_controlledRods = _controlledRods.OrderBy(r => (int)r.Type).ToList();
			GD.Print($"RLBotController: Registered {rod.Team} {rod.Type}");
		}
	}

	/// <summary>
	/// Registriert eine gegnerische Stange für Observation-Zwecke.
	/// </summary>
	public void RegisterOpponentRod(Rod rod)
	{
		if (!_opponentRods.Contains(rod))
		{
			_opponentRods.Add(rod);
			_opponentRods = _opponentRods.OrderBy(r => (int)r.Type).ToList();
		}
	}

	/// <summary>
	/// Entfernt eine Stange aus allen Listen.
	/// </summary>
	public void UnregisterRod(Rod rod)
	{
		_controlledRods.Remove(rod);
		_opponentRods.Remove(rod);
	}

	/// <summary>
	/// Gibt eine Kopie der kontrollierten Stangen zurück.
	/// </summary>
	public List<Rod> GetControlledRods() => new List<Rod>(_controlledRods);

	/// <summary>
	/// Löscht alle registrierten Stangen.
	/// </summary>
	public void ClearRods()
	{
		_controlledRods.Clear();
		_opponentRods.Clear();
	}

	#endregion

	/// <summary>
	/// Physics-Update für AI-Steuerung.
	/// Führt Inferenz aus, glättet Actions und wendet sie auf die Stangen an.
	/// </summary>
	public override void _PhysicsProcess(double delta)
	{
		if (!_modelLoaded || _ball == null || _controlledRods.Count == 0)
			return;

		// Nicht während Countdown, Pause oder Wartezuständen verarbeiten
		bool shouldPause = _gameManager != null && (
			_gameManager.CurrentState != GameManager.GameState.Playing ||
			_gameManager.IsWaitingForCountdown ||
			_gameManager.IsCountdownActive
		);

		if (shouldPause)
		{
			// Alle Stangen auf Null-Input setzen
			foreach (var rod in _controlledRods)
			{
				rod?.SetBotInput(Vector2.Zero);
			}
			// Geglättete Actions zurücksetzen
			for (int i = 0; i < 8; i++)
			{
				_currentActions[i] = 0f;
				_targetActions[i] = 0f;
			}
			return;
		}

		// Inferenz nur alle ACTION_REPEAT Frames ausführen
		_frameCounter++;
		if (_frameCounter >= ACTION_REPEAT)
		{
			_frameCounter = 0;

			// Observations zusammenstellen
			BuildObservations();

			// Inferenz ausführen und Ziel-Actions aktualisieren
			_targetActions = RunInference();
		}

		// Aktuelle Actions in jedem Frame zu Ziel-Actions glätten
		for (int i = 0; i < 8; i++)
		{
			_currentActions[i] = Mathf.Lerp(_currentActions[i], _targetActions[i], ACTION_SMOOTHING);
		}

		// Geglättete Actions auf Stangen anwenden
		ApplyActions(_currentActions);
	}

	/// <summary>
	/// Erstellt den Observation-Vektor für die Neural Network Inferenz.
	/// Beinhaltet Ball-Position/Geschwindigkeit, eigene Stangen und gegnerische Stangen (20 Werte).
	/// </summary>
	private void BuildObservations()
	{
		int idx = 0;

		// Ball Observation (normalisiert)
		if (_ballRigidBody != null)
		{
			Vector3 ballPos = _ballRigidBody.GlobalPosition;
			Vector3 ballVel = _ballRigidBody.LinearVelocity;

			// Positionen normalisieren (Tisch ist ca. -6 bis 6 auf X, -3 bis 3 auf Z)
			// Spiegelfaktor für Red Team anwenden
			_observations[idx++] = _mirror * ballPos.X / 6.0f;
			_observations[idx++] = ballPos.Z / 3.0f;
			// Geschwindigkeiten normalisieren
			_observations[idx++] = _mirror * Mathf.Clamp(ballVel.X / 10.0f, -1.0f, 1.0f);
			_observations[idx++] = Mathf.Clamp(ballVel.Z / 10.0f, -1.0f, 1.0f);
		}
		else
		{
			_observations[idx++] = 0f;
			_observations[idx++] = 0f;
			_observations[idx++] = 0f;
			_observations[idx++] = 0f;
		}

		// Eigene Stangen Observations (4 Stangen × 2 Werte = 8)
		for (int i = 0; i < 4; i++)
		{
			if (i < _controlledRods.Count && _controlledRods[i] != null)
			{
				var rod = _controlledRods[i];
				_observations[idx++] = rod.CurrentLateralOffset / 3.0f;
				_observations[idx++] = rod.Rotation.Z / Mathf.Pi;
			}
			else
			{
				_observations[idx++] = 0f;
				_observations[idx++] = 0f;
			}
		}

		// Gegnerische Stangen Observations (4 Stangen × 2 Werte = 8)
		for (int i = 0; i < 4; i++)
		{
			if (i < _opponentRods.Count && _opponentRods[i] != null)
			{
				var rod = _opponentRods[i];
				_observations[idx++] = rod.CurrentLateralOffset / 3.0f;
				_observations[idx++] = rod.Rotation.Z / Mathf.Pi;
			}
			else
			{
				_observations[idx++] = 0f;
				_observations[idx++] = 0f;
			}
		}
	}

	/// <summary>
	/// Führt ONNX Inferenz aus und gibt Action-Vektor zurück (8 Werte für 4 Stangen).
	/// Unterstützt sowohl feedforward als auch recurrent Policies.
	/// </summary>
	private float[] RunInference()
	{
		float[] actions = new float[8];

		try
		{
			// Input Tensor erstellen
			var inputTensor = new DenseTensor<float>(_observations, new[] { 1, 20 });

			// Inputs Dictionary erstellen
			var inputs = new List<NamedOnnxValue>
			{
				NamedOnnxValue.CreateFromTensor("obs", inputTensor)
			};

			// Prüfen ob Modell state_outs Input benötigt (für recurrent Policies)
			if (_session.InputMetadata.ContainsKey("state_outs"))
			{
				var stateIn = new DenseTensor<float>(new float[] { 0.0f }, new[] { 1 });
				inputs.Add(NamedOnnxValue.CreateFromTensor("state_outs", stateIn));
			}

			// Inferenz ausführen
			using var results = _session.Run(inputs);

			// Output abrufen - suche nach "output" oder "actions" oder ersten Output
			var outputName = _session.OutputMetadata.Keys.FirstOrDefault(k =>
				k.Contains("output") || k.Contains("action"))
				?? _session.OutputMetadata.Keys.First();

			var output = results.First(r => r.Name == outputName);
			var outputTensor = output.AsTensor<float>();

			// Actions kopieren und auf -1 bis 1 begrenzen
			for (int i = 0; i < Math.Min(8, outputTensor.Length); i++)
			{
				actions[i] = Mathf.Clamp(outputTensor.GetValue(i), -1.0f, 1.0f);
			}
		}
		catch (Exception e)
		{
			GD.PrintErr($"Inference error: {e.Message}");
		}

		return actions;
	}

	/// <summary>
	/// Wendet den Action-Vektor auf die kontrollierten Stangen an.
	/// Jede Stange erhält 2 Actions: Lateral-Bewegung und Rotation.
	/// </summary>
	private void ApplyActions(float[] actions)
	{
		for (int i = 0; i < Math.Min(_controlledRods.Count, 4); i++)
		{
			var rod = _controlledRods[i];
			if (rod != null)
			{
				float lateral = actions[i * 2];      // Seitliche Bewegung
				float rotation = actions[i * 2 + 1]; // Rotation (Schuss)
				rod.SetBotInput(new Vector2(lateral, rotation));
			}
		}
	}

	/// <summary>
	/// Räumt ONNX Session auf beim Entfernen des Nodes.
	/// </summary>
	public override void _ExitTree()
	{
		_session?.Dispose();
	}
}
