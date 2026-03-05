using Godot;
using System;

/// <summary>
/// Camera Controller der sanft rotiert um dem Ball zu folgen.
/// Camera bleibt an fester Position und dreht sich nur um den Ball "anzuschauen" mit verzögerter, glatter Bewegung.
/// </summary>
public partial class CameraController : Camera3D
{
	private const float SCALE = 5.0f;

	// Camera Einstellungen - feste Schrägsicht für 3D Foosball
	private const float CAMERA_HEIGHT = 1.5f * SCALE;      // Höhe über dem Tisch
	private const float CAMERA_DISTANCE = 1.05f * SCALE;   // Z-Versatz für Schrägsicht
	private const float CAMERA_FOV = 60.0f;                // Sichtfeld

	// Rotations-Tracking Einstellungen
	private const float ROTATION_SMOOTHNESS = 0.08f;  // Lerp Faktor (0.05-0.1 für glattes verzögertes Tracking)
	private const float MAX_ROTATION_ANGLE = 15.0f;   // Maximaler Rotationswinkel in Grad (verhindert exzessives Drehen)

	// Referenz zum Ball
	private RigidBody3D _ball;

	// Feste Camera Position
	private Vector3 _fixedPosition;

	// Aktuelle Rotation als Quaternion für glatte Interpolation
	private Quaternion _currentRotation;
	private Quaternion _targetRotation;

	/// <summary>
	/// Initialisiert die Camera beim Laden der Scene.
	/// Setzt feste Position, Sichtfeld und initiale Rotation zur Tischmitte.
	/// </summary>
	public override void _Ready()
	{
		// Feste Camera Position setzen
		_fixedPosition = new Vector3(0, CAMERA_HEIGHT, CAMERA_DISTANCE);
		Position = _fixedPosition;

		// Sichtfeld setzen
		Fov = CAMERA_FOV;

		// Rotation initialisieren - zur Tischmitte schauen
		Vector3 lookTarget = new Vector3(0, 0, 0);
		LookAt(lookTarget, Vector3.Up);
		_currentRotation = Quaternion.Normalized();
		_targetRotation = _currentRotation;

		// Ball in Scene finden (wird Geschwister-Node in Game.tscn sein)
		CallDeferred(nameof(FindBall));
	}

	/// <summary>
	/// Sucht den Ball-Node in der Parent Scene.
	/// Wird verzögert aufgerufen um sicherzustellen dass alle Nodes bereit sind.
	/// </summary>
	private void FindBall()
	{
		// Versuche Ball Node im Parent zu finden (Game Scene)
		var parent = GetParent();
		if (parent != null)
		{
			_ball = parent.GetNodeOrNull<RigidBody3D>("Ball");
			if (_ball == null)
			{
				GD.Print("CameraController: Ball not found, camera will remain static");
			}
		}
	}

	/// <summary>
	/// Update-Loop für Camera Rotation Tracking.
	/// Berechnet sanfte Rotation um dem Ball zu folgen mit begrenztem Rotationswinkel.
	/// </summary>
	public override void _Process(double delta)
	{
		if (_ball == null)
			return;

		// Sicherstellen dass Camera in fester Position bleibt (falls etwas sie bewegt)
		Position = _fixedPosition;

		// Ball Position abrufen
		Vector3 ballPos = _ball.Position;

		// Gewünschte Rotation berechnen um Ball anzuschauen
		// Wir erstellen einen temporären Transform um die Look-At Rotation zu berechnen
		Transform3D tempTransform = new Transform3D(Basis.Identity, _fixedPosition);
		tempTransform = tempTransform.LookingAt(ballPos, Vector3.Up);
		Quaternion desiredRotation = tempTransform.Basis.GetRotationQuaternion();

		// Basis-Rotation abrufen (zur Tischmitte schauend)
		Transform3D baseTransform = new Transform3D(Basis.Identity, _fixedPosition);
		baseTransform = baseTransform.LookingAt(Vector3.Zero, Vector3.Up);
		Quaternion baseRotation = baseTransform.Basis.GetRotationQuaternion();

		// Quaternions normalisieren um "Quaternion is not normalized" Fehler zu vermeiden
		baseRotation = baseRotation.Normalized();
		desiredRotation = desiredRotation.Normalized();

		// Rotationswinkel begrenzen um exzessive Camera-Bewegung zu verhindern
		float angleToBase = baseRotation.AngleTo(desiredRotation) * (180.0f / Mathf.Pi);

		if (angleToBase > MAX_ROTATION_ANGLE)
		{
			// Rotation begrenzen durch teilweises Interpolieren zur gewünschten Rotation
			float lerpFactor = MAX_ROTATION_ANGLE / angleToBase;
			desiredRotation = baseRotation.Slerp(desiredRotation, lerpFactor).Normalized();
		}

		_targetRotation = desiredRotation;

		// Aktuelle Rotation sanft zur Ziel-Rotation interpolieren
		// Beide Quaternions vor Slerp normalisieren um Fehler zu vermeiden
		_currentRotation = _currentRotation.Normalized().Slerp(_targetRotation.Normalized(), ROTATION_SMOOTHNESS).Normalized();

		// Rotation anwenden
		Quaternion = _currentRotation;
	}

	/// <summary>
	/// Setzt Camera auf zentrale Rotation zurück (z.B. nach einem Tor).
	/// </summary>
	public void ResetToCenter()
	{
		Position = _fixedPosition;

		// Rotation zurücksetzen um zur Tischmitte zu schauen
		Transform3D centerTransform = new Transform3D(Basis.Identity, _fixedPosition);
		centerTransform = centerTransform.LookingAt(Vector3.Zero, Vector3.Up);
		_currentRotation = centerTransform.Basis.GetRotationQuaternion().Normalized();
		_targetRotation = _currentRotation;
		Quaternion = _currentRotation;
	}

	/// <summary>
	/// Setzt eine neue Ball-Referenz (nützlich wenn Ball neu gespawnt wird).
	/// </summary>
	public void SetBallReference(RigidBody3D ball)
	{
		_ball = ball;
	}
}
