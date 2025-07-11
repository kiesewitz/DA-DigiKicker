# Teilaufgabe Schüler Schaar
\textauthor{Nikita Schaar}

## Theorie

Kurzbeschreibung

### Projektmanagement

Kurzbeschreibung Projektmanagement, später längerer Teil.

### Mikrocontrollerauswahl

Um die Leistung der verschiedenen Mikrocontroller vergleichbar zu machen müssen vorerst Kriterien festgelegt werden, anhand von denen die verschiedenen Optionen verglichen werden. Diese sollten so messbar und objektiv wie möglich vergleichbar sein, um eine gute Basis für die Auswahl zu schaffen. Mithilfe dieser Kriterien werde ich Punkte vergeben, anhand von denen ich danach eine Wahl treffen werde.

Eine Eigenschaft die diesen Vergleich gut ermöglicht sind die Kosten. Natürlich ist es hierbei positiv, diese möglichst niedrig zu halten, da es für uns als Entwicklungsteam gut ist, wenn wir geringere Hardware-Kosten haben. Hinsichtlich darauf, dass wir die Pläne zusammen mit einer Anleitung Open-Source zugänglich machen, ist es auch gut niedrige Kosten zu haben, da so weit mehr Menschen einen Zugang zu diesem Projekt haben werden. 

Die Verwendung der Mikrocontroller sollte "simpel" bleiben, sodass kein zu großes Vorwissen benötigt wird um den Controller zu Hause nachzubauen. Aus diesem Grund ist es ein Ziel, die Komplexität (der Verwendung) des Controllers so niedrig wie möglich zu halten. Da die Komplexität als einzelne Eigenschaft hier aber schwierig festzulegen ist, muss die Bewertung in dieser Hinsicht auf einer etwas subjektiveren Basis durchgeführt werden.

Für ein angenehmes Spielerlebnis ist eine schnelle und zuverlässige Übertragung der Eingaben wichtig, weshalb die Prozessorleistung zur Verarbeitung der Eingaben, sowie die Übertragungszeiten für den Austausch der Daten, für den Vergleich der verschiedenen Optionen auch wichtig sind. Diese sind einfach messbar und vergleichbar, weshalb sie sich auch als gutes Merkmal für den Vergleich eignen.

Bei der Entwicklung von Hardware ist natürlich, einerseits aus einer nachhaltigen Perspektive, aber auch wieder aus einer finanziellen Motivation, der Stromverbrauch ein wichtiger Entscheidungsgrund. Da die Unterschiede im Fall unseres Projektes, durch die Verwendung von eher weniger leistungsstarken Controllern geringer bleiben, werden diese Werte zwar in die Bewertung einfließen, aber für die Berechnung der Punkte nicht maßgeblich ausschlaggebend sein und eher für die engere Entscheidung herangezogen werden.

Da für das Projekt mehrere Sensoren benötigt werden sind eine größere Anzahl an Pins für die Datenmessung ein wichtiges Merkmal, welches bei der Bewertung berücksichtigt werden sollte. Wenn die Pinanzahl reicht kann man statt einem Sensor zu einem Mikrocontroller möglicherweise 2-zu-1 erreichen.

Ein weiterer Punkt, bei dem die Wichtigkeit wieder nur auf subjektiver Basis beigemessen werden kann, sind zusätzliche Features welche die verschiedenen Sensoren aufweisen. Der ESP32 unterstützt beispielsweise das ESP Now Protokoll (siehe offizielle Website[@esp-now-protocol]), welches drahtlose Datenübertragung erlaubt, wozu aber bei der Bewertung des ESP32 mehr kommt.

Anhand von all diesen Kriterien wird im folgenden Kapitel eine Entscheidung dafür getroffen, welche Mikrocontroller wir in dieser Arbeit verwenden.

#### Arduino Nano

Teurer, geringere Leistung

#### ESP32

Billiger, bessere Leistung

#### Raspberry Pi Pico



#### Vergleich der Optionen

Direkter Vergleich (mit Diagrammen, etc.)

### Sensorenauswahl

Kriterien

#### Maussensor

Billig, leicht nachzumachen

#### MPU6050

Potentiell genauer, Komplexer für Nachbau, teurer

#### HC-SR04 + Potentiometer

Warum eher nicht? (Schleifring) 

### Datenaustausch

Kriterien

#### ESP-Now

Großteils kabellose Datenübertragung möglich

#### Serielle Schnittstelle

Übertragung von Daten zwischen Microcontroller/Godot mithilfe von serieller Schnittstelle mit Code in C#

### Erstellung der Nachbauanleitung

Herangehensweise der Erstellung, Open-Source-Bereitstellung der Anleitung

## Praktischer Teil

Kurzbeschreibung

### Prototyping

In diesem Kapitel wird es um Unverwendete Prototypen und Ansätze, Demos und andere Entwicklungsschritte gehen, welche sich im Laufe der Arbeit am Controller ergeben haben. Durch sie soll es einfacher sein, mögliche Unterschiede, gefällte Entscheidungen und Änderungen zu verstehen.

#### Arduino <-> Godot Kommunikations-Demo

Für die Erstpräsentation unserer Arbeit habe ich mit Hr. Rath zusammen eine Demo erstellt, anhand von der die Kommunikation zwischen einem Arduino und der Godot Engine veranschaulicht wird. Die Grundstruktur dieser Demo habe ich aus einem Youtube-Video genommen[@connect-godot-arduino], in dem die Übertragung der Daten über die Serielle Schnittstelle erklärt und anhand von einem kurzen Beispiel gezeigt wird.

Am Arduino werden hier die Signale von einem MPU6050-Sensor eingelesen und über die Serielle Schnittstelle übertragen.

```c++
TODO Hier kommt der Code hin, sobald der richtige im Repo ist (wieso auch immer nur der Platzhalter drauf ist) TODO
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

Überall wo in den Code-Segmenten ein Kommentar mit ... steht ist zusätzlicher Code, welcher aber zur Erklärung des Kernprozesses nicht wichtig ist und unnötigen Platz verbrauchen würde, da es sich hauptsächlich um die Deklaration von Variablen handelt.

Mithilfe des Codes am Arduino, welcher vom Sensor die Rotationsdaten nimmt und sie im richtigen Format an die Serielle Schnittstelle schickt und dem C#-Code welcher diese Werte annimmt und zur Drehung eines Würfels verwendet ist schlussendlich diese Demo entstanden, welche die Verbindung zwischen einem Mikrocontroller und der Godot Engine und die sich daraus ergebenden Möglichkeiten darstellt.

### Design des Controllers

#### Optisches Design

Anforderungen an das Design, Iterationen, etc.

#### Internes Design

Anforderungen, Iterationen, (möglicherweise) Entwicklung spezialisierter Platinen, etc.

### Verbindung - Hardware & Software

Technischere Details und Beschreibung für Schnittstelle(n) auf der Hardware-Seite

### Debugging/Bug-Fixing

Aufgetretene Fehler, Lösungsansätze

### Finaler Controller