using Godot;

/// <summary>
/// Statistik-Menu zur Anzeige und Verwaltung von Spielstatistiken.
/// Zeigt gespielte Spiele, Siege, erzielte und kassierte Tore an.
/// </summary>
public partial class StatsMenu : Control
{
	// UI-Referenzen
	private Label _labelTitle;

	private Label _labelGoalsScored;
	private Label _valueGoalsScored;
	private Label _labelGoalsConceded;
	private Label _valueGoalsConceded;
	private Label _labelGamesPlayed;
	private Label _valueGamesPlayed;
	private Label _labelGamesWon;
	private Label _valueGamesWon;

	private Button _btnResetStats;
	private Button _btnBack;

	// Manager-Referenzen
	private StatsManager _statsManager;

	// Bestatigungs-Dialog
	private ConfirmationDialog _confirmationDialog;

	/// <summary>
	/// Initialisiert das Menu beim Laden.
	/// Holt alle Node-Referenzen, erstellt Bestatigungs-Dialog und ladt Statistiken.
	/// </summary>
	public override void _Ready()
	{
		// Manager-Referenzen holen
		_statsManager = GetNode<StatsManager>("/root/StatsManager");

		// UI-Node-Referenzen holen
		_labelTitle = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/LabelTitle");

		_labelGoalsScored = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/LabelGoalsScored");
		_valueGoalsScored = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/ValueGoalsScored");
		_labelGoalsConceded = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/LabelGoalsConceded");
		_valueGoalsConceded = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/ValueGoalsConceded");
		_labelGamesPlayed = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/LabelGamesPlayed");
		_valueGamesPlayed = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/ValueGamesPlayed");
		_labelGamesWon = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/LabelGamesWon");
		_valueGamesWon = GetNode<Label>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/StatsContainer/ValueGamesWon");

		_btnResetStats = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/BtnResetStats");
		_btnBack = GetNode<Button>("CenterContainer/PanelContainer/MarginContainer/VBoxContainer/BtnBack");

		// UI-Texte setzen
		_labelTitle.Text = "Statistics";
		_labelGoalsScored.Text = "Goals Scored:";
		_labelGoalsConceded.Text = "Goals Conceded:";
		_labelGamesPlayed.Text = "Games Played:";
		_labelGamesWon.Text = "Games Won:";
		_btnResetStats.Text = "Reset Statistics";
		_btnBack.Text = "Back";

		// Bestatigungs-Dialog erstellen
		_confirmationDialog = new ConfirmationDialog();
		_confirmationDialog.DialogText = "Are you sure you want to reset all statistics? This cannot be undone.";
		_confirmationDialog.Title = "Reset Statistics";
		_confirmationDialog.OkButtonText = "Reset";
		_confirmationDialog.CancelButtonText = "Cancel";
		AddChild(_confirmationDialog);

		// Signals verbinden
		_btnResetStats.Pressed += OnResetStatsPressed;
		_btnBack.Pressed += OnBackPressed;
		_confirmationDialog.Confirmed += OnResetStatsConfirmed;

		// Statistiken beim Start laden
		LoadStats();
	}

	/// <summary>
	/// Wird jeden Frame aufgerufen.
	/// Aktualisiert die Statistik-Anzeige, wenn das Menu sichtbar ist.
	/// </summary>
	public override void _Process(double delta)
	{
		// Statistik-Anzeige aktualisieren, wenn Menu sichtbar ist
		if (Visible)
		{
			LoadStats();
		}
	}

	/// <summary>
	/// Ladt die aktuellen Statistiken vom StatsManager und aktualisiert die Anzeige.
	/// </summary>
	private void LoadStats()
	{
		_valueGoalsScored.Text = _statsManager.TotalGoalsScored.ToString();
		_valueGoalsConceded.Text = _statsManager.TotalGoalsConceded.ToString();
		_valueGamesPlayed.Text = _statsManager.GamesPlayed.ToString();
		_valueGamesWon.Text = _statsManager.GamesWon.ToString();
	}

	/// <summary>
	/// Wird aufgerufen, wenn der "Reset Statistics"-Button gedruckt wird.
	/// Zeigt einen Bestatigungs-Dialog an.
	/// </summary>
	private void OnResetStatsPressed()
	{
		// Bestatigungs-Dialog anzeigen
		_confirmationDialog.PopupCentered();
	}

	/// <summary>
	/// Wird aufgerufen, wenn das Zurucksetzen der Statistiken bestatigt wurde.
	/// Setzt alle Statistiken zuruck und aktualisiert die Anzeige.
	/// </summary>
	private void OnResetStatsConfirmed()
	{
		// Alle Statistiken zurucksetzen
		_statsManager.ResetAllStats();

		// Anzeige neu laden
		LoadStats();
	}

	/// <summary>
	/// Wird aufgerufen, wenn der Zuruck-Button gedruckt wird.
	/// </summary>
	private void OnBackPressed()
	{
		Hide();
	}
}
