# Teilaufgabe Schüler Schaar
\textauthor{Nikita Schaar}

## Theoretischer Teil

Das folgende Kapitel befasst sich mit jeglicher Theorie rund um diese Diplomarbeit. Beschrieben werden sowohl Werkzeuge und Programme als auch jegliche Bauteile und wichtige Hardware, welche verwendet wurden.

### Projektmanagement

Die Zuständigkeit für das Projektmanagement der Arbeit "DigiKicker - Digitalisierung eines Tischfußballtisches" lag bei dem Schüler Schaar. Der Ansatz des Managements war hierbei agil mit regelmäßigen Besprechungen über den Projektstand (Diese Kurzbeschreibung noch etwas erweitern).

Für die detailreiche Dokumentation des Projektmanagements wird auf Seite xxx verwiesen.

### Mikrocontroller

In diesem Kapitel wird der Begriff Mikrocontroller erklärt und die Unterschiede zu Mikroprozessoren werden aufgezeigt. Darauf folgend werden drei verschiedene Mikrocontroller-Optionen betrachtet und anhand von ihren Eigenschaften und technischen Spezifikationen verglichen.

#### Was sind Mikrocontroller?

Um Mikrocontroller [@microcontroller-definition] zu erklären, muss erst ihr Unterschied zu Mikroprozessoren beleuchtet werden. Mikroprozessoren sind so wie die "größeren" Prozessoren - In diesem Fall sowohl auf physikalische Größe als auch auf Leistung bezogen - die Recheneinheit eines Systems. Während Mikroprozessoren ausschließlich rechnerische Aufgaben bearbeiten und für andere Funktionen separate Teile (RAM, ROM, Timer-Module, etc.) benötigen, fungieren Mikrocontroller als ein fertiges Paket, welches die verschiedenen Teile verbaut hat.

![Mikroprozessor vs. Mikrocontroller](img/Schaar/Difference_Microprocessor_Microcontroller.png)

Wie in Abbildung x [@microcontroller-microprocessor] ersichtlich, ist links der Mikroprozessor bzw. die CPU (Central Processing Unit) zentral, benötigt jedoch extern verbundene Teile, sodass alle Aufgaben ausgeführt werden können. Ihm gegenübergestellt wird der Mikrocontroller, bei dem veranschaulicht wird, dass es sich hierbei um ein fertiges System handelt, bei dem alle Teile in diesem Ökosystem fest verbaut sind. Um eine Analogie zum menschlichen Körper zu bilden wäre in diesem Fall der Mikroprozessor das Gehirn alleine und der Mikrocontroller ein vollständiger Körper, welcher zur Funktion auf keine zusätzlichen Hilfsmittel angewiesen ist.

Tendenziell sind Mikroprozessoren für leistungsintensivere Anwendungszwecke angedacht als Mikrocontroller (z. B. grafische Berechnungen, Multitasking). Mikrocontroller sind durch ihre autonome Struktur in puncto Kosten, Stromverbrauch und Größe beliebt und bringen viele Möglichkeiten mit ohne unnötig hohe Komplexität. Aufgrund von diesen Faktoren werden Mikrocontroller sehr gerne in eingebetteten Systemen (Embedded Systems) [@embedded-systems] verwendet. 

Dieser Begriff bezeichnet Systeme, welche für einen speziellen Zweck entwickelt und optimiert sind, im Gegensatz zu herkömmlichen Mikroprozessoren, welche für viele Anwendungszwecke existieren. Diese Systeme bilden sich aus einem Zusammenhang zwischen Software und Hardware und können eigenständig verwendet werden, wobei sie jedoch meist eher als Teil eines größeren Systems eine Aufgabe erledigen, wie z. B. in einem Auto oder einer Spielekonsole. Den Namen haben die Embedded Systems genau dieser Eigenschaft der "Einbettung" in andere Systeme zu verdanken.

Mikrocontroller sind in den verschiedensten Systemen zu zahlreichen Zwecken verbaut und erfüllen in jedem eine eigene Aufgabe. Heute gibt es Mikrocontroller in jeder erdenklichen Ausführung, weshalb sie für jegliche Anwendungszwecke von Hobbyprojekten bis zu Raketenwissenschaften nicht mehr wegzudenken sind.

#### Auswahlkriterien für Mikrocontroller

Um die Leistung der verschiedenen Mikrocontroller vergleichbar zu machen, müssen vorerst Kriterien festgelegt werden, anhand von denen die verschiedenen Optionen verglichen werden. Diese sollten bestmöglich messbar und objektiv vergleichbar sein, um eine gute Basis für die Auswahl zu schaffen. Mithilfe dieser Kriterien werden Punkte vergeben, anhand von denen eine Wahl getroffen wird.

Eine gut vergleichbare Eigenschaft sind die Kosten, bei denen eine Reduktion sowohl für das Entwicklerteam als auch für Endbenutzer, welche das Projekt zu Hause replizieren wollen, vorteilhaft ist. Um die Kosten in direkter Relation zu vergleichen, wird die Formel $Punkte = min(30, ceil(\frac{100}{Kosten}))$ zur Bewertung verwendet. Mit ihr wird 100 durch die Kosten dividiert und das Ergebnis gerundet, sodass eine Punktezahl herauskommt. Danach wird dieser Wert auf maximal 30 gesetzt, sodass nicht mehr Punkte möglich sind, auch wenn niedrigere Kosten bestehen. Dieser Ansatz wurde gewählt, da er simpel zu berechnen ist und Kostenunterschiede in der Punktzahl sichtbar macht.

Für ein angenehmes Spielerlebnis ist eine schnelle und zuverlässige Übertragung der Eingaben wichtig. Daher werden die Prozessorleistung zur Verarbeitung der Eingaben sowie die Übertragungszeiten für den Austausch der Daten als weitere Vergleichsbasis verwendet.

Damit nachhaltige Hardwareentwicklung mit einem Fokus auf Kostenminimierung ermöglicht wird, muss ein Augenmerk auf den Stromverbrauch der Mikrocontroller gelegt werden. Aufgrund von geringen Spannungsgrößen wird diesem Verbrauch in der endgültigen Entscheidung ein im Vergleich zu den anderen Eigenschaften kleinerer Stellenwert beigemessen.

Für die Gesamtbewertung werden vorerst für die Kosten Punkte zwischen 1 und 30 vergeben, welche anhand von der oben erklärten Formel errechnet werden. Die Punkte der anderen Kategorien werden relativ zu den anderen Optionen vergeben - Platz 1, 2 und 3 erhalten so 9, 6 und 3 Punkte (Kategorie Stromverbrauch: 3, 2, 1 Punkte) - und danach zur finalen Punkteanzahl addiert.

![Punkteverteilung Mikrocontroller](img/Schaar/PointWeightsMicrocontrollers.png)

In Abbildung x (erstellt in Microsoft Excel) sind die Anteile der Bewertungskategorien an der finalen Punktezahl visuell dargestellt. Wie man erkennt, machen die Kosten hierbei den größten Anteil aus, während die Eigenschaften, welche sich auf die Übertragung der Eingaben beziehen, mit jeweils 17 % auch eine relativ hohe Gewichtung besitzen. Da wie bereits erwähnt die Stromversorgung vernachlässigbar ist, macht sie mit 6 % keinen allzu großen Anteil an der Gesamtpunktzahl aus. Bei diesen Anteilen wird für alle Kategorien vom theoretisch maximalen Ergebnis an Punkten ausgegangen (51 Punkte bei perfekter Bewertung in allen Kategorien).

### Arduino Nano ESP32

![Arduino Nano ESP32](img/Schaar/Arduino-Nano-ESP32-IRL.png)

Der erste Mikrocontroller, welcher im Zuge dieser Arbeit bewertet wird, ist der Arduino Nano ESP32 (siehe Abbildung x [@arduino-nano-esp32-image]). Er ist ein Controller mit überschaubarer Komplexität, bietet jedoch trotzdem zahlreiche Funktionen für alle möglichen Zwecke, welche dank offiziellen Dokumentationen und Tutorials verständlich gemacht werden [@arduino-docs]. Wie schon am Namen ersichtlich beinhaltet dieser Controller auch einen ESP32, gleich wie das ESP-32 DevKit C, ein anderer behandelter Controller in dieser Arbeit, jedoch ist hierbei ein ESP32-S3 verbaut und kein ESP32-WROOM-32. Zum DevKit C bestehen auch Unterschiede, was technische Daten sowie den Kostenpunkt angeht. Im Detail folgen diese Unterschiede in der Auflistung der technischen Daten.

Während die primäre Programmiersprache des Arduino Nano ESP32 zwar eine simplifizierte Version von C bzw. C++ (Arduino Language) ist, gibt es bei dieser Ausführung des Nano die Möglichkeit, in MicroPython [@micropython] zu programmieren, da diese Sprache auf allen ESP32-basierten Controllern funktioniert. Bei MicroPython handelt es sich ähnlich wie bei der Arduino Language selbst um eine schlankere Version und effizientere Version von Python. Sie hat einen Teil der Funktionalitäten der Python Standard Library und ist für die Verwendung auf Mikrocontrollern optimiert. Die genauen Unterschiede zum standardmäßigen CPython sind zahlreich und reichen von Syntax-Unterschieden bis zu entfernten Core-Funktionen.

Beispiel:  Einzelne Array-Elemente können nicht wie in CPython gelöscht werden.

```Python
a = array.array("b", (1, 2, 3))
del a[1]
```

Während dieser Ausdruck in CPython zu einer Löschung des Elements am Index 1 geführt hätte bekommt man in MicroPython einen Error.

```
Traceback (most recent call last):
  File "<stdin>", line 11, in <module>
TypeError: 'array' object doesn't support item deletion
```

Dies war nur ein Beispiel für die vielen kleinen Änderungen von MicroPython, welche die Sprache dazu bringen so hochoptimiert und effizient auf vergleichsweise leistungsschwacher Hardware zu funktionieren.

#### Firma - Arduino

Der Arduino Nano ist ein Produkt von der Firma Arduino [@arduino-about] [@arduino-linkedin], einer Tochterfirma von Qualcomm. Arduino befasst sich mit dem Design, der Herstellung und der Weiterentwicklung von Mikrocontrollern. Die offizielle "Mission & Vision" der Firma ist die Erweiterung und Erleichterung des Zugangs zu Elektronik und digitalen Technologien. Für diese Vision arbeitet die Arduino-Community, welche sich weltweit aus Menschen von Hobbyist:innen über Schüler:innen bis hin zu professionellen Entwickler:innen zusammensetzt.

Die Firma wurde im Jahr 2005 in Italien gegründet und hat vier Co-Gründer, welche im folgenden Abschnitt kurz aufgelistet werden:

**1. Massimo Banzi**

- Ursprünglich arbeitete er als Softwarearchitekt, befasste sich später jedoch weitaus mehr mit der Elektrotechnik. Heutzutage lehrt er sowohl an der USI Universität sowie an der SUPSI in der Schweiz. 
Zusätzlich zu diesen Errungenschaften ist er der Autor des Buchs "Getting Started with Arduino" [@getting-started-with-arduino], in welchem er die Leser:innen an die Entwicklung mit Arduinos heranführt und über persönliche Erfahrungen berichtet.

**2. David Cuartielles**

- So wie Banzi lehrt auch er an einer Uni, wobei es bei ihm die Malmö-Universität in Schweden ist, an der er seit dem Jahr 2000 unterrichtet. Er hat sowohl einen MSc. als auch einen Doktortitel, war an verschiedenen Unis von Europa über Amerika bis Asien und spricht öffentlich über Open-Source-Hardware und STEAM-Unterricht [@steam-education].

**3. Tom Igoe**

- Seine Forschung umfasst verschiedene Themen von Netzwerken über Belichtungsdesign und die Auswirkungen von technologischer Entwicklung auf die Umwelt. Er hat für mehrere Museen und Design-Firmen als Berater gewirkt sowie 4 Bücher und zahlreiche Artikel verfasst, welche sich mit Elektronik befassen.

**4. David Mellis**

- Er arbeitet als Softwareentwickler bei der Firma Autodesk, wo es sein Ziel ist, Menschen für die kreative bzw. DIY-Nutzung von elektronischen Bauteilen zu inspirieren. Wie seine Mitgründer hat auch er mehrere Titel (MSc., PhD.) und unterrichtete ebenfalls an Universitäten wie dem Copenhagen Institute of Interaction Design (Dänemark).

#### Technische Daten

Alle hier tabellarisch angegebenen Spezifikationen kommen aus dem offiziellen Arduino-Nano-ESP32-Datenblatt [@arduino-nano-datasheet]. Die Daten beziehen sich hierbei auf die zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden.

| Controllereigenschaft         | Wert / Größe                                        |
|------------------------------:|:----------------------------------------------------|
| Taktfrequenz                  | 240 MHz                                             |
| Busbreite                     | 32 Bit                                              |
| SRAM                          | 512 kB                                              |
| Wi-Fi-Geschwindigkeiten       | Bis zu 150 Mbit/s bei 40 MHz                        |
| Flash-Speicher                | 16 MB                                               |
| VIN (Eingangsspannung) Rating | 6-21 V                                              |

: Arduino Nano ESP32 - Eigenschaften

![Arduino Nano ESP32 Pinout-Diagramm](img/Schaar/Pinout-Arduino-Nano-ESP32.png)

Abbildung x [@arduino-nano-pinout] stellt das Pin-Layout eines Arduino Nano ESP32 dar.

### ESP-32 DevKit C

![ESP-32 DevKit C V4](img/Schaar/ESP32-DevKit-C-V4-IRL.png)

Der nächste Mikrocontroller ist das ESP-32 Dev Kit C V4 [@esp32-data] (siehe Abbildung x [@esp32-devkit-c-image]), welcher wie der zuvor betrachtete Mikrocontroller auf einem ESP32 - genauer gesagt, einem ESP32-WROOM-32 - basiert ist, weshalb für die Programmierung die Arduino Language sowie MicroPython verwendet werden kann. Wie allen ESP32-basierten Controllern ist es auch diesem möglich, von dem Protokoll "ESP-Now" Gebrauch zu machen. 

Hierbei handelt es sich um ein eigens entwickeltes Protokoll von Espressif Systems, welches es mehreren Geräten erlaubt, sich untereinander zu verständigen. Das Protokoll [@esp-now-protocol] ist auf dem 802.11 Wi-Fi Standard basiert, benötigt jedoch keinen zusätzlichen Access Point für die Kommunikation.

#### Firma - Espressif Systems

Das ESP-32 DevKit C ist Teil der großen ESP32-Familie an Mikrocontrollern, welche von Espressif Systems [@espressif-about] entwickelt wird. Die Firma arbeitet an innovativen Lösungen rund um IoT und arbeitet in den letzten Jahren mehr und mehr an KI-Integration für Mikrocontroller. Sie veröffentlichen ihre Ressourcen und Modelle Open-Source und tragen damit maßgeblich zur globalen Weiterentwicklung von Mikrocontrollern, Embedded Systems und IoT bei. Ihre Produktlösungen umfassen Smart Home Systeme, IoT Security, Machine Vision, Deep Learning, Speech AI und viele weitere 

Teo Swee Ann hat die Firma im Jahr 2008 in Shanghai gegründet und ist bis heute der CEO bei Espressif Systems. 2016 wird dann der originale ESP32-Mikrocontroller veröffentlicht, welcher durch seine WLAN- & Bluetooth-Kapazitäten revolutionär ist. Seither wurden der Familie allerlei neue und spezialisierte Mikrocontroller hinzugefügt, wodurch für Projekte verschiedenster Größenordnungen Optionen existieren.

#### Technische Daten

Alle hier tabellarisch angegebenen Spezifikationen kommen aus dem offiziellen ESP32-WROOM-32-Datenblatt [@esp32-datasheet]. Die Daten beziehen sich hierbei auf die zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden.

| Controllereigenschaft         | Wert / Größe                                        |
|------------------------------:|:----------------------------------------------------|
| Taktfrequenz                  | 240 MHz                                             |
| Busbreite                     | 32 Bit                                              |
| SRAM                          | 520 kB                                              |
| Wi-Fi-Geschwindigkeiten       | Bis zu 150 Mbit/s                                   |
| Flash-Speicher                | 4/8/16 MB                                           |
| VIN (Eingangsspannung) Rating | 3.0-3.6 V                                           |

: ESP32-WROOM-32 - Eigenschaften

![ESP32-WROOM-32 Pinout-Diagramm](img/Schaar/Pinout-ESP32-DevBoard.jpg)

In Abbildung x [@esp32-pinout] werden die Pin-Belegungen eines ESP32-WROOM-32 dargestellt.

### Raspberry Pi Pico 2W

![Raspberry Pi Pico 2W](img/Schaar/Raspberry-Pi-Pico-2W-IRL.png)

Der letzte Mikrocontroller, der in dieser Arbeit betrachtet wird ist der Raspberry Pi Pico 2W (siehe Abbildung x [@raspberry-pi-pico-2w-image])

#### Firma - Raspberry Pi

#### Technische Daten

Alle hier tabellarisch angegebenen Spezifikationen kommen aus dem offiziellen Raspberry-Pi-Pico-2W-Datenblatt [@raspberry-pi-pico-datasheet]. Die Daten beziehen sich hierbei auf die zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden.

| Controllereigenschaft         | Wert / Größe                                        |
|------------------------------:|:----------------------------------------------------|
| Taktfrequenz                  | 150 MHz                                             |
| Busbreite                     | 32 Bit                                              |
| SRAM                          | 520 kB                                              |
| Wi-Fi-Geschwindigkeiten       | Bis 72.2 Mbit/s möglich (real ~9-10 Mbit/s)         |
| Flash-Speicher                | 4 MB                                                |
| VIN (Eingangsspannung) Rating | 5 V                                                 |

: Raspberry Pi Pico 2W - Eigenschaften

![Raspberry Pi Pico 2W Pinout-Diagramm](img/Schaar/Pinout-Raspberry-Pi-Pico-2W.png)

Die Abbildung x [@raspberry-pi-pico-2w-pinout] stellt die Pin-Belegungen eines Raspberry Pi Pico 2W dar.

### Vergleich der Optionen & Entscheidung

In diesem Teil folgt der direkte Vergleich sowie die Entscheidung für eine der Mikrocontroller-Optionen. Jegliche Diagramme, welche zur Veranschaulichung der Daten verwendet werden, wurden mithilfe von Microsoft Excel erstellt.

### Sensorenauswahl

Kriterien

#### Maussensor / Maus

Billig, leicht nachzumachen

#### HC-SR04 + MPU6050 / IMU-Sensor

Warum eher nicht? (Schleifring), Komplexer für Nachbau

### Datenaustausch

Kriterien

#### Serielle Schnittstelle

Übertragung von Daten zwischen Microcontroller/Godot mithilfe von serieller Schnittstelle mit Code in C#

### Erstellung der Nachbauanleitung

Herangehensweise der Erstellung, Open-Source-Bereitstellung der Anleitung, Nach Fertigstellung der Praxis

## Praktischer Teil

Kurzbeschreibung

### Prototyping

In diesem Kapitel geht es um Unverwendete Prototypen und Ansätze, Demos und andere Entwicklungsschritte, welche sich im Laufe der Arbeit am Controller ergeben haben. Anhand von ihnen werden der Entwicklungsprozess und verschiedene Iterationen dargestellt.

#### Arduino <-> Godot Kommunikations-Demo

Für die Erstpräsentation unserer Arbeit haben wir eine Demo erstellt, anhand von der die Kommunikation zwischen einem Arduino und der Godot Engine veranschaulicht wird. Die Grundstruktur dieser Demo wurde aus einem Youtube-Video genommen [@connect-godot-arduino], in dem die serielle Übertragung der Daten erklärt und beispielhaft dargestellt wird.

Am Arduino werden die Signale von einem MPU6050-Sensor eingelesen und über die Serielle Schnittstelle übertragen.

```c++
// ...

Adafruit_MPU6050 mpu;

void setup(void) {
  Serial.begin(9600);
  while (!Serial)
    delay(10);

  Serial.println("Adafruit MPU6050 test!");
  if (!mpu.begin()) {
    Serial.println("Failed to find MPU6050 chip");
    while (1) {
      delay(10);
    }
  }
  Serial.println("MPU6050 Found!");

  // ...
}

void loop() {
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  // ...

  Serial.print("Rotation X: ");
  Serial.print(g.gyro.x);
  Serial.print(", Y: ");
  Serial.print(g.gyro.y);
  Serial.print(", Z: ");
  Serial.print(g.gyro.z);
  Serial.println(" rad/s");

  // ...

  Serial.println("");
  delay(500);
}
```

In Godot wird das ganze über ein C#-Skript aufgenommen und ein 3D-Würfel wird anhand von den übernommenen Rotationswerten korrekt gedreht, obwohl hierbei teilweise noch Kalibrationsfehler vorkommen.

```c#
using Godot;
using System;
using System.IO.Ports;

public partial class Arduino : Node3D
{
    // ...
	public override void _Ready()
	{
        // ...
		serialPort = new SerialPort(
            "COM6", 9600, Parity.None, 8, StopBits.One
        );
		serialPort.DtrEnable = true;
		serialPort.RtsEnable = true;
		serialPort.Open();
	}

	public override void _Process(double delta)
	{
        // ...
		
		string serialMessage = serialPort.ReadLine().Replace('.', ',');
		
		coords = serialMessage.Split(' ');
		
		x = (float) (Convert.ToDouble(coords[2])*delta);
		y = (float) (Convert.ToDouble(coords[4])*delta);
		z = (float) (Convert.ToDouble(coords[6])*delta);
        
		cube.RotateX(x);
		cube.RotateY(y);
		cube.RotateZ(z);

        // ...
	}
}
```

Kommentare mit drei Punkten stellen hierbei zusätzlichen Code dar, welcher für die Erklärung des Kernprozesses keine Wichtigkeit hat und meist aus Variablendeklarationen besteht.

Mithilfe des Arduino-Codes, welcher vom Sensor die Rotationsdaten nimmt und sie im richtigen Format an die Serielle Schnittstelle schickt und dem C#-Code welcher diese Werte annimmt und zur Drehung eines Würfels verwendet ist schlussendlich diese Demo entstanden, welche die Verbindung zwischen einem Mikrocontroller und der Godot Engine und die sich daraus ergebenden Möglichkeiten darstellt.

Mithilfe des Arduino-Codes, werden die Rotationsdaten des MPU6050 [@arduino-guide-mpu6050] an die Serielle Schnittstelle gesendet. Der C#-Code nimmt diese Werte an und dreht anhand von ihnen ein Würfel-Objekt. Durch diese Verbindung werden die Möglichkeiten für dieses Projekt simpel und effektiv dargestellt.

#### Controller V1 - Demo für Sensoreingaben

Nachdem die Erstversion des physischen Controllers fertiggestellt wurde war natürlich eine erweiterte Simulation vonnöten, um die Eingaben auf ihre Funktion zu überprüfen.

![Simulation V1](img/Schaar/ControllerV1Demo.png)

Auch wenn die zweite Simulation visuell wieder

### Design des Controllers

Dieses Kapitel handelt von den verschiedenen Iterationen der Hülle des Controllers, sowie von den Entscheidungen, welche bei der Entwicklung getroffen worden sind.

Zu Beginn war die Zurechtfindung in der neu gewählten CAD-Umgebung "FreeCAD" ungewöhnlich, jedoch war es nicht schwierig sich daran anzupassen. Die Wahl für FreeCAD entstand, da das ganze Projekt so gut es geht mit Open Source Software realisiert werden sollte.

Zuallererst wurde ein Prototyp entwickelt, der nur zur Bestimmung der grundlegenden Abmessungen des Controllers diente und deshalb nichts außer einer vagen Ähnlichkeit zu einem Tischfußballtisch hat.

![Prototyp 0.0](img/Schaar/ControllerV0Closed.png)

Für die ersten Prototypen ist das Design auf einen Drehstab reduziert, da dieser sich für Testzwecke besser eignet und das endgültige Design auf vier Stäbe hochskaliert werden kann.

Da diese rudimentäre Erstdarstellung jedoch selbst für Testzwecke noch unzureichend war, ist als nächstes eine erweiterte Version 1.0 designt worden, welche dem Bildnis eines Tischfußballtisches in mehreren Hinsichten ähnelt.

![Prototyp 1.0 Closed](img/Schaar/ControllerV1Closed.png) ![Prototyp 1.0 Open](img/Schaar/ControllerV1Open.png)

![Prototyp 1.0 Open Backside](img/Schaar/ControllerV1OpenBackSide.png)

Diese Version weist einen Drehstab, eine Plattform für elektronische Bauteile, eine Trennwand zur Stabilisierung des Stabes sowie ein Loch für Kabel auf, sodass die Stromversorgung mit geschlossenem Deckel ermöglicht wird. Das erweiterte Design wurde dann, zur Erleichterung des Designprozesses und der Darstellung der Grundidee mithilfe eines BambuLab X1C 3D-Druckers ausgedruckt.

![Prototyp 1.0 Open IRL](img/Schaar/ControllerV1OpenIRL.jpg)

![Prototyp 1.0 Closed IRL](img/Schaar/ControllerV1ClosedIRL.jpg)

Um das visuelle Design attraktiver zu gestalten wurden Sticker in Form von dem DigiKicker-Logo auf dem Deckel und der Rückseite angebracht.

Nachdem der 

### Verbindung - Hardware & Software

Technischere Details und Beschreibung für Schnittstelle(n) auf der Hardware-Seite

### Debugging/Bug-Fixing

Aufgetretene Fehler, Lösungsansätze

### Finaler Controller

### Audio Design