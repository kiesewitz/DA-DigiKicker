# Theorieteil Rath

## Inhaltsverzeichnis

1. [Einführung in Game Engines](#1-einführung-in-game-engines)
2. [Unity Engine](#2-unity-engine)
3. [Unreal Engine](#3-unreal-engine)
4. [Godot Engine](#4-godot-engine)
5. [Godot im Detail](#5-godot-im-detail)
6. [WebRTC und Multiplayer Networking](#6-webrtc-und-multiplayer-networking)
7. [Grundlagen des Reinforcement Learning](#7-grundlagen-des-reinforcement-learning)
8. [Neuronale Netze und Deep Learning](#8-neuronale-netze-und-deep-learning)
9. [Godot RL Agents](#9-godot-rl-agents)

---

## 1 Einführung in Game Engines

### 1.1 Was ist eine Game Engine?

Eine Game Engine ist ein Software-Framework, das Entwicklern die grundlegenden Werkzeuge und Funktionalitäten zur Verfügung stellt, um Videospiele zu erstellen. Sie abstrahiert komplexe technische Aufgaben wie Rendering, Physik-Simulation, Audio-Management und Input-Handling und ermöglicht es Entwicklern, sich auf die eigentliche Spiellogik und das Game Design zu konzentrieren.

Die Kernkomponenten einer modernen Game Engine umfassen typischerweise eine Rendering-Pipeline für 2D- und 3D-Grafik, eine Physik-Engine für realistische Bewegungen und Kollisionen, ein Audio-System für Soundeffekte und Musik, ein Input-System für die Verarbeitung von Spielereingaben sowie Asset-Management-Tools für die Verwaltung von Texturen, Modellen und anderen Ressourcen.

Zusätzlich bieten die meisten Engines einen integrierten Editor, der eine visuelle Entwicklungsumgebung bereitstellt. Dieser ermöglicht das Erstellen und Bearbeiten von Spielszenen, die Konfiguration von Objekten und das Testen des Spiels direkt in der Entwicklungsumgebung. Die Wahl der richtigen Game Engine ist eine fundamentale Entscheidung im Entwicklungsprozess und hängt von verschiedenen Faktoren ab.

### 1.2 Kriterien für die Engine-Auswahl

Bei der Auswahl einer Game Engine spielen mehrere Faktoren eine entscheidende Rolle. Der Projektumfang und die Art des Spiels bestimmen maßgeblich, welche Engine am besten geeignet ist. Für fotorealistische 3D-Spiele mit aufwendiger Grafik bietet sich beispielsweise die Unreal Engine an, während für kleinere Indie-Projekte oder 2D-Spiele Godot eine hervorragende Alternative darstellt.

Die Lizenzkosten und das Geschäftsmodell sind ebenfalls wichtige Überlegungen. Unity verwendet ein Abonnement-Modell mit Runtime-Gebühren ab bestimmten Umsatzschwellen, während Unreal Engine 5% Royalties nach einer Million Dollar Umsatz verlangt. Godot hingegen ist vollständig kostenlos und Open Source unter der MIT-Lizenz, was es besonders attraktiv für Indie-Entwickler und Lernende macht.

Weitere Kriterien umfassen die verfügbare Dokumentation und Community-Unterstützung, die unterstützten Plattformen für den Export, die Programmiersprachen und Scripting-Möglichkeiten sowie die Lernkurve für neue Entwickler. Auch die Integration von Drittanbieter-Tools, insbesondere für Machine Learning und KI-Entwicklung, kann je nach Projektanforderungen relevant sein.

---

## 2 Unity Engine

### 2.1 Überblick und Geschichte

Unity ist eine der weltweit meistverwendeten Game Engines und wurde 2005 von Unity Technologies in Dänemark entwickelt. Ursprünglich als Mac-exklusive Engine konzipiert, hat sich Unity zu einer plattformübergreifenden Lösung entwickelt, die den Export auf über 20 verschiedene Plattformen ermöglicht, darunter Windows, macOS, Linux, iOS, Android, PlayStation, Xbox, Nintendo Switch und diverse VR/AR-Geräte.

Die Engine hat sich besonders im Mobile-Gaming-Bereich als Standard etabliert und wird auch für Anwendungen außerhalb der Spielebranche genutzt, etwa in der Architekturvisualisierung, im Film für Echtzeit-Rendering und in der Automobilindustrie für Simulationen. Bekannte Spiele, die mit Unity entwickelt wurden, umfassen Pokémon Go, Among Us, Cuphead, Hollow Knight und Cities: Skylines.

### 2.2 Architektur und Kernkonzepte

Unity basiert auf einem komponentenbasierten Architekturmodell, bei dem GameObjects die grundlegenden Container für alle Spielelemente darstellen. Diese GameObjects sind zunächst leere Hüllen, denen durch das Hinzufügen von Components spezifische Funktionalitäten verliehen werden. Standardkomponenten umfassen Transform für Position und Rotation, Renderer für die visuelle Darstellung, Collider für Physik-Interaktionen und diverse Script-Components für benutzerdefinierte Logik.

Das Scripting in Unity erfolgt primär in C#, einer objektorientierten Programmiersprache von Microsoft. Unity-Scripts erben typischerweise von der MonoBehaviour-Klasse und nutzen Lifecycle-Methoden wie Start(), Update() und FixedUpdate() zur Implementierung der Spiellogik. Die tiefe Integration von C# ermöglicht Zugriff auf das gesamte .NET-Ökosystem und macht Unity zu einer mächtigen Plattform für komplexe Anwendungen.

### 2.3 Rendering und Grafik-Pipelines

Unity bietet mehrere Rendering-Pipelines für unterschiedliche Anforderungen. Die Built-in Render Pipeline ist die traditionelle Standardoption, die Universal Render Pipeline (URP) ist optimiert für breite Plattformunterstützung und mobile Geräte, während die High Definition Render Pipeline (HDRP) für visuell anspruchsvolle Projekte auf High-End-Hardware konzipiert ist.

Die URP hat sich als beliebte Wahl für die meisten Projekte etabliert, da sie einen guten Kompromiss zwischen visueller Qualität und Performance bietet. Sie unterstützt moderne Rendering-Features wie Post-Processing-Effekte, Shader Graph für visuelles Shader-Design und ein skalierbares Lighting-System. Die Pipeline-Architektur ermöglicht es Entwicklern, das Rendering an ihre spezifischen Projektanforderungen anzupassen.

### 2.4 Asset Store und Ökosystem

Ein wesentlicher Vorteil von Unity ist der umfangreiche Asset Store, ein Marktplatz mit tausenden von vorgefertigten Assets, Tools und Plugins. Entwickler können hier 3D-Modelle, Texturen, Audio-Dateien, komplette Gameplay-Systeme und Editor-Erweiterungen erwerben oder kostenlos herunterladen. Dies beschleunigt den Entwicklungsprozess erheblich, insbesondere für kleinere Teams oder Solo-Entwickler.

Die große Community und die umfangreiche offizielle Dokumentation machen Unity zu einer zugänglichen Plattform für Einsteiger. Unity Learn bietet strukturierte Kurse und Tutorials für alle Erfahrungsstufen. Allerdings wurde Unity in der Vergangenheit für kontroverse Geschäftsentscheidungen kritisiert, insbesondere die Ankündigung von Runtime-Gebühren im Jahr 2023, die zu einem signifikanten Vertrauensverlust in der Entwickler-Community führte.

---

## 3 Unreal Engine

### 3.1 Überblick und Geschichte

Die Unreal Engine wurde 1998 von Epic Games entwickelt und ist nach Unity die zweithäufigst verwendete kommerzielle Game Engine. Sie wurde ursprünglich für den First-Person-Shooter Unreal entwickelt und hat sich seitdem zu einer der technisch fortschrittlichsten Engines auf dem Markt entwickelt. Die Engine ist bekannt für ihre Fähigkeit, visuell beeindruckende, fotorealistische Grafiken zu erzeugen.

Bekannte Titel, die mit der Unreal Engine entwickelt wurden, umfassen Fortnite, PlayerUnknown's Battlegrounds und zahlreiche AAA-Produktionen. Die Engine wird auch außerhalb der Spieleentwicklung eingesetzt, etwa in der Filmindustrie für virtuelle Produktionen (The Mandalorian) und in der Automobilindustrie für Design-Visualisierungen.

### 3.2 Technische Highlights

Unreal Engine 5 führte mehrere bahnbrechende Technologien ein. Nanite ist ein virtualisiertes Geometrie-System, das es ermöglicht, Assets mit filmischer Qualität direkt in Spielen zu verwenden, ohne aufwendige Level-of-Detail-Optimierungen manuell erstellen zu müssen. Die Technologie streamt und skaliert die Geometrie automatisch basierend auf der Bildschirmauflösung und Kameraposition.

Lumen ist ein vollständig dynamisches Global-Illumination-System, das realistische Beleuchtung ohne vorberechnete Lightmaps ermöglicht. Änderungen an der Beleuchtung oder Geometrie werden in Echtzeit reflektiert, was den Workflow für Künstler und Level-Designer erheblich verbessert. Diese Technologien machen die Unreal Engine besonders attraktiv für Projekte, bei denen visuelle Qualität höchste Priorität hat.

### 3.3 Programmierung und Blueprints

Die Unreal Engine unterstützt zwei primäre Entwicklungsansätze: C++ für maximale Kontrolle und Performance sowie Blueprints für visuelles Scripting. Blueprints ermöglichen es Entwicklern, komplexe Spiellogik durch das Verbinden von Nodes in einem visuellen Graph zu erstellen, ohne eine Zeile Code schreiben zu müssen. Dies macht die Engine zugänglicher für Designer und Künstler ohne Programmierhintergrund.

Für performancekritische Systeme bietet C++ die volle Kontrolle über die Engine. Die Kombination beider Ansätze ist üblich: C++ für die Grundsysteme und Performance-intensive Berechnungen, Blueprints für schnelles Prototyping und spielspezifische Logik. Die Lernkurve für die Unreal Engine ist steiler als bei Unity oder Godot, bietet aber entsprechend mehr Möglichkeiten für komplexe Projekte.

### 3.4 Lizenzmodell und Zielgruppe

Die Unreal Engine ist kostenlos nutzbar, verlangt jedoch 5% Royalties auf den Bruttoumsatz nach dem ersten Million Dollar. Für viele Indie-Entwickler bedeutet dies de facto kostenlose Nutzung, während erfolgreiche Projekte entsprechend beteiligt werden. Epic Games bietet zudem Grants und Förderprogramme für vielversprechende Projekte.

Die Zielgruppe der Unreal Engine umfasst primär mittlere bis große Studios, die an visuell ambitionierten Projekten arbeiten. Die komplexe Architektur und die steile Lernkurve machen sie weniger geeignet für absolute Anfänger oder sehr kleine Projekte. Für 2D-Spiele oder einfachere 3D-Projekte sind Unity oder Godot oft die bessere Wahl aufgrund ihrer schlankeren Architektur und schnelleren Entwicklungszyklen.

---

## 4 Godot Engine

### 4.1 Überblick und Geschichte

Godot ist eine Open-Source Game Engine, die am 14. Januar 2014 von Juan Linietsky und Ariel Manzur erstmals öffentlich auf GitHub veröffentlicht wurde. Die erste stabile Version 1.0 erschien am 15. Dezember 2014. Mit der Veröffentlichung als Open-Source-Projekt unter der MIT-Lizenz begann ein stetiges Wachstum der Community und der Feature-Entwicklung.

Die Veröffentlichung von Godot 4.0 am 1. März 2023 markierte einen Meilenstein mit einem komplett überarbeiteten Vulkan-basierten Rendering-Backend, verbesserter 3D-Unterstützung und zahlreichen Quality-of-Life-Verbesserungen. Godot hat sich als beliebte Alternative zu den kommerziellen Engines etabliert, insbesondere nach den kontroversen Lizenzänderungen bei Unity im September 2023, die zu einem signifikanten Zuwachs an Godot-Nutzern führten.

### 4.2 Vorteile von Godot

Der größte Vorteil von Godot ist seine vollständige Kostenfreiheit und Open-Source-Natur. Die MIT-Lizenz erlaubt uneingeschränkte kommerzielle Nutzung ohne Lizenzgebühren oder Royalties. Entwickler können den Quellcode einsehen, modifizieren und an ihre Bedürfnisse anpassen. Dies fördert Transparenz und ermöglicht tiefes Verständnis der Engine-Funktionsweise.

Die Engine ist bemerkenswert leichtgewichtig mit einer Größe von etwa 200 MB für den Editor inklusive Projektdateien und Cache, im Vergleich zu mehreren Gigabytes bei Unity und Unreal. Der Export auf verschiedene Plattformen erfordert zusätzlich das Herunterladen von Export Templates (ca. 1,3 GB nach Installation). Der Editor startet in Sekunden und ist auch auf älteren oder leistungsschwächeren Rechnern performant nutzbar. Godot bietet erstklassige 2D-Unterstützung mit einem dedizierten 2D-Rendering-System, das nicht auf 3D-Projektion basiert, sowie solide 3D-Capabilities für die meisten Projektanforderungen.

### 4.3 Community und Entwicklung

Die Godot-Entwicklung erfolgt vollständig öffentlich auf GitHub mit transparenten Roadmaps und Issue-Tracking. Jeder kann Bugs melden, Features vorschlagen oder Code beitragen. Die Engine wird von der Godot Foundation verwaltet, einer gemeinnützigen Organisation, die 2022 gegründet wurde, um die langfristige Entwicklung sicherzustellen.

Die offizielle Dokumentation ist umfangreich und wird kontinuierlich verbessert. Neben der Dokumentation existieren Community-Ressourcen wie das offizielle Forum, Discord-Server und diverse Online-Tutorials. Kommerziell erfolgreiche Spiele wie Dome Keeper, Cassette Beasts, Brotato und Buckshot Roulette demonstrieren, dass mit Godot professionelle Produkte entwickelt werden können.

---

## 5 Godot im Detail

### 5.1 Das Node-System

Das Herzstück der Godot-Architektur ist das Node-System. Ein Node ist das grundlegende Baustein-Element in Godot und repräsentiert eine einzelne, spezialisierte Funktionalität. Im Gegensatz zu Unity's komponentenbasiertem Ansatz, bei dem GameObjects mit Components bestückt werden, sind Godot-Nodes von Grund auf für ihre spezifische Aufgabe konzipiert. Diese Designentscheidung führt zu einer klaren, intuitiven Struktur.

Godot bietet über 200 verschiedene Node-Typen für unterschiedliche Zwecke. Die wichtigsten Kategorien umfassen Node2D und Node3D als Basis für 2D- und 3D-Objekte, Control-Nodes für Benutzeroberflächen, verschiedene Body-Typen für Physik-Interaktionen wie RigidBody, CharacterBody und StaticBody, sowie spezialisierte Nodes für Audio, Animation, Navigation und mehr.

Nodes werden in einer Baumstruktur organisiert, wobei jeder Node einen Parent und beliebig viele Children haben kann. Diese Hierarchie ist fundamental für die Funktionsweise von Godot: Transformationen werden von Parents an Children vererbt, Children bewegen sich mit ihren Parents, und das Entfernen eines Parent-Nodes entfernt automatisch alle Children.

```
# Typische Node-Hierarchie eines Spielers
Player (CharacterBody3D)
|--- CollisionShape3D
|--- MeshInstance3D (Spieler-Modell)
|--- Camera3D
|--- AnimationPlayer
+--- Weapon (Node3D)
    |--- MeshInstance3D
    +--- RayCast3D
```

### 5.2 Das Scene-System

Scenes sind wiederverwendbare Sammlungen von Nodes, die als eigenständige Einheiten gespeichert und instanziiert werden können. Eine Scene wird als .tscn-Datei (Text Scene) oder .scn-Datei (Binary Scene) gespeichert und kann einen einzelnen Node oder komplexe Hierarchien mit hunderten von Nodes enthalten. Das Scene-System ist eines der leistungsstärksten Features von Godot.

Das Konzept "Everything is a Scene" bedeutet, dass sowohl ein einzelner Spieler-Charakter als auch ein gesamtes Level als Scene behandelt werden. Scenes können andere Scenes als Children enthalten, was eine modulare Entwicklung ermöglicht. Ein Spieler-Scene könnte beispielsweise eine Waffen-Scene enthalten, die wiederum Projektil-Scenes spawnen kann.

Die Instanziierung von Scenes zur Laufzeit ist ein häufiges Pattern:

```gdscript
# GDScript: Scene zur Laufzeit instanziieren
extends Node3D

@export var enemy_scene: PackedScene

func spawn_enemy(position: Vector3) -> void:
    var enemy_instance = enemy_scene.instantiate()
    enemy_instance.global_position = position
    add_child(enemy_instance)

func _on_spawn_timer_timeout() -> void:
    var random_pos = Vector3(randf_range(-10, 10), 0, randf_range(-10, 10))
    spawn_enemy(random_pos)
```

Dies ermöglicht effizientes Memory-Management und flexible Spielsysteme, bei denen Objekte dynamisch erstellt und zerstört werden.

### 5.3 Scripting mit GDScript

GDScript ist Godots eigene, Python-ähnliche Programmiersprache, die speziell für die Game-Entwicklung optimiert wurde. Die Syntax ist bewusst einfach und verzichtet auf unnötige Boilerplate-Code. GDScript ist dynamisch typisiert, unterstützt aber auch optionale Type-Hints für bessere Editor-Unterstützung und Fehlererkennung.

```gdscript
# GDScript: Vollständiger Player Controller
extends CharacterBody3D

@export var move_speed: float = 5.0
@export var jump_velocity: float = 4.5
@export var mouse_sensitivity: float = 0.002

var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")

@onready var camera: Camera3D = $Camera3D

func _ready() -> void:
    Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _input(event: InputEvent) -> void:
    if event is InputEventMouseMotion:
        rotate_y(-event.relative.x * mouse_sensitivity)
        camera.rotate_x(-event.relative.y * mouse_sensitivity)
        camera.rotation.x = clamp(camera.rotation.x, -PI/2, PI/2)

func _physics_process(delta: float) -> void:
    # Schwerkraft anwenden
    if not is_on_floor():
        velocity.y -= gravity * delta
    
    # Springen
    if Input.is_action_just_pressed("jump") and is_on_floor():
        velocity.y = jump_velocity
    
    # Bewegung
    var input_dir := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
    var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
    
    if direction:
        velocity.x = direction.x * move_speed
        velocity.z = direction.z * move_speed
    else:
        velocity.x = move_toward(velocity.x, 0, move_speed)
        velocity.z = move_toward(velocity.z, 0, move_speed)
    
    move_and_slide()
```

Die enge Integration mit der Engine ist ein Hauptvorteil von GDScript. Alle Engine-Funktionen sind direkt zugänglich, Auto-Completion im Editor ist umfassend, und die Dokumentation ist direkt verlinkt. Die Lernkurve ist gering, insbesondere für Entwickler mit Python-Erfahrung, was GDScript ideal für Einsteiger und schnelles Prototyping macht.

Wichtige GDScript-Konzepte umfassen die Lifecycle-Methoden `_ready()` für Initialisierung, `_process()` für Frame-Updates und `_physics_process()` für physik-relevante Berechnungen. Variablen können mit `@export` annotiert werden, um sie im Editor bearbeitbar zu machen. `@onready` ermöglicht das sichere Referenzieren von Child-Nodes nach der Initialisierung.

### 5.4 C# in Godot

Neben GDScript unterstützt Godot auch C# als vollwertige Scripting-Sprache. C# bietet statische Typisierung, Zugang zum .NET-Ökosystem und ist für Entwickler mit Unity-Erfahrung vertraut. Die C#-Unterstützung erfordert die .NET-Version von Godot (separat downloadbar).

```csharp
// C# in Godot: Player Controller
using Godot;

public partial class Player : CharacterBody3D
{
    [Export]
    public float MoveSpeed { get; set; } = 5.0f;
    
    [Export]
    public float JumpVelocity { get; set; } = 4.5f;
    
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        
        // Schwerkraft
        if (!IsOnFloor())
            velocity.Y -= _gravity * (float)delta;
        
        // Springen
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
            velocity.Y = JumpVelocity;
        
        // Bewegung
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * MoveSpeed;
            velocity.Z = direction.Z * MoveSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0, MoveSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0, MoveSpeed);
        }
        
        Velocity = velocity;
        MoveAndSlide();
    }
}
```

C# in Godot verwendet eine leicht andere API als GDScript, folgt aber denselben Konzepten. Nodes erben von der entsprechenden C#-Klasse, und Lifecycle-Methoden werden überschrieben. Die Integration mit dem Editor ist gut, wenngleich GDScript manchmal tiefere Integration genießt. Für komplexe Projekte oder Teams mit C#-Erfahrung ist die Sprache eine hervorragende Wahl.

Ein wichtiger Aspekt bei der Arbeit mit C# in Godot sind Autoloads (Singletons) für global zugängliche Systeme. Diese werden in den Projekteinstellungen konfiguriert und stehen dann in allen Scripts zur Verfügung:

```csharp
// C# Autoload/Singleton Beispiel: GameManager
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }
    
    public int Score { get; private set; } = 0;
    public int HighScore { get; private set; } = 0;
    
    public override void _Ready()
    {
        Instance = this;
        LoadHighScore();
    }
    
    public void AddScore(int points)
    {
        Score += points;
        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }
    }
    
    private void LoadHighScore()
    {
        // Implementierung...
    }
    
    private void SaveHighScore()
    {
        // Implementierung...
    }
}
```

### 5.5 Das Signal-System

Signals sind Godots Implementierung des Observer-Patterns und ermöglichen lose gekoppelte Kommunikation zwischen Nodes. Ein Signal ist ein benanntes Event, das von einem Node emittiert wird und von beliebig vielen anderen Nodes empfangen werden kann. Dies fördert modulares Design, da Nodes nicht direkt aufeinander verweisen müssen.

```gdscript
# GDScript: Custom Signals definieren und verwenden
extends CharacterBody3D
class_name Player

# Custom Signals definieren
signal health_changed(new_health: int, max_health: int)
signal player_died
signal coin_collected(amount: int)

@export var max_health: int = 100
var current_health: int

func _ready() -> void:
    current_health = max_health

func take_damage(amount: int) -> void:
    current_health = max(0, current_health - amount)
    health_changed.emit(current_health, max_health)
    
    if current_health <= 0:
        player_died.emit()

func heal(amount: int) -> void:
    current_health = min(max_health, current_health + amount)
    health_changed.emit(current_health, max_health)

func collect_coin(value: int) -> void:
    coin_collected.emit(value)
```

```gdscript
# UI-Script das auf Signals reagiert
extends Control

@onready var health_bar: ProgressBar = $HealthBar
@onready var score_label: Label = $ScoreLabel

var score: int = 0

func _ready() -> void:
    # Signals verbinden
    var player = get_tree().get_first_node_in_group("player")
    player.health_changed.connect(_on_player_health_changed)
    player.player_died.connect(_on_player_died)
    player.coin_collected.connect(_on_coin_collected)

func _on_player_health_changed(new_health: int, max_health: int) -> void:
    health_bar.max_value = max_health
    health_bar.value = new_health

func _on_player_died() -> void:
    $GameOverScreen.visible = true

func _on_coin_collected(amount: int) -> void:
    score += amount
    score_label.text = "Score: %d" % score
```

Built-in Signals werden von der Engine bereitgestellt, wie `body_entered` bei Collision-Nodes oder `timeout` bei Timern. Custom Signals können mit dem `signal`-Keyword definiert werden und beliebige Parameter übertragen. Die Verbindung zwischen Signal und Receiver kann im Editor visuell oder per Code mit der `connect()`-Methode hergestellt werden.

### 5.6 Rendering und Shaders

Godot 4 verwendet eine moderne Vulkan-basierte Rendering-Pipeline, die signifikante Verbesserungen gegenüber dem OpenGL-basierten Godot 3 bietet. Die Engine unterstützt verschiedene Rendering-Methoden: Forward+, Mobile und Compatibility für unterschiedliche Plattform-Anforderungen. Die Forward+-Methode bietet die beste visuelle Qualität mit Unterstützung für unbegrenzte Lichtquellen.

Für Shader-Entwicklung bietet Godot eine eigene Shading-Sprache, die GLSL ähnelt, aber vereinfacht ist:

```glsl
// Godot Shader: Einfacher Dissolve-Effekt
shader_type spatial;

uniform sampler2D noise_texture;
uniform float dissolve_amount : hint_range(0.0, 1.0) = 0.0;
uniform vec3 edge_color : source_color = vec3(1.0, 0.5, 0.0);
uniform float edge_width : hint_range(0.0, 0.2) = 0.05;

void fragment() {
    float noise = texture(noise_texture, UV).r;
    
    // Dissolve-Berechnung
    float dissolve_edge = dissolve_amount + edge_width;
    
    if (noise < dissolve_amount) {
        discard;
    } else if (noise < dissolve_edge) {
        // Glühende Kante
        ALBEDO = edge_color;
        EMISSION = edge_color * 2.0;
    }
}
```

Shader können für Canvas (2D), Spatial (3D) und Particles geschrieben werden. Der Visual Shader Editor ermöglicht node-basiertes Shader-Design für Entwickler ohne Shader-Programmiererfahrung.

### 5.7 State Machines in Godot

Finite State Machines (FSM) sind ein fundamentales Designpattern für die Verwaltung komplexer Objektzustände, etwa für KI-Verhalten, Spieler-Charaktere oder UI-Systeme. Godot bietet mit dem AnimationTree einen integrierten State Machine für Animationen, für Gameplay-Logik werden State Machines typischerweise manuell implementiert.

```gdscript
# GDScript: State Machine Basisklasse
class_name State
extends Node

var state_machine: StateMachine

func enter() -> void:
    pass

func exit() -> void:
    pass

func update(_delta: float) -> void:
    pass

func physics_update(_delta: float) -> void:
    pass
```

```gdscript
# StateMachine Manager
class_name StateMachine
extends Node

@export var initial_state: State
var current_state: State
var states: Dictionary = {}

func _ready() -> void:
    for child in get_children():
        if child is State:
            states[child.name.to_lower()] = child
            child.state_machine = self
    
    if initial_state:
        current_state = initial_state
        current_state.enter()

func _process(delta: float) -> void:
    if current_state:
        current_state.update(delta)

func _physics_process(delta: float) -> void:
    if current_state:
        current_state.physics_update(delta)

func transition_to(state_name: String) -> void:
    if not states.has(state_name):
        return
    
    if current_state:
        current_state.exit()
    
    current_state = states[state_name]
    current_state.enter()
```

```gdscript
# Konkreter State: IdleState
class_name IdleState
extends State

@export var player: CharacterBody3D

func enter() -> void:
    player.get_node("AnimationPlayer").play("idle")

func physics_update(delta: float) -> void:
    var input_dir = Input.get_vector("move_left", "move_right", "move_forward", "move_back")
    
    if input_dir != Vector2.ZERO:
        state_machine.transition_to("walk")
    
    if Input.is_action_just_pressed("jump") and player.is_on_floor():
        state_machine.transition_to("jump")
```

Diese Architektur führt zu sauberem, wartbarem Code für komplexe Verhaltensmuster.

---

## 6 WebRTC und Multiplayer Networking

### 6.1 Grundlagen von WebRTC

WebRTC (Web Real-Time Communication) ist ein offener Standard für Echtzeit-Kommunikation direkt zwischen Browsern und Anwendungen ohne die Notwendigkeit von Plugins oder zusätzlicher Software. Ursprünglich von Google entwickelt und 2011 als Open-Source-Projekt veröffentlicht, ermöglicht WebRTC die Übertragung von Audio, Video und beliebigen Daten zwischen Peers.

Die Technologie basiert auf drei Hauptkomponenten: MediaStream für die Erfassung von Audio und Video, RTCPeerConnection für die Peer-to-Peer-Verbindung und den Datenaustausch, sowie RTCDataChannel für die bidirektionale Übertragung beliebiger Daten. Für Spieleanwendungen ist besonders der DataChannel relevant, da er niedrige Latenz und optionale Zuverlässigkeit bietet.

WebRTC verwendet standardmäßig UDP für die Datenübertragung, was minimale Latenz gewährleistet. Der DataChannel unterstützt sowohl zuverlässige (geordnete, garantierte Zustellung) als auch unzuverlässige (schnellere, aber möglicherweise verlorene Pakete) Übertragungsmodi, was Entwicklern Flexibilität für verschiedene Anwendungsfälle gibt.

### 6.2 NAT Traversal und Signaling

Eine der größten Herausforderungen bei Peer-to-Peer-Verbindungen ist das NAT (Network Address Translation) Traversal. Die meisten Geräte befinden sich hinter Routern mit NAT, was direkte Verbindungen erschwert. WebRTC löst dieses Problem durch ICE (Interactive Connectivity Establishment), ein Framework das verschiedene Verbindungsmethoden kombiniert.

STUN (Session Traversal Utilities for NAT) Server helfen Clients, ihre öffentliche IP-Adresse und Port zu ermitteln. Wenn eine direkte Verbindung nicht möglich ist, kommt TURN (Traversal Using Relays around NAT) zum Einsatz, wobei ein Relay-Server den gesamten Traffic zwischen den Peers weiterleitet. TURN ist ressourcenintensiver, funktioniert aber in nahezu allen Netzwerkkonfigurationen.

Der Signaling-Prozess selbst ist nicht Teil des WebRTC-Standards und kann über beliebige Kanäle erfolgen, typischerweise WebSockets. Während des Signalings tauschen die Peers SDP (Session Description Protocol) Nachrichten aus, die Informationen über unterstützte Codecs, Verschlüsselung und Netzwerkkonfiguration enthalten.

### 6.3 Coturn als TURN/STUN Server

Coturn ist eine Open-Source-Implementierung eines TURN und STUN Servers, die häufig für WebRTC-Anwendungen eingesetzt wird. Der Server kann selbst gehostet werden und bietet volle Kontrolle über die Infrastruktur sowie Unabhängigkeit von Drittanbietern.

Die Konfiguration von Coturn umfasst typischerweise die Festlegung von Listening-Ports, Realm-Einstellungen für die Authentifizierung, und optional TLS-Zertifikate für verschlüsselte Verbindungen. Für Produktionsumgebungen ist die Einrichtung von Authentifizierung und Rate-Limiting wichtig, um Missbrauch zu verhindern.

```bash
# Beispiel Coturn Konfiguration (turnserver.conf)
listening-port=3478
tls-listening-port=5349
realm=example.com
server-name=turn.example.com

# Authentifizierung
lt-cred-mech
user=username:password

# Logging
log-file=/var/log/coturn/turnserver.log
verbose
```

Die Integration in Spieleanwendungen erfolgt durch Angabe der STUN/TURN Server URLs in der ICE-Konfiguration. Clients versuchen zunächst eine direkte Verbindung, fallen bei Bedarf auf STUN zurück, und nutzen TURN nur als letzte Option.

### 6.4 WebRTC in Godot

Godot bietet native Unterstützung für WebRTC über die WebRTCPeerConnection und WebRTCDataChannel Klassen. Diese ermöglichen die Implementierung von Peer-to-Peer Multiplayer ohne externe Plugins. Die High-Level Multiplayer API von Godot kann mit WebRTC als Transport-Layer verwendet werden.

```gdscript
# GDScript: WebRTC Peer Connection Setup
extends Node

var peer: WebRTCPeerConnection
var data_channel: WebRTCDataChannel

func _ready() -> void:
    peer = WebRTCPeerConnection.new()
    
    # ICE Server konfigurieren
    var ice_servers = {
        "iceServers": [
            {"urls": ["stun:stun.l.google.com:19302"]},
            {
                "urls": ["turn:turn.example.com:3478"],
                "username": "user",
                "credential": "password"
            }
        ]
    }
    peer.initialize(ice_servers)
    
    # Signaling Callbacks
    peer.session_description_created.connect(_on_session_description_created)
    peer.ice_candidate_created.connect(_on_ice_candidate_created)
    
    # Data Channel erstellen (nur Initiator)
    data_channel = peer.create_data_channel("game_data", {
        "negotiated": true,
        "id": 1,
        "ordered": false  # Unzuverlässig für schnelle Updates
    })
    data_channel.message_received.connect(_on_message_received)

func create_offer() -> void:
    peer.create_offer()

func _on_session_description_created(type: String, sdp: String) -> void:
    peer.set_local_description(type, sdp)
    # SDP über Signaling Server an anderen Peer senden
    send_to_signaling_server({"type": type, "sdp": sdp})

func _on_ice_candidate_created(media: String, index: int, name: String) -> void:
    # ICE Candidate über Signaling Server senden
    send_to_signaling_server({
        "candidate": name,
        "sdpMid": media,
        "sdpMLineIndex": index
    })

func _on_message_received() -> void:
    var message = data_channel.get_packet().get_string_from_utf8()
    print("Received: ", message)

func send_game_data(data: String) -> void:
    if data_channel.get_ready_state() == WebRTCDataChannel.STATE_OPEN:
        data_channel.put_packet(data.to_utf8_buffer())
```

Für die Integration mit Godots High-Level Multiplayer API existiert die WebRTCMultiplayerPeer Klasse, die WebRTC als Transport für das MultiplayerAPI System verwendet. Dies ermöglicht die Nutzung von RPCs und synchronisierten Variablen über WebRTC-Verbindungen.

### 6.5 Multiplayer Architekturen

Bei der Entwicklung von Multiplayer-Spielen gibt es verschiedene Architekturansätze. Die Client-Server-Architektur verwendet einen autoritativen Server, der den Spielzustand verwaltet und Entscheidungen trifft. Clients senden Inputs an den Server und erhalten Zustandsupdates zurück. Diese Architektur bietet guten Schutz vor Cheating, erfordert aber Server-Infrastruktur.

Die Peer-to-Peer-Architektur, die WebRTC ermöglicht, verbindet Spieler direkt miteinander ohne zentralen Spielserver. Ein Peer kann als Host fungieren und autoritative Entscheidungen treffen, oder alle Peers können gleichberechtigt sein mit Lockstep-Synchronisation. P2P reduziert Infrastrukturkosten, ist aber anfälliger für Cheating und erfordert sorgfältige Synchronisation.

Hybride Ansätze kombinieren beide Architekturen: Ein leichtgewichtiger Server übernimmt Matchmaking und Authentifizierung, während die eigentliche Spielkommunikation peer-to-peer erfolgt. Dies reduziert Serverkosten bei gleichzeitiger Beibehaltung zentraler Kontrolle über wichtige Funktionen.

---

## 7 Grundlagen des Reinforcement Learning

### 7.1 Was ist Reinforcement Learning?

Reinforcement Learning (RL) ist ein Teilgebiet des maschinellen Lernens, bei dem ein Agent durch Interaktion mit einer Umgebung lernt, optimale Entscheidungen zu treffen. Im Gegensatz zu Supervised Learning, wo Trainingsbeispiele mit korrekten Antworten vorliegen, erhält der Agent im RL nur numerisches Feedback in Form von Rewards, die die Qualität seiner Aktionen bewerten.

Das grundlegende RL-Framework besteht aus einem Agenten, der in einer Umgebung (Environment) agiert. Der Agent beobachtet den aktuellen Zustand (State), wählt eine Aktion (Action), und die Umgebung antwortet mit einem neuen Zustand und einem Reward. Das Ziel des Agenten ist es, eine Policy zu erlernen, die den kumulativen, langfristigen Reward maximiert.

```
+-------------------------------------------------------+
|                     RL Framework                      |
|                                                       |
|                        Action                         |
|   +----------+   ----------------->   +-----------+   |
|   |          |                        |           |   |
|   |  Agent   |                        |Environment|   |
|   |          |                        |           |   |
|   +----------+   <-----------------   +-----------+   |
|                    State, Reward                      |
|                                                       |
+-------------------------------------------------------+
```

Diese Formulierung macht RL besonders geeignet für Probleme, bei denen der optimale Lösungsweg nicht bekannt ist, aber die Qualität von Ergebnissen bewertet werden kann. Prominente Beispiele umfassen das Erlernen von Spielstrategien wie bei AlphaGo, Roboter-Steuerung und autonomes Fahren.

### 7.2 Zentrale Konzepte

Der State beschreibt alle relevanten Informationen über die aktuelle Situation der Umgebung. In einem Videospiel könnte dies die Position aller Objekte, die Gesundheit des Spielers und andere Spielvariablen umfassen. Der Observation Space definiert die Struktur und den Wertebereich aller möglichen States, die der Agent wahrnehmen kann.

Actions sind die möglichen Entscheidungen, die der Agent treffen kann. Der Action Space kann diskret sein, etwa bei der Wahl zwischen links, rechts, oder springen, oder kontinuierlich, etwa bei der Steuerung eines Lenkrads mit beliebigen Winkeln. Die Wahl zwischen diskreten und kontinuierlichen Action Spaces beeinflusst die verwendbaren Algorithmen und die Komplexität des Lernproblems.

```python
# Python: Gymnasium Environment Interface
import gymnasium as gym
import numpy as np

# Environment erstellen
env = gym.make("CartPole-v1")

# Spaces inspizieren
print(f"Observation Space: {env.observation_space}")
print(f"Action Space: {env.action_space}")

# Einfache Interaktionsschleife
observation, info = env.reset()

for step in range(1000):
    # Zufällige Aktion wählen
    action = env.action_space.sample()
    
    # Aktion ausführen
    observation, reward, terminated, truncated, info = env.step(action)
    
    if terminated or truncated:
        observation, info = env.reset()

env.close()
```

Die Policy ist die Strategie des Agenten, die für jeden State eine Aktion (oder eine Wahrscheinlichkeitsverteilung über Aktionen) bestimmt. Das Ziel des RL-Trainings ist es, eine optimale Policy zu finden, die den erwarteten kumulativen Reward maximiert. Der Discount Factor Gamma gewichtet dabei zukünftige Rewards gegenüber unmittelbaren.

### 7.3 Algorithmen-Kategorien

Value-based Methods wie Q-Learning und DQN (Deep Q-Network) lernen eine Value Function, die den erwarteten kumulativen Reward für jeden State (oder State-Action-Paar) schätzt. Die Policy wird implizit abgeleitet, indem immer die Aktion mit dem höchsten geschätzten Wert gewählt wird.

Policy-based Methods wie REINFORCE und Policy Gradient lernen die Policy direkt, ohne den Umweg über eine Value Function. Diese Methoden sind besonders geeignet für kontinuierliche Action Spaces und können stochastische Policies darstellen. Sie sind jedoch oft mit hoher Varianz in den Gradienten konfrontiert.

Actor-Critic Methods kombinieren beide Ansätze: Ein Actor lernt die Policy, während ein Critic die Value Function schätzt. Der Critic reduziert die Varianz des Policy-Gradienten, was zu stabilerem Training führt. Moderne Algorithmen wie PPO (Proximal Policy Optimization) und A3C gehören zu dieser Kategorie.

### 7.4 Deep Q-Networks (DQN)

Deep Q-Networks revolutionierten das Feld des Deep Reinforcement Learning durch die Kombination von Q-Learning mit tiefen neuronalen Netzen. Der bahnbrechende Erfolg von DQN wurde 2015 von DeepMind demonstriert, als ein Agent lernte, Atari-Spiele auf übermenschlichem Niveau zu spielen, nur durch Beobachtung der Pixel-Ausgabe.

```python
# Python: Einfaches DQN mit PyTorch
import torch
import torch.nn as nn
import torch.optim as optim
import numpy as np
from collections import deque
import random

class DQNetwork(nn.Module):
    def __init__(self, state_size, action_size):
        super(DQNetwork, self).__init__()
        self.fc1 = nn.Linear(state_size, 64)
        self.fc2 = nn.Linear(64, 64)
        self.fc3 = nn.Linear(64, action_size)
    
    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        return self.fc3(x)

class DQNAgent:
    def __init__(self, state_size, action_size):
        self.state_size = state_size
        self.action_size = action_size
        self.memory = deque(maxlen=10000)
        self.gamma = 0.99    # Discount factor
        self.epsilon = 1.0   # Exploration rate
        self.epsilon_min = 0.01
        self.epsilon_decay = 0.995
        self.learning_rate = 0.001
        
        self.model = DQNetwork(state_size, action_size)
        self.target_model = DQNetwork(state_size, action_size)
        self.optimizer = optim.Adam(self.model.parameters(), lr=self.learning_rate)
        
        self.update_target_model()
    
    def update_target_model(self):
        self.target_model.load_state_dict(self.model.state_dict())
    
    def remember(self, state, action, reward, next_state, done):
        self.memory.append((state, action, reward, next_state, done))
    
    def act(self, state):
        if np.random.rand() <= self.epsilon:
            return random.randrange(self.action_size)
        
        with torch.no_grad():
            state_tensor = torch.FloatTensor(state).unsqueeze(0)
            q_values = self.model(state_tensor)
            return q_values.argmax().item()
    
    def replay(self, batch_size):
        if len(self.memory) < batch_size:
            return
        
        minibatch = random.sample(self.memory, batch_size)
        
        for state, action, reward, next_state, done in minibatch:
            state_tensor = torch.FloatTensor(state).unsqueeze(0)
            next_state_tensor = torch.FloatTensor(next_state).unsqueeze(0)
            
            target = reward
            if not done:
                with torch.no_grad():
                    target = reward + self.gamma * self.target_model(next_state_tensor).max().item()
            
            current_q = self.model(state_tensor)
            target_q = current_q.clone()
            target_q[0][action] = target
            
            loss = nn.MSELoss()(current_q, target_q)
            self.optimizer.zero_grad()
            loss.backward()
            self.optimizer.step()
        
        if self.epsilon > self.epsilon_min:
            self.epsilon *= self.epsilon_decay
```

DQN verwendet zwei wichtige Techniken für stabiles Training: Experience Replay speichert vergangene Erfahrungen (State, Action, Reward, Next State) in einem Buffer und sampelt daraus zufällige Mini-Batches für das Training. Dies bricht die Korrelation zwischen aufeinanderfolgenden Samples und verbessert die Dateneffizienz.

Das Target Network ist eine Kopie des Q-Networks, die seltener aktualisiert wird und für die Berechnung der Target-Q-Values verwendet wird. Dies verhindert instabile Updates, bei denen das Netzwerk seine eigenen, sich ändernden Predictions verfolgen würde. DQN ist gut geeignet für Probleme mit diskreten Action Spaces und bildbasierten States.

### 7.5 Proximal Policy Optimization (PPO)

PPO ist ein Policy-Gradient-Algorithmus, der sich als einer der robustesten und am häufigsten verwendeten RL-Algorithmen etabliert hat. Der Algorithmus bietet einen guten Kompromiss zwischen Implementierungskomplexität und Performance.

Die Kernidee von PPO ist das Clipping der Policy-Updates, um zu große Änderungen zu verhindern. Frühere Policy-Gradient-Methoden litten unter dem Problem, dass zu große Updates die Policy zerstören konnten. PPO begrenzt das Verhältnis zwischen neuer und alter Policy, was zu stabilem Training führt, auch mit größeren Lernraten.

```python
# Python: PPO Training mit Stable-Baselines3
from stable_baselines3 import PPO
from stable_baselines3.common.env_util import make_vec_env
from stable_baselines3.common.callbacks import EvalCallback

# Vectorized Environment für paralleles Sampling
env = make_vec_env("LunarLander-v2", n_envs=4)

# PPO Modell erstellen
model = PPO(
    "MlpPolicy",
    env,
    learning_rate=3e-4,
    n_steps=2048,
    batch_size=64,
    n_epochs=10,
    gamma=0.99,
    gae_lambda=0.95,
    clip_range=0.2,
    verbose=1,
    tensorboard_log="./ppo_tensorboard/"
)

# Callback für Evaluation
eval_callback = EvalCallback(
    env,
    best_model_save_path="./best_model/",
    log_path="./logs/",
    eval_freq=10000,
    deterministic=True
)

# Training starten
model.learn(
    total_timesteps=500000,
    callback=eval_callback,
    progress_bar=True
)

# Modell speichern
model.save("ppo_lunarlander")

# ONNX Export für Deployment
# model.policy.to("cpu")
# torch.onnx.export(model.policy, ...)
```

PPO ist besonders populär für kontinuierliche Control-Tasks, Robotik und Game AI. Die relative Einfachheit der Implementierung im Vergleich zu anderen Algorithmen wie TRPO, bei gleichzeitig vergleichbarer Performance, macht PPO zur Standardwahl für viele Anwendungen. Stable-Baselines3 bietet eine robuste, gut getestete PPO-Implementierung.

---

## 8 Neuronale Netze und Deep Learning

### 8.1 Grundlagen neuronaler Netze

Neuronale Netze sind von biologischen Nervensystemen inspirierte Berechnungsmodelle, die aus miteinander verbundenen Neuronen bestehen. Ein künstliches Neuron empfängt gewichtete Inputs, summiert diese auf und wendet eine Aktivierungsfunktion an, um den Output zu berechnen. Durch das Schichten von vielen Neuronen entstehen tiefe Netze mit der Fähigkeit, komplexe Muster zu lernen.

Die Architektur eines Feedforward-Netzes besteht aus einem Input Layer, das die Eingabedaten empfängt, einem oder mehreren Hidden Layers, die Zwischenrepräsentationen berechnen, und einem Output Layer, das die finale Vorhersage liefert. Die Stärke neuronaler Netze liegt in ihrer Fähigkeit, automatisch relevante Features aus den Rohdaten zu extrahieren.

```python
# Python: Einfaches Neuronales Netz mit PyTorch
import torch
import torch.nn as nn

class SimpleNeuralNetwork(nn.Module):
    def __init__(self, input_size, hidden_size, output_size):
        super(SimpleNeuralNetwork, self).__init__()
        
        self.layer1 = nn.Linear(input_size, hidden_size)
        self.activation1 = nn.ReLU()
        self.layer2 = nn.Linear(hidden_size, hidden_size)
        self.activation2 = nn.ReLU()
        self.layer3 = nn.Linear(hidden_size, output_size)
    
    def forward(self, x):
        x = self.layer1(x)
        x = self.activation1(x)
        x = self.layer2(x)
        x = self.activation2(x)
        x = self.layer3(x)
        return x

# Beispiel: Netzwerk erstellen und verwenden
model = SimpleNeuralNetwork(input_size=4, hidden_size=64, output_size=2)

# Beispiel-Input
sample_input = torch.randn(1, 4)
output = model(sample_input)
print(f"Output: {output}")
```

Das Training neuronaler Netze erfolgt durch Backpropagation, einen Algorithmus zur effizienten Berechnung von Gradienten. Ein Loss-Function misst den Unterschied zwischen Vorhersage und Zielwert, und Gradient Descent passt die Gewichte schrittweise an, um den Loss zu minimieren. Die Lernrate kontrolliert die Schrittgröße dieser Updates.

### 8.2 PyTorch als Framework

PyTorch ist ein Open-Source Deep Learning Framework, das von Meta (Facebook) entwickelt wurde und sich als eines der führenden Frameworks für Forschung und Produktion etabliert hat. Seine dynamische Computation Graph-Architektur ermöglicht flexibles Debugging und intuitive Entwicklung.

Die Kernkonzepte von PyTorch umfassen Tensors als mehrdimensionale Arrays mit GPU-Beschleunigung, Automatic Differentiation für die automatische Gradientenberechnung und ein modulares System zur Definition von Netzwerk-Architekturen. Die API ist Python-nativ und erlaubt natürliche Kontrollflussstrukturen innerhalb von Modellen.

```python
# Python: Training Loop mit PyTorch
import torch
import torch.nn as nn
import torch.optim as optim

# Model, Loss und Optimizer
model = SimpleNeuralNetwork(input_size=4, hidden_size=64, output_size=2)
criterion = nn.MSELoss()
optimizer = optim.Adam(model.parameters(), lr=0.001)

# Training Loop
def train_step(model, inputs, targets):
    model.train()
    
    # Forward pass
    outputs = model(inputs)
    loss = criterion(outputs, targets)
    
    # Backward pass
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    
    return loss.item()

# Beispiel-Training
for epoch in range(100):
    # Dummy-Daten
    inputs = torch.randn(32, 4)
    targets = torch.randn(32, 2)
    
    loss = train_step(model, inputs, targets)
    
    if epoch % 10 == 0:
        print(f"Epoch {epoch}, Loss: {loss:.4f}")
```

Für Reinforcement Learning ist PyTorch die bevorzugte Wahl vieler Forscher und Bibliotheken. Stable-Baselines3, die Standard-RL-Bibliothek, basiert vollständig auf PyTorch. Die Kombination aus Flexibilität, Performance und dem umfangreichen Ökosystem macht PyTorch ideal für die Entwicklung und das Training von RL-Agenten.

---

## 9 Godot RL Agents

### 9.1 Überblick und Motivation

Godot RL Agents ist ein Open-Source-Framework, das die Integration von Reinforcement Learning mit der Godot Game Engine ermöglicht. Das Projekt wurde von Edward Beeching entwickelt. Es adressiert den Bedarf an einer zugänglichen, gut integrierten Lösung für RL in Spielumgebungen.

Die Motivation für Godot RL Agents entstand aus der Beobachtung, dass bestehende RL-Frameworks für Spiele oft komplexe Setups erfordern oder auf spezifische Engines beschränkt sind. Die Kombination von Godots Zugänglichkeit und Open-Source-Natur mit modernen RL-Algorithmen schafft eine Plattform, die sowohl für Forschung als auch für praktische Game-AI-Anwendungen geeignet ist.

### 9.2 Architektur und Komponenten

Das Framework besteht aus zwei Hauptkomponenten: einem GDExtension-Plugin für Godot und einer Python-Bibliothek für das Training. Das Godot-Plugin stellt die notwendigen Nodes und Klassen bereit, um Spielumgebungen als RL-Environments zu definieren, während die Python-Seite das Training mit Stable-Baselines3 oder anderen Frameworks übernimmt.

```
+--------------------------------------------------------------+
|                  Godot RL Agents Architektur                 |
|                                                              |
|   +-------------------+            +---------------------+   |
|   |   Godot Engine    |            |   Python Training   |   |
|   |                   |            |                     |   |
|   | +---------------+ |    TCP/    | +-----------------+ |   |
|   | | AIController  | | <--------> | |Stable-Baselines3| |   |
|   | +---------------+ |   Shared   | +-----------------+ |   |
|   |        |          |   Memory   |         |           |   |
|   |        v          |            |         v           |   |
|   | +---------------+ |            | +-----------------+ |   |
|   | |    Sensors    | |            | |    PPO / DQN    | |   |
|   | +---------------+ |            | +-----------------+ |   |
|   |                   |            |                     |   |
|   +-------------------+            +---------------------+   |
|                                                              |
+---------------------------------------------------------------+
```

Die Kommunikation zwischen Godot und Python erfolgt über einen SharedMemory-Mechanismus oder Sockets. Dieser Ansatz erlaubt paralleles Training mit mehreren Godot-Instanzen für beschleunigtes Lernen. Die API ist inspiriert von Gymnasium (ehemals OpenAI Gym), dem Standard-Interface für RL-Environments, was die Integration mit bestehenden RL-Codebasen erleichtert.

### 9.3 AIController und Sensoren

Der AIController ist der zentrale Node für RL-Agenten in Godot RL Agents. Er verwaltet die Observation- und Action-Spaces, sammelt Sensor-Daten und wendet Aktionen an:

```gdscript
# GDScript: AI Controller Setup
extends AIController3D

@onready var player: CharacterBody3D = $".."
@onready var raycast_sensor: RaycastSensor3D = $RaycastSensor3D

func _physics_process(delta: float) -> void:
    # Observations sammeln und Actions anwenden
    n_steps += 1
    
    if n_steps >= 10:  # Action-Frequenz
        n_steps = 0

func get_obs() -> Dictionary:
    # Observations definieren
    var obs = []
    
    # Spieler-Position (normalisiert)
    obs.append(player.global_position.x / 10.0)
    obs.append(player.global_position.z / 10.0)
    
    # Spieler-Geschwindigkeit
    obs.append(player.velocity.x / 10.0)
    obs.append(player.velocity.z / 10.0)
    
    # Ziel-Richtung
    var to_goal = (goal_position - player.global_position).normalized()
    obs.append(to_goal.x)
    obs.append(to_goal.z)
    
    # Raycast-Sensor Daten
    obs.append_array(raycast_sensor.get_observation())
    
    return {"obs": obs}

func get_action_space() -> Dictionary:
    return {
        "move_x": {"size": 1, "action_type": "continuous"},
        "move_z": {"size": 1, "action_type": "continuous"},
        "jump": {"size": 2, "action_type": "discrete"}
    }

func set_action(action: Dictionary) -> void:
    # Actions auf Spieler anwenden
    var move_dir = Vector3(
        action["move_x"][0],
        0,
        action["move_z"][0]
    ).normalized()
    
    player.velocity.x = move_dir.x * player.move_speed
    player.velocity.z = move_dir.z * player.move_speed
    
    if action["jump"] == 1 and player.is_on_floor():
        player.velocity.y = player.jump_velocity

func get_reward() -> float:
    var reward = 0.0
    
    # Belohnung für Nähe zum Ziel
    var distance_to_goal = player.global_position.distance_to(goal_position)
    reward -= distance_to_goal * 0.01  # Strafe für Distanz
    
    # Bonus für Zielerreichung
    if distance_to_goal < 1.0:
        reward += 10.0
        done = true
    
    # Strafe für Fallen
    if player.global_position.y < -5:
        reward -= 5.0
        done = true
    
    return reward

func is_done() -> bool:
    return done

func reset() -> void:
    done = false
    player.global_position = start_position
    player.velocity = Vector3.ZERO
```

Das Framework bietet verschiedene Sensor-Typen für die Observation-Erfassung. RaycastSensors simulieren Lidar-ähnliche Wahrnehmung, indem sie Strahlen in die Umgebung senden und Distanzen zu Kollisionen zurückgeben. Zusätzlich können beliebige Spielvariablen wie Position, Geschwindigkeit oder Spielstand als Observations eingebunden werden.

### 9.4 Training und Inference

Das Training erfolgt typischerweise mit der Python-Bibliothek gdrl, die auf Stable-Baselines3 aufbaut:

```python
# Python: Godot RL Training Script
import argparse
from stable_baselines3 import PPO
from stable_baselines3.common.vec_env import SubprocVecEnv
from stable_baselines3.common.callbacks import CheckpointCallback
from godot_rl.core.godot_env import GodotEnv

def make_env(env_path, port):
    def _init():
        env = GodotEnv(
            env_path=env_path,
            port=port,
            show_window=True,
            seed=42
        )
        return env
    return _init

def train():
    # Mehrere parallele Environments
    num_envs = 4
    env_path = "path/to/godot_project.x86_64"
    
    envs = SubprocVecEnv([
        make_env(env_path, 10000 + i) 
        for i in range(num_envs)
    ])
    
    # PPO Model
    model = PPO(
        "MlpPolicy",
        envs,
        learning_rate=3e-4,
        n_steps=1024,
        batch_size=256,
        n_epochs=4,
        gamma=0.99,
        gae_lambda=0.95,
        clip_range=0.2,
        ent_coef=0.01,
        verbose=1,
        tensorboard_log="./logs/"
    )
    
    # Checkpoint Callback
    checkpoint_callback = CheckpointCallback(
        save_freq=50000,
        save_path="./checkpoints/",
        name_prefix="godot_agent"
    )
    
    # Training
    model.learn(
        total_timesteps=1_000_000,
        callback=checkpoint_callback,
        progress_bar=True
    )
    
    # Speichern
    model.save("godot_trained_agent")
    
    # ONNX Export
    export_to_onnx(model, "godot_agent.onnx")

def export_to_onnx(model, filename):
    """Export trained model to ONNX format for Godot inference"""
    import torch
    
    # Dummy input für Export
    obs_shape = model.observation_space.shape
    dummy_input = torch.randn(1, *obs_shape)
    
    torch.onnx.export(
        model.policy,
        dummy_input,
        filename,
        opset_version=11,
        input_names=["obs"],
        output_names=["action"],
        dynamic_axes={
            "obs": {0: "batch_size"},
            "action": {0: "batch_size"}
        }
    )
    print(f"Model exported to {filename}")

if __name__ == "__main__":
    train()
```

Trainierte Modelle können im ONNX-Format exportiert werden, einem standardisierten Format für Machine Learning Modelle. ONNX ermöglicht die Inference direkt in Godot ohne Python-Abhängigkeit, was für den finalen Einsatz in Spielen essentiell ist:

```gdscript
# GDScript: ONNX Inference in Godot
extends Node

var onnx_model: ONNXModel

func _ready() -> void:
    # ONNX Model laden
    onnx_model = ONNXModel.new()
    onnx_model.load("res://models/trained_agent.onnx")

func get_action(observations: Array) -> Array:
    # Observations als Input formatieren
    var input_tensor = PackedFloat32Array(observations)
    
    # Inference durchführen
    var output = onnx_model.run(input_tensor)
    
    return output
```

### 9.5 Reward Engineering

Die Definition einer geeigneten Reward-Funktion ist oft die größte Herausforderung beim Anwenden von RL auf neue Probleme. Das Reward-Signal muss das gewünschte Verhalten effektiv kommunizieren, ohne unbeabsichtigte Nebeneffekte zu fördern. Godot RL Agents ermöglicht flexible Reward-Definitionen direkt im GDScript-Code.

```gdscript
# GDScript: Komplexe Reward-Funktion
extends AIController3D

var previous_distance_to_goal: float = 0.0
var time_penalty_accumulator: float = 0.0

func get_reward() -> float:
    var reward = 0.0
    
    # 1. Distanz-basierter Shaping Reward
    var current_distance = player.global_position.distance_to(goal_position)
    var distance_delta = previous_distance_to_goal - current_distance
    reward += distance_delta * 1.0  # Belohnung für Annäherung
    previous_distance_to_goal = current_distance
    
    # 2. Ziel erreicht - großer Bonus
    if current_distance < 1.0:
        reward += 100.0
        done = true
        return reward
    
    # 3. Zeit-Strafe (ermutigt schnelles Handeln)
    time_penalty_accumulator += get_physics_process_delta_time()
    reward -= 0.001 * time_penalty_accumulator
    
    # 4. Kollisions-Strafen
    if player.is_colliding_with_obstacle():
        reward -= 1.0
    
    # 5. Energie-Effizienz (optional)
    var speed = player.velocity.length()
    if speed > player.max_efficient_speed:
        reward -= 0.01 * (speed - player.max_efficient_speed)
    
    # 6. Tod / Fallen
    if player.global_position.y < -10 or player.health <= 0:
        reward -= 50.0
        done = true
    
    return reward
```

Typische Reward-Komponenten in Spielen umfassen positive Rewards für Zielerreichung (Punkte sammeln, Level abschließen), negative Rewards als Strafen (Schaden nehmen, Zeit verschwenden) und Shaping Rewards für Zwischenziele (näher ans Ziel kommen, richtige Richtung einschlagen). Die Balance dieser Komponenten bestimmt maßgeblich das erlernte Verhalten.

Curriculum Learning ist eine Technik, bei der die Schwierigkeit der Aufgabe während des Trainings schrittweise erhöht wird. In Godot kann dies durch dynamische Anpassung der Environment-Parameter implementiert werden:

```gdscript
# GDScript: Curriculum Learning
extends Node

var difficulty_level: int = 0
var success_count: int = 0
var required_successes: int = 10

func on_episode_end(success: bool) -> void:
    if success:
        success_count += 1
        
        if success_count >= required_successes:
            increase_difficulty()
            success_count = 0

func increase_difficulty() -> void:
    difficulty_level += 1

    match difficulty_level:
        1:
            # Mehr Hindernisse
            obstacle_count = 5
        2:
            # Bewegende Hindernisse
            enable_moving_obstacles = true
        3:
            # Kürzere Zeitlimits
            time_limit = 30.0
        4:
            # Komplexere Ziele
            enable_multiple_goals = true

    print("Difficulty increased to level ", difficulty_level)
```


---




# Praxisteil Rath

## Inhaltsverzeichnis

10. [Projektübersicht und Anforderungen](#10-projektübersicht-und-anforderungen)
11. [Projektstruktur und Setup](#11-projektstruktur-und-setup)
12. [Spielmechanik-Implementierung](#12-spielmechanik-implementierung)
13. [Multiplayer-Implementierung](#13-multiplayer-implementierung)
14. [KI-Gegner mit Reinforcement Learning](#14-ki-gegner-mit-reinforcement-learning)
15. [User Interface](#15-user-interface)
16. [Testing und Qualitätssicherung](#16-testing-und-qualitätssicherung)
17. [Deployment und Distribution](#17-deployment-und-distribution)
18. [Herausforderungen und Lösungen](#18-herausforderungen-und-lösungen)

---

*Hinweis: Die in diesem Teil gezeigten Code-Beispiele basieren auf dem tatsächlichen Quellcode des Projekts, wurden jedoch zur besseren Lesbarkeit gekürzt und vereinfacht. Nicht relevante Teile wie Fehlerbehandlung, Logging oder Hilfsmethoden wurden ausgelassen.*

## 10 Projektübersicht und Anforderungen

### 10.1 Spielkonzept

DigiKicker ist ein vollständig in 3D umgesetztes Tischfußball-Spiel (Foosball), das die klassische Spielmechanik eines physischen Kickertisches in eine digitale Form überträgt. Das Projekt demonstriert die Anwendung moderner Spieleentwicklungstechnologien und kombiniert drei zentrale Aspekte: realistische Physik-Simulation, Online-Multiplayer über WebRTC und einen KI-Gegner, der mittels Reinforcement Learning trainiert wurde.

Das Spielprinzip folgt den etablierten Regeln des Tischfußballs: Zwei Spieler steuern jeweils vier Stangen mit aufmontierten Spielerfiguren. Durch Rotation der Stangen werden die Figuren bewegt, um einen Ball ins gegnerische Tor zu befördern. Die laterale Bewegung der Stangen ermöglicht die Positionierung der Figuren entlang der Querachse des Spielfelds.

### 10.2 Funktionale Anforderungen

Die funktionalen Anforderungen gliedern sich in drei Hauptbereiche:

**Spielmechanik:**
- Realistische Ballphysik mit Kollisionserkennung und natürlichem Rollverhalten
- Acht Stangen (vier pro Team) mit korrekter Figurenanzahl je Stangentyp
- Intuitive Steuerung für Rotation und laterale Bewegung der Stangen
- Automatische Torerkennung und Punktezählung
- Spielablauf-Management mit Countdown, Pause und Spielende

**Multiplayer-Funktionalität:**
- Peer-to-Peer Verbindung ohne dedizierten Spielserver
- Lobby-System mit Raumcode für einfaches Beitreten
- Synchronisation des Spielzustands in Echtzeit
- Latenzanzeige und robuste Verbindungshandhabung

**KI-Gegner:**
- Trainierbarer KI-Agent basierend auf Reinforcement Learning
- Export des trainierten Modells im ONNX-Format für Inference in Godot
- Regelbasierter Bot als Alternative für verschiedene Schwierigkeitsstufen
- Self-Play Training für symmetrisches Lernverhalten

### 10.3 Nicht-funktionale Anforderungen

**Performance:**
- Stabile 60 FPS auf modernen Desktop-Systemen
- Physik-Simulation bei 50 Hz für konsistentes Spielgefühl
- Niedrige Latenz bei der Netzwerkkommunikation

**Plattformen:**
- Primäre Zielplattform: Windows
- Export-Möglichkeit für Linux und macOS

**Benutzerfreundlichkeit:**
- Intuitive Menüführung
- Unterstützung für Tastatur und Controller
- Visuelle Rückmeldung zur aktiven Stangenauswahl

### 10.4 Technologie-Stack

```
SPIELLOGIK & PHYSIK
|--- Godot Engine 4.5
|--- C# (.NET 8.0) für Gameplay-Code
+--- GDScript für RL-Integration und Networking

MULTIPLAYER
|--- WebRTC (WebRTCPeerConnection)
|--- PHP Backend für Signaling
+--- Coturn STUN/TURN Server

REINFORCEMENT LEARNING
|--- Godot RL Agents (GDExtension)
|--- Stable-Baselines3 (PPO)
|--- PyTorch für neuronale Netze
+--- ONNX Runtime für Inference
```

---

## 11 Projektstruktur und Setup

### 11.1 Godot-Projektstruktur

Das Projekt folgt einer modularen Ordnerstruktur, die eine klare Trennung zwischen Szenen, Scripts, Assets und Konfigurationsdateien ermöglicht:

```
DigiKicker/
|--- project.godot              # Godot Projektkonfiguration
|--- DickiKicker.csproj         # C# Projekt-Datei
|
|--- scenes/                    # Godot Scene-Dateien (.tscn)
|   |--- main/
|   |   +--- Main.tscn          # Einstiegspunkt der Anwendung
|   |--- game/
|   |   |--- Game.tscn          # Hauptspielszene
|   |   |--- Table.tscn         # Tischgeometrie mit Wänden und Toren
|   |   |--- Rod.tscn           # Einzelne Spielstange
|   |   |--- Ball.tscn          # Physik-Ball
|   |   +--- Figure.tscn        # Spielerfigur (3D-Modell)
|   |--- menu/
|   |   |--- MainMenu.tscn      # Hauptmenü
|   |   |--- ModeSelectionMenu.tscn # Spielmodus-Auswahl
|   |   |--- GameSetupMenu.tscn # Spielkonfiguration
|   |   |--- OnlineMenu.tscn    # Multiplayer-Lobby
|   |   |--- OptionsMenu.tscn   # Einstellungen
|   |   |--- ControllerMenu.tscn # Controller-Konfiguration
|   |   +--- StatsMenu.tscn     # Statistiken
|   |--- ui/
|   |   |--- Hud.tscn           # In-Game HUD
|   |   |--- PauseMenu.tscn     # Pausemenü
|   |   +--- GameOverScreen.tscn
|   +--- training/
|       +--- RLTraining.tscn    # RL-Trainingsszene
|
|--- scripts/                   # Quellcode
|   |--- autoload/              # Singleton-Manager
|   |   |--- GameManager.cs
|   |   |--- AudioManager.cs
|   |   |--- InputManager.cs
|   |   +--- StatsManager.cs
|   |--- game/                  # Spiellogik
|   |   |--- Ball.cs
|   |   |--- Rod.cs
|   |   |--- Game.cs
|   |   |--- Figure.cs
|   |   |--- Table.cs
|   |   |--- Goal.cs
|   |   |--- CameraController.cs
|   |   |--- BotController.cs
|   |   +--- RLBotController.cs
|   |--- ai/                    # RL-spezifischer Code
|   |   |--- FoosballAIController.gd
|   |   +--- RLTrainingManager.gd
|   |--- online/                # Multiplayer-Code
|   |   |--- OnlineMultiplayerManager.gd
|   |   |--- OnlineGameIntegration.gd
|   |   +--- NetworkGameSync.gd
|   |--- menu/                  # Menü-Controller
|   +--- ui/                    # UI-Controller
|
|--- assets/                    # Ressourcen
|   |--- models/                # 3D-Modelle
|   |--- textures/              # Texturen
|   |--- audio/                 # Sound-Dateien
|   +--- materials/             # Godot Materials
|
|--- models/                    # Trainierte KI-Modelle
|   +--- foosball_ai.onnx
|
+--- addons/                    # Godot-Plugins
    |--- godot_rl_agents/       
    +--- webrtc/ 
```

### 11.2 C#-Projekt-Setup

Das C#-Projekt verwendet .NET 8.0 und integriert die ONNX Runtime für die KI-Inferenz. Die Projektkonfiguration in der `.csproj`-Datei:

```xml
<Project Sdk="Godot.NET.Sdk/4.5.1">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <RootNamespace>DickiKicker</RootNamespace>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>disable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.19.2" />
  </ItemGroup>
</Project>
```

Die ONNX Runtime ermöglicht die Ausführung trainierter neuronaler Netze direkt in Godot ohne Python-Abhängigkeit. Das Paket wird automatisch beim Build heruntergeladen und in das Export-Verzeichnis kopiert.

### 11.3 Autoload-Konfiguration

Godot's Autoload-System wird genutzt, um globale Manager-Klassen als Singletons zu registrieren. Diese sind in der `project.godot` definiert:

```ini
[autoload]
GameManager="*res://scripts/autoload/GameManager.cs"
AudioManager="*res://scripts/autoload/AudioManager.cs"
InputManager="*res://scripts/autoload/InputManager.cs"
StatsManager="*res://scripts/autoload/StatsManager.cs"
```

Der GameManager verwaltet den zentralen Spielzustand und definiert wichtige Enumerationen:

```csharp
public partial class GameManager : Node
{
    public enum GameState { Menu, Countdown, Playing, Paused, GameOver }
    public enum Team { Red, Blue }
    public enum WinCondition { Score, Time }
    public enum BotType { Easy, Medium, Hard, TrainedAI }

    // Spielzustand
    public GameState CurrentState { get; private set; } = GameState.Menu;
    public int RedScore { get; private set; } = 0;
    public int BlueScore { get; private set; } = 0;

    // Signale für Event-Driven Architecture
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void GoalScoredEventHandler(int team);
    [Signal] public delegate void GameEndedEventHandler(int winningTeam);
    [Signal] public delegate void BallResetRequestedEventHandler();
}
```

### 11.4 Entwicklungsumgebung

Die Entwicklung erfolgte mit folgenden Werkzeugen:

- **IDE:** Visual Studio Code mit C#-Extension und Godot-Tools
- **Godot Version:** 4.5 (.NET-Version für C#-Unterstützung)
- **Python:** 3.11 für RL-Training
- **PHP:** Backend für WebRTC-Signaling-Server
- **Coturn:** STUN/TURN-Server für NAT-Traversal

Für das RL-Training wurde eine separate Python-Umgebung eingerichtet. Die wichtigsten Abhängigkeiten (Auszug):

```
stable_baselines3==2.4.0
godot_rl==0.8.2
gymnasium==1.0.0
torch==2.11.0
onnx==1.20.0
onnxruntime==1.23.2
```

---

## 12 Spielmechanik-Implementierung

### 12.1 3D-Szenenaufbau

Der Spieltisch wird prozedural in der `Table.cs`-Klasse aufgebaut. Alle Dimensionen werden mit einem globalen Skalierungsfaktor (`SCALE = 5.0`) multipliziert.

Anfangs war das Spielfeld sehr klein skaliert, was zu fehlerhafter Schattendarstellung führte - Godots Shadow-Mapping produziert bei sehr kleinen Objekten Artefakte. Die Einführung des Skalierungsfaktors löste dieses Problem und ermöglicht zudem einfache Größenanpassungen des gesamten Spielfelds durch Änderung einer einzigen Konstante:

```csharp
public partial class Table : Node3D
{
    private const float SCALE = 5.0f;

    // Tisch-Dimensionen
    private const float TABLE_LENGTH = 2.4f * SCALE;  // 12 Einheiten
    private const float TABLE_WIDTH = 1.2f * SCALE;   // 6 Einheiten
    private const float TABLE_HEIGHT = 0.1f * SCALE;  // 0.5 Einheiten

    // Tor-Dimensionen
    private const float GOAL_WIDTH = (TABLE_WIDTH - WALL_THICKNESS * 2) * 0.35f;
    private const float GOAL_HEIGHT = 0.25f * SCALE;
    private const float GOAL_DEPTH = 0.15f * SCALE;
}
```

Die Stangenpositionen folgen dem Standard-Tischfußball-Layout mit acht Stangen:

```csharp
// Stangenpositionen entlang der X-Achse (von Red-Tor zu Blue-Tor)
private const float ROD_POS_1 = -1.00f * SCALE;  // Red Goalkeeper
private const float ROD_POS_2 = -0.70f * SCALE;  // Red Defense
private const float ROD_POS_3 = -0.40f * SCALE;  // Blue Attack
private const float ROD_POS_4 = -0.15f * SCALE;  // Red Midfield
private const float ROD_POS_5 = 0.15f * SCALE;   // Blue Midfield
private const float ROD_POS_6 = 0.40f * SCALE;   // Red Attack
private const float ROD_POS_7 = 0.70f * SCALE;   // Blue Defense
private const float ROD_POS_8 = 1.00f * SCALE;   // Blue Goalkeeper

// Figurenanzahl pro Stangentyp
private const int GOALKEEPER_FIGURES = 1;
private const int DEFENSE_FIGURES = 2;
private const int MIDFIELD_FIGURES = 5;
private const int ATTACK_FIGURES = 3;
```

### 12.2 Physik-System

#### Ball-Physik

Der Ball ist als `RigidBody3D` implementiert und nutzt Godots integrierte Physik-Engine. Die Konfiguration optimiert das Spielgefühl für reaktive, aber kontrollierbare Bewegungen:

```csharp
public partial class Ball : RigidBody3D
{
    private const float BALL_RADIUS = 0.035f * SCALE;  // 0.175 Einheiten
    private const float BALL_MASS = 0.1f;
    private const float BALL_BOUNCE = 0.7f;
    private const float BALL_FRICTION = 0.08f;
    private const float LINEAR_DAMP = 0.5f;
    private const float ANGULAR_DAMP = 0.6f;
    private const float MAX_VELOCITY = 15.0f;

    private void SetupBallPhysics()
    {
        Mass = BALL_MASS;

        // Physics Material für Abprallverhalten
        var physicsMaterial = new PhysicsMaterial();
        physicsMaterial.Bounce = BALL_BOUNCE;
        physicsMaterial.Friction = BALL_FRICTION;
        PhysicsMaterialOverride = physicsMaterial;

        // Continuous Collision Detection gegen Tunneling
        ContinuousCd = true;

        // Keine Gravitation - Ball bleibt auf Tischoberfläche
        GravityScale = 0.0f;

        // Y-Achse sperren (kein Springen)
        AxisLockLinearY = true;

        // Kollisions-Layer: Ball auf Layer 2
        CollisionLayer = 2;
        CollisionMask = 1 | 4;  // Kollidiert mit Tisch (1) und Figuren (4)
    }
}
```

Die `_PhysicsProcess`-Methode implementiert wichtige Sicherheitsmechanismen:

```csharp
public override void _PhysicsProcess(double delta)
{
    // Geschwindigkeitsbegrenzung
    float speed = LinearVelocity.Length();
    if (speed > MAX_VELOCITY)
    {
        LinearVelocity = LinearVelocity.Normalized() * MAX_VELOCITY;
    }

    // Mikro-Bewegungen stoppen
    if (speed > 0 && speed < MIN_VELOCITY_THRESHOLD)
    {
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
    }

    // Y-Position auf Tischhöhe halten
    var pos = GlobalPosition;
    if (Mathf.Abs(pos.Y - RESET_Y) > 0.1f)
    {
        pos.Y = RESET_Y;
        GlobalPosition = pos;
        var vel = LinearVelocity;
        vel.Y = 0;
        LinearVelocity = vel;
    }

    // Erkennung festgefahrener Bälle
    if (_stuckTimer >= STUCK_NUDGE_TIME)
    {
        ApplyStuckNudge();  // Leichter Anstoß in zufällige Richtung
        _stuckTimer = 0f;
    }
}
```

#### Schuss-Mechanik

Wenn eine Figur den Ball trifft, wird ein Impuls basierend auf der Rotationsgeschwindigkeit der Stange angewendet:

```csharp
public void ApplyKick(Vector3 direction, float strength)
{
    // Schuss-Immunität nach Reset prüfen
    if (_kickImmunityTimer > 0) return;

    direction.Y = 0;
    direction = direction.Normalized();

    // Stehende Bälle bekommen Boost
    float currentSpeed = LinearVelocity.Length();
    float effectiveStrength = strength;
    if (currentSpeed < 0.5f)
        effectiveStrength = strength * 1.8f;
    else if (currentSpeed < 2.0f)
        effectiveStrength = strength * 1.3f;

    // Impuls anwenden
    ApplyCentralImpulse(direction * effectiveStrength);

    // Spin für realistisches Rollen
    var spinAxis = direction.Cross(Vector3.Up);
    ApplyTorqueImpulse(spinAxis * effectiveStrength * 0.3f);
}
```

### 12.3 Stangen-Steuerung

Die `Rod`-Klasse verwaltet Rotation und laterale Bewegung einer Stange:

```csharp
public partial class Rod : Node3D
{
    public enum RodType { Goalkeeper, Defense, Midfield, Attack }

    [Export] public GameManager.Team Team { get; set; }
    [Export] public RodType Type { get; set; }
    [Export] public int FigureCount { get; set; }

    private const float ROTATION_SPEED = 10.0f;   // rad/s
    private const float LATERAL_SPEED = 3.5f;     // Einheiten/s

    private float _currentOffset = 0.0f;
    private float _maxLateralOffset;  // Dynamisch berechnet

    private void HandleRotation(float input, float delta)
    {
        // Rotationsgeschwindigkeit tracken für Schussstärke
        _currentRotationVelocity = input * ROTATION_SPEED;

        // Rotation um lokale Z-Achse (Längsachse der Stange)
        float rotationAmount = input * ROTATION_SPEED * delta;
        RotateObjectLocal(Vector3.Back, rotationAmount);

        // Rotation auf +-180° begrenzen
        var currentRotation = Rotation;
        currentRotation.Z = Mathf.Clamp(currentRotation.Z, -Mathf.Pi, Mathf.Pi);
        Rotation = currentRotation;
    }

    private void HandleLateralMovement(float input, float delta)
    {
        _currentOffset += input * LATERAL_SPEED * delta;

        // Begrenzung basierend auf Figurenposition
        if (Type != RodType.Goalkeeper)
        {
            _currentOffset = Mathf.Clamp(_currentOffset,
                -_maxLateralOffset, _maxLateralOffset);
        }

        var pos = Position;
        pos.Z = _currentOffset;
        Position = pos;
    }
}
```

Die maximale laterale Bewegung wird dynamisch beim Spawn der Figuren berechnet:

```csharp
private void SpawnFigures()
{
    // Dynamischen Abstand basierend auf Figurenanzahl
    float usableWidth = FigureCount switch
    {
        1 => 0.0f,
        2 => TABLE_WIDTH * 0.35f,
        3 => TABLE_WIDTH * 0.55f,
        5 => TABLE_WIDTH * 0.6f,
        _ => USABLE_WIDTH
    };

    // Figuren gleichmäßig verteilen
    float spacing = (FigureCount == 1) ? 0 : usableWidth / (FigureCount - 1);
    float startZ = -usableWidth / 2.0f;

    for (int i = 0; i < FigureCount; i++)
    {
        var figure = _figureScene.Instantiate<Figure>();
        figure.Team = Team;

        float zPos = (FigureCount == 1) ? 0 : startZ + (i * spacing);
        figure.Position = new Vector3(0, 0.04f * SCALE, zPos);

        _figureSlots.AddChild(figure);
    }

    // Max-Offset berechnen
    float outermostFigureZ = (FigureCount == 1) ? 0 : usableWidth / 2.0f;
    _maxLateralOffset = (TABLE_WIDTH / 2.0f) - outermostFigureZ - WALL_MARGIN;
}
```

### 12.4 Input-System

Der `InputManager` abstrahiert verschiedene Eingabegeräte und ermöglicht sowohl Tastatur- als auch Controller-Steuerung:

```csharp
public enum InputDevice
{
    Keyboard,
    Keyboard2,
    Controller0,
    Controller1,
    Controller2,
    Controller3
}

// Tastatur: Eine Stange zur Zeit auswählen
private int _selectedRod = 0;

// Controller: Zwei Stangen gleichzeitig (Bumper-Paare)
// L1 = GK/Def, R1 = Mid/Atk
// L2/R2 wechselt innerhalb des Paares
private (int left, int right) _controllerSelectedRods = (0, 2);

public Vector2 GetRodInput(int player, int rodIndex)
{
    if (!IsRodActiveForPlayer(player, rodIndex))
        return Vector2.Zero;

    if (IsUsingController(player))
    {
        // Controller: Analog-Sticks für Bewegung
        return new Vector2(
            Input.GetAxis($"p{player}_lateral_neg", $"p{player}_lateral_pos"),
            Input.GetAxis($"p{player}_rotate_neg", $"p{player}_rotate_pos")
        );
    }
    else
    {
        // Tastatur: Digitale Eingabe
        return new Vector2(
            Input.GetAxis($"p{player}_key_left", $"p{player}_key_right"),
            Input.GetAxis($"p{player}_key_rotate_ccw", $"p{player}_key_rotate_cw")
        );
    }
}
```

### 12.5 Kamera-System

Die Kamera folgt dem Ball mit sanfter Interpolation und bietet eine isometrische Perspektive:

```csharp
public partial class CameraController : Node3D
{
    private const float CAMERA_HEIGHT = 7.5f;
    private const float CAMERA_FOV = 60.0f;
    private const float SMOOTHNESS = 0.08f;

    private Camera3D _camera;
    private Node3D _ball;

    public override void _PhysicsProcess(double delta)
    {
        if (_ball == null) return;

        // Zielposition berechnen
        Vector3 ballPos = _ball.GlobalPosition;
        Vector3 targetPos = new Vector3(
            Mathf.Clamp(ballPos.X, -3.0f, 3.0f),
            CAMERA_HEIGHT,
            ballPos.Z * 0.3f
        );

        // Sanfte Interpolation
        GlobalPosition = GlobalPosition.Lerp(targetPos, SMOOTHNESS);

        // Immer auf Spielfeldmitte schauen
        LookAt(new Vector3(0, 0, 0), Vector3.Up);
    }
}
```

### 12.6 Tor-Erkennung

Tore werden über `Area3D`-Trigger-Zonen erkannt, die sich im Inneren der Torräume befinden:

```csharp
public partial class Goal : Area3D
{
    [Export] public GameManager.Team ScoringTeam { get; set; }

    private GameManager _gameManager;
    private bool _goalScored = false;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (_goalScored || body is not Ball) return;

        // Tor nur im Spielzustand werten
        if (_gameManager.CurrentState != GameManager.GameState.Playing)
            return;

        _goalScored = true;

        // Tor-Signal an GameManager
        _gameManager.OnGoalScored(ScoringTeam);

        // Reset nach kurzer Verzögerung
        GetTree().CreateTimer(1.5).Timeout += () => _goalScored = false;
    }
}
```

### 12.7 Lebenszyklus eines Spiels

Der `GameManager` verwaltet den Spielzustand über ein Enum mit vier möglichen Zuständen: `Menu`, `Playing`, `Paused` und `GameOver`. Der Countdown nach Spielstart oder nach einem Tor wird separat über Flags gesteuert.

```
                        Lebenszyklus eines Spiels

    +------+
    | Menu |
    +--|---+
       |
       | StartGame()
       v
   Countdown
       |
       |
       |      +-----------------------------------------+
       |      |                                         |
       v      v                                         |
  +-------------+    Pause()     +--------+             |
  |             | -------------> |        |             |
  |   Playing   |                | Paused |             |
  |             | <------------- |        |             |
  +--|-------|--+    Resume()    +--------+             |
     |       |                                          |
     |       | Tor erzielt                              |
     |       +---------------> Countdown ---------------+
     |
     | Win Condition erfüllt
     v
+----------+
| GameOver |
+----------+
```

---

## 13 Multiplayer-Implementierung

### 13.1 Netzwerk-Architektur

DigiKicker verwendet eine Peer-to-Peer-Architektur basierend auf WebRTC. Diese Entscheidung eliminiert die Notwendigkeit eines dedizierten Spielservers und reduziert die Latenz, da Spielzustandsdaten direkt zwischen den Spielern ausgetauscht werden.

```
                           WebRTC P2P Architektur


     Host                                                      Joiner
   (Player 1)                                                (Player 2)
       |                                                          |
       |                                                          |
       |                     +--------------+                     |
       |  1. Create Match    |  Signaling   |                     |
       | ------------------->|   Server     |                     |
       |                     |    (PHP)     |                     |
       |  2. Room Code       |              |                     |
       | <-------------------|              |                     |
       |                     |              |  3. Join            |
       |                     |              |<--------------------|
       |                     +-------|-------+                     |
       |                            |                             |
       |  4. SDP Offer              |                             |
       | -------------------------->|---------------------------->|
       |                            |              SDP Answer     |
       | <--------------------------|<----------------------------|
       |                            |                             |
       |  5. ICE Candidates         |                             |
       | <------------------------->|<--------------------------->|
       |                            |                             |
       |                            |                             |
       |                   +--------┴--------+                    |
       |                   |  STUN/TURN      |                    |
       |                   |    Server       |                    |
       |                   |   (Coturn)      |                    |
       |                   +---------|--------+                    |
       |                            |                             |
       |  6a. STUN: Public IP       |                             |
       | <------------------------->|<--------------------------->|
       |                            |                             |
       |  6b. Direct P2P (wenn möglich)                           |
       |<════════════════════════════════════════════════════════>|
       |                   (WebRTC DataChannel)                   |
       |                            |                             |
       |  6c. TURN Relay (Fallback bei NAT)                       |
       | <------------------------->|<--------------------------->|
       |                            |                             |
```

Das Diagramm zeigt den vollständigen Verbindungsaufbau: Nach dem Signaling über den PHP-Server wird versucht, eine direkte P2P-Verbindung herzustellen. Der STUN-Server hilft dabei, die öffentliche IP zu ermitteln. Sollte aufgrund von NAT-Restriktionen keine direkte Verbindung möglich sein, wird der TURN-Server als Relay verwendet.

### 13.2 Signaling-Server

Der Signaling-Prozess wird über ein PHP-Backend abgewickelt, das auf einem eigenen Server gehostet ist. Die Implementierung besteht aus mehreren Endpunkten:

**create_match.php:** Erstellt einen neuen Spielraum und generiert einen eindeutigen Raumcode.

**join_match.php:** Ermöglicht einem Spieler, einem existierenden Raum beizutreten.

**signal.php:** Übermittelt WebRTC-Signaling-Daten (SDP und ICE Candidates).

**get_signals.php:** Polling-Endpunkt zum Abrufen neuer Signaling-Nachrichten.

### 13.3 STUN/TURN-Server mit Coturn

Für NAT-Traversal wird ein eigener Coturn-Server betrieben, der sowohl STUN- als auch TURN-Dienste bereitstellt:

- **STUN (Session Traversal Utilities for NAT):** Ermöglicht die Ermittlung der öffentlichen IP-Adresse und des Ports eines Clients hinter NAT. Bei symmetrischen NATs reicht STUN alleine nicht aus.

- **TURN (Traversal Using Relays around NAT):** Fungiert als Relay-Server, der den gesamten Datenverkehr weiterleitet, wenn eine direkte P2P-Verbindung nicht möglich ist. Dies erhöht die Latenz, garantiert aber Konnektivität.

Eine typische Coturn-Konfiguration umfasst folgende Parameter:

```
Server:     <Server-IP>
STUN Port:  3478 (UDP)
TURN Port:  3478 (UDP/TCP)
Realm:      <Realm-Name>
```

Die Authentifizierung erfolgt über Long-Term Credentials mit Benutzername und Passwort.

### 13.4 WebRTC-Implementierung in Godot

Der `OnlineMultiplayerManager` koordiniert den gesamten Verbindungsaufbau:

```gdscript
extends Node
class_name OnlineMultiplayerManager

# Verbindungszustände
enum ConnectionState {
    DISCONNECTED,
    CREATING_MATCH,
    WAITING_FOR_JOINER,
    JOINING_MATCH,
    SIGNALING,
    CONNECTING,
    CONNECTED,
    ERROR
}

# STUN/TURN Server Konfiguration
const STUN_SERVERS := ["stun:example_ip:3478"]
const TURN_SERVERS := [{
    "urls": "turn:example_ip:3478?transport=udp",
    "username": "example_user",
    "credential": "example_password",
    "credentialType": "password"
}]

var peer_connection: WebRTCPeerConnection = null
var data_channel: WebRTCDataChannel = null

func _setup_peer_connection() -> void:
    peer_connection = WebRTCPeerConnection.new()

    # ICE Server konfigurieren
    var ice_config = {
        "iceServers": []
    }

    for stun in STUN_SERVERS:
        ice_config["iceServers"].append({"urls": [stun]})

    for turn in TURN_SERVERS:
        ice_config["iceServers"].append(turn)

    peer_connection.initialize(ice_config)

    # Callbacks verbinden
    peer_connection.session_description_created.connect(
        _on_session_description_created)
    peer_connection.ice_candidate_created.connect(
        _on_ice_candidate_created)
```

Der DataChannel wird für die bidirektionale Kommunikation konfiguriert:

```gdscript
func _create_data_channel() -> void:
    data_channel = peer_connection.create_data_channel("game_data", {
        "negotiated": true,
        "id": 1,
        "ordered": false,      # Ungeordnet für niedrigere Latenz
        "maxRetransmits": 0    # Keine Wiederholungen
    })

    data_channel.message_received.connect(_on_message_received)

func send_game_state(state: Dictionary) -> void:
    if data_channel == null or \
       data_channel.get_ready_state() != WebRTCDataChannel.STATE_OPEN:
        return

    var json = JSON.stringify(state)
    data_channel.put_packet(json.to_utf8_buffer())
```

### 13.5 Spielzustand-Synchronisation

Die Synchronisation erfolgt über regelmäßige Zustandsupdates, die Position und Rotation aller Spielobjekte enthalten:

```gdscript
# NetworkGameSync.gd
extends Node

const SYNC_RATE := 1.0 / 30.0  # 30 Hz Synchronisation

func _physics_process(delta: float) -> void:
    _sync_timer += delta
    if _sync_timer >= SYNC_RATE:
        _sync_timer = 0.0
        _send_game_state()

func _send_game_state() -> void:
    var state = {
        "type": "game_state",
        "timestamp": Time.get_ticks_msec(),
        "ball": {
            "pos": [ball.global_position.x, ball.global_position.z],
            "vel": [ball.linear_velocity.x, ball.linear_velocity.z]
        },
        "rods": []
    }

    # Nur eigene Stangen senden
    for rod in own_rods:
        state["rods"].append({
            "type": rod.Type,
            "lateral": rod.CurrentLateralOffset,
            "rotation": rod.Rotation.z
        })

    online_manager.send_game_state(state)

func _on_game_state_received(state: Dictionary) -> void:
    # Gegnerische Stangen aktualisieren
    for rod_data in state.get("rods", []):
        var rod_type = rod_data["type"]
        var rod = opponent_rods[rod_type]
        rod.ApplyNetworkState(rod_data["lateral"], rod_data["rotation"])
```

### 13.6 Latenz-Kompensation

Für netzwerk-gesteuerte Stangen wird Interpolation verwendet, um ruckfreie Bewegungen zu gewährleisten:

```csharp
// Rod.cs - Netzwerk-Interpolation
private void HandleNetworkInterpolation(float delta)
{
    // Laterale Position interpolieren
    float lateralDiff = _networkTargetLateral - _currentOffset;
    float lateralStep = NETWORK_LATERAL_SPEED * delta;

    if (Mathf.Abs(lateralDiff) <= lateralStep)
        _currentOffset = _networkTargetLateral;
    else
        _currentOffset += Mathf.Sign(lateralDiff) * lateralStep;

    // Rotation interpolieren mit Winkel-Wrapping
    var currentRot = Rotation;
    float rotDiff = _networkTargetRotation - currentRot.Z;

    // Differenz auf -PI bis PI normalisieren
    while (rotDiff > Mathf.Pi) rotDiff -= Mathf.Tau;
    while (rotDiff < -Mathf.Pi) rotDiff += Mathf.Tau;

    float rotStep = NETWORK_ROTATION_SPEED * delta;

    if (Mathf.Abs(rotDiff) <= rotStep)
        currentRot.Z = _networkTargetRotation;
    else
        currentRot.Z += Mathf.Sign(rotDiff) * rotStep;

    Rotation = currentRot;
}
```

### 13.7 Ping-Messung

Die Verbindungsqualität wird kontinuierlich durch Ping-Messungen überwacht:

```gdscript
func _send_ping() -> void:
    ping_id += 1
    ping_send_time = Time.get_ticks_msec()

    var ping_msg = {
        "type": "ping",
        "id": ping_id
    }
    _send_data_channel_message(ping_msg)

func _handle_pong(msg: Dictionary) -> void:
    if msg.get("id", -1) != ping_id:
        return

    var rtt = Time.get_ticks_msec() - ping_send_time

    # Gleitender Durchschnitt über letzte 5 Messungen
    ping_history.append(rtt)
    if ping_history.size() > 5:
        ping_history.pop_front()

    current_ping_ms = int(ping_history.reduce(
        func(acc, val): return acc + val, 0) / ping_history.size())

    emit_signal("ping_updated", current_ping_ms, connection_type)
```

---

## 14 KI-Gegner mit Reinforcement Learning

### 14.1 Training-Environment Setup

Das Training nutzt das Godot RL Agents Framework, das eine Brücke zwischen Godot und Python-basierten RL-Bibliotheken schlägt. Die Trainingsszene `RLTraining.tscn` enthält zwei AI-Controller für Self-Play:

```
+------------------------------------------------------------+
|                   RL Training Setup                        |
|                                                            |
|   +-----------------------------------------------------+  |
|   |              RLTraining.tscn                        |  |
|   |                                                     |  |
|   |   +-------------+           +-------------+         |  |
|   |   | AIController|           | AIController|         |  |
|   |   |   (Red)     |           |   (Blue)    |         |  |
|   |   | mirror: -1  |           | mirror: +1  |         |  |
|   |   +-------|------+           +-------|------+         |  |
|   |          |                         |                |  |
|   |          |    +---------------+    |                |  |
|   |          +---->|    sync.gd    |<---+                |  |
|   |               |   (Godot RL)  |                     |  |
|   |               +--------|-------+                     |  |
|   |                       |                             |  |
|   +------------------------|-----------------------------+  |
|                           | TCP/Shared Memory              |
|                           v                                |
|   +-----------------------------------------------------+  |
|   |              Python Training                        |  |
|   |                                                     |  |
|   |   StableBaselinesGodotEnv --> PPO --> ONNX Export   |  |
|   |                                                     |  |
|   +------------------------------------------------------+  |
|                                                            |
+-------------------------------------------------------------+
```

### 14.2 Observation Space

Der Observation Space definiert, welche Informationen der Agent über den Spielzustand erhält. Er umfasst 20 kontinuierliche Werte:

```gdscript
## FoosballAIController.gd

func get_obs_space() -> Dictionary:
    # Observations:
    # - Ball: Position (x, z), Velocity (vx, vz) = 4
    # - Eigene Rods (4 Stück): lateral_offset, rotation = 8
    # - Gegner Rods (4 Stück): lateral_offset, rotation = 8
    # Gesamt: 20 Floats
    return {
        "obs": {
            "size": [20],
            "space": "box",
            "low": -10.0,
            "high": 10.0
        }
    }

func get_obs() -> Dictionary:
    var obs: Array[float] = []

    # Ball Observation (normalisiert)
    if ball != null and is_instance_valid(ball):
        var ball_pos = ball.global_position
        var ball_vel = ball.linear_velocity

        # X-Position und X-Geschwindigkeit gespiegelt für Red Team
        obs.append(mirror * ball_pos.x / 6.0)
        obs.append(ball_pos.z / 3.0)
        obs.append(mirror * clampf(ball_vel.x / 10.0, -1.0, 1.0))
        obs.append(clampf(ball_vel.z / 10.0, -1.0, 1.0))
    else:
        obs.append_array([0.0, 0.0, 0.0, 0.0])

    # Eigene Rod Observations
    for rod in controlled_rods:
        if rod != null:
            obs.append(rod.CurrentLateralOffset / 3.0)
            obs.append(rod.rotation.z / PI)
        else:
            obs.append_array([0.0, 0.0])

    # Gegnerische Rod Observations
    for rod in opponent_rods:
        if rod != null:
            obs.append(rod.CurrentLateralOffset / 3.0)
            obs.append(rod.rotation.z / PI)
        else:
            obs.append_array([0.0, 0.0])

    return {"obs": obs}
```

Die Spiegelung der X-Koordinaten für das rote Team ist entscheidend für Self-Play-Training: Beide Agenten sehen das Spiel aus ihrer eigenen Perspektive, wodurch ein einziges trainiertes Modell für beide Seiten verwendet werden kann.

### 14.3 Action Space

Der Action Space besteht aus 8 kontinuierlichen Werten im Bereich [-1, 1]:

```gdscript
func get_action_space() -> Dictionary:
    # 4 Rods × 2 Actions (lateral, rotation) = 8 kontinuierliche Actions
    return {
        "rod_actions": {
            "size": 8,
            "action_type": "continuous"
        }
    }

func set_action(action: Dictionary) -> void:
    var actions = action.get("rod_actions", [])

    for i in range(mini(4, int(actions.size() / 2))):
        var lateral = clampf(actions[i * 2], -1.0, 1.0)
        var rotation = clampf(actions[i * 2 + 1], -1.0, 1.0)
        rod_actions[i] = Vector2(lateral, rotation)
```

### 14.4 Reward Function

Die Reward-Funktion ist so gestaltet, dass sie sowohl das Erzielen von Toren belohnt als auch dynamisches Spielverhalten fördert:

```gdscript
# Belohnungen und Strafen
const GOAL_REWARD := 3.0        # Tor erzielt
const CONCEDE_PENALTY := -2.0   # Gegentor kassiert
const TOUCH_REWARD := 0.01      # Ball berührt
const IDLE_PENALTY := -0.005    # Ball zu lange unbewegt

func on_goal_scored(scoring_team: int) -> void:
    # scoring_team: 1 = Red scored, 2 = Blue scored
    var our_team_scored = \
        (scoring_team == 1 and controlled_team == 0) or \
        (scoring_team == 2 and controlled_team == 1)

    if our_team_scored:
        reward += GOAL_REWARD
    else:
        reward += CONCEDE_PENALTY

func _physics_process(delta: float) -> void:
    # Idle Ball Penalty
    var ball_movement = ball.global_position.distance_to(_last_ball_position)

    if ball_movement < BALL_IDLE_THRESHOLD:
        _ball_idle_timer += delta

        if _ball_idle_timer >= BALL_IDLE_TIME:
            var responsible_team = _get_team_responsible_for_ball(
                ball.global_position)

            if responsible_team == controlled_team:
                reward += IDLE_PENALTY

            _ball_idle_timer = 0.0
    else:
        _ball_idle_timer = 0.0

    _last_ball_position = ball.global_position
```

### 14.5 Training-Script

Das Python-Trainingsskript verwendet Stable-Baselines3 mit dem PPO-Algorithmus:

```python
# train_foosball.py

from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import CheckpointCallback
from stable_baselines3.common.vec_env.vec_monitor import VecMonitor
from godot_rl.wrappers.stable_baselines_wrapper import StableBaselinesGodotEnv

def main():
    # Godot-Umgebung erstellen
    env = StableBaselinesGodotEnv(
        env_path=args.env_path,  # Pfad zur exportierten Executable
        show_window=args.viz,
        seed=args.seed,
        n_parallel=args.n_parallel,
        speedup=args.speedup
    )
    env = VecMonitor(env)

    # Policy-Parameter für stabiles Training
    policy_kwargs = dict(
        log_std_init=-1.0,  # Niedrigere initiale Standardabweichung
        net_arch=dict(
            pi=[256, 256],  # Policy-Netzwerk
            vf=[256, 256]   # Value-Netzwerk (getrennt)
        ),
    )

    model = PPO(
        "MultiInputPolicy",
        env,
        learning_rate=linear_schedule(1e-4),  # Lineare Absenkung
        n_steps=512,
        batch_size=128,
        n_epochs=5,
        gamma=0.99,
        gae_lambda=0.95,
        clip_range=0.2,
        ent_coef=0.01,
        vf_coef=0.5,
        max_grad_norm=0.5,
        use_sde=True,           # State-Dependent Exploration
        sde_sample_freq=4,
        target_kl=0.03,         # Frühzeitiger Stopp bei hoher KL
        verbose=1,
        tensorboard_log=args.experiment_dir,
        policy_kwargs=policy_kwargs,
    )

    # Callbacks
    checkpoint_callback = CheckpointCallback(
        save_freq=50000,
        save_path=checkpoint_dir,
        name_prefix="ppo_foosball",
    )

    # Training starten
    model.learn(
        total_timesteps=args.timesteps,
        callback=checkpoint_callback,
        progress_bar=True,
    )

    # ONNX Export
    export_model_as_onnx(model, "foosball_ai.onnx")
```

### 14.6 Hyperparameter und Konfiguration

Die PPO-Hyperparameter beeinflussen das Trainingsverhalten maßgeblich. Die wichtigsten Parameter und ihre Funktion:

**Learning Rate:** Bestimmt die Schrittgröße bei Gewichtsaktualisierungen. Eine niedrige Learning Rate mit linearem Decay verhindert Instabilitäten im späteren Training.

**n_steps:** Die Anzahl der Schritte pro Rollout bevor ein Update erfolgt. Längere Rollouts ermöglichen dem Agenten, längerfristige Zusammenhänge zu erkennen.

**batch_size:** Die Größe der Mini-Batches für das Training. Kleinere Batches erhöhen die Varianz der Gradienten, größere stabilisieren das Training.

**gamma (Discount Factor):** Gewichtet zukünftige Rewards. Ein hoher Wert nahe 1.0 fördert langfristige Planung, wichtig für strategisches Spielverhalten.

**gae_lambda:** Parameter für Generalized Advantage Estimation. Balanciert Bias und Varianz bei der Advantage-Schätzung.

**clip_range:** Begrenzt die maximale Policy-Änderung pro Update. Verhindert zu große Sprünge im Parameterraum.

**ent_coef (Entropy Coefficient):** Fördert Exploration durch Belohnung von Aktionsdiversität. Verhindert vorzeitige Konvergenz zu suboptimalen Strategien.

**use_sde (State-Dependent Exploration):** Aktiviert zustandsabhängige Exploration, bei der die Explorationsstärke vom aktuellen Zustand abhängt. Wurde als kritisch für stabiles Training identifiziert.

**target_kl:** Maximale erlaubte KL-Divergenz zwischen alter und neuer Policy. Stoppt Updates frühzeitig bei zu großen Änderungen.

### 14.7 Training-Monitoring

Das Training wird mit TensorBoard überwacht. Ein eigener Callback protokolliert zusätzliche Stabilitätsmetriken:

```python
class StabilityMonitorCallback(BaseCallback):
    def __init__(self, check_freq: int = 1000):
        super().__init__()
        self.check_freq = check_freq

    def _on_step(self) -> bool:
        if self.n_calls % self.check_freq == 0:
            if hasattr(self.model.policy, 'log_std'):
                log_std = self.model.policy.log_std.detach().cpu().numpy()
                std = np.exp(log_std)

                self.logger.record("train/policy_std_mean", np.mean(std))
                self.logger.record("train/policy_std_max", np.max(std))

                if np.mean(std) > 5.0:
                    print("WARNUNG: Policy-Std ist hoch!")

        return True
```

### 14.8 ONNX-Export und Integration

Nach erfolgreichem Training wird das Modell im ONNX-Format exportiert:

```python
from godot_rl.wrappers.onnx.stable_baselines_export import export_model_as_onnx

export_model_as_onnx(model, "foosball_ai.onnx")
```

Die Integration in Godot erfolgt über den `RLBotController`:

```csharp
public partial class RLBotController : Node
{
    [Export] public string ModelPath = "res://models/foosball_ai.onnx";

    private InferenceSession _session;
    private float[] _observations = new float[20];

    private void LoadModel()
    {
        string absolutePath = ProjectSettings.GlobalizePath(ModelPath);

        var sessionOptions = new SessionOptions();
        _session = new InferenceSession(absolutePath, sessionOptions);

        GD.Print($"ONNX model loaded: {_session.InputMetadata.Keys}");
    }

    private float[] RunInference()
    {
        float[] actions = new float[8];

        // Input Tensor erstellen
        var inputTensor = new DenseTensor<float>(
            _observations, new[] { 1, 20 });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("obs", inputTensor)
        };

        // Inferenz ausführen
        using var results = _session.Run(inputs);

        var output = results.First();
        var outputTensor = output.AsTensor<float>();

        for (int i = 0; i < 8; i++)
        {
            actions[i] = Mathf.Clamp(
                outputTensor.GetValue(i), -1.0f, 1.0f);
        }

        return actions;
    }
}
```

Für flüssiges Gameplay werden zwei Optimierungen angewendet:

**Action Repeat:** Inferenz wird nur alle 4 Frames ausgeführt (~12.5 Hz statt 50 Hz), was Rechenzeit spart.

**Action Smoothing:** Die Aktionen werden interpoliert, um ruckartiges Verhalten zu vermeiden:

```csharp
private const float ACTION_SMOOTHING = 0.15f;
private const int ACTION_REPEAT = 4;

public override void _PhysicsProcess(double delta)
{
    _frameCounter++;
    if (_frameCounter >= ACTION_REPEAT)
    {
        _frameCounter = 0;
        BuildObservations();
        _targetActions = RunInference();
    }

    // Sanfte Interpolation in jedem Frame
    for (int i = 0; i < 8; i++)
    {
        _currentActions[i] = Mathf.Lerp(
            _currentActions[i],
            _targetActions[i],
            ACTION_SMOOTHING);
    }

    ApplyActions(_currentActions);
}
```

---

## 15 User Interface

### 15.1 Hauptmenü

Das Hauptmenü bietet Zugang zu allen Spielmodi und Einstellungen. Die verfügbaren Optionen sind:

- **Singleplayer:** Startet ein Spiel gegen einen KI-Gegner. Es kann zwischen verschiedenen Schwierigkeitsstufen und dem trainierten RL-Bot gewählt werden.

- **Multiplayer:** Öffnet die Multiplayer-Auswahl mit lokalem Multiplayer (zwei Spieler an einem Gerät) und Online-Multiplayer.

- **Options:** Ermöglicht die Anpassung von Audio-Einstellungen (Lautstärke für Musik und Soundeffekte) und Grafikoptionen.

- **Controller:** Öffnet ein dediziertes Menü zur Zuweisung von Eingabegeräten (Tastatur oder Controller).

- **Stats:** Zeigt Spielstatistiken wie gespielte Matches, gewonnene Spiele und erzielte Tore an.

- **Exit:** Beendet die Anwendung.

### 15.2 In-Game HUD

Das HUD zeigt während des Spiels relevante Informationen an:

```csharp
public partial class HUD : Control
{
    private Label _redScoreLabel;
    private Label _blueScoreLabel;
    private Label _timerLabel;
    private Label _pingLabel;

    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager");

        _gameManager.GoalScored += OnGoalScored;
        _gameManager.TimeUpdated += OnTimeUpdated;
    }

    private void OnGoalScored(int team)
    {
        _redScoreLabel.Text = _gameManager.RedScore.ToString();
        _blueScoreLabel.Text = _gameManager.BlueScore.ToString();

        // Tor-Animation
        PlayScoreAnimation(team);
    }

    private void OnTimeUpdated(float remainingTime)
    {
        int minutes = (int)(remainingTime / 60);
        int seconds = (int)(remainingTime % 60);
        _timerLabel.Text = $"{minutes:D2}:{seconds:D2}";
    }
}
```

### 15.3 Pause-Menü

Das Pausemenü wird durch Drücken von ESC aktiviert und pausiert das Spiel:

```csharp
public partial class PauseMenu : Control
{
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("pause"))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        bool isPaused = !GetTree().Paused;
        GetTree().Paused = isPaused;
        Visible = isPaused;

        if (isPaused)
            _gameManager.PauseGame();
        else
            _gameManager.ResumeGame();
    }
}
```

### 15.4 Online-Lobby

Die Online-Lobby ermöglicht das Erstellen und Beitreten von Matches:

```gdscript
# OnlineMenu.gd
extends Control

@onready var create_button := $CreateButton
@onready var join_button := $JoinButton
@onready var room_code_input := $RoomCodeInput
@onready var player_name_input := $PlayerNameInput
@onready var status_label := $StatusLabel
@onready var ping_label := $PingLabel

func _ready() -> void:
    var online_manager = get_node("/root/OnlineMultiplayerManager")

    online_manager.match_created.connect(_on_match_created)
    online_manager.opponent_joined.connect(_on_opponent_joined)
    online_manager.ping_updated.connect(_on_ping_updated)
    online_manager.error_occurred.connect(_on_error)

func _on_create_pressed() -> void:
    var name = player_name_input.text
    var team = team_selector.get_selected_team()

    OnlineMultiplayerManager.create_match(name, team)
    status_label.text = "Erstelle Match..."

func _on_match_created(room_code: String) -> void:
    status_label.text = "Match erstellt! Code: " + room_code
    room_code_display.text = room_code
    _show_waiting_screen()

func _on_opponent_joined(joiner_name: String) -> void:
    status_label.text = joiner_name + " ist beigetreten!"
    start_button.visible = true
```

---

## 16 Testing und Qualitätssicherung

### 16.1 Gameplay-Testing

Das Gameplay wurde in verschiedenen Szenarien getestet:

- Einzelspieler gegen regelbasierten Bot (alle Schwierigkeitsgrade)
- Einzelspieler gegen trainierte KI
- Lokaler Mehrspieler (zwei Spieler an einem PC)
- Online-Mehrspieler über verschiedene Netzwerkkonfigurationen

### 16.2 Netzwerk-Testing

Für das Netzwerk-Testing wurden folgende Szenarien geprüft:

- Verbindungsaufbau über STUN (direktes P2P)
- Fallback auf TURN bei NAT-Problemen
- Verhalten bei Verbindungsabbruch
- Latenz-Simulation mit künstlicher Verzögerung

---

## 17 Deployment und Distribution

### 17.1 Plattform-Export

Das Projekt wurde primär für Windows exportiert und getestet. Godot 4.5 unterstützt jedoch auch den Export für macOS und Linux, sodass diese Plattformen mit geringem Aufwand ebenfalls bedient werden können.

Der Export erfolgt über Godots integrierten Export-Dialog, der entsprechende Export-Templates für die jeweilige Zielplattform benötigt.

### 17.2 Besonderheiten beim Export

Bei der Distribution von DigiKicker sind folgende Besonderheiten zu beachten:

**Nicht-einbettbare Ressourcen:**

Die folgenden Ordner können nicht in die `.exe`-Datei eingebettet werden und müssen als separate Verzeichnisse neben der Executable bereitgestellt werden:

- **`addons/`**: Enthält die WebRTC- und Godot RL Agents-Plugins als GDExtension-Bibliotheken (`.dll`/`.so`-Dateien). Diese nativen Bibliotheken müssen zur Laufzeit dynamisch geladen werden.

- **`models/`**: Enthält die trainierten ONNX-Modelle für den KI-Gegner. Das ONNX-Runtime-System benötigt Zugriff auf diese Dateien im Dateisystem.

```
DigiKicker/
|--- DigiKicker.exe
|--- addons/
|   |--- godot_rl_agents/
|   +--- webrtc/
+--- models/
    |--- foosball_ai.onnx
    +--- foosball_ai.onnx.data
```

---

## 18 Herausforderungen und Lösungen

### 18.1 Physik-Stabilität

**Problem:** Bei hohen Geschwindigkeiten tunnelte der Ball manchmal durch Wände.

**Lösung:** Aktivierung von Continuous Collision Detection (CCD) und Geschwindigkeitsbegrenzung:

```csharp
ContinuousCd = true;

if (speed > MAX_VELOCITY)
{
    LinearVelocity = LinearVelocity.Normalized() * MAX_VELOCITY;
}
```

### 18.2 RL-Training-Instabilität

**Problem:** Die Policy-Standardabweichung explodierte während des Trainings, was zu divergierendem Verhalten führte.

**Lösung:** Verwendung von State-Dependent Exploration (SDE) und niedrigerer initialer Standardabweichung:

```python
policy_kwargs = dict(
    log_std_init=-1.0,  # exp(-1) ≈ 0.37 statt 1.0
)

model = PPO(
    use_sde=True,
    sde_sample_freq=4,
    target_kl=0.03,  # Frühzeitiger Stopp
)
```

### 18.3 WebRTC NAT-Traversal

**Problem:** Direkte P2P-Verbindungen scheiterten bei restriktiven NAT-Konfigurationen.

**Lösung:** Einrichtung eines eigenen TURN-Servers als Fallback:

```gdscript
const TURN_SERVERS := [{
    "urls": "turn:server-ip:3478?transport=udp",
    "username": "example_user",
    "credential": "example_password",
    "credentialType": "password"
}]
```

**Hinweis:** Die Credentials im obigen Beispiel sind Platzhalter. In einer Produktionsumgebung sollten echte Zugangsdaten sicher verwaltet und nicht im Quellcode hartcodiert werden.

**Zusätzlicher Bug:** Ein weiteres Problem war, dass die WebRTC-Verbindung ohne explizite Angabe des `credentialType`-Feldes nicht funktionierte. Der Wert muss auf `"password"` gesetzt werden, damit die Long-Term Credentials korrekt vom TURN-Server akzeptiert werden.

### 18.4 Observation-Spiegelung für Self-Play

**Problem:** Beim Self-Play-Training mussten beide Agenten das Spiel aus ihrer eigenen Perspektive sehen.

**Lösung:** X-Koordinaten werden für das rote Team gespiegelt:

```gdscript
# mirror = 1.0 für Blue, -1.0 für Red
obs.append(mirror * ball_pos.x / 6.0)
obs.append(mirror * ball_vel.x / 10.0)
```

### 18.5 Stangen-Steuerung und Physik-Interaktion

**Problem:** Die Implementierung der Stangen-Steuerung erwies sich als aufwendiger Debugging-Prozess. Die Interaktion zwischen der Stangen-Bewegung und der dynamischen Ball-Physik führte zu zahlreichen Edge Cases: Figuren konnten den Ball einklemmen, Rotationen verursachten unberechenbare Impulse, und die laterale Bewegung interagierte nicht konsistent mit dem Ball.

**Lösung:** Die Probleme wurden iterativ durch umfangreiches Testing und schrittweise Anpassungen behoben. Zentrale Verbesserungen waren die Einführung von Geschwindigkeitsbegrenzungen für die Stangen-Rotation, die Berechnung der Schussimpulse basierend auf der tatsächlichen Rotationsgeschwindigkeit statt der Eingabe, und die Implementierung einer kurzen Kick-Immunität nach dem Ball-Reset.

### 18.6 WebRTC-Verbindung nicht ordnungsgemäß beendet

**Problem:** Nach Beendigung eines Online-Spiels wurde die WebRTC-Verbindung nicht korrekt geschlossen. Dies führte zu einem kritischen Bug: Wenn ein Spieler anschließend ein Singleplayer-Spiel startete und dabei das Team wählte, das zuvor der Online-Gegner kontrolliert hatte, konnte der ehemalige Gegner das Spiel weiterhin über die offene Verbindung steuern.

**Lösung:** Implementierung einer expliziten Verbindungstrennung beim Verlassen des Online-Spiels. Der `OnlineMultiplayerManager` ruft nun beim Spielende `close_connection()` auf, das sowohl den DataChannel als auch die PeerConnection ordnungsgemäß schließt und alle Signale trennt.

---

