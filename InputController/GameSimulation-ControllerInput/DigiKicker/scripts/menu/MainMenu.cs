using Godot;

/// <summary>
/// Hauptmenu-Controller des Spiels.
/// Verwaltet die Navigation zwischen allen Menu-Bereichen inklusive Online-Multiplayer.
/// </summary>
public partial class MainMenu : Control
{
	// UI-Referenzen
	private Label _titleLabel;
	private Button _btnSingleplayer;
	private Button _btnMultiplayer;
	private Button _btnOptions;
	private Button _btnStatistics;
	private Button _btnController;
	private Button _btnQuit;

	// SubMenu-Referenzen
	private Control _subMenuContainer;
	private Control _gameSetupMenu;
	private Control _optionsMenu;
	private Control _statsMenu;
	private Control _controllerMenu;
	private Control _onlineMenu;            // Online Multiplayer Menu (GDScript)

	// Multiplayer-Auswahl Popup
	private Control _multiplayerSelectPopup;

	// Manager-Referenzen
	private AudioManager _audioManager;

	/// <summary>
	/// Initialisiert das Hauptmenu beim Laden.
	/// Holt alle Node-Referenzen, setzt UI-Texte und verbindet Button-Signals.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_audioManager = GetNodeOrNull<AudioManager>("/root/AudioManager");

		// UI-Node-Referenzen holen (Pfad: CenterContainer/VBoxContainer/...)
		_titleLabel = GetNode<Label>("CenterContainer/VBoxContainer/TitleLabel");
		_btnSingleplayer = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnSingleplayer");
		_btnMultiplayer = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnMultiplayer");
		_btnOptions = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnOptions");
		_btnStatistics = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnStatistics");
		_btnController = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnController");
		_btnQuit = GetNode<Button>("CenterContainer/VBoxContainer/ButtonContainer/BtnQuit");

		// SubMenu-Referenzen holen
		_subMenuContainer = GetNode<Control>("SubMenuContainer");
		_gameSetupMenu = GetNode<Control>("SubMenuContainer/GameSetupMenu");
		_optionsMenu = GetNode<Control>("SubMenuContainer/OptionsMenu");
		_statsMenu = GetNode<Control>("SubMenuContainer/StatsMenu");
		_controllerMenu = GetNode<Control>("SubMenuContainer/ControllerMenu");
		_onlineMenu = GetNode<Control>("SubMenuContainer/OnlineMenu");

		// UI-Texte setzen
		_titleLabel.Text = "DigiKicker";
		_btnSingleplayer.Text = "Singleplayer";
		_btnMultiplayer.Text = "Multiplayer";
		_btnOptions.Text = "Options";
		_btnStatistics.Text = "Statistics";
		_btnController.Text = "Controller";
		_btnQuit.Text = "Quit";

		// Button-Signals verbinden
		_btnSingleplayer.Pressed += OnSingleplayerPressed;
		_btnMultiplayer.Pressed += OnMultiplayerPressed;
		_btnOptions.Pressed += OnOptionsPressed;
		_btnStatistics.Pressed += OnStatisticsPressed;
		_btnController.Pressed += OnControllerPressed;
		_btnQuit.Pressed += OnQuitPressed;

		// Multiplayer-Auswahl-Popup erstellen
		CreateMultiplayerSelectPopup();

		// Alle Submenus beim Start verstecken
		HideAllSubMenus();

		// Hinweis: Musik wird automatisch in AudioManager._Ready() gestartet
	}

	/// <summary>
	/// Erstellt das Popup-Menu zur Auswahl zwischen lokalem und Online-Multiplayer.
	/// Baut die komplette UI-Hierarchie programmatisch auf.
	/// </summary>
	private void CreateMultiplayerSelectPopup()
	{
		// Container für das Popup
		_multiplayerSelectPopup = new Control();
		_multiplayerSelectPopup.Name = "MultiplayerSelectPopup";
		_multiplayerSelectPopup.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_multiplayerSelectPopup.MouseFilter = Control.MouseFilterEnum.Stop;
		_multiplayerSelectPopup.Visible = false;

		// Halbtransparenter Hintergrund
		var background = new ColorRect();
		background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		background.Color = new Color(0, 0, 0, 0.7f);
		background.MouseFilter = Control.MouseFilterEnum.Stop;
		_multiplayerSelectPopup.AddChild(background);

		// Center Container
		var center = new CenterContainer();
		center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_multiplayerSelectPopup.AddChild(center);

		// Panel Container
		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(350, 0);
		center.AddChild(panel);

		// Margin Container
		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 30);
		margin.AddThemeConstantOverride("margin_right", 30);
		margin.AddThemeConstantOverride("margin_top", 30);
		margin.AddThemeConstantOverride("margin_bottom", 30);
		panel.AddChild(margin);

		// VBox für Inhalt
		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 20);
		margin.AddChild(vbox);

		// Titel
		var title = new Label();
		title.Text = "Multiplayer Modus";
		title.HorizontalAlignment = HorizontalAlignment.Center;
		title.AddThemeFontSizeOverride("font_size", 24);
		vbox.AddChild(title);

		// Button Container
		var btnContainer = new VBoxContainer();
		btnContainer.AddThemeConstantOverride("separation", 15);
		vbox.AddChild(btnContainer);

		// Lokal Button
		var btnLocal = new Button();
		btnLocal.Text = "Lokal (geteilter Bildschirm)";
		btnLocal.CustomMinimumSize = new Vector2(280, 50);
		btnLocal.Pressed += OnLocalMultiplayerSelected;
		btnContainer.AddChild(btnLocal);

		// Online Button
		var btnOnline = new Button();
		btnOnline.Text = "Online (P2P WebRTC)";
		btnOnline.CustomMinimumSize = new Vector2(280, 50);
		btnOnline.Pressed += OnOnlineMultiplayerSelected;
		btnContainer.AddChild(btnOnline);

		// Separator
		var separator = new HSeparator();
		vbox.AddChild(separator);

		// Zurück Button
		var btnBack = new Button();
		btnBack.Text = "Zurück";
		btnBack.CustomMinimumSize = new Vector2(120, 40);
		btnBack.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		btnBack.Pressed += OnMultiplayerSelectBack;
		vbox.AddChild(btnBack);

		// Popup als Kind hinzufügen
		AddChild(_multiplayerSelectPopup);
	}

	/// <summary>
	/// Spielt den Button-Click-Sound über den AudioManager.
	/// </summary>
	private void PlayButtonSound()
	{
		_audioManager?.PlaySFX("button_click");
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Singleplayer-Button gedrückt wird.
	/// Öffnet das GameSetupMenu im Singleplayer-Modus.
	/// </summary>
	private void OnSingleplayerPressed()
	{
		PlayButtonSound();
		ShowSubMenu(_gameSetupMenu);

		// Spielmodus auf Singleplayer setzen
		if (_gameSetupMenu is GameSetupMenu setupMenu)
		{
			setupMenu.IsMultiplayer = false;
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Multiplayer-Button gedrückt wird.
	/// Navigiert zum ModeSelectionMenu.
	/// </summary>
	private void OnMultiplayerPressed()
	{
		PlayButtonSound();
		// Zum ModeSelectionMenu navigieren
		GetTree().ChangeSceneToFile("res://scenes/menu/ModeSelectionMenu.tscn");
	}

	/// <summary>
	/// Wird aufgerufen, wenn lokaler Multiplayer im Popup ausgewählt wird.
	/// Öffnet das GameSetupMenu im Multiplayer-Modus.
	/// </summary>
	private void OnLocalMultiplayerSelected()
	{
		PlayButtonSound();
		_multiplayerSelectPopup.Visible = false;

		ShowSubMenu(_gameSetupMenu);

		// Spielmodus auf lokalen Multiplayer setzen
		if (_gameSetupMenu is GameSetupMenu setupMenu)
		{
			setupMenu.IsMultiplayer = true;
		}
	}

	/// <summary>
	/// Wird aufgerufen, wenn Online-Multiplayer im Popup ausgewählt wird.
	/// Öffnet das Online-Menu (GDScript).
	/// </summary>
	private void OnOnlineMultiplayerSelected()
	{
		PlayButtonSound();
		_multiplayerSelectPopup.Visible = false;

		// Online Menu anzeigen
		ShowSubMenu(_onlineMenu);
	}

	/// <summary>
	/// Wird aufgerufen, wenn im Multiplayer-Auswahl-Popup auf Zurück geklickt wird.
	/// </summary>
	private void OnMultiplayerSelectBack()
	{
		PlayButtonSound();
		_multiplayerSelectPopup.Visible = false;
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Options-Button gedrückt wird.
	/// </summary>
	private void OnOptionsPressed()
	{
		PlayButtonSound();
		ShowSubMenu(_optionsMenu);
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Statistics-Button gedrückt wird.
	/// </summary>
	private void OnStatisticsPressed()
	{
		PlayButtonSound();
		ShowSubMenu(_statsMenu);
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Controller-Button gedrückt wird.
	/// </summary>
	private void OnControllerPressed()
	{
		PlayButtonSound();
		ShowSubMenu(_controllerMenu);
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Quit-Button gedrückt wird.
	/// Beendet die Anwendung.
	/// </summary>
	private void OnQuitPressed()
	{
		PlayButtonSound();
		GetTree().Quit();
	}

	/// <summary>
	/// Zeigt ein bestimmtes SubMenu an und versteckt alle anderen.
	/// </summary>
	/// <param name="subMenu">Das anzuzeigende SubMenu</param>
	private void ShowSubMenu(Control subMenu)
	{
		HideAllSubMenus();
		subMenu.Show();
	}

	/// <summary>
	/// Versteckt alle Submenus und das Multiplayer-Auswahl-Popup.
	/// Öffentlich, damit es von Submenus aufgerufen werden kann.
	/// </summary>
	public void HideAllSubMenus()
	{
		_gameSetupMenu.Hide();
		_optionsMenu.Hide();
		_statsMenu.Hide();
		_controllerMenu.Hide();
		_onlineMenu.Hide();

		// Popup auch verstecken
		if (_multiplayerSelectPopup != null)
		{
			_multiplayerSelectPopup.Visible = false;
		}
	}
}
