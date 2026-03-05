using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

/// <summary>
/// Empfängt Maus-Eingaben vom Raspberry Pi über MQTT und stellt sie
/// den vier Tischfußball-Stangen bereit.
///
/// Topic-Schema:  digikicker/mouse/0  bis  digikicker/mouse/3
/// Payload (JSON): { "id": 0, "button": 0, "x": 128, "y": 130 }
///
/// Bewegungslogik:
///   Maus X (links/rechts) → Rotation der Stange
///   Maus Y (vor/zurück)   → Laterale Verschiebung
/// </summary>
public partial class MqttInputManager : Node
{
	// ─── Inspector-Properties ────────────────────────────────────────────────

	/// <summary>IP oder Hostname des MQTT-Brokers (Mosquitto auf diesem PC).</summary>
	[Export] public string BrokerHost { get; set; } = "localhost";

	[Export] public int BrokerPort { get; set; } = 1883;

	/// <summary>Topic-Präfix muss mit dem Python-Skript übereinstimmen.</summary>
	[Export] public string TopicPrefix { get; set; } = "digikicker/mouse";

	/// <summary>Skalierung der Maus-X-Bewegung auf Stangen-Rotation.</summary>
	[Export] public float RotationSensitivity { get; set; } = 0.012f;

	/// <summary>Skalierung der Maus-Y-Bewegung auf laterale Verschiebung.</summary>
	[Export] public float LateralSensitivity { get; set; } = 0.012f;

	/// <summary>Rohdelta unterhalb dieses Werts wird als Rauschen ignoriert.</summary>
	[Export] public float DeadZone { get; set; } = 2.0f;

	// ─── Interner Zustand ─────────────────────────────────────────────────────

	private IMqttClient _mqttClient;
	private CancellationTokenSource _cts;

	// Verarbeiteter Input pro Maus: X = Lateral, Y = Rotation
	private readonly Vector2[] _mouseInput = new Vector2[4];
	private readonly object _inputLock = new object();

	// ─── Godot Lifecycle ─────────────────────────────────────────────────────

	public override void _Ready()
	{
		for (int i = 0; i < 4; i++)
			_mouseInput[i] = Vector2.Zero;

		_cts = new CancellationTokenSource();
		// MQTT-Verbindung asynchron starten ohne den Hauptthread zu blockieren
		Task.Run(() => ConnectAsync(_cts.Token));
	}

	public override void _ExitTree()
	{
		_cts?.Cancel();
		if (_mqttClient?.IsConnected == true)
			_mqttClient.DisconnectAsync().Wait(1000);
		_mqttClient?.Dispose();
	}

	// ─── MQTT-Verbindung ──────────────────────────────────────────────────────

	private async Task ConnectAsync(CancellationToken ct)
	{
		var factory = new MqttFactory();
		_mqttClient = factory.CreateMqttClient();

		var options = new MqttClientOptionsBuilder()
			.WithTcpServer(BrokerHost, BrokerPort)
			.WithClientId("godot-digikicker")
			.WithCleanSession()
			.Build();

		// Handler registrieren bevor Connect aufgerufen wird
		_mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;

		_mqttClient.DisconnectedAsync += async args =>
		{
			if (ct.IsCancellationRequested) return;
			GD.PrintErr("[MqttInputManager] Verbindung verloren – versuche Reconnect in 3s...");
			await Task.Delay(3000, ct);
			try { await _mqttClient.ConnectAsync(options, ct); }
			catch (Exception ex) { GD.PrintErr($"[MqttInputManager] Reconnect fehlgeschlagen: {ex.Message}"); }
		};

		try
		{
			await _mqttClient.ConnectAsync(options, ct);
			GD.Print($"[MqttInputManager] Verbunden mit Broker {BrokerHost}:{BrokerPort}");

			// Alle vier Maus-Topics abonnieren
			for (int i = 0; i < 4; i++)
			{
				string topic = $"{TopicPrefix}/{i}";
				await _mqttClient.SubscribeAsync(
					new MqttTopicFilterBuilder()
						.WithTopic(topic)
						.WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce) // QoS 0 = niedrigste Latenz
						.Build(),
					ct
				);
				GD.Print($"[MqttInputManager] Topic abonniert: {topic}");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[MqttInputManager] Verbindungsfehler: {ex.Message}");
		}
	}

	// ─── Nachrichten-Handler ──────────────────────────────────────────────────

	private Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
	{
		try
		{
			string payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
			var doc = JsonDocument.Parse(payload);
			var root = doc.RootElement;

			int  idx    = root.GetProperty("id").GetInt32();
			byte rawX   = (byte)root.GetProperty("x").GetInt32();
			byte rawY   = (byte)root.GetProperty("y").GetInt32();

			if (idx < 0 || idx > 3) return Task.CompletedTask;

			// Unsigned byte → vorzeichenbehaftetes Delta (Linux-PS/2-Protokoll)
			float dx = (rawX > 127) ? (rawX - 256) : rawX;
			float dy = (rawY > 127) ? (rawY - 256) : rawY;

			// Dead-Zone
			if (MathF.Abs(dx) < DeadZone) dx = 0f;
			if (MathF.Abs(dy) < DeadZone) dy = 0f;

			// Laut Diagramm:
			//   Maus X → Rotation  (Drehen des Stabs = Schuss/Block)
			//   Maus Y → Lateral   (Schieben = Position auf dem Tisch)
			// Y wird negiert: Maus vorwärts schieben = positiver Offset
			float rotation = dx * RotationSensitivity;
			float lateral  = -dy * LateralSensitivity;

			lock (_inputLock)
			{
				_mouseInput[idx] += new Vector2(lateral, rotation);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"[MqttInputManager] Fehler beim Parsen: {ex.Message}");
		}

		return Task.CompletedTask;
	}

	// ─── Öffentliche API ──────────────────────────────────────────────────────

	/// <summary>
	/// Gibt den gesammelten Input für eine Maus zurück und setzt ihn zurück.
	/// Rückgabe: Vector2(Lateral, Rotation)
	/// </summary>
	public Vector2 GetMouseInput(int mouseIndex)
	{
		if (mouseIndex < 0 || mouseIndex > 3) return Vector2.Zero;

		lock (_inputLock)
		{
			Vector2 val = _mouseInput[mouseIndex];
			_mouseInput[mouseIndex] = Vector2.Zero;
			return val;
		}
	}

	/// <summary>
	/// Kompatibilitäts-Wrapper mit gleicher Signatur wie InputManager.GetRodInput().
	/// Mapping:
	///   Spieler 1, Stange 0 → mouse0
	///   Spieler 1, Stange 1 → mouse1
	///   Spieler 2, Stange 0 → mouse2
	///   Spieler 2, Stange 1 → mouse3
	/// </summary>
	public Vector2 GetRodInput(int playerIndex, int rodIndex)
	{
		int mouseIdx = Mathf.Clamp(((playerIndex - 1) * 2) + rodIndex, 0, 3);
		return GetMouseInput(mouseIdx);
	}

	/// <summary>Gibt zurück ob der MQTT-Client aktuell verbunden ist.</summary>
	public new bool IsConnected => _mqttClient?.IsConnected ?? false;
}
