# Zusammenfassung
\textauthor{Nikita Schaar}

## Allgemein

Das gemeinsame bzw. alleinige Bespielen eines Tischfußballtisches wird an einem analogen Tisch durch den Faktor, dass ein Spiel zu zweit oder zu viert oft nicht zustandekommt, erschwert. Wenn man komplett alleine Spielen will ist dies zwar möglich, bringt dem Spieler jedoch nichts, da kein Lerneffekt dabei entstehen kann.

## Arbeit

In dieser Diplomarbeit wird ein System aus Hardware und Software geschaffen, mithilfe dessen das Spielen mit einem Tischfußballtisch erleichtert wird und dazu noch spaßig sein soll.

Das System besteht aus einem Controller, welcher ein 3D-modelliertes und -gedrucktes Gehäuse besitzt, in dem sich ein Raspberry Pi 4B befindet, an den mehrere Computer-Mäuse angeschlossen sind. Diese Mäuse befinden sich in vier Halterungen, welche über dazugehörige Drehstäbe platziert sind und die Bewegung/Drehung dieser erfassen. Anhand von den eingegebenen Signalen, welche über MQTT vom Raspberry Pi an den PC geschickt werden, werden die vier Stäbe dann gedreht und geschoben um das Spiel zu spielen.

Als Spieler ist es möglich, sowohl gegen echte Spieler, als auch virtuelle Computer-Gegner zu spielen, welche entweder aufgrund von Algorithmen handeln oder ihre Eingaben von einem trainierten KI-Modell erhalten.

## Probleme

Aufgrund von zeitlichen Problemen ist sich der fertige Druck des finalen Controller-Modells nicht vor Abgabe der schriftlichen Ausarbeitung ausgegangen, die Funktionalität wurde jedoch vollständig überprüft und ist einwandfrei.

Zu Beginn des KI-Trainings erfolgte dies auf Basis von Python und dem PyTorch-Framework. Auch wenn dieses Training zwar funktionell problemlos funktionierte, stieg die Geschwindigkeit des Trainings, nach einem Umstieg auf ein System in der Godot Engine, auf mehr als das hundertfache von davor.

Für die Realisierung des Multiplayer-Modus fiel die Entscheidung nach längerem Überlegen auf ein Peer-to-Peer-Modell. Dieses sollte grundsätzlich einwandfrei funktionieren, jedoch stellte sich während der Entwicklung heraus, dass manchmal ein zusätzlicher TURN-Server verwendet werden muss, sodass restriktive NATs umgangen werden können und der Verbindungsaufbau immer funktioniert.

Aufgrund von mangelnder Kommunikation/Disziplin haben sich einige Entwicklungsschritte länger gezogen, als sie es hätten tun müssen. Dies führte in der Fertigstellung der Arbeit zum Glück trotzdem zu keinen direkten Problemen, da alle Terminziele eingehalten werden konnten.

## Ausblick

### Weiterentwicklung der Hülle

Um das Design der Hülle visuell und funktionell zu optimieren, können in Zukunft noch einige Zusatzfeatures, wie z. B. ein eingebautes Mini-Display verbaut werden.

### Batterie-/Akkusystem

Da der Controller aktuell aufgrund von dem eingebauten Raspberry Pi eine externe Stromversorgung benötigt, wäre es schlau, einen Akku o. Ä. zu verbauen, sodass auch ohne angesteckten Controller gespielt werden kann.

### Visuelle Erweiterung der Simulation

Auch wenn die visuelle Gestaltung der Simulation grundsätzlich so ausgefallen ist, wie wir es uns vorgestellt haben, ist die Gestaltung der HUD-Elemente ein wenig knapper ausgefallen als gewünscht. Um hier Verbesserung zu bringen, könnten in einem Programm wie Adobe Illustrator^[https://www.adobe.com/] zusätzliche Grafik-Elemente wie Buttons oder eine stilisierte Punkte-Anzeige gestaltet werden.

### Signalermittlung

Zur Steuerung der Simulation wird ein Raspberry Pi mit vier angeschlossenen Mäusen verwendet, welcher per MQTT Daten an den PC sendet. Auch wenn dies zur Steuerung der Simulation sehr gut funktioniert gibt es möglicherweise einen Weg, die Dreh- und Schiebebewegungen noch zuverlässiger zu erfassen, ohne die Simplizität der aktuellen Lösung zu zerstören.

### KI-Training

Da das Training eines KI-Modells von Grund auf kein einfacher Prozess ist und viel Zeit in Anspruch nimmt, ist es im Zuge dieser Arbeit nicht möglich gewesen ein "perfektes" Modell für die Steuerung der gegnerischen Drehstäbe zu entwickeln. Auch wenn sie gut funktioniert gibt es immer noch Raum für Verbesserung, welcher in Zukunft noch ausgenutzt werden kann.

