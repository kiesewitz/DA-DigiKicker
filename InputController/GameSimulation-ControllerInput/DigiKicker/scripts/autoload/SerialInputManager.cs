using Godot;
using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Liest binäre Maus-Pakete vom Raspberry Pi über die serielle Schnittstelle
/// und stellt die Eingaben für die vier Tischfußball-Stangen bereit.
///
/// Paketformat vom Pi (6 Bytes):
///   [0xFF] [mouse_id 0-3] [button] [x 0-255] [y 0-255] [XOR-Checksum]
///
/// Maus → Stange Zuordnung (konfigurierbar via Export-Properties):
///   mouse0 → Stange 0 (z.B. Goalkeeper Rot)
///   mouse1 → Stange 1 (z.B. Defense Rot)
///   mouse2 → Stange 2 (z.B. Goalkeeper Blau)
///   mouse3 → Stange 3 (z.B. Defense Blau)
///
/// Bewegungslogik (laut Diagramm):
///   Maus vorwärts/rückwärts (Y-Achse) → Laterale Stangen-Verschiebung (Z-Achse im Spiel)
///   Maus links/rechts     (X-Achse) → Rotation der Stange um ihre Längsachse
///
/// WICHTIG: Das Skript wird als Autoload/Singleton eingebunden (InputManager-kompatibel).
/// </summary>
public partial class SerialInputManager : Node
{
	// ─── Serielle Schnittstelle ───────────────────────────────────────────────

	/// <summary>COM-Port des USB-Serial-Adapters am PC (z.B. "COM6" unter Windows,
	/// "/dev/ttyUSB0" unter Linux).</summary>
	[Export] public string SerialPortName { get; set; } = "COM6";

	/// <summary>Muss mit BAUD_RATE im Python-Skript übereinstimmen.</summary>
	[Export] public int BaudRate { get; set; } = 9600;

	// ─── Empfindlichkeitsregler ───────────────────────────────────────────────

	/// <summary>Skalierungsfaktor für die Lateral-Bewegung (Maus Y → Stangen-Verschiebung).</summary>
	[Export] public float LateralSensitivity { get; set; } = 0.012f;

	/// <summary>Skalierungsfaktor für die Rotation (Maus X → Stangen-Drehung).</summary>
	[Export] public float RotationSensitivity { get; set; } = 0.012f;

	/// <summary>Unterhalb dieses Rohdelta-Werts wird Mausbewegung als Rauschen ignoriert.</summary>
	[Export] public float DeadZone { get; set; } = 2.0f;

	// ─── Interner Zustand ─────────────────────────────────────────────────────

	private SerialPort _serialPort;
	private Thread _readThread;
	private bool _running = false;

	// Verarbeiteter Input pro Maus: X=Rotation, Y=Lateral  (thread-safe via Lock)
	private readonly Vector2[] _mouseInput = new Vector2[4];
	private readonly object _inputLock = new object();

	// Puffer für den Paket-Parser
	private readonly byte[] _packetBuf = new byte[6];
	private int _bufPos = 0;

	// ─── Godot Lifecycle ─────────────────────────────────────────────────────

	public override void _Ready()
	{
		for (int i = 0; i < 4; i++)
			_mouseInput[i] = Vector2.Zero;

		OpenSerialPort();
	}

	public override void _ExitTree()
	{
		_running = false;
		_readThread?.Join(500);
		if (_serialPort?.IsOpen == true)
			_serialPort.Close();
	}

	// ─── Serielle Schnittstelle ───────────────────────────────────────────────

	private void OpenSerialPort()
	{
		try
		{
			_serialPort = new SerialPort(SerialPortName, BaudRate, Parity.None, 8, StopBits.One)
			{
				DtrEnable = true,
				RtsEnable = true,
				ReadTimeout  = 500,
				WriteTimeout = 500
			};
			_serialPort.Open();
			GD.Print($"[SerialInputManager] Port {SerialPortName} geöffnet.");

			_running = true;
			_readThread = new Thread(ReadLoop) { IsBackground = true, Name = "SerialRead" };
			_readThread.Start();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[SerialInputManager] Fehler beim Öffnen von {SerialPortName}: {ex.Message}");
		}
	}

	// ─── Lese-Thread ─────────────────────────────────────────────────────────

	/// <summary>
	/// Läuft in einem Hintergrund-Thread.
	/// Liest Bytes vom seriellen Port und sucht nach dem Startbyte 0xFF,
	/// um Pakete zu synchronisieren.
	/// </summary>
	private void ReadLoop()
	{
		while (_running && _serialPort?.IsOpen == true)
		{
			try
			{
				int b = _serialPort.ReadByte();
				if (b < 0) continue;

				byte by = (byte)b;

				// Auf Startbyte synchronisieren
				if (_bufPos == 0 && by != 0xFF)
					continue;

				_packetBuf[_bufPos++] = by;

				if (_bufPos == 6)
				{
					_bufPos = 0;
					ParsePacket(_packetBuf);
				}
			}
			catch (TimeoutException)
			{
				// Normal – einfach weiterlesen
			}
			catch (Exception ex)
			{
				if (_running)
					GD.PrintErr($"[SerialInputManager] Lesefehler: {ex.Message}");
				_bufPos = 0;
			}
		}
	}

	// ─── Paket-Parser ────────────────────────────────────────────────────────

	/// <summary>
	/// Paketformat: [0xFF][idx][button][x][y][checksum]
	/// Checksum = XOR von idx, button, x, y.
	///
	/// Rohwerte kommen als unsigned byte (0-255).
	/// Werte über 127 sind negative Bewegungen (two's complement, wie PS/2-Protokoll).
	/// </summary>
	private void ParsePacket(byte[] pkt)
	{
		// Startbyte prüfen
		if (pkt[0] != 0xFF) return;

		byte idx     = pkt[1];
		byte button  = pkt[2];
		byte rawX    = pkt[3];
		byte rawY    = pkt[4];
		byte checksum = pkt[5];

		// Prüfsumme validieren (XOR aller Datenbytes)
		byte expected = (byte)(idx ^ button ^ rawX ^ rawY);
		if (checksum != expected)
		{
			GD.PrintErr($"[SerialInputManager] Paket-Checksum-Fehler (mouse{idx}): erwartet {expected}, erhalten {checksum}");
			return;
		}

		if (idx > 3) return; // Nur 4 Mäuse unterstützt

		// Unsigned byte → vorzeichenbehafteter Wert (-128 … 127, wie Linux-Mausprotokoll)
		float dx = (rawX > 127) ? (rawX - 256) : rawX;
		float dy = (rawY > 127) ? (rawY - 256) : rawY;

		// Dead-Zone anwenden
		if (MathF.Abs(dx) < DeadZone) dx = 0f;
		if (MathF.Abs(dy) < DeadZone) dy = 0f;

		// Laut Diagramm:
		//   Maus X (links/rechts) → Drehrichtung der Stange  (= Rotation)
		//   Maus Y (vor/zurück)   → Laterale Verschiebung     (= Lateral)
		//
		// Das Linux-Mausprotokoll meldet Y positiv nach unten;
		// für "vorwärts schieben = Stange nach vorne" kehren wir Y um.
		float rotation = dx * RotationSensitivity;
		float lateral  = -dy * LateralSensitivity;  // negiert: Maus vor → positiver Offset

		lock (_inputLock)
		{
			_mouseInput[idx] = new Vector2(lateral, rotation);
		}
	}

	// ─── Öffentliche API (kompatibel mit Rod.cs / InputManager-Erwartungen) ──

	/// <summary>
	/// Gibt den aktuellen Input für die angegebene Maus/Stange zurück.
	/// Rückgabe: Vector2(Lateral, Rotation)  – gleiche Semantik wie InputManager.GetRodInput().
	///
	/// Nach dem Lesen wird der Wert zurückgesetzt (Delta-Bewegung, keine Achsenstellung).
	/// </summary>
	/// <param name="mouseIndex">0-3 entspricht mouse0–mouse3 vom Raspberry Pi</param>
	public Vector2 GetMouseInput(int mouseIndex)
	{
		if (mouseIndex < 0 || mouseIndex > 3) return Vector2.Zero;

		lock (_inputLock)
		{
			Vector2 val = _mouseInput[mouseIndex];
			_mouseInput[mouseIndex] = Vector2.Zero; // einmalig konsumieren
			return val;
		}
	}

	/// <summary>
	/// Kompatibilitäts-Wrapper: Gibt Input für PlayerIndex + RodType-Index zurück.
	/// Mapping: (playerIndex=1,rodIndex=0) → mouse0,  (playerIndex=1,rodIndex=1) → mouse1
	///          (playerIndex=2,rodIndex=0) → mouse2,  (playerIndex=2,rodIndex=1) → mouse3
	///
	/// Passe das Mapping unten an deine physische Controller-Belegung an.
	/// </summary>
	public Vector2 GetRodInput(int playerIndex, int rodIndex)
	{
		// Beispiel-Mapping (2 Spieler × 2 Stangen pro Spieler):
		//   Spieler 1: Stangen 0 und 1  → mouse0 / mouse1
		//   Spieler 2: Stangen 0 und 1  → mouse2 / mouse3
		int mouseIdx = ((playerIndex - 1) * 2) + rodIndex;
		mouseIdx = Mathf.Clamp(mouseIdx, 0, 3);
		return GetMouseInput(mouseIdx);
	}

	/// <summary>Gibt zurück ob der serielle Port aktuell geöffnet ist.</summary>
	public new bool IsConnected => _serialPort?.IsOpen ?? false;
}
