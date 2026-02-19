# Teilaufgabe Schüler Schaar
\textauthor{Nikita Schaar}

## Theoretischer Teil

Das folgende Kapitel befasst sich mit jeglicher Theorie rund um diese Diplomarbeit. Beschrieben werden sowohl Werkzeuge und Programme als auch jegliche Bauteile und wichtige Hardware, welche verwendet bzw. in Betracht gezogen wurden.

### Projektmanagement

Die Zuständigkeit für das Projektmanagement der Arbeit "DigiKicker - Digitalisierung eines Tischfußballtisches" lag bei dem Schüler Schaar. Der Ansatz des Managements war hierbei agil mit regelmäßigen Besprechungen über den Projektstand (Diese Kurzbeschreibung noch etwas erweitern).

Für die detailreiche Dokumentation des Projektmanagements wird auf Seite xxx verwiesen.

### Mikrocontroller

In diesem Kapitel wird der Begriff Mikrocontroller erklärt und die Unterschiede zu Mikroprozessoren werden aufgezeigt. Darauf folgend werden drei verschiedene Mikrocontroller-Optionen betrachtet und anhand von ihren Eigenschaften und technischen Spezifikationen verglichen.

#### Was sind Mikrocontroller?

Um Mikrocontroller [@microcontroller-definition] zu erklären, muss erst ihr Unterschied zu Mikroprozessoren beleuchtet werden. Mikroprozessoren sind so wie die "größeren" Prozessoren - in diesem Fall sowohl auf physikalische Größe als auch auf Leistung bezogen - die Recheneinheit eines Systems. Während Mikroprozessoren ausschließlich rechnerische Aufgaben bearbeiten und für andere Funktionen separate Teile (RAM, ROM, Timer-Module, etc.) benötigen, fungieren Mikrocontroller als ein fertiges Paket, welches die verschiedenen Teile verbaut hat.

![Mikroprozessor vs. Mikrocontroller](img/Schaar/Difference_Microprocessor_Microcontroller.png)

Wie in Abbildung x [@microcontroller-microprocessor] ersichtlich, ist links der Mikroprozessor bzw. die CPU (Central Processing Unit) zentral, benötigt jedoch extern verbundene Teile, sodass alle Aufgaben ausgeführt werden können. Ihm gegenübergestellt wird der Mikrocontroller, bei dem veranschaulicht wird, dass es sich hierbei um ein fertiges System handelt, bei dem alle Teile in diesem Ökosystem fest verbaut sind. Um eine Analogie zum menschlichen Körper zu bilden wäre in diesem Fall der Mikroprozessor das Gehirn alleine und der Mikrocontroller ein vollständiger Körper, welcher zur Funktion auf keine zusätzlichen Hilfsmittel angewiesen ist.

Tendenziell sind Mikroprozessoren für leistungsintensivere Anwendungszwecke angedacht als Mikrocontroller (z. B. grafische Berechnungen, Multitasking). Mikrocontroller sind durch ihre autonome Struktur in puncto Kosten, Stromverbrauch und Größe beliebt und bringen viele Möglichkeiten mit ohne unnötig hohe Komplexität. Aufgrund von diesen Faktoren werden Mikrocontroller sehr gerne in eingebetteten Systemen (Embedded Systems) [@embedded-systems] verwendet. 

Dieser Begriff bezeichnet Systeme, welche für einen speziellen Zweck entwickelt und optimiert sind, im Gegensatz zu herkömmlichen Mikroprozessoren, welche für viele Anwendungszwecke existieren. Diese Systeme bilden sich aus einem Zusammenhang zwischen Software und Hardware und können eigenständig verwendet werden, wobei sie jedoch meist eher als Teil eines größeren Systems eine Aufgabe erledigen, wie z. B. in einem Auto oder einer Spielekonsole. Den Namen haben die Embedded Systems genau dieser Eigenschaft der "Einbettung" in andere Systeme zu verdanken.

Mikrocontroller sind in den verschiedensten Systemen zu zahlreichen Zwecken verbaut und erfüllen in jedem eine eigene Aufgabe. Heute gibt es Mikrocontroller in jeder erdenklichen Ausführung, weshalb sie für jegliche Anwendungszwecke von Hobbyprojekten bis zu Raketenwissenschaften nicht mehr wegzudenken sind.

#### Auswahlkriterien für Mikrocontroller

Um die Leistung der verschiedenen Mikrocontroller vergleichbar zu machen, müssen vorerst Kriterien festgelegt werden, anhand von denen die verschiedenen Optionen verglichen werden. Diese sollten bestmöglich messbar und objektiv vergleichbar sein, um eine gute Basis für die Auswahl zu schaffen. Mithilfe dieser Kriterien werden Punkte vergeben, anhand von denen eine Wahl getroffen wird.

Eine gut vergleichbare Eigenschaft sind die Kosten, bei denen eine Reduktion sowohl für das Entwicklerteam als auch für Endbenutzer, welche das Projekt zu Hause replizieren wollen, vorteilhaft ist. Um die Kosten in direkter Relation zu vergleichen, wird die Formel $Punkte = min(20, ceil(\frac{100}{Kosten}))$ zur Bewertung verwendet. Mit ihr wird 100 durch die Kosten dividiert und das Ergebnis gerundet, sodass eine Punktezahl herauskommt. Danach wird dieser Wert auf maximal 20 gesetzt, sodass nicht mehr Punkte möglich sind, auch wenn niedrigere Kosten bestehen. Dieser Ansatz wurde gewählt, da er simpel zu berechnen ist und Kostenunterschiede in der Punktzahl sichtbar macht.

Für ein angenehmes Spielerlebnis ist eine schnelle und zuverlässige Übertragung der Eingaben wichtig. Daher werden die Prozessorleistung zur Verarbeitung der Eingaben sowie die Übertragungszeiten für den Austausch der Daten als weitere Vergleichsbasis verwendet.

Damit nachhaltige Hardwareentwicklung mit einem Fokus auf Kostenminimierung ermöglicht wird, muss ein Augenmerk auf den Stromverbrauch der Mikrocontroller gelegt werden. Aufgrund von geringen Spannungsgrößen wird diesem Verbrauch in der endgültigen Entscheidung ein im Vergleich zu den anderen Eigenschaften kleinerer Stellenwert beigemessen.

Für die Gesamtbewertung werden vorerst für die Kosten Punkte zwischen 1 und 30 vergeben, welche anhand von der oben erklärten Formel errechnet werden. Die Punkte der anderen Kategorien werden relativ zu den anderen Optionen vergeben - Platz 1, 2 und 3 erhalten so 9, 6 und 3 Punkte (Kategorie Stromverbrauch: 3, 2, 1 Punkte) - und danach zur finalen Punkteanzahl addiert.

![Punkteverteilung Mikrocontroller](img/Schaar/PointWeightsMicrocontrollers.png)

In Abbildung x (erstellt in Microsoft Excel) sind die Anteile der Bewertungskategorien an der finalen Punktezahl visuell dargestellt. Wie man erkennt, machen die Kosten hierbei den größten Anteil aus, während die Eigenschaften, welche sich auf die Übertragung der Eingaben beziehen, mit jeweils 22 % auch eine relativ hohe Gewichtung besitzen. Da wie bereits erwähnt die Stromversorgung vernachlässigbar ist, macht sie mit 7 % keinen allzu großen Anteil an der Gesamtpunktzahl aus. Bei diesen Anteilen wird für alle Kategorien vom theoretisch maximalen Ergebnis an Punkten ausgegangen (41 Punkte bei perfekter Bewertung in allen Kategorien).

### Arduino Nano ESP32

![Arduino Nano ESP32](img/Schaar/Arduino-Nano-ESP32-IRL.png)

Der erste Mikrocontroller, welcher im Zuge dieser Arbeit bewertet wird, ist der Arduino Nano ESP32 (siehe Abbildung x [@arduino-nano-esp32-image]). Er ist ein Controller mit überschaubarer Komplexität, bietet jedoch trotzdem zahlreiche Funktionen für alle möglichen Zwecke, welche dank offiziellen Dokumentationen und Tutorials verständlich gemacht werden [@arduino-docs]. Wie schon am Namen ersichtlich beinhaltet dieser Controller auch einen ESP32, gleich wie das ESP-32 DevKit C, ein anderer behandelter Controller in dieser Arbeit, jedoch ist hierbei ein ESP32-S3 verbaut und kein ESP32-WROOM-32. Zum DevKit C bestehen auch Unterschiede, was technische Daten sowie den Kostenpunkt angeht. Im Detail folgen diese Unterschiede in der Auflistung der technischen Daten.

Während die primäre Programmiersprache des Arduino Nano ESP32 zwar eine simplifizierte Version von C bzw. C++ (Arduino Language) ist, gibt es bei dieser Ausführung des Nano die Möglichkeit, in MicroPython [@micropython] zu programmieren, da diese Sprache auf allen ESP32-basierten Controllern funktioniert. Bei MicroPython handelt es sich ähnlich wie bei der Arduino Language selbst um eine schlankere Version und effizientere Version von Python. Sie hat einen Teil der Funktionalitäten der Python Standard Library und ist für die Verwendung auf Mikrocontrollern optimiert. Die genauen Unterschiede zum standardmäßigen CPython sind zahlreich und reichen von Syntax-Unterschieden bis zu entfernten Core-Funktionen.

Beispiel:  Einzelne Array-Elemente können nicht wie in CPython gelöscht werden.

```{caption="Beispiel für Unterschied zwischen Micro- und CPython - Array" .py}
a = array.array("b", (1, 2, 3))
del a[1]
```

Während dieser Ausdruck in CPython zu einer Löschung des Elements am Index 1 geführt hätte bekommt man in MicroPython einen Error.

```{caption="Error-Message | Beispiel für Unterschied zwischen Micro- und CPython - Array" .txt}
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

Alle hier tabellarisch angegebenen Spezifikationen stammen aus dem offiziellen Arduino-Nano-ESP32-Datenblatt [@arduino-nano-datasheet]. Die Daten beziehen sich hierbei auf einen Teil der zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden. Für die exakten Daten, die für die Entscheidungen herangenommen werden, wird auf die Datenblätter im Anhang dieser Arbeit verwiesen.

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

#### ESP-NOW-Protokoll

Bei ESP-Now handelt es sich um ein eigens entwickeltes Protokoll von Espressif Systems, welches es mehreren Geräten erlaubt, sich untereinander zu verständigen. Das Protokoll [@esp-now-protocol] [@esp-now-introduction] ist auf dem 802.11 Wi-Fi Standard basiert, benötigt jedoch keinen zusätzlichen Access Point für die Kommunikation. Die maximale "Payload-Größe" beträgt hierbei 250 Bytes, wobei die Datenübertragung jedoch sinngemäß schneller ist, je kleiner die Payload ist.

TechTerms.com, eine Website für so gut wie alle technischen Begriffe rund um IT, definiert den Begriff Payload [@payload-definition] wie folgt:

> The term "payload" in computing terms can mean several different things. 
> 1) In computer networking, a payload is the part of a data packet containing the transmitted data.
>
> 2) In computer security, a payload is the part of a computer virus or other malware containing the code that carries out the virus's harmful activity.

Payload hat in dem hier verwendeten Kontext jedoch nichts mit der zweiten Definition zu tun, sondern bezieht sich auf die tatsächliche Datengröße versendeter Pakete.

> A payload is the part of a protocol data unit (PDU) that contains the transmitted data or message. When one device sends data over a network, it needs to combine that data with a header into a packet.

Der "Header" einer Dateneinheit beinhaltet Daten über die Herkunft bzw. das Ziel dieser sowie die Reihenfolge, in der die versendeten Pakete rekonstruiert werden müssen. Bei ihm geht es also nur um Informationen zur Route einer PDU und nicht um ihren Inhalt.

Da die maximale Größe dieser Payload 250 Bytes beträgt, wird dieses Protokoll nicht für die Übertragung größerer Dateimengen verwendet, eignet sich aber sehr gut für das Senden kleinerer Daten, wie z. B. von Sensoren. Aus diesem Grund eignet sich das Protokoll für die Realisierung dieser Arbeit gut, da hierbei nur kleine Daten zu Dreh- und Schiebebewegungen versendet werden müssen.

ESP-Now ermöglicht Master-Slave-Beziehungen zwischen Boards, für Kommunikation in eine Richtung, aber auch Kommunikation zwischen zwei oder mehreren Boards in beide Richtungen.

![ESP-Now Kommunikation](img/Schaar/ESP-Now-Connections.png)

In Abbildung x [@esp-now-introduction] wird die Kommunikation mehrerer ESP-32-Controller in beide Richtungen dargestellt. Dies ist zwar nur eine simple Visualisierung, zeigt jedoch dass die Verbindung vieler Mikrocontroller dadurch einfach ermöglicht wird. Dadurch ergeben sich zahlreiche Möglichkeiten für zusammenhängende Systeme, wie z. B. die Messung und Übermittlung verschiedener Sensordaten in einem Smart Home.

Hier folgen nun beispielhafte Code-Snippets [@esp-now-introduction] zur Realisierung des ESP-Now-Protokolls (One-Way-Form) in C-Code, so wie er auf einem Mikrocontroller laufen würde:

```{caption="Beispiel ESP-Now One Way Kommunikation - Ermittlung der MAC-Adresse" .c}
#include "WiFi.h"

void setup(){
 Serial.begin(115200);
 WiFi.mode(WIFI_MODE_STA);
 Serial.println(WiFi.macAddress());
}

void loop(){}
```

Mit diesem Code wird mithilfe der WiFi-Library die MAC-Adresse des Empfänger-Gerätes ermittelt, welche im folgenden Code verwendet wird. Gehen wir nun beispielhaft davon aus, dass hierbei die Adresse **66:94:6B:59:97:35** herauskommt. Diese Adresse wird nun zur weiteren Verwendung im folgenden Code in ein Array gespeichert.

```{caption="Beispiel ESP-Now One Way Kommunikation - Sender-Code" .c}
#include <esp_now.h>
#include <WiFi.h>

/* 
  Die hier eingetragenen Werte entsprechen der
  ermittelten MAC-Adresse aus dem vorherigen Snippet
*/
uint8_t broadcastAddress[] = {0x66, 0x94, 0x6B, 0x59, 0x97, 0x35};

char msg[] = "Hello World!";

esp_now_peer_info_t peerInfo;

void OnDataSent(const uint8_t *mac_addr, esp_now_send_status_t status) {
  Serial.print("\r\nDelivery Status: ");
  Serial.println(status == ESP_NOW_SEND_SUCCESS ? "Delivered Successfully" : "Delivery Fail");
}
 
void setup() {
  Serial.begin(115200);
  WiFi.mode(WIFI_STA);
  if (esp_now_init() != ESP_OK) {
    Serial.println("Error initializing ESP-NOW");
    return;
  }

  esp_now_register_send_cb(OnDataSent);
  memcpy(peerInfo.peer_addr, broadcastAddress, 6);
  peerInfo.channel = 0; 
  peerInfo.encrypt = false;
         
  if (esp_now_add_peer(&peerInfo) != ESP_OK){
    Serial.println("Failed to add peer");
    return;
  }
}
 
void loop() {
  esp_err_t result = esp_now_send(broadcastAddress, (uint8_t *) &msg, sizeof(msg));

  if (result == ESP_OK) {
    Serial.println("Sent Successfully");
  }
  else {
    Serial.println("Error while sending data");
  }
  delay(1000);
}
```

Dieser Code wird auf dem Mikrocontroller aufgerufen, der die Message - in diesem Fall ein ganz simples "Hello World!" - im Sekundentakt versendet. Hierbei werden die Daten verschickt und es wird eine Erfolgs- bzw. Fehlernachricht je nach Ergebnis ausgegeben. In der ```esp_now_peer_info_t```-Variable werden die Informationen zu "Peers" - bezeichnet die zusammenhängenden Mikrocontroller, welche die versendeten Informationen erhalten - gespeichert.

```{caption="Beispiel ESP-Now One Way Kommunikation - Empfänger-Code" .c}
#include <esp_now.h>
#include <WiFi.h>

typedef struct struct_message {
    char a[32];
} struct_message;

struct_message myData;

void OnDataRecv(const uint8_t * mac, const uint8_t *incomingData, int len) {
  memcpy(&myData, incomingData, sizeof(myData));
  Serial.println(myData.a);
}


void setup() {
  Serial.begin(115200);
  WiFi.mode(WIFI_STA);
  if (esp_now_init() != ESP_OK) {
    Serial.println("Error initializing ESP-NOW");
    return;
  }
  esp_now_register_recv_cb(OnDataRecv);
}
 
void loop() {}
```

Dieses kurze Snippet ist alles, was auf der Seite des Empfängers laufen muss, damit dieser die Nachrichten vom Sender empfangen kann. Zuerst wird eine Datenstruktur mit ```struct_message myData;``` erstellt, in der die empfangenen Daten gespeichert werden. Sobald per ESP-Now Informationen eingelangen wird die ```void OnDataRecv(const uint8_t * mac, const uint8_t *incomingData, int len)```-Methode aufgerufen, welche zuerst die eingehenden Daten in die erstellte Variable speichert und diese danach ausgibt.

Auch wenn dieses kurze Beispiel keine komplexe Aufgabe erledigt, wird trotzdem veranschaulicht, dass es nicht sehr aufwendig ist mithilfe von ESP-Now Daten zwischen Mikrocontrollern zu versenden.


#### Firma - Espressif Systems

Das ESP-32 DevKit C ist Teil der großen ESP32-Familie an Mikrocontrollern, welche von Espressif Systems [@espressif-about] entwickelt wird. Die Firma arbeitet an innovativen Lösungen rund um IoT und arbeitet in den letzten Jahren mehr und mehr an KI-Integration für Mikrocontroller. Sie veröffentlichen ihre Ressourcen und Modelle Open-Source und tragen damit maßgeblich zur globalen Weiterentwicklung von Mikrocontrollern, Embedded Systems und IoT bei. Ihre Produktlösungen umfassen Smart Home Systeme, IoT Security, Machine Vision, Deep Learning, Speech AI und viele weitere Anwendungen. 

Teo Swee Ann hat die Firma im Jahr 2008 in Shanghai gegründet und ist bis heute der CEO bei Espressif Systems. 2016 wird dann der originale ESP32-Mikrocontroller veröffentlicht, welcher durch seine WLAN- & Bluetooth-Kapazitäten revolutionär ist. Seither wurden der Familie allerlei neue und spezialisierte Mikrocontroller hinzugefügt, wodurch für Projekte verschiedenster Größenordnungen Optionen existieren.

Espressif Systems bemühen sich durch verschiedene Projekte umweltfreundlicher zu sein. Durch konstante Forschung versuchen sie den Energieverbrauch ihrer Produkte sowie den produktionsbedingten Materialverlust zu minimieren. Außerdem haben sie ein Wildlife Protection Programm gestartet, um die Leben von bedrohten Tierarten zu wahren.

#### Technische Daten

Alle hier tabellarisch angegebenen Spezifikationen kommen aus dem offiziellen ESP32-WROOM-32-Datenblatt [@esp32-datasheet]. Die Daten beziehen sich hierbei auf einen Teil der zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden. Für die exakten Daten, die für die Entscheidungen herangenommen werden, wird auf die Datenblätter im Anhang dieser Arbeit verwiesen.

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

Der letzte Mikrocontroller, der in dieser Arbeit betrachtet wird, ist der Raspberry Pi Pico 2W [@raspberry-pi-pico-documentation] (siehe Abbildung x [@raspberry-pi-pico-2w-image]). Dieser Controller verwendet keinen ESP-32 basierten Chip für kabellose Datenübertragung, sondern einen Infineon CYW43439 in Kombination mit einer ABRACON-lizensierten Antenne.

Für die Programmierung eines Pi Pico kann sowohl C/C++ als auch MicroPython verwendet werden, gleich wie bei den vorherigen Mikrocontrollern auch. Während bei den anderen beiden jedoch vorwiegend C/C++ verwendet wird, werden Programme auf dem Pi Pico primär in MicroPython geschrieben, was vorwiegend daran liegt, dass die Programmiersprache nativ auf dem Controller läuft. Für C/C++ ist eine robuste SDK verfügbar, die eher für erfahrenere Entwickler gedacht ist, aber trotzdem alle Funktionen des Pico nutzbar macht.

![Raspberry Pi Code Club](img/Schaar/Raspberry-Pi-Code-Club.png)

Da (Micro-)Python als Programmiersprache weitaus einsteigerfreundlicher als andere Sprachen ist, wird der Raspberry Pi Pico gerne für den Einstieg in die Hardware-Programmierung genommen. Dafür gibt es auf der offiziellen Raspberry Pi Code Club Seite [@raspberry-pi-code-club] viele Projekte, welche wichtige Kernkonzepte der Low-Level-Programmierung erklären (siehe Abbildung x [@raspberry-pi-code-club]).

Dank der ```gpiozero```/```picozero```-Library [@gpiozero-library] [@picozero-library] - ```picozero``` ist hierbei eine abgeänderte Version der ```gpiozero```-Library, welche ihre grundlegende Struktur wiederverwendet, aber für den Raspberry Pi Pico optimiert ist - wird das Interfacing verschiedener GPIO-Geräte mit dem Raspberry Pi Pico ein leichtes. Diese Library hat viele Funktionen für die verschiedensten Input-/Output-Teile, welche die Nutzung dieser auch für unerfahrene Programmierer:innen ermöglichen und bei der Entwicklung viel Code ersparen.

```{caption="LED steuern mithilfe von picozero-Library" .py}
from picozero import RGBLED
from time import sleep

rgb = RGBLED(red=1, green=2, blue=3) # Pin numbers

def pop():
    rgb.color = (255, 0, 255) # Purple
    sleep(2)
    rgb.off()

pop() # Call the pop function
```

Dieses simple Beispiel von der Raspberry Pi Code Club Website [@raspberry-pi-code-club] zeigt, wie einfach es ist, mithilfe der Library eine LED in der Farbe lila zum Leuchten zu bringen. Man muss nur eine Variable als LED initialisieren, indem man die dazugehörigen Pins für jede Farbe angibt und daraufhin ihre Farbe eingeben und sie wieder ausschalten.

```{caption="Steuerung Bewegungssensor mit picozero-Library" .py}
from picozero import MotionSensor

pir = MotionSensor(4)

pir.wait_for_motion()
print("Motion detected!")
```

Das vorherige Beispiel war für eine LED, also ein Output-Bauteil, während dieses für einen Bewegungsmelder ist, der auf ein Signal wartet. Da dieser Code ohne Leerzeilen nur 4 Zeilen lang ist, wird klar, dass mit dieser Library viele Projekte ohne allzu viel eigene Tüftelarbeit realisierbar sind. Bei diesem Beispiel wird nur der Pin des Sensors initialisiert und danach eine Methode aufgerufen, die das Programm stoppt, bis eine Bewegung wahrgenommen wird und danach eine Nachricht ausgibt.

```{caption="Verwendung eines Ultraschallsensors in C (Arduino Language)" .c}
const int trigPin = 17;
const int echoPin = 18;

double duration, cm;

void setup(void) {
  Serial.begin(9600);

  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
}

void loop() {
  digitalWrite(trigPin, LOW);
  delayMicroseconds(5);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
 
  pinMode(echoPin, INPUT);
  duration = pulseIn(echoPin, HIGH);

  cm = (duration/2) / 29.1;

  Serial.print("Distance: ");
  Serial.println(cm);
  
  delay(1000);
}
```

```{caption="Verwendung eines Ultraschallsensors in MicroPython mit picozero" .py}
from picozero import DistanceSensor
from time import sleep

sensor = DistanceSensor(echo=18, trigger=17)
while True:
    print('Distance: ', sensor.distance * 100)
    sleep(1)
```

Diese beiden Code-Segmente erfüllen dieselbe Funktion, während das erste in C bzw. Arduino Language - wie sie z. B. auf einem Arduino Nano ESP32 laufen würde - und das zweite in MicroPython mit der ```picozero```-Library geschrieben ist. Direkt erkennbar ist, dass hierbei bei dem MicroPython-Code für den/die Programmierer:in weitaus weniger zu tun ist als beim C-Code. 

Natürlich gibt es für viele Sensoren auch in C Libraries welche einen großen Teil dieses zusätzlichen Aufwandes entfernen, jedoch ist keine so simple und weitreichende Library für die gleiche Menge an verschiedenen Sensoren verfügbar wie in MicroPython. Grundsätzlich ist es also persönliche Präferenz, in welcher Sprache programmiert wird, jedoch ist MicroPython in Kombination mit der ```picozero```-Library auf jeden Fall sehr viel einfacher in der Zurechtfindung.

#### Firma - Raspberry Pi

Gleich wie die anderen Geräte der Raspberry-Pi-Familie ist auch der Raspberry Pi Pico 2W ein Gerät von der Raspberry Pi Foundation [@raspberry-pi-about] [@raspberry-pi-story]. Die Raspberry Pi Foundation wurde 2012 gegründet, um einerseits neue und besser ausgebildete Schüler an die University of Cambridge zu bringen und andererseits um junge Menschen mithilfe von kostengünstiger Hardware an die Programmierung heranzuführen.

Konzipiert und entwickelt wurde der erste Raspberry Pi damals von einem Team an der University of Cambridge. Ein paar der treibenden Kräfte in diesem Team sind/waren Eben Upton, Robert Mullins und Alan Mycroft sowie zahlreiche weitere Entwickler. Für die Entwicklung des Computers stellte das Team damals 4 Anforderungen auf:

**1. Programmierbare Hardware**

**2. Spaß bei der Entwicklung**

**3. Leistbar für alle**

**4. Robustheit**

Für das Team war es von Anfang an wichtig, diese Anforderungen bei der Entwicklung einzuhalten. Auf dem Gerät sollte es möglich sein zu programmieren, während die Entwicklung für die Kinder und Jugendlichen auch noch Spaß machen sollte. Als Preis hatten sie von Anfang an 25 Dollar im Kopf, da dies für sie ein Betrag war, der für die meisten Familien tragbar wäre. Der vierte Punkt musste für sie auch erreicht werden, da der Raspberry Pi ein Gerät werden sollte, das unter anderem von Kindern jeden Tag verwendet werden würde, weshalb es stabil sein müsste.

14 Jahre später werden immer noch neue Geräte entwickelt und die Bemühungen der Foundation, die Entwicklung von Hardware- und Software-Projekten einfach zugänglich und verständlich zu machen, gehen bis heute international weiter. Nur in der UK wurden bereits über 26.000 Lehrer unterstützt und auf der ganzen Welt werden offizielle Projekte durchgeführt, um junge Menschen zu informieren.

#### Technische Daten

Alle hier tabellarisch angegebenen Spezifikationen kommen aus dem offiziellen Raspberry-Pi-Pico-2W-Datenblatt [@raspberry-pi-pico-datasheet]. Die Daten beziehen sich hierbei auf einen Teil der zu bewertenden Kategorien, welche für die Entscheidung eines Mikrocontrollers herangenommen werden. Für die exakten Daten, die für die Entscheidungen herangenommen werden, wird auf die Datenblätter im Anhang dieser Arbeit verwiesen.

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

In diesem Teil folgt der direkte Vergleich sowie die Entscheidung für eine der Mikrocontroller-Optionen. Jegliche Diagramme und Berechnungen, welche zur Veranschaulichung und Ermittlung der Daten verwendet werden, wurden mithilfe von Microsoft Excel erstellt. Die Daten für den Vergleich der Preise werden durch einen Vergleich von mehreren Händlerangeboten pro Mikrocontroller ermittelt. Für die exakten Berechnungen und Händler-Daten wird an dieser Stelle auf die Tabellenberechnungen im Anhang verwiesen.

![Punkteverteilung - Mikrocontroller](img/Schaar/PointDistributionMicrocontrollers.png)

Die Punkteverteilung, wie sie in Abbildung x (siehe Anhang für Berechnungen) ersichtlich ist, ergibt sich aus:

**1. Kosten (~49% der möglichen Gesamtpunkte)**

**2. Prozessorleistung (~22% der möglichen Gesamtpunkte)**

**3. Datenübertragung (~22% der möglichen Gesamtpunkte)**

**4. Stromverbrauch (~7% der möglichen Gesamtpunkte)**

Für den genauen Vergleich der Prozessorleistung, Datenübertragung und dem Stromverbrauch wird hierbei wieder auf die Datenblätter im Anhang verwiesen.

Die durchschnittlichen Kosten (in €) und die sich daraus ergebenden Punkte jedes Mikrocontrollers belaufen sich nach dem Vergleich verschiedener Online-Händler auf die folgenden Beträge:

| Mikrocontroller | Preis H1 | Preis H2 | Preis H3 | **Durchschnittspreis** | **Punkte** |
|--|--|--|--|--|--|
| Arduino Nano ESP32 | 21,60€ | 16,60€ | 19,97€ | **19,39€** | **6** |
| ESP32 DevKit C V4 | 13,00€ | 13,99€ | 11,90€ | **12,96€** | **8** |
| Raspberry Pi Pico 2W | 7,60€ | 8,00€ | 8,57€ | **8,06€** | **13** |

: Kostenvergleich Mikrocontroller

![Gesamtpunktzahl - Mikrocontroller](img/Schaar/PointsSumMicrocontrollers.png)

Nach dem Addieren aller Teilpunktzahlen der Kategorien ergibt sich für die Gesamtpunktzahl und die finale Entscheidung dieses Ergebnis. Den ersten Platz belegt mit einem klaren Vorsprung der **Raspberry Pi Pico 2W** weshalb er für die finale Entscheidung für einen Mikrocontroller für den Bau eines Controllers ausgewählt wird. Das **ESP32 DevKit C** belegt den zweiten Platz, während der **Arduino Nano ESP32** insgesamt am schlechtesten abschneidet.

Der Raspberry Pi Pico 2W ist nicht nur anhand von diesem faktischen Vergleich für die Entwicklung sehr angenehm, sondern auch wegen der umfassenden Code-Library ```picozero```, welche die Entwicklung für diesen stark vereinfacht. Hiermit endet das Subkapitel über die Entscheidung für den Mikrocontroller, der für diese Diplomarbeit am Besten geeignet ist. Als nächstes werden die möglichen Sensoren zur Messung der Eingabesignale genauer beleuchtet.

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

## Praktischer Teil

Kurzbeschreibung

### Prototyping

In diesem Kapitel geht es um Unverwendete Prototypen und Ansätze, Demos und andere Entwicklungsschritte, welche sich im Laufe der Arbeit am Controller ergeben haben. Anhand von ihnen werden der Entwicklungsprozess und verschiedene Iterationen dargestellt.

#### Arduino <-> Godot Kommunikations-Demo

Für die Erstpräsentation unserer Arbeit haben wir eine Demo erstellt, anhand von der die Kommunikation zwischen einem Arduino und der Godot Engine veranschaulicht wird. Die Grundstruktur dieser Demo wurde aus einem Youtube-Video genommen [@connect-godot-arduino], in dem die serielle Übertragung der Daten erklärt und beispielhaft dargestellt wird.

Am Arduino werden die Signale von einem MPU6050-Sensor eingelesen und über die Serielle Schnittstelle übertragen.

```{caption="Kommunikations-Demo Arduino <-> Godot - Arduino-Code" .c}
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

```{caption="Kommunikations-Demo Arduino <-> Godot - C-Sharp-Code" .cs}
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

### Erstellung der Nachbauanleitung

Herangehensweise der Erstellung, Open-Source-Bereitstellung der Anleitung, Nach Fertigstellung der Praxis

