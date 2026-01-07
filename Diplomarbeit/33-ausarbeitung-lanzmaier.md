# Teilaufgabe Schüler Lanzmaier
\textauthor{Luan Lanzmaier}

## Theorie

Kurzbeschreibung

### Auswahl eines Verbindungsmodells

Kriterien

#### P2P

Verwendung eines eigenen Servers nicht benötigt, aber Skalierbarkeit auf mehrere Teilnehmer immer schwieriger, da die einzelnen Netzwerkgeschwindigkeiten immer auf den Teilnehmer mit der geringsten Leistung "gedrosselt" werden => Spielfluss kann von einem/mehreren Teilnehmern stark verlangsamt werden.

#### Dedicated Server

Zusätzliche Serverkosten, dafür aber stabilerer Spielfluss, da er nicht vom Netzwerk einzelner abhängig ist.

### Entscheidungen für Multiplayer-Spielprinzipien

Jegliche Entscheidungen (und deren Hintergründe) hinsichtlich des Designs der Spielprinzipien für den Mehrspieler-Modus (eigene Minispiele, verschiedene Modi, etc.)

## Praktische Arbeit

Kurzbeschreibung

### Prototyping

Entwickelte Lösungsansätze für die Verbindung mehrerer Clients, Messung der verschiedenen Latenzen und Delays, Proof-of-Concept-Demos, etc.

### Modellierung/Design der Assets

Für alle Komponenten in unserer Tischfußball-Simulation werden selbst erstellte Assets verwendet.

Die Grafiksoftware "Blender" eignet sich für unser Szenario am besten, da sie als weltweiter "Standard" für Graphic Design bekannt ist und eine große Palette von Design-Funktionen mit sich bringt. Durch die große Community sind neue Lösungsansätze sowie Ideen schnell im Internet zu finden.

### Für die Simulation sind folgende zentrale Assets nötig:

#### Spielfiguren

#### Tischplatte

#### Fußball

### Anforderungen an die 3D-Assets

Für die Tischfußball-Simulation müssen die erstellten Assets sowohl optischen als auch technischen Anforderungen gerecht werden. Da die Assets in einer interaktiven Simulation verwendet werden, ist eine Balance zwischen realistischer und performanter Darstellung notwendig.

### Zu den grundlegenden Anforderungen zählen:

### Designentscheidungen & Stilkonzept

Unser Ziel war von Beginn an, einen einheitlichen Stil aller Assets zu bewahren. Zuerst war geplant, dass wir alle Assets von einer realistischen Referenz nachgestalten. Das Problem hierbei war jedoch, dass reines nachbauen bereits existierender Gegenstände etwas langweilig ist. Wir haben uns schlussendlich für eine Mischung aus Realismus und Comic-Stil entschieden, um unsere Kreativität beim Designen freien Lauf zu lassen.

### Geringe Polygonanzahl zur Sicherstellung der Performance

In unserer Simulation ist Echtzeit-Performance extrem wichtig. Um das zu erreichen, darf die erforderliche Rechenleistung nicht zu hoch sein und das Rendern der Assets sollte während der Simulation sehr schnell erfolgen. Aus diesem Grund werden unsere Game-Assets so optimiert, dass ihre Polygonanzahl so gering wie nur möglich ist.

### Korrekte Skalierung im Verhältnis zueinander

Damit die Simulation gut funktioniert, müssen die einzelnen Komponenten im Größenverhältnis zueinander passen.

### Kompatibilität mit der verwendeten Game-Engine

Unsere grafischen Komponenten werden vor der Fertigstellung häufig in der Godot-Engine getestet, um die Funktionalität sicherzustellen. Wenn es beispielsweise Probleme mit den Hitboxen gibt oder das Asset in der Engine nicht so aussieht wie erwünscht, werden Änderungen vorgenommen.

### Allgemeiner Workflow zur Asset-Erstellung

### Referenzanalyse

Zur realitätsnahen Umsetzung wurden reale Tischfußballtische sowie Spielfiguren analysiert. Dabei wurden insbesondere Proportionen, Farben und charakteristische Merkmale berücksichtigt.

### Grundmodellierung

Zu Beginn wird ein Grundmodell mit einer geringen Polygonanzahl erstellt, welches mit Sculpt-Tools so angerichtet wird, dass es der Form des Referenzmodells in etwa entspricht.

### Detailmodellierung

Durch den Einsatz des Subdivision Surface-Modifiers wird die Polygonanzahl des Grundmodells erhöht, wodurch eine feinere Geometrie entsteht. Diese bildet die Grundlage für die anschließende Detailausarbeitung mithilfe der Sculpting-Werkzeuge.

### Retopology

Um die Polygonanzahl zu minimieren, wird eine Low-Poly Kopie des bereits bestehenden Detailmodells erstellt. Bei sehr detailreichen Meshes (wie z.B. beim Kopf der Spielfigur) wird das Low-Poly Modell per Hand erstellt, damit keine wichtigen Details verloren gehen. Bei Komponenten mit wenigen Details reicht für die Retopology der Decimate Modifier.

### UV-Unwrapping

Damit man eine Mesh texturieren und Maps (Normal, Diffuse, Emition etc.) verwenden kann, muss man sie zuvor "unwrappen". Dies kann man entweder per Hand oder wie in unserem Fall mit dem Smart UV Project machen.

![Bild des Unwrapped Characters](image.png)

### Texturierung

Wenn externe Texturen benötigt werden, kann man sie jetzt mit einer Image-Texture-Node im Shader Editor hinzufügen.
Damit unsere Assets game-ready sind verwenden wir folgende Texture Maps:

#### Normal Map

Mit einer Normal Map können wir echte Oberflächendetails erzeugen, um den realistischen Look unseres Characters zu verstärken. Die geometrische Komplexität erhöht sich hierbei nicht und die Anzahl der Polygone bleiben gleich.

![Normal Map des Charakters](CharacterNormal.png)

#### Diffuse Map

Mit einer Diffuse Map können wir die Base Color aller benutzten Materialien auf eine UV-Map projizieren. Statt für jede Farbe ein eigenes Material zu benutzen, können wir so ein Material mit allen benötigten Farben erstellen. Je weniger Materialien verwendet werden, desto geringer ist die Anzahl der Draw Calls, da jedes zusätzliche Material einen separaten Renderaufruf erfordert. Dadurch wird der Kommunikationsaufwand mit der Grafik-API reduziert, was die Performance verbessert.

![Diffuse Map des Charakters](CharacterColor.png)

#### Emission Map

Eine Emissive Map ist eine Textur, die in der 3D-Grafik bestimmt, welche Bereiche eines Modells selbst Licht aussenden, unabhängig von der Szene-Beleuchtung.
Weiße/helle Bereiche - leuchten stark.
Schwarze/dunkle Bereiche - senden kein Licht aus.
Das ist wichtig bei Materialien die beispielsweise unterschiedliche Alpha bzw. Metallic Values besitzen.

![Emission Map der Tischplatte](PlateInv-1.png)

### Ausarbeitung der einzelnen Game-Assets

### Spielfigur

### Funktion

Die Spielfigur ist das zentrale, visuelle Element. Sie sollte optisch gut umgesetzt sein aber auch während der Simulation ihren Zweck erfüllen können.

### Design- und Stilentscheidung

Das Spielfiguren-Asset sollte oberflächlich als "Tischfußballmännchen" wiedererkennbar sein. Nichtsdestotrotz verwenden wir für die Details keine Referenzen, sondern designen ihn nach unseren eigenen Vorstellungen.

### Modellierung

Bei der Modellierung fangen wir beim Kopf an. Die erste Aufgabe ist es eine neue Cube-Mesh mit "Shift+A" zu erstellen. Auf die Mesh wird ein Subdivision Surface Modifier verwendet, um die Polygon-Anzahl ein wenig zu erhöhen.

### Optisches Design/Arrangement des Spiels

Design der Assets im Spiel, Stilisierte Darstellung und Design-Iterationen

### Fine-Tuning der Assets

Änderungen für die finale Version, etc.

### Finalisiertes Spiel
