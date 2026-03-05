using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Verwaltet Input-Belegungen für Tastatur und Controller.
/// Unterstützt spielerspezifische Anpassungen und persistente Speicherung via ConfigFile.
/// </summary>
public partial class InputBindings : RefCounted
{
	/// <summary>
	/// Enum für alle verfügbaren Spielaktionen (Tastatur-Spieler 1+2, Controller, Pause).
	/// </summary>
	public enum GameAction
	{
		// Spieler 1 Tastatur
		P1_MoveUp,
		P1_MoveDown,
		P1_RotateLeft,
		P1_RotateRight,
		P1_SwitchRodLeft,
		P1_SwitchRodRight,

		// Spieler 2 Tastatur
		P2_MoveUp,
		P2_MoveDown,
		P2_RotateLeft,
		P2_RotateRight,
		P2_SwitchRodLeft,
		P2_SwitchRodRight,

		// Controller (gilt für beide Spieler)
		Controller_ToggleLeftPair,
		Controller_ToggleRightPair,

		// Allgemein
		Pause
	}

	// Binding-Datenstrukturen: Action -> InputEvent
	private Dictionary<GameAction, InputEvent> _keyboardPlayer1Bindings = new();
	private Dictionary<GameAction, InputEvent> _keyboardPlayer2Bindings = new();
	private Dictionary<GameAction, InputEvent> _controllerBindings = new();

	// ConfigFile-Konstanten
	private const string CONFIG_FILE_PATH = "user://input_bindings.cfg";
	private const string SECTION_KB_P1 = "keyboard_player1";
	private const string SECTION_KB_P2 = "keyboard_player2";
	private const string SECTION_CONTROLLER = "controller";

	/// <summary>
	/// Konstruktor: Initialisiert die Standard-Bindings.
	/// </summary>
	public InputBindings()
	{
		SetDefaultBindings();
	}

	/// <summary>
	/// Setzt alle Bindings auf ihre Standardwerte zurück.
	/// Player 1: WASD + QE, Player 2: Pfeiltasten + Numpad, Controller: Shoulder Buttons.
	/// </summary>
	public void SetDefaultBindings()
	{
		// Spieler 1 Tastatur (WASD + QE)
		_keyboardPlayer1Bindings[GameAction.P1_MoveUp] = CreateKeyEvent(Key.W);
		_keyboardPlayer1Bindings[GameAction.P1_MoveDown] = CreateKeyEvent(Key.S);
		_keyboardPlayer1Bindings[GameAction.P1_RotateLeft] = CreateKeyEvent(Key.A);
		_keyboardPlayer1Bindings[GameAction.P1_RotateRight] = CreateKeyEvent(Key.D);
		_keyboardPlayer1Bindings[GameAction.P1_SwitchRodLeft] = CreateKeyEvent(Key.Q);
		_keyboardPlayer1Bindings[GameAction.P1_SwitchRodRight] = CreateKeyEvent(Key.E);

		// Spieler 2 Tastatur (Pfeiltasten + Numpad)
		_keyboardPlayer2Bindings[GameAction.P2_MoveUp] = CreateKeyEvent(Key.Up);
		_keyboardPlayer2Bindings[GameAction.P2_MoveDown] = CreateKeyEvent(Key.Down);
		_keyboardPlayer2Bindings[GameAction.P2_RotateLeft] = CreateKeyEvent(Key.Left);
		_keyboardPlayer2Bindings[GameAction.P2_RotateRight] = CreateKeyEvent(Key.Right);
		_keyboardPlayer2Bindings[GameAction.P2_SwitchRodLeft] = CreateKeyEvent(Key.KpSubtract);
		_keyboardPlayer2Bindings[GameAction.P2_SwitchRodRight] = CreateKeyEvent(Key.KpAdd);

		// Controller (Shoulder Buttons für Stangenwechsel)
		_controllerBindings[GameAction.Controller_ToggleLeftPair] = CreateJoyButtonEvent(JoyButton.LeftShoulder);
		_controllerBindings[GameAction.Controller_ToggleRightPair] = CreateJoyButtonEvent(JoyButton.RightShoulder);

		// Allgemeine Aktionen
		_keyboardPlayer1Bindings[GameAction.Pause] = CreateKeyEvent(Key.Escape);
		_controllerBindings[GameAction.Pause] = CreateJoyButtonEvent(JoyButton.Start);
	}

	/// <summary>
	/// Erstellt ein Tastatur-InputEventKey für eine bestimmte Taste.
	/// </summary>
	/// <param name="keycode">Die zu bindende Taste</param>
	/// <returns>Konfiguriertes InputEventKey</returns>
	private InputEventKey CreateKeyEvent(Key keycode)
	{
		var keyEvent = new InputEventKey();
		keyEvent.Keycode = keycode;
		keyEvent.Pressed = true;
		return keyEvent;
	}

	/// <summary>
	/// Erstellt ein Controller-InputEventJoypadButton für einen bestimmten Button.
	/// </summary>
	/// <param name="button">Der zu bindende Controller-Button</param>
	/// <returns>Konfiguriertes InputEventJoypadButton</returns>
	private InputEventJoypadButton CreateJoyButtonEvent(JoyButton button)
	{
		var joyEvent = new InputEventJoypadButton();
		joyEvent.ButtonIndex = button;
		joyEvent.Pressed = true;
		return joyEvent;
	}

	/// <summary>
	/// Gibt das Binding für eine bestimmte Action und Input-Typ zurück.
	/// </summary>
	/// <param name="action">Die gewünschte Action</param>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <param name="isController">True für Controller, false für Tastatur</param>
	/// <returns>Das entsprechende InputEvent oder null</returns>
	public InputEvent GetBinding(GameAction action, int player = 1, bool isController = false)
	{
		if (isController)
		{
			return _controllerBindings.GetValueOrDefault(action);
		}
		else
		{
			var bindings = player == 1 ? _keyboardPlayer1Bindings : _keyboardPlayer2Bindings;
			return bindings.GetValueOrDefault(action);
		}
	}

	/// <summary>
	/// Setzt ein neues Binding für eine bestimmte Action.
	/// </summary>
	/// <param name="action">Die zu ändernde Action</param>
	/// <param name="inputEvent">Das neue InputEvent</param>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <param name="isController">True für Controller, false für Tastatur</param>
	public void SetBinding(GameAction action, InputEvent inputEvent, int player = 1, bool isController = false)
	{
		if (isController)
		{
			_controllerBindings[action] = inputEvent;
		}
		else
		{
			var bindings = player == 1 ? _keyboardPlayer1Bindings : _keyboardPlayer2Bindings;
			bindings[action] = inputEvent;
		}
	}

	/// <summary>
	/// Gibt die Taste für eine Tastatur-Action zurück.
	/// </summary>
	/// <param name="action">Die gewünschte Action</param>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <returns>Key-Code oder Key.None falls nicht gefunden</returns>
	public Key GetKey(GameAction action, int player = 1)
	{
		var binding = GetBinding(action, player, false);
		if (binding is InputEventKey keyEvent)
		{
			return keyEvent.Keycode;
		}
		return Key.None;
	}

	/// <summary>
	/// Gibt den Controller-Button für eine Action zurück.
	/// </summary>
	/// <param name="action">Die gewünschte Action</param>
	/// <returns>JoyButton oder JoyButton.Invalid falls nicht gefunden</returns>
	public JoyButton GetJoyButton(GameAction action)
	{
		var binding = GetBinding(action, 1, true);
		if (binding is InputEventJoypadButton joyEvent)
		{
			return joyEvent.ButtonIndex;
		}
		return JoyButton.Invalid;
	}

	/// <summary>
	/// Prüft ob eine Action aktuell gedrückt ist.
	/// </summary>
	/// <param name="action">Die zu prüfende Action</param>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <param name="isController">True für Controller, false für Tastatur</param>
	/// <returns>True wenn die Action gedrückt ist</returns>
	public bool IsActionPressed(GameAction action, int player = 1, bool isController = false)
	{
		if (isController)
		{
			var binding = GetBinding(action, player, true);
			if (binding is InputEventJoypadButton joyEvent)
			{
				// Prüfe alle verbundenen Controller auf Button-Druck
				var joypads = Input.GetConnectedJoypads();
				foreach (int joyId in joypads)
				{
					if (Input.IsJoyButtonPressed(joyId, joyEvent.ButtonIndex))
						return true;
				}
			}
		}
		else
		{
			var binding = GetBinding(action, player, false);
			if (binding is InputEventKey keyEvent)
			{
				return Input.IsKeyPressed(keyEvent.Keycode);
			}
		}
		return false;
	}

	/// <summary>
	/// Prüft ob ein InputEvent mit einem Binding übereinstimmt.
	/// </summary>
	/// <param name="inputEvent">Das zu prüfende InputEvent</param>
	/// <param name="action">Die Action zum Vergleichen</param>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <param name="isController">True für Controller, false für Tastatur</param>
	/// <returns>True wenn das Event dem Binding entspricht</returns>
	public bool MatchesBinding(InputEvent inputEvent, GameAction action, int player = 1, bool isController = false)
	{
		var binding = GetBinding(action, player, isController);
		if (binding == null)
			return false;

		// Vergleiche Tastatur-Inputs
		if (inputEvent is InputEventKey keyInput && binding is InputEventKey keyBinding)
		{
			return keyInput.Keycode == keyBinding.Keycode;
		}
		// Vergleiche Controller-Inputs
		else if (inputEvent is InputEventJoypadButton joyInput && binding is InputEventJoypadButton joyBinding)
		{
			return joyInput.ButtonIndex == joyBinding.ButtonIndex;
		}

		return false;
	}

	/// <summary>
	/// Speichert alle Bindings in eine Konfigurationsdatei.
	/// Die Datei wird unter user:// gespeichert und enthält drei Sektionen für P1, P2 und Controller.
	/// </summary>
	public void SaveToFile()
	{
		var config = new ConfigFile();

		// Spieler 1 Keyboard-Bindings speichern
		foreach (var kvp in _keyboardPlayer1Bindings)
		{
			string key = kvp.Key.ToString();
			config.SetValue(SECTION_KB_P1, key, SerializeInputEvent(kvp.Value));
		}

		// Spieler 2 Keyboard-Bindings speichern
		foreach (var kvp in _keyboardPlayer2Bindings)
		{
			string key = kvp.Key.ToString();
			config.SetValue(SECTION_KB_P2, key, SerializeInputEvent(kvp.Value));
		}

		// Controller-Bindings speichern
		foreach (var kvp in _controllerBindings)
		{
			string key = kvp.Key.ToString();
			config.SetValue(SECTION_CONTROLLER, key, SerializeInputEvent(kvp.Value));
		}

		// Datei auf Disk schreiben
		Error err = config.Save(CONFIG_FILE_PATH);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Fehler beim Speichern der Input-Bindings: {err}");
		}
		else
		{
			GD.Print("Input-Bindings erfolgreich gespeichert");
		}
	}

	/// <summary>
	/// Lädt alle Bindings aus einer Konfigurationsdatei.
	/// Falls die Datei nicht existiert oder ein Fehler auftritt, werden die Standardwerte verwendet.
	/// </summary>
	public void LoadFromFile()
	{
		var config = new ConfigFile();
		Error err = config.Load(CONFIG_FILE_PATH);

		if (err != Error.Ok)
		{
			GD.Print($"Keine gespeicherten Bindings gefunden, verwende Standardwerte. Fehler: {err}");
			return;
		}

		// Für alle Actions versuchen zu laden
		foreach (GameAction action in Enum.GetValues(typeof(GameAction)))
		{
			string key = action.ToString();

			// Spieler 1 Keyboard laden
			if (config.HasSectionKey(SECTION_KB_P1, key))
			{
				var serialized = config.GetValue(SECTION_KB_P1, key).AsGodotDictionary();
				var inputEvent = DeserializeInputEvent(serialized);
				if (inputEvent != null)
					_keyboardPlayer1Bindings[action] = inputEvent;
			}

			// Spieler 2 Keyboard laden
			if (config.HasSectionKey(SECTION_KB_P2, key))
			{
				var serialized = config.GetValue(SECTION_KB_P2, key).AsGodotDictionary();
				var inputEvent = DeserializeInputEvent(serialized);
				if (inputEvent != null)
					_keyboardPlayer2Bindings[action] = inputEvent;
			}

			// Controller laden
			if (config.HasSectionKey(SECTION_CONTROLLER, key))
			{
				var serialized = config.GetValue(SECTION_CONTROLLER, key).AsGodotDictionary();
				var inputEvent = DeserializeInputEvent(serialized);
				if (inputEvent != null)
					_controllerBindings[action] = inputEvent;
			}
		}

		GD.Print("Input-Bindings erfolgreich geladen");
	}

	/// <summary>
	/// Serialisiert ein InputEvent in ein Godot Dictionary zum Speichern.
	/// Unterstützt InputEventKey und InputEventJoypadButton.
	/// </summary>
	/// <param name="inputEvent">Das zu serialisierende InputEvent</param>
	/// <returns>Dictionary mit Typ und Daten</returns>
	private Godot.Collections.Dictionary SerializeInputEvent(InputEvent inputEvent)
	{
		var dict = new Godot.Collections.Dictionary();

		if (inputEvent is InputEventKey keyEvent)
		{
			dict["type"] = "key";
			dict["keycode"] = (int)keyEvent.Keycode;
		}
		else if (inputEvent is InputEventJoypadButton joyEvent)
		{
			dict["type"] = "joybutton";
			dict["button_index"] = (int)joyEvent.ButtonIndex;
		}

		return dict;
	}

	/// <summary>
	/// Deserialisiert ein InputEvent aus einem Godot Dictionary.
	/// </summary>
	/// <param name="dict">Das Dictionary mit den gespeicherten Daten</param>
	/// <returns>Das rekonstruierte InputEvent oder null bei Fehler</returns>
	private InputEvent DeserializeInputEvent(Godot.Collections.Dictionary dict)
	{
		if (!dict.ContainsKey("type"))
			return null;

		string type = dict["type"].AsString();

		// Tastatur-Event
		if (type == "key" && dict.ContainsKey("keycode"))
		{
			var keyEvent = new InputEventKey();
			keyEvent.Keycode = (Key)dict["keycode"].AsInt32();
			keyEvent.Pressed = true;
			return keyEvent;
		}
		// Controller-Event
		else if (type == "joybutton" && dict.ContainsKey("button_index"))
		{
			var joyEvent = new InputEventJoypadButton();
			joyEvent.ButtonIndex = (JoyButton)dict["button_index"].AsInt32();
			joyEvent.Pressed = true;
			return joyEvent;
		}

		return null;
	}

	/// <summary>
	/// Gibt einen lesbaren Namen für ein InputEvent zurück (für UI-Anzeige).
	/// </summary>
	/// <param name="inputEvent">Das InputEvent</param>
	/// <returns>Lesbarer Name der Taste/des Buttons</returns>
	public static string GetInputEventDisplayName(InputEvent inputEvent)
	{
		if (inputEvent is InputEventKey keyEvent)
		{
			return OS.GetKeycodeString(keyEvent.Keycode);
		}
		else if (inputEvent is InputEventJoypadButton joyEvent)
		{
			return GetJoyButtonName(joyEvent.ButtonIndex);
		}
		return "None";
	}

	/// <summary>
	/// Gibt einen lesbaren Namen für einen Controller-Button zurück.
	/// </summary>
	/// <param name="button">Der JoyButton</param>
	/// <returns>Lesbarer Button-Name</returns>
	private static string GetJoyButtonName(JoyButton button)
	{
		return button switch
		{
			JoyButton.A => "A Button",
			JoyButton.B => "B Button",
			JoyButton.X => "X Button",
			JoyButton.Y => "Y Button",
			JoyButton.LeftShoulder => "LB / L1",
			JoyButton.RightShoulder => "RB / R1",
			JoyButton.LeftStick => "Left Stick",
			JoyButton.RightStick => "Right Stick",
			JoyButton.Start => "Start",
			JoyButton.Back => "Back / Select",
			JoyButton.DpadUp => "D-Pad Up",
			JoyButton.DpadDown => "D-Pad Down",
			JoyButton.DpadLeft => "D-Pad Left",
			JoyButton.DpadRight => "D-Pad Right",
			_ => button.ToString()
		};
	}

	/// <summary>
	/// Gibt alle Bindings für einen bestimmten Spieler zurück (für UI-Anzeige).
	/// </summary>
	/// <param name="player">Spielernummer (1 oder 2)</param>
	/// <param name="isController">True für Controller, false für Tastatur</param>
	/// <returns>Dictionary mit allen Bindings</returns>
	public Dictionary<GameAction, InputEvent> GetAllBindings(int player, bool isController)
	{
		if (isController)
		{
			return new Dictionary<GameAction, InputEvent>(_controllerBindings);
		}
		else
		{
			return player == 1
				? new Dictionary<GameAction, InputEvent>(_keyboardPlayer1Bindings)
				: new Dictionary<GameAction, InputEvent>(_keyboardPlayer2Bindings);
		}
	}
}
