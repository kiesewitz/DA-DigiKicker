# Teilaufgabe Schüler Bravo
\textauthor{Schueler 2}

## Theorie

Dieses Kapitel wird oft auch als _Literaturrecherche_ bezeichnet. Da gehört alles rein was der __normale__ Leser braucht um den praktischen Ansatz zu verstehen. Das bedeutet Sie brauchen einen roten Faden !

Das sind z.B: allgemeine Definitionen, Beschreibung von fachspezifischen Vorgehensweisen, Frameworks, Theorie zu verwendeten Algorithmen, besondere Umstände, ...

## Praktische Arbeit

Hier beschreiben Sie ihren praktischen Teil. Es geht darum seine Implementierung / Versuche so darzustellen dass anhand dieser dre Leser erkennen kann was sie wie gemacht haben.

Die Frage nach der Detailgenauigkeit lässt sich wie folgt beantworten: So, dass man Ihre Aufgabenstellung vollständig  nachvollziehen kann wenn man nur diese Diplomarbeit in Händen hat!

### Erzeugen von Java Quellcode

Unter einem Array in Java versteht man ein Feld oder Container, das in der Lage ist, mehrere Objekte vom gleichen Typ aufzunehmen und zu verwalten. Dabei wird in Java das Array als eine spezielle Klasse repräsentiert, was unter anderem mit sich bringt, dass man auf spezielle Methoden und Operationen bei Arrays zurückgreifen kann. Der Umgang mit Arrays mag gerade am Anfang etwas schwerer sein und birgt viele Fehlerquellen, nach und nach wird man das System das hinter den Arrays steht aber gut nachvollziehen können. 

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{caption="Initialisieren eines Arrays" .java}
Typ[] Name = new Typ[Anzahl];
Typ Name[] = new Typ[Anzahl];
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Etwas erfahrenere Programmierer werden jetzt schon erkennen, worauf es beim Zugriff auf Elemente im Array meist hinausläuft: Auf Schleifen!
Schleifen sind ein komfortables Mittel um alle Elemente eines Arrays durchzugehen und auf Wunsch auszugeben oder andere Operationen darauf anzuwenden. Allerdings muss man nicht nur hier aufpassen, dass man die länge des Arrays in der Schleife nicht überschreitet und so auf Felder zugreift die gar nicht existieren. Damit so etwas erst gar nicht passiert, kann man in der Abbruchbedingung der for-Schleife direkt die Länge des Arrays ausgeben mit: array.length.

Möchte man nun also alle 5 Elemente unseres Beispiels-Arrays mit einer Schleife ausgeben lassen, dann würde das so gehen:

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{caption="Examples of array manipulations" .java}
// (c) by Mike Scott

public class ArrayExamples
{	public static void main(String[] args)
	{	int[] list = {1, 2, 3, 4, 1, 2, 3};
		findAndPrintPairs(list, 5);
		bubblesort(list);
		showList(list);

		list = new int[]{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};
		bubblesort(list);
		showList(list);

		list = new int[]{11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, -1, -2};
		bubblesort(list);
		showList(list);

		list = new int[]{1};
		bubblesort(list);
		showList(list);
	}


	// pre: list != null, list.length > 0
	// post: return index of minimum element of array
	public static int findMin(int[] list)
	{	assert list != null && list.length > 0 : "failed precondition";

		int indexOfMin = 0;
		for(int i = 1; i < list.length; i++)
		{	if(list[i] < list[indexOfMin])
			{	indexOfMin = i;
			}
		}

		return indexOfMin;
	}
}
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Obwohl hier nur java gezeigt ist, unterstützt das Template auch scala, java, javascript, css, html5 und xml

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{caption="Ein einfaches XML" .xml}
<?xml version="1.0" standalone="yes"?>
<!DOCTYPE module [
    <!ELEMENT module (module|property|metadata|message)*>
    <!ATTLIST module name NMTOKEN #REQUIRED>
    <!ELEMENT property EMPTY>
    <!ATTLIST property
        name NMTOKEN #REQUIRED
        value CDATA #REQUIRED
        default CDATA #IMPLIED
    >
    <!ELEMENT metadata EMPTY>
    <!ATTLIST metadata
        name NMTOKEN #REQUIRED
        value CDATA #REQUIRED
    >
    <!ELEMENT message EMPTY>
    <!ATTLIST message
        key NMTOKEN #REQUIRED
        value CDATA #REQUIRED
    >
]>

<!--
    Checkstyle configuration that checks if the braces are set correctly
 -->

<module name = "Checker">
    <property name="charset" value="UTF-8"/>
    <property name="severity" value="warning"/>

    <property name="fileExtensions" value="java"/>
    <!-- Checks for whitespace                               -->
    <!-- See http://checkstyle.sf.net/config_whitespace.html -->

    <module name="TreeWalker">
        
        <module name="NeedBraces"/>
        <module name="LeftCurly">
        	<property name="option" value="nl"/>
        </module>

        <module name="RightCurly">
            <property name="id" value="RightCurlyAlone"/>
            <property name="option" value="alone"/>
            <property name="tokens"
             value="CLASS_DEF, METHOD_DEF, CTOR_DEF, LITERAL_FOR, LITERAL_WHILE, STATIC_INIT,
                    INSTANCE_INIT,LITERAL_TRY, LITERAL_CATCH, LITERAL_FINALLY, LITERAL_IF, LITERAL_ELSE,
                    LITERAL_DO"/>
        </module>
    </module>
</module>
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

Hier etwas in kotlin

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{caption="Ein einfaches Kotlin Beispiel" .kotlin}
// this is a simple code listing:
println("hello kotlin from latex")
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


Und noch ein Beispiel in vba

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~{caption="Ein einfaches Visual Basic for Applications Beispiel" .vba}
Private Sub ExitSub()
 
    Dim i As Integer
 
    For i = 1 To 10      
        If i = 5 Then
            Exit Sub
            MsgBox "The value of i is" & i
        End If
    Next i 
 
End Sub
 
 
Private Sub CallExitSub()
    Call ExitSub
    MsgBox "Exit Sub"  
End Sub
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


und noch was in Dart (im Markdown direkt als Latex Quellcode eingefügt - damit funktionieren jegliche Sprachen welche als langdef vorliegen) 

\begin{lstlisting}[language=Dart, caption={Ein Beispiel für Dart}]
library hallo;

void main() {
  String x;
  print('Hello, World!');
  x = 'hallo';
}
\end{lstlisting}


### Auswertung der Ergebnisse

Anhand von XY kann man folgende Tabelle ableiten:

| Right | Left | Default | Center |
|------:|:-----|---------|:------:|
|   12  |  12  |    12   |    12  |
|  123  |  123 |   123   |   123  |
|    1  |    1 |     1   |     1  |

: Eine Tolle tabelle

#### Eine Überschrift 4ter Ordnung

Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext.


#### Noch ein Überschrift 4ter Ordnung

Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext.

Und mit einer Aufzählung:

* Alpha
* Bravo
* Charlie
    * Charlie 1
    * Charlie 2
    * Charlie 3
    * Charlie 4
* Delta
* Epsilon

 Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext. Mit etwas Fließtext.

