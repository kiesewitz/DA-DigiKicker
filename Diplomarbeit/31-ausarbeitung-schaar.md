# Theorie - Schaar
\textauthor{Nikita Schaar}

# Theorie

Das folgende Kapitel befasst sich mit jeglicher Theorie rund um diese Diplomarbeit. Beschrieben werden sowohl Werkzeuge und Programme als auch jegliche Bauteile und wichtige Hardware, welche verwendet wurde.

## Projektmanagement

Projektmanagement

## Mikrocontroller

### Was sind Mikrocontroller?



### Auswahlkriterien für Mikrocontroller

Um die Leistung der verschiedenen Mikrocontroller vergleichbar zu machen, müssen vorerst Kriterien festgelegt werden, anhand von denen die verschiedenen Optionen verglichen werden. Diese sollten bestmöglich messbar und objektiv vergleichbar sein, um eine gute Basis für die Auswahl zu schaffen. Mithilfe dieser Kriterien werden Punkte vergeben, anhand von denen eine Wahl getroffen wird.

Eine gut vergleichbare Eigenschaft sind die Kosten, bei denen eine Reduktion sowohl für das Entwicklerteam als auch für Endbenutzer, welche das Projekt zu Hause replizieren wollen, vorteilhaft ist. Um die Kosten in direkter Relation zu vergleichen, wird die Formel $Punkte = min(30, ceil(\frac{100}{Kosten}))$ zur Bewertung verwendet.

Für ein angenehmes Spielerlebnis ist eine schnelle und zuverlässige Übertragung der Eingaben wichtig. Daher werden die Prozessorleistung zur Verarbeitung der Eingaben sowie die Übertragungszeiten für den Austausch der Daten als weitere Vergleichsbasis verwendet.

Damit nachhaltige Hardwareentwicklung mit einem Fokus auf Kostenminimierung möglich wird, muss ein Augenmerk auf den Stromverbrauch der Mikrocontroller gelegt werden. Aufgrund von geringen Spannungsgrößen wird diesem Verbrauch in der endgültigen Entscheidung ein im Vergleich zu den anderen Eigenschaften kleinerer Stellenwert beigemessen.

Da das Design mehrere Sensoren beinhaltet, ist ein Mikrocontroller mit mehreren Pins zur Übertragung vorteilhaft. Wenn ein Controller mit einer ausreichenden Pinzahl gewählt wird, wäre eine Reduktion der gesamten Anzahl an Mikrocontrollern möglich.

Ein Punkt, dessen Wichtigkeit nur auf subjektiver Basis ermessen werden kann, sind zusätzliche Features der Optionen. Der ESP32 unterstützt z. B. das ESP Now Protokoll (siehe offizielle Website [@esp-now-protocol]), welches drahtlose Datenübertragung erlaubt, wozu aber bei der individuellen Bewertung des ESP32 mehr kommt.

Für die Gesamtbewertung werden vorerst für die Kosten Punkte zwischen 1 und 30 vergeben, welche anhand von der oben erklärten Formel errechnet werden. Die Punkte der anderen Kategorien werden relativ zu den anderen Optionen vergeben - Platz 1, 2 und 3 erhalten so 9, 6 und 3 Punkte (Kategorie Stromverbrauch: 5, 3, 1 Punkte) - und danach mit einer prozentuellen Gewichtung zur finalen Punkteanzahl addiert.

## Arduino Nano

Der erste Mikrocontroller, welcher im Zuge dieser Arbeit bewertet wird, ist der Arduino Nano. Er ist ein gutes Einsteigermodell für die Entwicklung mit Mikrocontrollern, da er viele Funktionen bietet, welche dank offiziellen Dokumentationen und Tutorials verständlich gemacht werden [@arduino-docs].

### Organisation - Arduino

Der Arduino Nano ist ein Produkt von der Firma Arduino[@arduino-about] [@arduino-linkedin], einer Tochterfirma von Qualcomm. Arduino befasst sich mit dem Design, der Herstellung und der Weiterentwicklung von Mikrocontrollern. Die offizielle "Mission & Vision" der Firma ist die Erweiterung und Erleichterung des Zugangs zu Elektronik und digitalen Technologien. Für diese Vision arbeitet die Arduino-Community, welche sich weltweit aus Menschen von Hobbyist:innen über Schüler:innen bis hin zu professionellen Entwickler:innen zusammensetzt.

Die Firma wurde im Jahr 2005 in Italien gegründet und hat vier Co-Gründer, welche im folgenden Abschnitt kurz aufgelistet werden:
- Massimo Banzi
  - Ursprünglich arbeitete er als Softwarearchitekt, befasste sich später jedoch weitaus mehr mit der Elektrotechnik. Heutzutage lehrt er sowohl an der USI Universität sowie an der SUPSI in der Schweiz. 
  Zusätzlich zu diesen Errungenschaften ist er der Autor des Buchs "Getting Started with Arduino"[@getting-started-with-arduino], in welchem er die Leser:innen an die Entwicklung mit Arduinos heranführt und über persönliche Erfahrungen berichtet.
- David Cuartielles
  - So wie Banzi lehrt auch er an einer Uni, wobei es bei ihm die Malmö-Universität in Schweden ist, an der er seit dem Jahr 2000 unterrichtet. Er hat sowohl einen MSc. als auch einen Doktortitel, war an verschiedenen Unis von Europa über Amerika, bis Asien und spricht öffentlich über Open-Source-Hardware und STEAM-Unterricht [@steam-education].
- Tom Igoe
  - Seine Forschung umfasst verschiedene Themen von Netzwerken über Belichtungsdesign und die Auswirkungen von technologischer Entwicklung auf die Umwelt. Er hat für mehrere Museen und Design-Firmen als Berater gewirkt sowie 4 Bücher und zahlreiche Artikel verfasst, welche sich mit Elektronik befassen.
- David Mellis
  - Er arbeitet als Softwareentwickler bei der Firma Autodesk, wo es sein Ziel ist, Menschen für die kreative bzw. DIY-Nutzung von elektronischen Bauteilen zu inspirieren. Wie seine Mitgründer hat auch er mehrere Titel (MSc., PhD.) und unterrichtete ebenfalls an Universitäten wie dem Copenhagen Institute of Interaction Design (Dänemark).

Die vier Gründer in Reihenfolge der Nennung:

![Massimo Banzi](img/Schaar/massimo-banzi.png)
![David Cuartielles](img/Schaar/david-cuartielles.png)
![Tom Igoe](img/Schaar/tom-igoe.png)
![David Mellis](img/Schaar/david-mellis.png)

### Technische Spezifikationen


## ESP32

Billiger, bessere Leistung

## Raspberry Pi Pico

raspi pico

## Vergleich der Optionen & Entscheidung

Direkter Vergleich (mit Diagrammen, etc.)

## Sensorenauswahl

Kriterien

### Maussensor

Billig, leicht nachzumachen

### MPU6050

Potentiell genauer, Komplexer für Nachbau, teurer

### HC-SR04 + Potentiometer

Warum eher nicht? (Schleifring) 

## Datenaustausch

Kriterien

### ESP-Now

Großteils kabellose Datenübertragung möglich

### Serielle Schnittstelle

Übertragung von Daten zwischen Microcontroller/Godot mithilfe von serieller Schnittstelle mit Code in C#

## Erstellung der Nachbauanleitung

Herangehensweise der Erstellung, Open-Source-Bereitstellung der Anleitung

# Praxis - Schaar

Kurzbeschreibung

## Prototyping

In diesem Kapitel wird es um Unverwendete Prototypen und Ansätze, Demos und andere Entwicklungsschritte gehen, welche sich im Laufe der Arbeit am Controller ergeben haben. Anhand von ihnen werden der Entwicklungsprozess und verschiedene Iterationen dargestellt.

### Arduino <-> Godot Kommunikations-Demo

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

### Controller V1 - Demo für Sensoreingaben

Nachdem die Erstversion des physischen Controllers fertiggestellt wurde war natürlich eine erweiterte Simulation vonnöten, um die Eingaben auf ihre Funktion zu überprüfen.

![Simulation V1](img/Schaar/ControllerV1Demo.png)

Auch wenn die zweite Simulation visuell wieder

## Design des Controllers

### Case Design

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

## Verbindung - Hardware & Software

Technischere Details und Beschreibung für Schnittstelle(n) auf der Hardware-Seite

## Debugging/Bug-Fixing

Aufgetretene Fehler, Lösungsansätze

## Finaler Controller

## Audio Design