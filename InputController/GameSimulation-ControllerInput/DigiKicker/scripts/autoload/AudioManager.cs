using Godot;
using System;
using System.Collections.Generic;

// Zentrale Verwaltung von Musik und Soundeffekten mit Lautstärkeregelung und Einstellungspersistenz
public partial class AudioManager : Node
{
	// Audio Bus Namen
	private const string MASTER_BUS = "Master";
	private const string MUSIC_BUS = "Music";
	private const string SFX_BUS = "SFX";

	// Öffentliche Lautstärke-Properties
	public float MasterVolume { get; private set; } = 0.8f;
	public float MusicVolume { get; private set; } = 0.7f;
	public float SFXVolume { get; private set; } = 0.8f;

	// Audio Player und Sound-Bibliothek
	private AudioStreamPlayer _musicPlayer;
	private Dictionary<string, AudioStream> _sfxLibrary = new Dictionary<string, AudioStream>();
	private List<AudioStreamPlayer> _sfxPlayers = new List<AudioStreamPlayer>();
	private const int SFX_PLAYER_POOL_SIZE = 8;

	// Bus-Indizes für direkten Zugriff
	private int _masterBusIndex;
	private int _musicBusIndex;
	private int _sfxBusIndex;

	// Pfad zur Einstellungsdatei
	private const string SETTINGS_PATH = "user://audio_settings.json";

	// Debug-Ausgabesteuerung
	private const bool DEBUG_VERBOSE = false;

	/// <summary>
	/// Initialisiert den AudioManager und lädt alle Audiodateien
	/// </summary>
	public override void _Ready()
	{
		GD.Print("========================================");
		GD.Print("AudioManager initializing...");

		// Bus-Indizes abrufen
		_masterBusIndex = AudioServer.GetBusIndex(MASTER_BUS);
		_musicBusIndex = AudioServer.GetBusIndex(MUSIC_BUS);
		_sfxBusIndex = AudioServer.GetBusIndex(SFX_BUS);

		GD.Print($"Master Bus Index: {_masterBusIndex}");
		GD.Print($"Music Bus Index: {_musicBusIndex}");
		GD.Print($"SFX Bus Index: {_sfxBusIndex}");

		// Musik-Player erstellen
		_musicPlayer = new AudioStreamPlayer();
		_musicPlayer.Bus = MUSIC_BUS;
		AddChild(_musicPlayer);

		// SFX Player Pool erstellen
		for (int i = 0; i < SFX_PLAYER_POOL_SIZE; i++)
		{
			var sfxPlayer = new AudioStreamPlayer();
			sfxPlayer.Bus = SFX_BUS;
			AddChild(sfxPlayer);
			_sfxPlayers.Add(sfxPlayer);
		}

		// Audio-Ressourcen laden
		LoadAudioResources();

		// Gespeicherte Einstellungen laden
		LoadSettings();

		// Lautstärken anwenden
		ApplyVolumeSettings();

		// Hintergrundmusik automatisch starten
		GD.Print("Starting background music...");
		PlayMusic("music");
		GD.Print("AudioManager initialization complete");
		GD.Print("========================================");
	}

	/// <summary>
	/// Lädt alle Audiodateien aus dem Assets-Verzeichnis
	/// </summary>
	private void LoadAudioResources()
	{
		var sfxPaths = new Dictionary<string, string>()
		{
			{ "hit", "res://assets/audio/sfx/hit.wav" },
			{ "goal", "res://assets/audio/sfx/goal.wav" },
			{ "ball_roll", "res://assets/audio/sfx/ball_roll.wav" },
			{ "button_click", "res://assets/audio/sfx/button_click.wav" }
		};

		// SFX in Bibliothek laden
		foreach (var kvp in sfxPaths)
		{
			if (ResourceLoader.Exists(kvp.Value))
			{
				var stream = ResourceLoader.Load<AudioStream>(kvp.Value);
				if (stream != null)
				{
					_sfxLibrary[kvp.Key] = stream;
					GD.Print($"Loaded SFX: {kvp.Key}");
				}
			}
			else
			{
				GD.PrintErr($"SFX file not found: {kvp.Value}");
			}
		}
	}

	/// <summary>
	/// Spielt einen Soundeffekt mit optionaler Tonhöhenanpassung ab
	/// </summary>
	public void PlaySFX(string name, float pitchScale = 1.0f)
	{
		if (!_sfxLibrary.ContainsKey(name))
		{
			GD.PrintErr($"SFX '{name}' not found in library. Available: {string.Join(", ", _sfxLibrary.Keys)}");
			return;
		}

		// Verfügbaren Player suchen
		AudioStreamPlayer availablePlayer = null;
		foreach (var player in _sfxPlayers)
		{
			if (!player.Playing)
			{
				availablePlayer = player;
				break;
			}
		}

		// Falls alle Player belegt, ersten verwenden (unterbricht laufenden Sound)
		if (availablePlayer == null)
		{
			availablePlayer = _sfxPlayers[0];
			if (DEBUG_VERBOSE)
				GD.Print("Warning: All SFX players busy, interrupting oldest");
		}

		// Sound abspielen
		availablePlayer.Stream = _sfxLibrary[name];
		availablePlayer.PitchScale = pitchScale;
		availablePlayer.Play();
	}

	/// <summary>
	/// Spielt einen Soundeffekt mit zufälliger Tonhöhenvariation (90% - 110%) für mehr Abwechslung
	/// </summary>
	public void PlaySFXWithRandomPitch(string name)
	{
		var random = new RandomNumberGenerator();
		random.Randomize();
		float randomPitch = random.RandfRange(0.9f, 1.1f);
		PlaySFX(name, randomPitch);
	}

	/// <summary>
	/// Spielt einen Pfiff-Sound durch Manipulation des button_click Sounds
	/// </summary>
	public void PlayWhistle()
	{
		if (_sfxLibrary.ContainsKey("button_click"))
		{
			// Verfügbaren Player suchen
			AudioStreamPlayer availablePlayer = null;
			foreach (var player in _sfxPlayers)
			{
				if (!player.Playing)
				{
					availablePlayer = player;
					break;
				}
			}

			if (availablePlayer == null)
				availablePlayer = _sfxPlayers[0];

			// button_click mit niedrigerer Tonhöhe für längeren Pfiff-Effekt abspielen
			availablePlayer.Stream = _sfxLibrary["button_click"];
			availablePlayer.PitchScale = 0.55f;
			availablePlayer.Play();
		}
		else
		{
			GD.PrintErr("Cannot play whistle - button_click sound not loaded");
		}
	}

	/// <summary>
	/// Spielt Hintergrundmusik mit optionaler Loop-Funktion ab
	/// </summary>
	public void PlayMusic(string name, bool loop = true)
	{
		GD.Print($"========================================");
		GD.Print($"PlayMusic called: {name}, loop: {loop}");

		string musicPath = $"res://assets/audio/music/{name}.wav";
		GD.Print($"Music path: {musicPath}");

		if (!ResourceLoader.Exists(musicPath))
		{
			GD.PrintErr($"Music file not found: {musicPath}");
			return;
		}

		GD.Print("Music file exists, loading...");

		var stream = ResourceLoader.Load<AudioStream>(musicPath);
		if (stream == null)
		{
			GD.PrintErr($"Failed to load music: {musicPath}");
			return;
		}

		_musicPlayer.Stream = stream;

		// Loop-Modus aktivieren falls unterstützt
		if (stream is AudioStreamWav wavStream)
		{
			wavStream.LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled;
		}

		_musicPlayer.Play();
		GD.Print($"Playing music: {name}");
	}

	/// <summary>
	/// Stoppt die aktuell abgespielte Musik
	/// </summary>
	public void StopMusic()
	{
		_musicPlayer.Stop();
		GD.Print("Music stopped");
	}

	/// <summary>
	/// Stoppt alle Audio-Ausgaben (Musik und SFX)
	/// </summary>
	public void StopAllAudio()
	{
		_musicPlayer.Stop();
		foreach (var player in _sfxPlayers)
		{
			player.Stop();
		}
		GD.Print("All audio stopped");
	}

	/// <summary>
	/// Setzt die Master-Lautstärke (0.0 bis 1.0)
	/// </summary>
	public void SetMasterVolume(float value)
	{
		MasterVolume = Mathf.Clamp(value, 0.0f, 1.0f);
		ApplyVolumeTobus(_masterBusIndex, MasterVolume);
		SaveSettings();
	}

	/// <summary>
	/// Setzt die Musik-Lautstärke (0.0 bis 1.0)
	/// </summary>
	public void SetMusicVolume(float value)
	{
		MusicVolume = Mathf.Clamp(value, 0.0f, 1.0f);
		ApplyVolumeTobus(_musicBusIndex, MusicVolume);
		SaveSettings();
	}

	/// <summary>
	/// Setzt die SFX-Lautstärke (0.0 bis 1.0)
	/// </summary>
	public void SetSFXVolume(float value)
	{
		SFXVolume = Mathf.Clamp(value, 0.0f, 1.0f);
		ApplyVolumeTobus(_sfxBusIndex, SFXVolume);
		SaveSettings();
	}

	/// <summary>
	/// Wendet eine Lautstärke auf einen spezifischen Audio-Bus an
	/// </summary>
	private void ApplyVolumeTobus(int busIndex, float linearVolume)
	{
		if (busIndex < 0)
			return;

		if (linearVolume <= 0.0f)
		{
			// Bus stumm schalten
			AudioServer.SetBusMute(busIndex, true);
		}
		else
		{
			// Stummschaltung aufheben und Lautstärke setzen
			AudioServer.SetBusMute(busIndex, false);
			float db = Mathf.LinearToDb(linearVolume);
			AudioServer.SetBusVolumeDb(busIndex, db);
		}
	}

	/// <summary>
	/// Wendet alle Lautstärkeeinstellungen auf ihre jeweiligen Busse an
	/// </summary>
	private void ApplyVolumeSettings()
	{
		ApplyVolumeTobus(_masterBusIndex, MasterVolume);
		ApplyVolumeTobus(_musicBusIndex, MusicVolume);
		ApplyVolumeTobus(_sfxBusIndex, SFXVolume);
		GD.Print($"Applied volumes - Master: {MasterVolume}, Music: {MusicVolume}, SFX: {SFXVolume}");
	}

	/// <summary>
	/// Speichert die Audio-Einstellungen in eine Datei
	/// </summary>
	public void SaveSettings()
	{
		var settings = new Godot.Collections.Dictionary
		{
			{ "master_volume", MasterVolume },
			{ "music_volume", MusicVolume },
			{ "sfx_volume", SFXVolume }
		};

		string json = Json.Stringify(settings, "\t");

		using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Write);
		if (file != null)
		{
			file.StoreString(json);
			GD.Print($"Audio settings saved to {SETTINGS_PATH}");
		}
		else
		{
			GD.PrintErr($"Failed to save audio settings to {SETTINGS_PATH}");
		}
	}

	/// <summary>
	/// Lädt die Audio-Einstellungen aus einer Datei
	/// </summary>
	public void LoadSettings()
	{
		if (!FileAccess.FileExists(SETTINGS_PATH))
		{
			GD.Print("No saved audio settings found, using defaults");
			return;
		}

		using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr($"Failed to open audio settings file: {SETTINGS_PATH}");
			return;
		}

		string jsonString = file.GetAsText();
		var json = new Json();
		var parseResult = json.Parse(jsonString);

		if (parseResult != Error.Ok)
		{
			GD.PrintErr($"Failed to parse audio settings JSON: {json.GetErrorMessage()}");
			return;
		}

		var settings = json.Data.AsGodotDictionary();

		// Lautstärkewerte aus Dictionary auslesen
		if (settings.ContainsKey("master_volume"))
			MasterVolume = (float)settings["master_volume"];

		if (settings.ContainsKey("music_volume"))
			MusicVolume = (float)settings["music_volume"];

		if (settings.ContainsKey("sfx_volume"))
			SFXVolume = (float)settings["sfx_volume"];

		GD.Print($"Audio settings loaded - Master: {MasterVolume}, Music: {MusicVolume}, SFX: {SFXVolume}");
	}

	/// <summary>
	/// Gibt den Musik-Player für erweiterte Steuerung zurück
	/// </summary>
	public AudioStreamPlayer GetMusicPlayer()
	{
		return _musicPlayer;
	}
}
