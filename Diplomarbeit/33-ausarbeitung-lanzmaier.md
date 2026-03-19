# Teilaufgabe Schüler Lanzmaier
\textauthor{Luan Lanzmaier}

## Theoretischer Teil

Im Theorieteil werden die Grundlagen für die grafische Umsetzung der Tischfußball‑Simulation sowie die Multiplayer‑Grundlagen beschrieben. Dazu zählen die Auswahl von Verbindungsmodellen, Entscheidungen für Multiplayer‑Spielprinzipien, der Unterschied zwischen Offline‑ und Echtzeit‑Rendering sowie zentrale Optimierungsprinzipien für Game‑Assets (High‑/Low‑Poly, Normal Maps, Retopology und Reduktion von Draw Calls).

### Auswahl eines Verbindungsmodells

#### P2P
Verwendung eines eigenen Servers nicht erforderlich, aber Skalierbarkeit auf mehrere Teilnehmer immer schwieriger, da die einzelnen Netzwerkgeschwindigkeiten immer auf den Teilnehmer mit der geringsten Leistung "gedrosselt" werden => Spielfluss kann von einem/mehreren Teilnehmern stark verlangsamt werden.

#### Dedicated Server
Zusätzliche Serverkosten, dafür aber stabilerer Spielfluss, da er nicht vom Netzwerk einzelner Teilnehmer abhängig ist.

### Entscheidungen für Multiplayer-Spielprinzipien
Wegen der vielen Online-Tutorials und der Kompatibilität mit der Godot-Engine wurde für die Simulation die Lösung mit der WebRTC API gewählt. WebRTC ist eine Webtechnologie zur Echtzeitkommunikation (Real-Time Communication), die Peer-to-Peer-Verbindungen ermöglicht und unter anderem für Spiele genutzt werden kann. WebRTC nutzt standardmäßig das UDP-Protokoll, wodurch ein sehr schneller Datenaustausch möglich ist. [@godot-webrtc-docs]

*Folgende Arten von Datenaustausch sind möglich:*

* Reliable Messages mit hoher Latenz - Daten kommen immer zuverlässig an
* Unreliable Messages mit niedriger Latenz - Daten kommen nicht immer zuverlässig an
* Partially Reliable Messages mit mittlerer Latenz - Mittelweg zwischen den beiden anderen
[@snopek-webrtc-nakama]

#### ICE (Interactive Connectivity Establishment)
Die WebRTC API nutzt das ICE Framework, um Datenaustausch zwischen zwei Parteien sicherzustellen. ICE ermöglicht die direkte Verbindung zwischen zwei Parteien, bei denen mindestens eine der zwei Parteien hinter einem Router bzw. einer Firewall ist. Dies funktioniert mit einem sogenannten "STUN/TURN Server", der die NAT-Adressen beider Parteien kurzfristig speichert und diese dem jeweils anderen Peer freigibt. Wenn die direkte Verbindung fehlschlägt, wird der Datenaustausch auf den TURN Server ausgelegt. [@stun-and-turn-servers]

![ICE - Grafische Darstellung [@stun-and-turn-servers]](img/Lanzmaier/ICE.png)

### Unterschied Offline-Rendering vs Echtzeit-Rendering

#### Definition Rendering
Rendering ist im Grunde der Prozess, bei dem ein Computer 3D-Modelle in 2D-Bilder umwandelt, die man auf dem Bildschirm sieht. Grafikkarten (GPU) verwenden dafür ein Verfahren namens Rasterisierung. Dabei berechnet die GPU die Positionen der 3D-Objekte neu und berechnet dann noch Farben, Texturen und Schatten dazu, um ein fertiges Bild zu bekommen. 

Eine wichtige Rolle spielen dabei sogenannte Shaders – das sind kleine Programme, die festlegen, wie Licht, Reflektionen und Texturen aussehen. Es gibt auch noch fortgeschrittene Techniken wie Raytracing, die noch realistischere Ergebnisse liefern, indem sie den Weg von Lichtstrahlen nachbilden. Je nachdem, wofür man das Rendering braucht, muss man unterschiedliche Methoden wählen – manche Anwendungen brauchen schnelle Ergebnisse, andere können sich Zeit für bessere Qualität nehmen. [@rendering-definition]

#### Offline-Rendering (Vorrendern)
Offline-Rendering ist darauf ausgelegt, Bilder mit maximaler Qualität darzustellen. Durch die hohe Qualität dauert das Offline-Rendering länger. Ein Rechner kann sich beliebig viel Zeit nehmen, um ein einzelnes Bild zu berechnen – manchmal Stunden oder sogar Tage pro Frame. Dies ermöglicht eine extrem hohe visuelle Qualität mit maximalen Details, realistischen Lichtsimulationen und komplexen Effekten. Typische Anwendungsbereiche sind Filmproduktionen oder Animationen.
[@realtime-offline-rendering]

#### Echtzeit-Rendering
Echtzeit-Rendering bezeichnet die Berechnung und Darstellung von Bildern in der Interaktion mit einem Nutzer. Ein Computer muss dabei ein neues Bild in wenigen Millisekunden berechnen und anzeigen - typischerweise 30 bis 60 oder mehr Bilder pro Sekunde. Dies ist notwendig, damit eine Anwendung flüssig und reaktionsschnell wirkt. Beispiele dafür sind Computerspiele oder Virtual Reality.
[@realtime-offline-rendering]

#### Unterschiede und Auswirkungen auf die Asset-Erstellung
In Echtzeit-Anwendungen wie Computerspielen unterscheidet sich die Asset-Erstellung deshalb grundlegend von der für Offline-Renderings. Während bei vorgerenderten Szenen Rechenzeit eine untergeordnete Rolle spielt und maximale Detailgetreue priorisiert wird, müssen Spiel-Assets innerhalb weniger Millisekunden berechnet und dargestellt werden. Dies bedeutet, dass die visuellen Qualitätsansprüche den Performance-Anforderungen untergeordnet werden müssen. Daraus ergeben sich strenge Anforderungen an die Polygonanzahl (Geometrische Komplexität), die Materialanzahl (Draw Calls) und die Texturgröße – alle diese Faktoren beeinflussen direkt die Rechengeschwindigkeit der Engine und damit die erreichbare Bildrate (FPS).
[@realtime-offline-rendering]

#### High Poly vs. Low Poly
Bei der Erstellung von 3D-Assets für Spiele muss man immer einen Kompromiss zwischen visueller Qualität und Performance finden. High Poly Modelle haben viele Polygone und sehen sehr detailliert aus, brauchen aber viel Rechenleistung. Low-Poly-Modelle haben deutlich weniger Polygone und können schneller gerendert werden, sehen aber weniger detailliert aus. Da Echtzeit-Rendering wie in Spielen sehr schnell sein muss, kann man nicht einfach High Poly Modelle verwenden – das würde die Bildrate zu sehr senken.[@game-ready-3DModels]

Die Lösung ist, mit beiden Versionen zu arbeiten: Man erstellt zuerst ein High Poly Modell mit allen Details und berechnet davon eine Normal Map. Diese Normal Map wird dann auf ein Low-Poly-Modell angewendet. Für das Auge sieht das Low-Poly-Modell dann so aus, als wäre es High Poly, obwohl es viel weniger Polygone hat. Auf diese Weise kann man geometrische Details (Polygone) sparen und trotzdem visuell anspruchsvolle Ergebnisse erzielen. Dieser Prozess nennt sich Retopology und ist einer der wichtigsten Optimierungsschritte bei der Asset-Erstellung für Spiele.[@game-ready-3DModels]

#### Normal Mapping als Approximation
Normal Maps täuschen Details auf einer Oberfläche vor, ohne die Geometrie tatsächlich zu verändern. Mithilfe von Lichtrechnungen sieht ein einfaches Modell detailliert aus, obwohl es wenig Polygone hat. Das funktioniert, weil wir hauptsächlich Licht und Schatten wahrnehmen.

#### Retopology als Optimierungsstrategie
Retopology erstellt aus einem detaillierten High Poly Modell eine vereinfachte Low Poly Version mit deutlich weniger Polygonen. Kombiniert mit Normal Maps sieht das Ergebnis genauso gut aus, braucht aber viel weniger Rechenleistung. Das ist der Schlüssel zur Performance bei Game-Assets. [@blender-retopology]

#### Draw Calls und Material‑Reduktion
Jedes zusätzliche Material erzeugt in der Engine einen eigenen Render‑Aufruf. Durch das Zusammenführen mehrerer Materialien in ein einziges Material (Texture Atlas) reduzieren wir die sogenannten Draw Calls und steigern die Performance. Dieser Ansatz wird in der praktischen Umsetzung beim Backen der Maps direkt eingesetzt. [@blender-create-texture-atlas]

## Praktischer Teil
Im Praxisteil wird die Umsetzung der Assets und der Prototypen beschrieben. Dazu zählen die Modellierung in Blender, die Optimierung der Polygonanzahl, das Erstellen und Baken von Texture Maps sowie der Import und die Prüfung der Assets in der Godot‑Engine. Da der Umfang meiner Arbeit bereits den erforderlichen Rahmen abdeckte, wurde der Multiplayer-Modus überwiegend von Herrn Rath umgesetzt und implementiert, da er sich ohnehin intensiv mit den dafür relevanten Aspekten beschäftigte. Dieser Aspekt wird daher in dieser praktischen Ausarbeitung nicht behandelt.

### Modellierung/Design der Assets
Für alle Komponenten in der Tischfußball-Simulation werden selbst erstellte Assets verwendet.

Die Grafiksoftware "Blender" eignet sich für dieses Szenario am besten, da sie als als weit verbreitete Software für 3D-Modellierung und Rendering bekannt ist und eine große Palette von Design-Funktionen mit sich bringt. Durch die große Community sind neue Lösungsansätze sowie Ideen schnell im Internet zu finden.

Für die Simulation sind folgende zentrale Assets nötig:

#### Spielfiguren

#### Drehstäbe

#### Tischplatte

### Anforderungen an die 3D-Assets
Für die Tischfußball-Simulation müssen die erstellten Assets sowohl optischen als auch technischen Anforderungen gerecht werden. Da die Assets in einer interaktiven Simulation verwendet werden, ist eine Balance zwischen realistischer und performanter Darstellung notwendig.

### Zu den grundlegenden Anforderungen zählen:

### Designentscheidungen & Stilkonzept
Das Ziel war es von Beginn an, einen einheitlichen Stil für alle Assets zu bewahren. Zunächst war geplant, dass alle Assets anhand realistischer Referenzen nachgestaltet werden. Das Problem hierbei war jedoch, dass beim reinen Nachbauen bereits existierender Gegenstände die Innovation fehlte. Das endgültige Stilkonzept der Assets entwickelte sich daher zu einer Mischung aus Realismus und Comic-Stil.

### Geringe Polygonanzahl zur Sicherstellung der Performance
In der Simulation ist Echtzeit-Performance extrem wichtig. Um das zu erreichen, darf die erforderliche Rechenleistung nicht zu hoch sein und das Rendern der Assets sollte während der Simulation sehr schnell erfolgen. Aus diesem Grund werden die Game-Assets so optimiert, dass ihre Polygonanzahl so gering wie nur möglich ist.

*!Wichtig:* Die GPU rechnet mit Triangles. 1 Polygon = 2 Tris.
Da stark auf die Performance geachtet werden soll und das Ziel verfolgt wird, dass die Simulation auf jedem Rechner laufen kann, wurde als Richtwert eine maximale Gesamtanzahl von etwa 250.000 Triangles festgelegt. [@triangle-count-for-good-performance]

### Korrekte Skalierung im Verhältnis zueinander
Damit die Simulation gut funktioniert, müssen die einzelnen Komponenten im Größenverhältnis zueinander passen. Dies wird später in der Godot-Engine angepasst.

### Kompatibilität mit der verwendeten Game-Engine
Die grafischen Komponenten werden vor der Fertigstellung häufig in der Godot-Engine getestet, um die Funktionalität sicherzustellen und um visuelle Bugs zu vermeiden. Wenn es beispielsweise Probleme mit den Hitboxen gibt oder das Asset in der Engine nicht so aussieht wie erwünscht, werden Änderungen vorgenommen.

### Allgemeiner Workflow zur Erstellung des Character-Assets

### Referenzanalyse
Zur realitätsnahen Umsetzung wurden reale Spielfiguren analysiert. Dabei wurden insbesondere Proportionen, Farben und charakteristische Merkmale berücksichtigt.

### Grundmodellierung
Zu Beginn wird ein Grundmodell mit einer geringen Polygonanzahl erstellt, welches mit Sculpt-Tools so angerichtet wird, dass es der Form des Referenzmodells in etwa entspricht.

### Detailmodellierung
Durch den Einsatz des Subdivision Surface-Modifiers wird die Polygonanzahl des Grundmodells erhöht, wodurch eine feinere Geometrie entsteht. Diese bildet die Grundlage für die anschließende Detailausarbeitung mithilfe der Sculpting-Werkzeuge. [@blender-sculpting-tutorial]

### Retopology
Um die Polygonanzahl zu minimieren, wird eine Low-Poly Kopie des bereits bestehenden Detailmodells erstellt. Bei sehr detailreichen Meshes (wie z.B. beim Kopf der Spielfigur) wird das Low-Poly Modell per Hand erstellt, damit keine wichtigen Details verloren gehen. Bei Komponenten mit wenigen Details reicht für die Retopology der Decimate Modifier.

### UV-Unwrapping
Damit man eine Mesh texturieren und Maps (Normal, Diffuse, Emission etc.) verwenden kann, muss man sie zuvor "unwrappen". Dies kann man entweder per Hand oder wie in unserem Fall mit dem Smart UV Project machen.
[@blender-retopology]

### Texturierung
Wenn externe Texturen benötigt werden, kann man sie jetzt mit einer Image-Texture-Node im Shader Editor hinzufügen.
Damit unsere Assets game-ready sind verwenden wir folgende Texture Maps:

#### Normal Map
Mit einer Normal Map können wir echte Oberflächendetails erzeugen, um den realistischen Look unseres Characters zu verstärken. Die geometrische Komplexität erhöht sich hierbei nicht und die Anzahl der Polygone bleiben gleich.

#### Diffuse Map
Mit einer Diffuse Map können wir die Base Color aller benutzten Materialien auf eine UV-Map projizieren. Statt für jede Farbe ein eigenes Material zu benutzen, können wir so ein Material mit allen benötigten Farben erstellen. Je weniger Materialien verwendet werden, desto geringer ist die Anzahl der Draw Calls, da jedes zusätzliche Material einen separaten Renderaufruf erfordert. Dadurch wird der Kommunikationsaufwand mit der Grafik-API reduziert, was die Performance verbessert. [@blender-create-texture-atlas] 

### Ausarbeitung der einzelnen Game-Assets

### Spielfigur

### Funktion
Die Spielfigur ist das zentrale visuelle Element des Spiels und trägt maßgeblich zur Wiedererkennbarkeit sowie zur Atmosphäre der Simulation bei. Sie sollte daher nicht nur optisch ansprechend und stilistisch konsistent gestaltet sein, sondern auch funktional den Anforderungen des Spiels gerecht werden.

Während der Simulation übernimmt die Spielfigur eine aktive Rolle im Spielgeschehen: Sie interagiert direkt mit dem Ball und beeinflusst dessen Bewegung. Aus diesem Grund ist eine saubere Geometrie essenziell, um ein realistisches und nachvollziehbares Spielverhalten zu gewährleisten.

Darüber hinaus muss die Spielfigur performant umgesetzt sein, da sie mehrfach im Spiel vorkommt. Eine optimierte Polygonanzahl in Kombination mit geeigneten Texture- und Normal Maps ermöglicht es, eine hohe visuelle Qualität beizubehalten, ohne die Performance der Simulation negativ zu beeinflussen.

### Design- und Stilentscheidung
Das Spielfiguren-Asset sollte auf den ersten Blick als klassisches „Tischfußballmännchen“ erkennbar sein, um eine klare visuelle Zuordnung zum bekannten Spielprinzip zu ermöglichen. Charakteristische Merkmale wie die vereinfachte Körperform, die aufrechte Haltung sowie die reduzierte Detailtiefe dienen dabei als grundlegende Orientierung.

Gleichzeitig wird bewusst auf direkte Referenzen realer Tischfußballfiguren verzichtet. Stattdessen wird das Design nach eigenen Vorstellungen entwickelt, um einen individuellen Stil zu schaffen und dem Asset einen eigenständigen Charakter zu verleihen. Die Formen und Proportionen sind leicht stilisiert, sodass die Spielfigur sowohl funktional als auch visuell ansprechend wirkt und sich harmonisch in das Gesamtbild der Simulation einfügt.

### Modellierung

Die Modellierung beginnt mit dem Kopf. Zunächst wird mit der Tastenkombination „Shift + A“ ein neues Cube-Mesh erstellt. Auf das Mesh wird ein Subdivision-Surface-Modifier verwendet, um die Polygon-Anzahl ein wenig zu erhöhen und anschließend grob die Form des Kopfes modelliert werden kann.

![Cube Mesh mit Subdivision-Surface-Modifier [@cube-mesh-subdivision-modifier]](img/Lanzmaier/image-1.png)

Als nächstes wird mit Sculpt Tools, wie beispielsweise dem Grab-Tool, die ungefähre Form des Kopfes geformt.

![Grundmodellierung der Kopf-Mesh [@foundation-head]](img/Lanzmaier/image-4.png)
Hinweis: Bei der Grundmodellierung wählt man beim sculpten einen relativ großen Radius, da man nur die wichtigsten Merkmale hervorzuheben möchte. Um den Kopf gleichmäßig zu designen wird hier auch standardmäßig die Mesh-Symmetrie auf der X-Achse ausgewählt (rechts oben)

Für genauere Merkmale verwendet man nochmal einen Subdivision-Surface-Modifier und aktiviert Shade-Smooth. Dadurch kann man mit Sculpt-Tools, wie dem Draw-Tool oder dem Grab-Tool mit geringem Radius, mit der Detailmodellierung starten.

![Detailmodellierung der Kopf-Mesh [@detailed-foundation-head]](img/Lanzmaier/image-6.png)

Hinweis: Nase, Augen, Ohren, Hals, Augenbrauen und Haare werden in diesem Fall als eigene Mesh designt und später mit der Haupt-Mesh (dem Kopf) mit "Ctrl+J" gejoint und "geremesht". Wenn beim "remeshen" unschöne Kanten entstehen, können diese ausgeglättet werden, in dem man mit dem Grab-Tool "Shift" gedrückt hält und damit an den Kanten entlang fährt.

Definition Remesh: Remesh in Blender ist ein Werkzeug, das die Geometrie eines 3D-Modells automatisch neu aufbaut, um eine gleichmäßigere Topologie zu erzeugen.

*!Wichtig:* Man sollte während diesem Prozess immer wieder Backups in Form von Collections in Blender oder als .blend-Files machen. Das ist hier besonders wichtig, da das Mesh beim Modellieren schnell vom einen auf den anderen Moment nicht so wie erwünscht aussehen kann. [@blender-sculpting-tutorial]

![Detailmodellierung der Kopf-Mesh2 [@detailed-foundation-head2]](img/Lanzmaier/image-7.png)
![Detailmodellierung der Kopf-Mesh3 [@detailed-foundation-head3]](img/Lanzmaier/image-8.png)
![Detailmodellierung der Kopf-Mesh4 [@detailed-foundation-head4]](img/Lanzmaier/image-9.png)
![High-Poly Modell der Kopf-Mesh [@high-poly-head]](img/Lanzmaier/image-10.png)
Jetzt ist es wichtig, die Anzahl der Tris zu senken. Da der Kopf jetzt noch eine sehr hohe Anzahl an Polygonen hat, verwendet man "Retopology", um eine Low-Poly-Kopie der High-Poly-Mesh zu erstellen.

Definition Retopology: Retopology ist der Prozess, bei dem man eine neue, saubere Low-Poly-Mesh auf der Oberfläche der High-Poly-Mesh erstellt, um das Modell performanter zu machen.

![Retopology [@retopology]](img/Lanzmaier/image-11.png)

Zuerst wird eine Plane mit "Shift+A" erstellt und an der Oberfläche des High-Poly-Modells positioniert.

![Retopolgy2 [@retopology2]](img/Lanzmaier/image-12.png)

Zunächst aktivieren wir das Retopology Kontrollkästchen, damit man die Plane während dem Edit Mode im Vordergrund sehen kann.

![Retopology3 [@retopology3]](img/Lanzmaier/image-13.png)

Damit die Plane genau auf dem High-Poly-Modell liegt, wird der Shrinkwrap-Modifier verwendet. Beim Target kann man die High-Poly-Mesh auswählen.

![Retopology4 [@retopology4]](img/Lanzmaier/image-15.png)

Jetzt wird der Mirror-Modifier verwendet, um die Plane an der x-Achse zu spiegeln. Das Target ist hier ebenfalls die High-Poly-Mesh.

*!Wichtig:* Clipping muss aktiviert sein, damit sich die beiden Planes beim späteren "Extruden" in der Mitte treffen. Bis man mit der Retopology fertig ist, darf man den Mirror-Modifier nicht anwenden.

![Retopology5 [@retopology4]](img/Lanzmaier/image-16.png)

Im Edit Mode kann man die Kanten der Plane "extruden" und somit die High-Poly-Mesh nachbauen.

*!Wichtig:* Bei sehr detailreichen Stellen, wie in diesem Fall die Ohren bzw. die Nase, müssen wir mit "Ctrl+R" mehrere "Loop-Cuts" hinzufügen um am Ende eine saubere Kopie der Mesh zu erhalten. Die Retopology der Augen und Augenbrauen erfolgt extra, da sie einem anderen Material zugehören.

Definition Loop-Cuts: Loop-Cuts in Blender sind Werkzeuge, um neue Kanten (Loops) in ein Mesh einzufügen. [@blender-retopology]

### Die fertige Low-Poly-Version
![Low-Poly-Modell Kopf [@low-poly-head]](img/Lanzmaier/image-17.png)

Mit einer Normal Map kann man die Low-Poly-Mesh so aussehen lassen, als wäre sie High-Poly. Dafür erstellen wir eine Normal Map der High-Poly-Version und wenden sie an der Low-Poly-Version an.

### Normal Map erstellen
Der erste Schritt ist es, die Low-Poly-Version zu UV-Unwrappen. Dafür geht man in den "Edit-Mode", markiert alle Faces mit "A", drückt "U" und wählt "Smart UV Project" aus. Mit "Smart UV Project" versucht der Rechner so gut wie möglich aus der 3D-Mesh einzelne 2D-Parts zu machen und sie auf eine UV-Map zu projizieren. Der "Island Margin" sagt dem Rechner, wie groß der Abstand der einzelnen 2D-Parts sein soll. Ein guter Wert für den "Island Margin" ist 0.01, da er nicht zu klein ist, sodass sich die Texturen vermischen, aber auch nicht zu groß, sodass der Margin zu viel Platz auf der Map einnimmt.

![Normal Map im Shader Editor erstellen [@normal-map-shader-editor]](img/Lanzmaier/NormalMapCharacter1.png)
Als Nächstes wechselt man von dem 3D-Viewport in den Shader-Editor und erstellt ein neues Material für die Low-Poly-Version. Mit "Shift+A" kann man neue "Nodes" hinzufügen. Für eine Normal Map braucht man eine "Image Texture Node" und eine "Normal Map Node". Mit einer "Image Texture Node" kann man die erstellten "Texture Maps" auf ein Material abbilden. Da es sich um eine Normal Map handelt, benötigt man zusätzlich eine "Normal Map Node", um die "Image Texture" mit dem Material verbinden zu können. Auf der "Image Texture Node" klickt man auf "New", um eine neue Textur zu erstellen. Als Auflösung wählt man standardmäßig 4096px. Beim "Color Space" benötigen wir bei der Normal Map keine Farben, deshalb wählen wir "Non Color".

![Normal Map der Low-Poly-Version erstellen [@create-normal-map]](img/Lanzmaier/NormalMapCharacter3.png)
Die beiden Versionen, High-Poly und Low-Poly, müssen direkt aufeinander positioniert werden. Im Shader Editor muss die "Image Texture Node" ausgewählt werden, sodass man einen weißen Rand an der Node sehen kann. Oben rechts wählt man zuerst die High Poly-Version aus und dann die Low-Poly-Version indem man "Ctrl" gedrückt hält. Im "Render Tab" unter den "Mesh Properties" wählt man für die Render Engine "Cycles". Unter "Bake" wählt man "Bake Type Normal" und drückt auf Bake. In der "Image Editor Ansicht" kann man nun die fertige Normal Map der High-Poly-Mesh erkennen.

### Der fertige Kopf nach Anwendung der Normal Map
![Kopf nach Anwendung der Normal Map [@head-with-normal-map]](img/Lanzmaier/NormalMapCharacter4.png)

Oberkörper und Unterkörper werden nach dem gleichen Prinzip wie der Kopf designt. Da diese beiden Komponenten allerdings nicht so detailreich sind, werden die Low-Poly-Modelle nicht per Hand designt, sondern der "Decimate Modifier" wird benutzt. [@blender-high-poly-to-low-poly]

### Erstellung eines Texture Atlases für das Character-Asset
Sobald das Mesh komplett ist und die gewünschte Anzahl der Polygone erreicht wird, sollte man alle genutzten Materialien in ein Material zusammenfassen. Dadurch muss die Engine weniger Draw-Calls an die Graphic API machen, was die Performance eindeutig erhöht.

Zuerst muss man die Einzelteile des Assets in eine Mesh mit "Ctrl+J" zusammenführen. Dadurch kann man das Asset als eine Mesh UV-Unwrappen. Dass man alle Materials auf eine UV-Map bekommt, erstellt man in der "Shader Editor Ansicht" bei jedem Material ein "Image Texture Node". Am Einfachsten geht das, wenn man die Node in einem Material mit einer neuen Texture Map erstellt (4096px) und diese in die anderen Materials mit "Ctrl+C / Ctrl+V" kopiert.

Der erste Schritt ist es, die Farben aller Materials auf eine Texture Map zu projizieren. Dafür muss man bei den Mesh-Properties unter "Render" für die Render Engine "Cycles" auswählen. Die "Samples" unter "Sampling" kann man auf 1 stellen. Das beschleunigt das Baken, hat aber keinen Einfluss auf das Endergebnis. Unter "Bake" wählt man für den Bake Type "Diffuse". Da wir nur die Farben projizieren wollen, können wir unter den "Contributions" "Direct" und "Indirect" weglassen und nur "Color" anklicken. Bevor man auf "Bake" klickt, sollte man sicherstellen, dass man die "Image Texture Node" bei jedem Material angeklickt hat und für "Color Space" "sRGB" ausgewählt hat. Derselbe Vorgang muss anschließend auch für die Bake Types „Normal“ und „Roughness“ durchgeführt werden, da das Character-Asset derzeit Normal Maps auf mehrere Materialien verteilt hat und zudem unterschiedliche Roughness-Werte verwendet.

*!Wichtig:* Unter der "Image Editor Ansicht" muss man jede neu erstellte Texture Map unter "Image -> Save As" abspeichern, weil das "Image Texture Node" nach jedem Baking-Vorgang die Texture Map überschreibt.

![Diffuse Map Character-Asset [@diffuse-map-character]](img/Lanzmaier/CharacterColorMap.png)

![Normal Map Character-Asset [@normal-map-character]](img/Lanzmaier/CharacterNormalMap.png)

![Roughness Map Character-Asset [@roughness-map-character]](img/Lanzmaier/CharacterRoughnessMap.png)

Nachdem man alle Texture Maps erstellt hat, muss man unter "Edit -> Preferences -> Add-ons" den "Node Wrangler" aktivieren, damit man gleich alle Texturen einfach in ein neues Material einbetten kann. In der "Shader Editor Ansicht" kann man nun ein neues Material erstellen. Um die Texture Maps einfach hinzufügen zu können, muss man die "Principled BSDF Node" anklicken und "Ctrl+Shift+T" drücken, um den Explorer aufzumachen. Nun wählt man alle abgespeicherten Maps aus und drückt "Enter". [@blender-create-texture-atlas]

![Fertig modelliertes Character-Asset [@finished-asset]](img/Lanzmaier/BlenderModelAndMaterialNodes.png)

### Drehstäbe

### Funktion
Der Drehstab ist das einzige Asset im Spiel, das direkt vom Spieler gesteuert werden kann. Außerdem werden die Spielfiguren auf dem Stab positioniert, wodurch er die zentrale Verbindung zwischen Spielerinteraktion und Spielfiguren darstellt.

Aus diesen Gründen muss der Drehstab sowohl funktional zuverlässig als auch optisch klar gestaltet sein: Er sollte einfach zu erkennen und intuitiv zu bedienen sein, ohne die Übersicht auf dem Spielfeld zu beeinträchtigen.

### Design- und Stilentscheidung
Der Drehstab ist bewusst schlicht und funktional gestaltet: Er besteht aus einem länglichen Metallstab mit einem schwarzen Griff am Ende. Diese minimalistische Gestaltung sorgt dafür, dass der Stab als steuerbares Spielelement sofort erkennbar ist, ohne die Übersicht auf der Spielfläche zu beeinträchtigen.

### Modellierung
Der Drehstab wurde in Blender als Zylinder-Mesh modelliert und durch einen erhöhten „Metallic“-Wert mit einer realistischen Metalloberfläche versehen. Durch Strecken des Zylinders entlang seiner Achse "S+Z" entstand die typische Form eines Spielstabs.

Für den Griff wurde ein schwarzes Würfel-Mesh verwendet, das anschließend durch den Einsatz von "Bevels" und "Edge-Loops" zu einer Griffgestaltung geformt wurde.

### Tischplatte

### Funktion
Die Tischplatte bildet die zentrale Spielfläche und stellt die Grundlage für die gesamte Spielsimulation dar. Auf ihr bewegen sich Ball und Spielfiguren, weshalb sie sowohl visuell klar strukturiert als auch funktional gut umgesetzt sein muss.

Neben der optischen Darstellung des Spielfeldes (z. B. Linienmarkierungen, Tore und Spielfeldbegrenzungen) erfüllt die Tischplatte eine wichtige technische Rolle: Sie definiert die Kollisionsfläche für den Ball und grenzt den Spielraum ab.

Darüber hinaus dient die Tischplatte als Referenzebene für die Positionierung weiterer Komponenten wie Spielfiguren, Stangen, Tore und Bande. Eine saubere Modellierung und korrekte Skalierung sind daher essenziell, um ein realistisches Spielgefühl sowie eine stabile und performante Simulation zu gewährleisten.

### Design- und Stilentscheidung
Die Tischplatte ist an ein klassisches Tischfußball-Spielfeld angelehnt und so gestaltet, dass sie eindeutig als Spielfläche erkennbar ist. Die grüne Grundfarbe in Kombination mit weißen Linienmarkierungen dient der klaren Darstellung des Spielfeldes und unterstützt die Orientierung während der Simulation. Zentrale Spielfeldmarkierungen wie Mittellinie, Mittelkreis, Strafräume und Torbereiche sind reduziert, aber eindeutig dargestellt.

Die seitlichen Banden sind farblich dunkler gehalten, um eine klare Abgrenzung zur Spielfläche zu schaffen. Die Tore sind farblich unterschiedlich (rot und blau) gestaltet, um die beiden Spielseiten eindeutig voneinander zu unterscheiden.

Insgesamt wurde ein funktionales und übersichtliches Design gewählt, das den Fokus auf Spielbarkeit und Lesbarkeit legt. Auf unnötige Details wurde bewusst verzichtet, um die Performance nicht zu beeinträchtigen und eine klare visuelle Struktur zu gewährleisten.

### Modellierung
Als Grundlage für die Tischplatte erstellt man mit "Shift+A" ein neues Cube-Mesh. Diese skaliert man mit "S+X" in Richtung der x-Achse, sodass die Größe verhältnismäßig mit einem Tischfußballtisch zusammenpasst. Um eine Box, welche oben offen ist, zu erschaffen, wird der "Boolean Modifier" verwendet. Hierfür wird die bereits erstellte Mesh kopiert und ein wenig runterskaliert. Anschließend positioniert man die kopierte Mesh so, dass sie an der größeren Cube-Mesh oben hinausschaut. Jetzt wendet man auf der größeren Cube-Mesh den "Boolean Modifier" an. Als Target wählt man die kleinere Mesh. Jetzt wurde die Grundlage für die Tischplatte erstellt.

![Plate Foundation [@plate-foundation]](img/Lanzmaier/PlateFoundation.png)

Für die Tore und die Löcher, an denen der Ball reingeworfen wird, verwendet man die gleiche Methode. Zuerst werden die Formen als separate Meshes erstellt. Mit dem "Boolean Modifier" kann man jetzt die Löcher erschaffen, in dem man die separaten Meshes als Target auswählt. Auch für die Bodenmarkierungen werden zuerst dementsprechende Meshes erstellt und per "Boolean Modifier" hinzugefügt. Für ein schöneres Aussehen, werden die Bodenmarkierungen mit "E" nach unten extrudiert. [@cut-holes-in-mesh]

*!Wichtig:* Damit die Hitbox für das Spielfeld in der Engine richtig erstellt werden kann, muss über den Bodenmarkierungen, die jetzt weiter unten liegen, ein unsichtbarer Boden erstellt werden. Dafür erstellt man ein neues Material und stellt den "Alpha Value" auf 0.

### Optimierung und Export in die Game-Engine
 
### Tischplatte
Da das bereits existierende Skript für die Tischplatte so funktioniert, dass die Physik und die Position separat für die Tischfläche, die Wände und die Tore aufgesetzt werden, muss man diese Teile der Tischplatte voneinander getrennt als "PackedScenes" exportieren.

*!Wichtig:* Beim Exportieren muss man darauf achten, dass der "Origin" der Mesh im Mittelpunkt liegt und keine unsaubere Topologie (eher wichtig bei der Character-Mesh, da die Tischplatte sowieso wenig Polygone und Edge-Loops beinhaltet) vorliegt. Es kann sonst passieren, dass das Mesh nach dem Export in der Engine eine falsche Position annimmt oder visuelle Bugs vorkommen.

In Godot kann man aus einer PackedScene ein "MeshArray" Objekt erstellen, in dem man aus der PackedScene eine sogenannte "Inherited Scene" erstellt und diese dann wiederum als "MeshArray" abspeichert. Dieses MeshArray kann man einfach an eine "MeshInstance3D" anhängen und somit in der Simulation darstellen. Die Collision Shape für das Mesh kann man in der 3D-Ansicht generieren lassen.

Im Skript lässt sich die Form einer 3D Node folgendermaßen verändern:

```
var tableMesh = _tableBody.GetNode<MeshInstance3D>("TableMesh");

// Prüfen, ob Mesh in der Scene vorhanden ist.
if (tableMesh != null)
	{
	// Aktuelle Dimensionen der Mesh
	Vector3 meshSize = tableMesh.Mesh.GetAabb().Size;

	// Anpassung der Dimensionen auf die gewünschten Maße
	tableMesh.Scale = new Vector3(
		TABLE_LENGTH / meshSize.X,
		TABLE_HEIGHT / meshSize.Y,
		TABLE_WIDTH / meshSize.Z
	);

	}
```

Zuerst wird sich die Referenz der Mesh mit .GetNode geholt. Die gewünschten Dimensionen müssen mit den aktuellen Dimensionen der Mesh dividiert werden, da der neue Vektor mit den Maßen der bereits bestehenden Mesh multipliziert wird. Wenn die bereits bestehenden Maße nicht 1x1x1 entsprechen, nimmt das Mesh eine unerwünschte Form an. [@get-dimensions-of-mesh] [@set-dimensions-of-mesh]

Die Position von 3D Nodes im dreidimensionalen Raum lässt sich folgendermaßen ändern:

```
private void SetupWall(string wallName, Vector3 position, float length, float height, float thickness)
{
	...
	wall.Position = position;
	...
}
```
[@node3D-documentation]

### Figuren und Drehstäbe
Die Figuren und Drehstäbe lassen sich leicht in die Simulation miteinbinden, da man sie als ein ganzes Mesh exportiert.

```
_rodMesh = GetNode<MeshInstance3D>("Bar");
_characterMesh = GetNode<MeshInstance3D>("Character");
```

In Adobe Photoshop kann man einen bestimmten Farbton auf einem Bild ändern, in dem man in die Farbton/Sättigung-Einstellungen geht und den gewünschten Farbton im Bild auswählt. Der Farbton lässt sich dann nach Belieben umstellen. Für das Figuren-Asset muss im Skript überprüft werden, zu welchem Team sie gehört (rot oder blau), damit man die dementsprechende Farbtextur anwenden kann.

```
// Farbtexturen in die Figure Scene exportieren
[Export] public Texture2D RedTexture;
[Export] public Texture2D BlueTexture;

...

var material = new StandardMaterial3D();

		// Farbe basierend auf Team setzen
		switch (Team)
		{
			case GameManager.Team.Red:
				material.AlbedoTexture = RedTexture;
				GD.Print("Setting RED material");
				break;

			case GameManager.Team.Blue:
				material.AlbedoTexture = BlueTexture;
				GD.Print("Setting BLUE material");
				break;

			default:
				material.AlbedoColor = new Color(0.5f, 0.5f, 0.5f); // Grauer Fallback
				GD.Print("Setting GRAY fallback material");
				break;
		}
```
[@exported-properties-godot]

Wenn der Drehstab dem Gegner gehört (Team Blau), muss dieser in die andere Richtung zeigen.

```
if(Team == GameManager.Team.Blue) {
	_rodMesh.RotationDegrees = new Vector3(0, -90, 90);
}
```

Nachdem alle Collision Shapes erstellt wurden und alle Assets korrekt in die Simulation eingebunden wurden, ist das optische Design/Arrangement des Spiels finalisiert.

![Finalisierte Version der Simulation [@final-version]](img/Lanzmaier/CompleteSImulation.png)
