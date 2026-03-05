using Godot;

/// <summary>
/// Einstellungsmenu für Audio- und Video-Optionen.
/// Ermöglicht Anpassung von Lautstarke, Vollbild-Modus, Fenstergrosse und VSync.
/// </summary>
public partial class OptionsMenu : Control
{
	// UI-Referenzen
	private Label _labelTitle;
	private Label _labelAudio;
	private Label _labelVideo;

	private HSlider _sliderMasterVolume;
	private HSlider _sliderMusicVolume;
	private HSlider _sliderSFXVolume;

	private CheckBox _checkFPSDisplay;
	private OptionButton _optionFullscreen;
	private OptionButton _optionWindowSize;
	private CheckBox _optionVSync;

	private Button _btnBack;

	// Verfugbare Fensterauflosungen
	private readonly Vector2I[] _windowSizes = new Vector2I[]
	{
		new Vector2I(1280, 720),
		new Vector2I(1366, 768),
		new Vector2I(1600, 900),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1440)
	};

	// Manager-Referenzen
	private AudioManager _audioManager;
	private GameManager _gameManager;

	/// <summary>
	/// Initialisiert das Menu beim Laden.
	/// Holt alle Node-Referenzen, konfiguriert UI-Elemente und ladt gespeicherte Einstellungen.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_audioManager = GetNode<AudioManager>("/root/AudioManager");
		_gameManager = GetNode<GameManager>("/root/GameManager");

		// UI-Node-Referenzen holen
		_labelTitle = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/LabelTitle");
		_labelAudio = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/AudioSection/LabelAudio");
		_labelVideo = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/VideoSection/LabelVideo");

		_sliderMasterVolume = GetNode<HSlider>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/AudioSection/SliderMasterVolume");
		_sliderMusicVolume = GetNode<HSlider>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/AudioSection/SliderMusicVolume");
		_sliderSFXVolume = GetNode<HSlider>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/AudioSection/SliderSFXVolume");

		_checkFPSDisplay = GetNode<CheckBox>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/VideoSection/CheckFPSDisplay");
		_optionFullscreen = GetNode<OptionButton>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/VideoSection/OptionFullscreen");
		_optionWindowSize = GetNode<OptionButton>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/VideoSection/OptionWindowSize");
		_optionVSync = GetNode<CheckBox>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/VideoSection/OptionVSync");

		_btnBack = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/BtnBack");

		// UI-Texte setzen
		_labelTitle.Text = "Options";
		_labelAudio.Text = "Audio";
		_labelVideo.Text = "Video";
		_btnBack.Text = "Back";

		// Audio-Slider konfigurieren
		_sliderMasterVolume.MinValue = 0;
		_sliderMasterVolume.MaxValue = 100;
		_sliderMasterVolume.Step = 1;
		_sliderMasterVolume.Value = 100;

		_sliderMusicVolume.MinValue = 0;
		_sliderMusicVolume.MaxValue = 100;
		_sliderMusicVolume.Step = 1;
		_sliderMusicVolume.Value = 80;

		_sliderSFXVolume.MinValue = 0;
		_sliderSFXVolume.MaxValue = 100;
		_sliderSFXVolume.Step = 1;
		_sliderSFXVolume.Value = 100;

		// Vollbild-Optionen konfigurieren
		_optionFullscreen.Clear();
		_optionFullscreen.AddItem("Windowed");
		_optionFullscreen.AddItem("Fullscreen");
		_optionFullscreen.AddItem("Borderless");
		_optionFullscreen.Selected = 0;

		// Fenstergrossen-Optionen konfigurieren
		_optionWindowSize.Clear();
		for (int i = 0; i < _windowSizes.Length; i++)
		{
			_optionWindowSize.AddItem($"{_windowSizes[i].X} x {_windowSizes[i].Y}");
		}
		_optionWindowSize.Selected = 0;

		// Checkboxen konfigurieren
		_checkFPSDisplay.Text = "Show FPS";
		_checkFPSDisplay.ButtonPressed = false;

		_optionVSync.Text = "VSync";
		_optionVSync.ButtonPressed = true;

		// Signals verbinden
		_sliderMasterVolume.ValueChanged += OnMasterVolumeChanged;
		_sliderMusicVolume.ValueChanged += OnMusicVolumeChanged;
		_sliderSFXVolume.ValueChanged += OnSFXVolumeChanged;

		_checkFPSDisplay.Toggled += OnFPSDisplayToggled;
		_optionFullscreen.ItemSelected += OnFullscreenChanged;
		_optionWindowSize.ItemSelected += OnWindowSizeChanged;
		_optionVSync.Toggled += OnVSyncToggled;

		_btnBack.Pressed += OnBackPressed;

		// Aktuelle Einstellungen laden
		LoadSettings();
	}

	/// <summary>
	/// Ladt die aktuellen Einstellungen aus den Managern und setzt die UI-Elemente entsprechend.
	/// Liest Audio-Einstellungen, Fenstermodus, Fenstergrosse, VSync und FPS-Anzeige.
	/// </summary>
	private void LoadSettings()
	{
		// Audio-Einstellungen vom AudioManager laden
		_sliderMasterVolume.Value = _audioManager.MasterVolume * 100;
		_sliderMusicVolume.Value = _audioManager.MusicVolume * 100;
		_sliderSFXVolume.Value = _audioManager.SFXVolume * 100;

		// Video-Einstellungen laden
		var windowMode = DisplayServer.WindowGetMode();
		switch (windowMode)
		{
			case DisplayServer.WindowMode.Windowed:
				_optionFullscreen.Selected = 0;
				break;
			case DisplayServer.WindowMode.Fullscreen:
				_optionFullscreen.Selected = 1;
				break;
			case DisplayServer.WindowMode.ExclusiveFullscreen:
				_optionFullscreen.Selected = 2;
				break;
		}

		// Aktuelle Fenstergrosse laden und nachstliegende Auflosung finden
		var currentSize = DisplayServer.WindowGetSize();
		int closestIndex = 0;
		int closestDiff = int.MaxValue;
		for (int i = 0; i < _windowSizes.Length; i++)
		{
			// Manhattan-Distanz zur Bestimmung der ahnlichsten Auflosung
			int diff = Mathf.Abs(_windowSizes[i].X - currentSize.X) + Mathf.Abs(_windowSizes[i].Y - currentSize.Y);
			if (diff < closestDiff)
			{
				closestDiff = diff;
				closestIndex = i;
			}
		}
		_optionWindowSize.Selected = closestIndex;

		_optionVSync.ButtonPressed = DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;

		// FPS-Anzeige-Einstellung laden
		_checkFPSDisplay.ButtonPressed = _gameManager.ShowFps;
	}

	/// <summary>
	/// Wird bei Anderung der Master-Lautstarke aufgerufen.
	/// Spielt bewusst keinen Sound, da dies zu haufig ausgelost wird.
	/// </summary>
	private void OnMasterVolumeChanged(double value)
	{
		// Kein Sound fur Slider - wird zu oft ausgelost
		_audioManager.SetMasterVolume((float)value / 100f);
	}

	/// <summary>
	/// Wird bei Anderung der Musik-Lautstarke aufgerufen.
	/// Spielt bewusst keinen Sound, da dies zu haufig ausgelost wird.
	/// </summary>
	private void OnMusicVolumeChanged(double value)
	{
		// Kein Sound fur Slider - wird zu oft ausgelost
		_audioManager.SetMusicVolume((float)value / 100f);
	}

	/// <summary>
	/// Wird bei Anderung der SFX-Lautstarke aufgerufen.
	/// Spielt bewusst keinen Sound, da dies zu haufig ausgelost wird.
	/// </summary>
	private void OnSFXVolumeChanged(double value)
	{
		// Kein Sound fur Slider - wird zu oft ausgelost
		_audioManager.SetSFXVolume((float)value / 100f);
	}

	/// <summary>
	/// Wird aufgerufen, wenn die FPS-Anzeige ein-/ausgeschaltet wird.
	/// </summary>
	private void OnFPSDisplayToggled(bool toggled)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Hoherer Pitch fur Toggle
		// FPS-Anzeige uber GameManager umschalten
		_gameManager.SetFpsDisplay(toggled);
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Vollbild-Modus geandert wird.
	/// Setzt den Fenstermodus: Windowed, Fullscreen oder Borderless.
	/// </summary>
	private void OnFullscreenChanged(long index)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Hoherer Pitch fur Auswahl

		switch (index)
		{
			case 0: // Fenster-Modus
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				break;
			case 1: // Vollbild
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				break;
			case 2: // Randloser Vollbild
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
				break;
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn die Fenstergrosse geandert wird.
	/// Andert die Fensterauflosung und zentriert das Fenster auf dem Bildschirm.
	/// </summary>
	private void OnWindowSizeChanged(long index)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Hoherer Pitch fur Auswahl

		if (index >= 0 && index < _windowSizes.Length)
		{
			var size = _windowSizes[index];
			DisplayServer.WindowSetSize(size);

			// Fenster auf dem Bildschirm zentrieren
			var screenSize = DisplayServer.ScreenGetSize();
			var windowPos = new Vector2I(
				(screenSize.X - size.X) / 2,
				(screenSize.Y - size.Y) / 2
			);
			DisplayServer.WindowSetPosition(windowPos);
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn VSync ein-/ausgeschaltet wird.
	/// </summary>
	private void OnVSyncToggled(bool toggled)
	{
		_audioManager?.PlaySFX("button_click", 1.15f); // Hoherer Pitch fur Toggle

		DisplayServer.WindowSetVsyncMode(toggled
			? DisplayServer.VSyncMode.Enabled
			: DisplayServer.VSyncMode.Disabled);
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Zuruck-Button gedruckt wird.
	/// </summary>
	private void OnBackPressed()
	{
		_audioManager?.PlaySFX("button_click");
		Hide();
	}
}
