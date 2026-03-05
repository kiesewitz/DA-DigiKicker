using Godot;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Controller-Einstellungs-Menu fur Eingabegerate und Tastenbelegung.
/// Zeigt angeschlossene Controller an, ermoglicht Rebinding von Tastenkombinationen
/// fur Keyboard Player 1, Keyboard Player 2 und Controller.
/// </summary>
public partial class ControllerMenu : Control
{
	// UI-Referenzen - Oberer Bereich
	private Label _labelTitle;
	private Label _labelConnected;
	private ItemList _deviceList;

	// Rebinding-UI
	private TabContainer _tabContainer;

	// Player 1 Keyboard-Belegungen
	private VBoxContainer _player1KeyboardContainer;
	private System.Collections.Generic.Dictionary<InputBindings.GameAction, Button> _p1KeyboardButtons = new();

	// Player 2 Keyboard-Belegungen
	private VBoxContainer _player2KeyboardContainer;
	private System.Collections.Generic.Dictionary<InputBindings.GameAction, Button> _p2KeyboardButtons = new();

	// Controller-Belegungen
	private VBoxContainer _controllerContainer;
	private System.Collections.Generic.Dictionary<InputBindings.GameAction, Button> _controllerButtons = new();

	// Buttons
	private Button _btnResetToDefault;
	private Button _btnBack;

	// Manager-Referenzen
	private InputManager _inputManager;
	private AudioManager _audioManager;

	// Rebinding-Status
	private bool _waitingForInput = false;
	private Label _rebindingPrompt;

	/// <summary>
	/// Initialisiert das Menu beim Laden.
	/// Holt alle Node-Referenzen, verbindet Signals und erstellt die Tastenbelegungs-UI.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_inputManager = GetNode<InputManager>("/root/InputManager");
		_audioManager = GetNode<AudioManager>("/root/AudioManager");

		// UI-Node-Referenzen holen - Oberer Bereich
		_labelTitle = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/LabelTitle");
		_labelConnected = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/ConnectedDevices/LabelConnected");
		_deviceList = GetNode<ItemList>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/ConnectedDevices/DeviceList");

		// Tab-Container fur Rebinding-Bereiche
		_tabContainer = GetNode<TabContainer>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/TabContainer");

		// Belegungs-Container
		_player1KeyboardContainer = GetNode<VBoxContainer>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/TabContainer/Player 1 Keyboard/ScrollContainer/BindingsContainer");
		_player2KeyboardContainer = GetNode<VBoxContainer>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/TabContainer/Player 2 Keyboard/ScrollContainer/BindingsContainer");
		_controllerContainer = GetNode<VBoxContainer>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/TabContainer/Controller/ScrollContainer/BindingsContainer");

		// Untere Buttons
		_btnResetToDefault = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/BottomButtons/BtnResetToDefault");
		_btnBack = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/ScrollContainer/VBoxContainer/BottomButtons/BtnBack");

		// Rebinding-Hinweis
		_rebindingPrompt = GetNode<Label>("RebindingPrompt");

		// UI-Texte setzen
		_labelTitle.Text = "Controller Settings";
		_labelConnected.Text = "Connected Devices:";
		_btnResetToDefault.Text = "Reset to Default";
		_btnBack.Text = "Back";

		// Signals verbinden
		_deviceList.ItemClicked += OnDeviceListItemClicked;
		_btnResetToDefault.Pressed += OnResetToDefaultPressed;
		_btnBack.Pressed += OnBackPressed;

		// InputManager-Signals verbinden
		_inputManager.ControllerConnected += OnControllerConnected;
		_inputManager.ControllerDisconnected += OnControllerDisconnected;
		_inputManager.RebindingStarted += OnRebindingStarted;
		_inputManager.RebindingCompleted += OnRebindingCompleted;
		_inputManager.RebindingCancelled += OnRebindingCancelled;

		// Visibility-Signal verbinden, um Status zuruckzusetzen, wenn Menu sichtbar wird
		VisibilityChanged += OnVisibilityChanged;

		// Initiale Einrichtung
		RefreshDeviceList();
		CreateBindingButtons();
		UpdateAllBindingLabels();
	}

	/// <summary>
	/// Wird beim Entfernen aus dem Scene-Tree aufgerufen.
	/// Trennt InputManager-Signals, um Memory-Leaks zu vermeiden.
	/// </summary>
	public override void _ExitTree()
	{
		// Von InputManager-Signals trennen, um Memory-Leaks zu vermeiden
		if (_inputManager != null)
		{
			_inputManager.ControllerConnected -= OnControllerConnected;
			_inputManager.ControllerDisconnected -= OnControllerDisconnected;
			_inputManager.RebindingStarted -= OnRebindingStarted;
			_inputManager.RebindingCompleted -= OnRebindingCompleted;
			_inputManager.RebindingCancelled -= OnRebindingCancelled;
		}

		VisibilityChanged -= OnVisibilityChanged;
	}

	/// <summary>
	/// Wird aufgerufen, wenn sich die Sichtbarkeit andert.
	/// Setzt den Rebinding-Status zuruck, wenn das Menu sichtbar wird.
	/// </summary>
	private void OnVisibilityChanged()
	{
		if (Visible)
		{
			// Rebinding-Status zurucksetzen, wenn Menu sichtbar wird
			// Dies stellt einen sauberen Zustand nach Ruckkehr aus dem Spiel sicher
			_waitingForInput = false;
			_rebindingPrompt.Visible = false;

			// Aktives Rebinding im InputManager abbrechen
			if (_inputManager != null && _inputManager.IsRebinding())
			{
				_inputManager.CancelRebinding();
			}

			// UI aktualisieren
			RefreshDeviceList();
			UpdateAllBindingLabels();
		}
	}

	/// <summary>
	/// Wird jeden Frame aufgerufen.
	/// Pruft periodisch auf Anderungen bei angeschlossenen Geraten.
	/// </summary>
	public override void _Process(double delta)
	{
		// Alle 60 Frames auf Gerate-Anderungen prufen, wahrend Menu sichtbar ist
		if (Visible && Engine.GetFramesDrawn() % 60 == 0)
		{
			RefreshDeviceList();
		}
	}

	/// <summary>
	/// Wandelt rohe Controller-Namen in benutzerfreundliche Namen um.
	/// Erkennt gangige Controller-Typen wie Xbox, PlayStation, Nintendo Switch.
	/// </summary>
	/// <param name="rawName">Roher Controller-Name vom System</param>
	/// <returns>Benutzerfreundlicher Controller-Name</returns>
	private string GetFriendlyControllerName(string rawName)
	{
		string nameLower = rawName.ToLower();

		// Xbox-Controller
		if (nameLower.Contains("xbox") || nameLower.Contains("xinput"))
		{
			if (nameLower.Contains("360"))
				return "Xbox 360 Controller";
			else if (nameLower.Contains("one"))
				return "Xbox One Controller";
			else if (nameLower.Contains("series"))
				return "Xbox Series Controller";
			else
				return "Xbox Controller";
		}

		// PlayStation-Controller
		if (nameLower.Contains("playstation") || nameLower.Contains("ps") ||
			nameLower.Contains("dualshock") || nameLower.Contains("dualsense"))
		{
			if (nameLower.Contains("5") || nameLower.Contains("dualsense"))
				return "PlayStation 5 Controller";
			else if (nameLower.Contains("4") || nameLower.Contains("dualshock 4"))
				return "PlayStation 4 Controller";
			else if (nameLower.Contains("3") || nameLower.Contains("dualshock 3"))
				return "PlayStation 3 Controller";
			else
				return "PlayStation Controller";
		}

		// Nintendo-Controller
		if (nameLower.Contains("nintendo") || nameLower.Contains("switch"))
		{
			if (nameLower.Contains("pro"))
				return "Nintendo Switch Pro Controller";
			else if (nameLower.Contains("joycon") || nameLower.Contains("joy-con"))
				return "Nintendo Joy-Con";
			else
				return "Nintendo Controller";
		}

		// Steam Controller
		if (nameLower.Contains("steam"))
			return "Steam Controller";

		// Generischer Fallback
		return "Generic Controller";
	}

	/// <summary>
	/// Aktualisiert die Liste der angeschlossenen Eingabegerate.
	/// Zeigt Keyboard und alle angeschlossenen Controller an.
	/// </summary>
	private void RefreshDeviceList()
	{
		_deviceList.Clear();

		// Keyboard-Option immer hinzufugen
		_deviceList.AddItem("Keyboard", null, true);

		// Angeschlossene Joypads holen
		Array<int> joypads = Input.GetConnectedJoypads();

		if (joypads.Count == 0)
		{
			_deviceList.AddItem("No controllers detected", null, false);
		}
		else
		{
			foreach (int joyId in joypads)
			{
				string joyName = Input.GetJoyName(joyId);
				string friendlyName = GetFriendlyControllerName(joyName);
				_deviceList.AddItem($"{friendlyName} (Port {joyId})", null, true);
			}
		}
	}

	/// <summary>
	/// Erstellt alle Rebinding-Buttons dynamisch fur alle drei Tabs.
	/// </summary>
	private void CreateBindingButtons()
	{
		// Player 1 Keyboard-Belegungen
		CreateBindingButton(InputBindings.GameAction.P1_MoveUp, "Move Up", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.P1_MoveDown, "Move Down", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.P1_RotateLeft, "Rotate Left", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.P1_RotateRight, "Rotate Right", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.P1_SwitchRodLeft, "Switch Rod Left", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.P1_SwitchRodRight, "Switch Rod Right", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);
		CreateBindingButton(InputBindings.GameAction.Pause, "Pause", _player1KeyboardContainer, _p1KeyboardButtons, 1, false);

		// Player 2 Keyboard-Belegungen
		CreateBindingButton(InputBindings.GameAction.P2_MoveUp, "Move Up", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);
		CreateBindingButton(InputBindings.GameAction.P2_MoveDown, "Move Down", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);
		CreateBindingButton(InputBindings.GameAction.P2_RotateLeft, "Rotate Left", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);
		CreateBindingButton(InputBindings.GameAction.P2_RotateRight, "Rotate Right", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);
		CreateBindingButton(InputBindings.GameAction.P2_SwitchRodLeft, "Switch Rod Left", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);
		CreateBindingButton(InputBindings.GameAction.P2_SwitchRodRight, "Switch Rod Right", _player2KeyboardContainer, _p2KeyboardButtons, 2, false);

		// Controller-Belegungen
		CreateBindingButton(InputBindings.GameAction.Controller_ToggleLeftPair, "Toggle Left Pair (GK/Def)", _controllerContainer, _controllerButtons, 1, true);
		CreateBindingButton(InputBindings.GameAction.Controller_ToggleRightPair, "Toggle Right Pair (Mid/Atk)", _controllerContainer, _controllerButtons, 1, true);
		CreateBindingButton(InputBindings.GameAction.Pause, "Pause", _controllerContainer, _controllerButtons, 1, true);
	}

	/// <summary>
	/// Erstellt einen einzelnen Rebinding-Button mit Label.
	/// </summary>
	/// <param name="action">Die GameAction, die gebunden werden soll</param>
	/// <param name="actionName">Anzeigename der Aktion</param>
	/// <param name="container">Container, in den der Button eingefugt wird</param>
	/// <param name="buttonDict">Dictionary zur Speicherung der Button-Referenz</param>
	/// <param name="player">Spieler-Nummer (1 oder 2)</param>
	/// <param name="isController">Ob es sich um Controller-Belegung handelt</param>
	private void CreateBindingButton(InputBindings.GameAction action, string actionName,
		VBoxContainer container, System.Collections.Generic.Dictionary<InputBindings.GameAction, Button> buttonDict,
		int player, bool isController)
	{
		// HBoxContainer fur diese Belegung erstellen
		var hbox = new HBoxContainer();
		hbox.CustomMinimumSize = new Vector2(0, 30);

		// Label fur Aktionsname erstellen
		var label = new Label();
		label.Text = actionName + ":";
		label.CustomMinimumSize = new Vector2(200, 0);
		label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
		hbox.AddChild(label);

		// Button fur Rebinding erstellen
		var button = new Button();
		button.CustomMinimumSize = new Vector2(150, 0);
		button.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
		button.Pressed += () =>
		{
			_audioManager?.PlaySFX("button_click");
			OnRebindButtonPressed(action, player, isController);
		};
		hbox.AddChild(button);

		container.AddChild(hbox);
		buttonDict[action] = button;
	}

	/// <summary>
	/// Aktualisiert alle Binding-Button-Labels mit den aktuellen Belegungen.
	/// </summary>
	private void UpdateAllBindingLabels()
	{
		// Player 1 Keyboard aktualisieren
		foreach (var kvp in _p1KeyboardButtons)
		{
			string bindingName = _inputManager.GetBindingDisplayName(kvp.Key, 1, false);
			kvp.Value.Text = bindingName;
		}

		// Player 2 Keyboard aktualisieren
		foreach (var kvp in _p2KeyboardButtons)
		{
			string bindingName = _inputManager.GetBindingDisplayName(kvp.Key, 2, false);
			kvp.Value.Text = bindingName;
		}

		// Controller aktualisieren
		foreach (var kvp in _controllerButtons)
		{
			string bindingName = _inputManager.GetBindingDisplayName(kvp.Key, 1, true);
			kvp.Value.Text = bindingName;
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Rebinding-Button gedruckt wird.
	/// Startet den Rebinding-Prozess uber den InputManager.
	/// </summary>
	private void OnRebindButtonPressed(InputBindings.GameAction action, int player, bool isController)
	{
		if (_waitingForInput)
			return;

		_inputManager.StartRebinding(player, action, isController);
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Item in der Gerate-Liste angeklickt wird.
	/// Bei Controller-Auswahl wird der entsprechende Controller vibriert zur Bestatigung.
	/// </summary>
	private void OnDeviceListItemClicked(long index, Vector2 atPosition, long mouseButtonIndex)
	{
		// Nur auf Linksklick reagieren
		if (mouseButtonIndex != 1)
			return;

		_audioManager?.PlaySFX("button_click");

		// Index 0 ist Keyboard, keine Vibration dafur
		if (index == 0)
			return;

		// Prufen, ob es die "No controllers detected"-Nachricht ist
		if (_deviceList.GetItemText((int)index) == "No controllers detected")
			return;

		// Angeschlossene Joypads holen
		Array<int> joypads = Input.GetConnectedJoypads();

		// Controller-Index berechnen (1 abziehen fur Keyboard)
		int controllerIndex = (int)index - 1;

		if (controllerIndex >= 0 && controllerIndex < joypads.Count)
		{
			int joyId = joypads[controllerIndex];

			// Controller vibrieren lassen (starke Vibration fur 0.3 Sekunden)
			Input.StartJoyVibration(joyId, 0.5f, 0.8f, 0.3f);

			GD.Print($"Teste Vibration auf Controller {joyId}");
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn der "Reset to Default"-Button gedruckt wird.
	/// Setzt alle Tastenbelegungen auf Standardwerte zuruck.
	/// </summary>
	private void OnResetToDefaultPressed()
	{
		_audioManager?.PlaySFX("button_click");
		_inputManager.ResetBindingsToDefault();
		UpdateAllBindingLabels();
		GD.Print("Tastenbelegungen auf Standard zuruckgesetzt");
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Controller angeschlossen wird.
	/// </summary>
	private void OnControllerConnected(int device)
	{
		GD.Print($"Controller verbunden: {device}");
		RefreshDeviceList();
	}

	/// <summary>
	/// Wird aufgerufen, wenn ein Controller getrennt wird.
	/// </summary>
	private void OnControllerDisconnected(int device)
	{
		GD.Print($"Controller getrennt: {device}");
		RefreshDeviceList();
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Rebinding-Prozess startet.
	/// Zeigt einen Hinweis an, dass auf Eingabe gewartet wird.
	/// </summary>
	private void OnRebindingStarted(int player, string actionName)
	{
		_waitingForInput = true;
		_rebindingPrompt.Text = $"Press a key/button for '{actionName}'...\n(ESC to cancel)";
		_rebindingPrompt.Visible = true;
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Rebinding-Prozess erfolgreich abgeschlossen wurde.
	/// </summary>
	private void OnRebindingCompleted(int player, string actionName, string inputName)
	{
		_waitingForInput = false;
		_rebindingPrompt.Visible = false;
		UpdateAllBindingLabels();
		GD.Print($"Rebinding abgeschlossen: {actionName} -> {inputName}");
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Rebinding-Prozess abgebrochen wurde.
	/// </summary>
	private void OnRebindingCancelled(int player)
	{
		_waitingForInput = false;
		_rebindingPrompt.Visible = false;
		GD.Print("Rebinding abgebrochen");
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Zuruck-Button gedruckt wird.
	/// Bricht laufendes Rebinding ab und versteckt das Menu.
	/// </summary>
	private void OnBackPressed()
	{
		_audioManager?.PlaySFX("button_click");

		// Rebinding abbrechen, falls aktiv
		if (_waitingForInput)
		{
			_inputManager.CancelRebinding();
		}
		Hide();
	}
}
