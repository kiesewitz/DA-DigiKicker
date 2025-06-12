[![Release](https://img.shields.io/github/release/HTL-Leoben/da-base-template.svg)](https://github.com/HTL-Leoben/da-base-template/releases/latest)
[![Build Docker](https://img.shields.io/github/actions/workflow/status/HTL-Leoben/da-base-template/image-build.yml?branch=main&label=docker-build)](https://github.com/HTL-Leoben/da-base-template/actions/workflows/image-build.yml)
[![Push Docker](https://img.shields.io/github/actions/workflow/status/HTL-Leoben/da-base-template/image-push.yml?branch=main&label=docker-push)](https://github.com/HTL-Leoben/da-base-template/actions/workflows/image-push.yml)
[![Test Template](https://img.shields.io/github/actions/workflow/status/HTL-Leoben/da-base-template/template-test.yml?branch=main&label=test-template)](https://github.com/HTL-Leoben/da-base-template/actions/workflows/template-test.yml)
[![Test Action](https://img.shields.io/github/actions/workflow/status/HTL-Leoben/da-base-template/action-test.yml?branch=main&label=test-action)](https://github.com/HTL-Leoben/da-base-template/actions/workflows/action-test.yml)

# HTLLE-DA-Vorlage

Diese Vorlage ist zur Erstellung der Diplomarbeiten an der HTL Leoben gedacht.
**Bitte sprechen Sie mit Ihren Betreuern ab, ob Sie Ihre Arbeit mit Hilfe dieses Templates verfassen dürfen.**

Das hier vorliegende Template sollte die Erstellung von Diplomarbeiten in Teams soweit wie möglich vereinfachen.
Deshalb setzen wir auf die Verwendung von freien Technologien welche ohne Lizenzkosten auf jedem Rechner installiert werden dürfen:

* [GIT](https://git-scm.com/) zur Verwaltung und Versionierung der Diplomarbeit.
* [Make](https://www.gnu.org/software/make/) sowie einige Linux Standardtools zum Aufruf und zur Orchestrierung der Werkzeuge
* [Pandoc](https://pandoc.org/) zur Transformation von [Markdown](https://pandoc.org/MANUAL.html#pandocs-markdown) Dateien zu LaTex Dateien, sowie
* [BibTex](http://www.bibtex.org/) zur Erstellung von Literaturverzeichnissen und Referenzen innerhalb des Dokuments  
* [TexLive](https://www.tug.org/texlive/) zur erstellung des fertigen PDFs aus dem generierten LaTex Dokument
* [Hunspell](https://hunspell.github.io/) als Rechtschreibüberprüfung

Zur Erstellung der Arbeit sollte immer das neuest Template aus dem Repository [HTL-Leoben/da-base-template](https://github.com/HTL-Leoben/da-base-template) verwendet werden. Das dort abgelegte Template wird von HG gepflegt und enthält die jeweils letztgültige (und mit der Direktion abestimmte) Fassung.

Wenn Sie Änderungen an diesem Template wünschen, dann erstellen Sie bitte ein [Issue](https://github.com/HTL-Leoben/da-base-template/issues) in dem sie auf einen ebenfalls von Ihnen aufgegebenen [Pull Request](https://github.com/HTL-Leoben/da-base-template/pulls) verweisen der Ihren Änderungswunsch dokumentiert.  

# Vorbereitung des eigenen Rechners

Damit die DA gebaut werden kann müssen mehrere Programme installiert sein. Theoretisch funktioniert das auch mit 'purem' Windows, aber einfacher ist es die Arbeit mit Hilfe von Linux zu erstellen. Aus diesem Grund finden Sie hier nur die Anweisungen die sich auf einem **Ubuntu 18.04 LTS**  beziehen.

Sie brauchen die Tools nicht unbedingt installieren und Sie können sich die Arbeit als PDF per E-Mail oder in einen Microsoft Teams Channel zuschicken lassen. Mehr dazu weiter [unten](#alternative-platformen).

## Windows

Wenn man Windows 10 oder Windows 11 als primäres Betriebssystem verwendet, hat man zwei Möglichkeiten um zu einer Linux Umgebung zu kommen:

### VM installieren

Installation einer virtuellen Maschine auf der ein Linux läuft (z.B: mit [HyperV](https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/quick-start/enable-hyper-v), [VmWare](https://www.vmware.com/at/products/workstation-player.html), [VirtualBox](https://www.virtualbox.org/) oder einem anderen Virtualisierer) -> Dann erhält man eine VM welche man zum bearbeiten der VM extra starten muss.

### Windows Subsystem for Linux

Aktivierung des [Windows Subsystem für Linux](https://docs.microsoft.com/en-us/windows/wsl/about) anhand [dieser Anleitung](https://docs.microsoft.com/en-us/windows/wsl/install-win10) welches genau so eine Umgebung erzeugt und bei der die Windows und Linux Welt miteinenader zusammenwachsen.

Es ist egal für welche der beiden Varianten man sich entscheidet - funktionieren werden technisch gesehen beide gleich gut ... wobei die WSL Variante sicherlich mehr Comfort bietet weil damit [direkt auf das Windows Dateisystem zugegriffen](https://docs.microsoft.com/en-us/windows/wsl/faq#how-do-i-access-my-c-drive) werden kann.

Das Dateisystem der `C:`-Festplatte von Windows ist im WSL unter folgendem Pfad erreichbar `/mnt/c/`. Die Festplatte `D:` unter dem Pfad `/mnt/d` usw. Es macht also sinn seine Diplomarbeit gleich auf dem C: Laufwerk anzulegen weil man dann von beiden Welten aus Zugriff auf die Dateien hat.

Damit diese Anleitung für Windows und Linux passt habe ich angenommen dass sie in Windows einen Ordner `c:\Diplomarbeit` haben der mit folgendem Befehl in das WSL home Verzeichnis verlinkt ist:

```sh
ln -s /mnt/c/Diplomarbeit ~/Diplomarbeit
```

## Linux

Zuerst müssen (einmalig) die notwendigen Pakete in der Linux Umgebung installiert werden. Dieser Vorgang kann einige Zeit in Anspruch nehmen weil ca. 3-4 GB an Daten installiert werden.

```sh
sudo apt-get update
sudo apt-get install git build-essential make-guile texlive-full pandoc pandoc-citeproc tree rsync hunspell hunspell-de-at hunspell-en-us
```

Bei einer englischen Diplomarbeit muss außerdem entweder das Paket `hunspell-en-us` installiert werden.

## Alternative platformen

Sie können die Diplomarbeit auch mit Hilfe von Docker oder Github Workflows bauen. Nähere Details dazu finden Sie im Verzeichnis `tools`.

# Grundlegende Schritte

## Manuelles erstellen einer Diplomarbeit

Wenn Sie mit einer neuen Diplomarbeit beginnen, dann gehen Sie am besten wie folgt vor:

### Erstellen eines leeren GIT Repositories in dem Sie Ihre DA Daten verwalten werden

Es ist prinzipiell egal auf welchem GIT Server sie Ihr Repository hosten. Wir empfehlen dies aber auf [GitHub](https://github.com) zu machen. **Achten Sie darauf, dass Sie ein privates Repository verwenden**, denn sonst wäre der Inhalt der Arbeit bereits (unabsichtlich) veröffentlicht und beim Plagiatscheck würde Ihre gesamte Arbeit als Plagiat aufscheinen - und in weiterer Folge dann abgelehnt werden.

#### Fall 1: Sie beginnen mit einem komplett neuen Repository

Das hier gezeigte Beispiel legt ein neues lokales Repository an und verknüpft es mit einem (zuvor uninitialisiert angelegtem) Repository im Internet.

```sh
~/Diplomarbeit$ git init
~/Diplomarbeit$ echo "Meine Diplomarbeit" > README.md
~/Diplomarbeit$ git add README.md
~/Diplomarbeit$ git commit -m "first commit"
~/Diplomarbeit$ git remote add origin https://github.com/schueler/Diplomarbeit_JAHR.git
~/Diplomarbeit$ git push -u origin master
```

#### Fall 2: Sie haben bereits ein eigenes DA Repository mit Daten drinnen

Dieser Befehl klont Ihr bestehendes Repository in das aktuelle verzeichnis.

```sh
~/Diplomarbeit$ git clone https://github.com/schueler/Diplomarbeit_JAHR.git .
```

### Hinzufügen des Templates als git-submodul

Wenn Sie ein lokales Repo haben, dann wird dieses Repository als [git-submodule](https://git-scm.com/docs/git-submodule) dazugeklont:

```sh
~/Diplomarbeit$ git config core.filemode false
~/Diplomarbeit$ git submodule add https://github/HTL-Leoben/da-base-template.git
```

Der erste Befehl verhindert, dass Änderungen an Berechtigungen (wie sie WSL im Hintergrund durchführt) dazu führen dass die Datei als modifiziert angesehen wird. Der zweite Befehl holt das Submodul dazu.  

Falls das Template durch HG geändert wird, können Sie mit Hilfe des Befehls `git submodule update --remote` ihre derzeitige Version durch die jeweils aktuellste Version des Templates ersetzen. Damit die Änderungen sichtbar werden müssen Sie natürlich die Diplomarbeit vorher neu bauen.

### Auschecken eines Diplomarbeitsrepositories eines anderen Teammitgliedes

Sollten die ganzen obigen Schritte bereits durch eines Ihrer Teammitglieder erledigt worden sein, dann reicht es aus wenn sie sich das Repository inklusive der Submodule einfach klonen.

```sh
~/Diplomarbeit$ git clone --recursive https://github.com/schueler/Diplomarbeit_JAHR.git .
```

Falls man irgendwann später die Submodule dazuklonen möchte (weil man z.B: den parameter `recursive` vergessen hat) dann kann man das einfach nachholen indem man folgende Befehle nachschießt:

```sh
~/Diplomarbeit$ git submodule init
~/Diplomarbeit$ git submodule update
```

### Erstellen der Inhaltsfiles für die DA

Die Inhaltsdateien legen Sie in Ihrem Repository ab. Am besten Sie beginnen damit, die Inhlatsdateien aus dem Template als Grundlage für Ihre DA zu verwenden. Sie können diese aus dem Template ganz einfach herkopieren und anschließend bearbeiten.

```sh
~/Diplomarbeit$ cp -R ./HTLLE-DA-Vorlage/example/* .
```

Damit sollten sie am Ende dann in etwa so eine Verzeichnisstruktur haben

```txt
$ tree ~/Diplomarbeit
.
├── 10-einleitung.md               <= Kapitel: Einleitung
├── 20-zielsetzung.md              <= Kapitel: Zielsetzung
├── 30-ausarbeitungen.md           <= Kapitel: Ausarbeitungen
├── 31-ausarbeitung_schueler1.md   <=          Ausarbeitung Schueler A
├── 32-ausarbeitung_schueler2.md   <=          Ausarbeitung Schueler B
├── 40-zusammenfassung.md          <= Kapitel: Zusammenfassung
│
├── literatur.bib                  <= Das ist das Literaturverzeichnis
├── metadata.yaml                  <= Grundlegende Informationen zur Diplomarbeit
│
├── .vscode                        <= Einstellungen und hilfreiche Dateien für VS Code
│   ├── bibtex.code-snippets          
│   ├── extensions.json
│   ├── markdown.code-snippets
│   ├── settings.json
│   └── tasks.json                 
├── img                            <= Hier kommen alle Ihre Bilder rein
│   ├── graph.png
│   └── screenshot_eclipse.png
│
├── pdfs                           <= PDFs für den Anhang (referenziert in metadata.yaml)
│   ├── begleitprotokolle.pdf
│   ├── HTL-DA-Vereinbarung.pdf
│   └── projekthandbuch.pdf
│
│  
└── HTLLE-DA-Vorlage
    ├──                            ... Inhalte aus der DA Vorlage
    └──                            ... wurden hier ausgeblendet
```

Es ist wichtig dass diese Verzeichnisstruktur so beibehalten wird, weil sonst der Build-prozess schief gehen kann. Achten Sie auf Groß und Kleinschreibung der Dateien und verzeichnisse.

Normalerweise sollten sie mit diesen Dateien auskommen. Sie können den Inhalt dieser Dateien (unter Einhaltung der entsprechenden Formatierungsvorschriften) durch Ihren Inhalt ersetzen. Es ist normalerseise nicht notwendig weitere Dateien einzufügen, denn Dinge wie das Deckblatt, Eidesstattliche Erklärung, div. Verzeichnisse werden automatisch erstellt und gleich korrekt fomatiert.

Hier nochmals eine Erklärung der verschiedenen Dateien:

* Die `*.md` Dateien im Basisverzeichnis bilden den eigentlichen Inhalt Ihrer Diplomarbeit. Sie können diese Dateien mit jedem handelsüblichen Editor (zur Not auch direkt in Gitea) bearbeiten. Als Sytnax verwenden Sie [Markdown mit einigen Spezialfeatures für pandoc](https://pandoc.org/MANUAL.html#pandocs-markdown). Die Nummerierung am Anfang dient dazu, dass die Reihenfolge in der Diplomarbeit passt. So erscheint z.B: der Inhalt der Datei **20-zielsetzung.md** nach **10-einleitung.md** und vor **30*.**. Die im Template angebotene Reihenfolge ist jene wie sie in der Diplomarbeit gewünscht ist. Mit dieser Technik kann die Nummerierung der Dateinamen nicht mehr mit der Nummerierung der Abschnitte / Kapitel im Text übereinstimmen, aber das ist in Ordnung. Sie sollten sich im Allgemeinen auf die Namen / Bezeichnungen von Kapiteln / Abschnitten als Bezeichner verlassen und Pandoc selbst, LaTeX die eigentliche Abschnittsnummerierung übernehmen lassen. Die Nummern in den Dateinamen sind Dateinummern in einem für die Shell guten Format und so benutzerfreundlich wie möglich.
* Das `/img` Verzeichnis beinhaltet alle Grafiken zur Diplomarbeit. Die können hier zur besseren Strukturierung auch gerne Unterverzeichnisse erstellen. Folgendes ist bei der Verwendung von Grafiken zu beachten:
  * Achten Sie darauf dass sie dort nur `*.jpg` und `*.png` Dateien ablegen
  * Groß und Kleinschreibung müssen beachtet werden. So werden unter Linux die Datein `Hallo.jpg` und `hallo.jpg` als zwei unterschiedliche Dateien angesehen. Unter Windows hingegen nicht.
* Das `/pdfs` Verzeichnis beinhaltet Dateien welche im Appendix (auszugsweise oder ganz) eingefügt werden. Damit dies geschieht müssen sie aber im `metadata.yaml` korrekt referenziert werden. Es ist keine Schande hier viele Dateien abzulegen - sofern diese einen Bezug zur Diplomarbeit haben.
* Das Verzeichnisse `.vscode` enthält Einstellungen, Snippets und eine Liste von empfohlenen Erweiterungen für VS Code. VS Code sollte selbständig eine Benachrichtigung rechts unten einblenden, ob die Empfohlenen Erweiterungen installiert werden sollen. Wenn das nicht passiert kann man auch in den Tab Erweiterungen gehen und nach `@recommended:workspace` suchen. Dort kann man dann alle empfohlenen Erweiterungen installieren. Die Erweiterungen beinhalten eine simple Rechtschreibüberprüfung in Deutsch und Englisch (Code Spell Checker), einige nützliche git-Tools (Git Graph, Blame & Merge), die Möglichkeit PDFs direkt in VS Code anzuzeigen (vscode-pdf) und ein Tool womit vereinfacht Literatur Referenzen eingefügt werden können (Pandoc Citer).

### Befüllen der speziellen Dateien

#### metadata.yaml

In dieser Datei befinden sich alle Informationen rund um Ihre Diplomarbeit. Das Dateiformat ist [YAML](https://yaml.org/spec/1.2/spec.html) in welchem folgende Felder befüllt sein müssen:

```yml
---

# Informationen für das Titelblatt
da-titel: Titel lt. DA Portal
da-jahr: 2020/21 

# Schlüsselwörter werden im PDF als Schlüsselwörter hinterlegt
da-keywords: HTLLE, Keyword1, Keyword2, Keyword3

# Sprache in der die Diplomarbeit verfasst wird. 
# Je nach dem wird dann das Template übersetzt. Die Übersetzung des Inhaltes
# ist Aufgabe der Schüler. Mögliche Werte: <lang_nicht_setzen> oder english
# lang: english

# Autoren der Diplomarbeit (=Schüler)
# Je Autor sind die Felder vorname, nachname, klasse, und thema verpflichtend auszufüllen
# Sofern KI Toools verwendet wurden sind diese auch auszufüllen
# Information wird am Titelblatt und bei der Eidesstattlichen Erklärung verwendet
da-author:
- vorname: Joltawan 
  nachname: Barodscheff
  klasse: 5AHWIN
  thema: Bauen des LaTex Templates
  ki-tools:
  - name: ChatGPT3.5
    zweck: Informationsbeschaffung
  - name: Midjourney
    zweck: Künstlerische Illustration

- vorname: Craig 
  nachname: Tester
  klasse: 5AHWIN
  thema: Entwicklung eines Prototypen
  ki-tools:
  - name: Github Copilot
    zweck: Boilerplate codegenerierung, Codedokumentation


# Diplomarbeitsbetreuer
# Je Betreuer sind alle Felder auszufüllen
# Information wird am Titelblatt und bei den PDF Metadaten verwendet
da-betreuer:
- paraffe: hg
  name: Ing. DI(FH) Günther Hutter, Msc.

- paraffe: me
  name: DI Dr. mont Thomas Messner

# Kurzfassungen sind in Deutsch und Englisch zu verfassen und enthalten folgende Information welche auf nicht mehr als 
# einer A4 Seite (ohne Grafiken) dargestellt wird:
# Einleitung und Hintergrund: Ein kurzer Überblick über das Thema der Arbeit, die Forschungsfrage(n) und die Relevanz des Forschungsvorhabens.
# Ziele: Eine klare Darstellung der Ziele oder Hypothesen, die die Arbeit zu untersuchen oder zu beweisen beabsichtigt.
# Methodik: Eine Zusammenfassung der angewandten Forschungsmethoden und Ansätze, inklusive der Datenerhebungs- und Analyseverfahren.
# Ergebnisse: Die wichtigsten Ergebnisse der Forschung, präsentiert in einer knappen und prägnanten Form. Dies kann quantitative oder qualitative Daten umfassen, je nach Art der Arbeit.
# Schlussfolgerungen: Eine Zusammenfassung der Schlussfolgerungen, die aus den Ergebnissen gezogen wurden, inklusive der Beantwortung der Forschungsfrage(n).

# Eine Kurzfassung in Deutsch.
da-kurzfassung-de: Hier kommt eine deutsche Kurzfassung hin.

# Eine Kurzfassung in Englisch
da-kurzfassung-en: Hier kommt eine englische Kurzfassung hin.


# PDF Dateien welche die Wissensbeschaffung von generativer KI dokumentieren.
# Je Anwendungsfall ist jeweils ein PDF einzukopieren das im Ordner pdfs abgelegt ist, und
# den Prompt sowie die gesamte KI Antwort beinhaltet. 

da-kiquellen:
- ki: ChatGPT4
  short-prompt: Erklärung Pandoc
  pdf-file: gpt-pandoc.pdf

- ki: ChatGPT3.5
  short-prompt: Aufbau eines Atoms
  pdf-file: gpt-atom.pdf

# PDF Dateien die im Anhang zur Diplomarbeit entweder ganz oder nur seitenweise einkopiert werden
# Das Pflichtattribut 'abschnitt' bezeichnet hier die Überschrift für desen Anhang
# Das Pflichtattribut 'pdf-file' ist der Dateiname des PDFs welches im pdfs Ordner zu finden ist (ohne Pfadangabe)
# Wenn man nur einige Seiten aus einem großen PDF einfügen will dann kann man das optionale Attribut 'seiten' verwenden
# in dem man entweder durch Beistrich getrennt szezifische Seiten aufzälen kann oder mittels Bindestrich einen Bereich.  
# Sollte gar kein Appendix erzeugt werden kann dieser Abschnitt auch gelöscht werden. Am besten verwenden Sie als
# Dateinamen ohne Sonderzeichen als Eingabedateien.

da-appendix:
- abschnitt: Technische Dokumentation
  pdf-file: pandoc-manual.pdf
  seiten: 53-73
  
# Betreuungsprotokolle und organisatorische Dokumente sind verpflichtender Bestandteil des Appendix
- abschnitt: Betreuungsprotokolle
  pdf-file: begleitprotokolle.pdf    

# Die Diplomarbeitsvereinbarung sollte als letztes Dokument aufscheinen
- abschnitt: Diplomatbeitsvereinbarung
  pdf-file: HTL-DA-Vereinbarung.pdf
---
```

#### literatur.bib

Aus dieser Datei wird als Literaturverzeichnis erstellt. Wann immer sie Informationen aus anderen Quellen beziehen oder daraus Wissen ableiten sollten sie diese Quelle zitieren. Alle Informationen zu direkten und auch indirekten Quellen sollten hier möglichst genau dokumentiert werden. Sobald die Quellenangabe dann in der DA verwendet wird, erstellt das Framework einen entsprechenden Literaturverweis.

Egal ob die Quellenangabe aus einem Buch, einer Zeitschrift order irgend einer anderen wissenschaftlichen Arbeit stammt, versuchen sie immer möglichst alle Felder zu befüllen. In diesen [bibtex style examples](https://verbosus.com/bibtex-style-examples.html?lang=de) sieht man wie solch eine korrekt befüllter Quelleneintrag aussieht.

Um das Zusammensuchen der Quellenangaben für Bücher zu vereinfachen kann man in vielen Fällen den Bibliographiedienst [OttoBib](https://www.ottobib.com) verwenden, in dem alleine durch Angabe der [ISBN Nummer](https://en.wikipedia.org/wiki/International_Standard_Book_Number) eines Buches in vielen Fällen schon einen fertigen BibTex Block bekommt. Einen ähnlichen Dienst bietet die Internetseite [bibtexsearch](https://www.bibtexsearch.com/) an - wo man durch die Eingabe von Schlüsselwörtern nach BibTexeinträgen suchen kann. Wenn man wirklich nichts im Internet findet, dann kann man sich die Einträge auch selbst basteln - wobei sich die Software [JabRef](https://www.jabref.org/) als besonders nützlichlicher Editor dafür herauskristallisiert hat.

## Arbeiten im Rahmen der Diplomarbeit

### Bauen der Diplomarbeit als PDF Datei

Damit das PDF für die Diplomarbeit erstellt wird muss (aus Ihrem Basisverzeichnis heraus) folgender Befehl abgesetzt werden:

```sh
make pdf -C HTLLE-DA-Vorlage SOURCEDIR=$(pwd)
```

Danach erscheint (sofern alles gut geht) die Datei `./diplomarbeit.pdf`. Sollte die Datei bereits von einem früheren Lauf her existiert haben wird sie einfach überschrieben. Sollte die Arbeit nicht erfolgreich gebaut werden können, dann kann in der Datei `pandoc.log` der Grund dafür herausgefunden werden.

Damit Sie nicht immer den gesamten Befehl schreiben müssen können Sie sich auch einen Alias setzen:

```sh
alias da="make pdf -C HTLLE-DA-Vorlage SOURCEDIR=$(pwd)"
```

Damit beschränkt sich Ihre eigentliche Arbeit darauf, die Markdown Files zu editieren (nicht veressen zu speichern) und anschließend den neuen Alias `da` aufzurufen. Danach haben Sie immer die neuste Diplomarbeit gebaut.

Mit den mitgelieferten Tasks kann in VS Code das Bauen entweder über das Ausführen der Tasks geschehen oder mittels Hotkeys. Standardmäßig ist `Strg + Shift + B` für den Standarttask vorgesehen. Dieser ist bei den angegebenen Tasks `Build PDF`, welcher, wie der Name sagt, die PDF für die Diplomarbeit baut. Dazu gibt es auch einen Task, um die Rechtschreibprüfung zu starten.

Noch einfacher ist das Starten der Tasks mit der Erweiterung "Task Explorer". Dort können beide Tasks in einem separaten Fenster gefunden und gestartet werden. Die Tasks funktionieren jedoch nur wenn im Lokalen wsl alle benötigten Pakete installiert sind.

### Übertragen des aktuellen Standes nach GIT

Es mach Sinn, die Diplomarbeit (auch nach kleinen Änderungen) immer wieder nach GIT zu übertragen. Damit ist sie optimal gesichert und falls Ihre Teammitglieder auch an der DA Arbeiten bekommen sie auch Zugang zum aktuellsten Stand. Falls Sie noch nicht mit GIT gearbeitet haben, stellen die folgenden Absätze eine (ultra-) [Kurzeinführung](https://rogerdudler.github.io/git-guide/) dar.

Git ist ein dezentrales Quellcodeverwaltungssystem bei dem jeder Entwickler eine volständige eigene Kopie der Daten (=Repository) haben kann. Normalerweise holt man sich einen Initialstand von einem GIT Server indem man mittels `git clone https://github.com/schueler/Diplomarbeit_JAHR.git` das entsprechende Repositoiry lokal herkopiert.

Diesen kann man dann lokal bearbeiten und wenn man fertig ist, überprüft man mit `git status` welche Dateien sich geändert haben. Die geänderten Dateien (und auch solche die neu hinzukommen sollen) markiert man mittels `git add` gefolgt von den Dateinamen zur Übertragung.

Der Befehl `git commit -m "Aussagekräftige Commitmessage damit die anderen sehen was getan wurde"` speichert die zuvor markierten Änderungen in das lokale git Repository. Damit sind diese Daten für Sie erstmal gesichert.

Um diese Dateien dann noch an den Server (auf den nach korrekter Einstellung der Berechtigungen alle Zugriff haben) zu übertragen wendet man den folgenden Befehl an `git push origin master`. Damit werden alle lokalen Änderungen an den Server gesendet.

Jetzt kann es auch vorkommen, dass jemand anders ihnen zuvor gekommen ist, schon Änderungen an den Dateien vorgenommen hat, und diese für Sie am Server bereit liegen. (Anders betrachtet haben sie in diesem Moment eine 'alte' Version Ihrer Diplomarbeit). Diese neueste Version können Sie ganz einfach mit dem Befehl `git pull` in Ihr Repository übernehmen. Dabei werden sofort alle geänderten Dateien durch die neueren übersetzt und sie können von da weg weiterarbeiten.

Wenn Ihnen die Arbeit mit der Kommandozeile nicht so geheuer ist, dann können Sie (so Ihre Diplomarbeit unter Windows über das WLS erreichbar ist) auch einen grafischen GIT Client wie z.B: [TortoiseGit](https://tortoisegit.org/docs/tortoisegit/) verwenden.

Bei speziellen Fragen zu diesem Vorgehen wenden Sie sich bitte an den Programmierlehrer Ihres Vertrauens.

## Tipps & Tricks

### Generell

* Man kann To-Do Blöcke in die DA einfügen indem man folgenden Block verwendet `\todo{Was noch zu tun wäre}`. Diese erscheinen dann als Textblasen am Rand der Arbeit.

### Versionsverwaltung

* Vor dem Schreiben: Neuesten Stand vom Server holen
* Nach dem Schreiben: Sofort einchecken und zum Server übertragen
* Verwenden Sie sinnvolle commit Kommentare (Nachrichten)

### Text

* Grammatik + Rechtschreibung beachten (Markdown hat keinen Spell-checker integriert). ggf. `aspell` installieren und durch die Markdown files suchen lassen. Lassen Sie die Dateien remote bauen, so bekommen Sie den Output einer Rechtschreibprüfung mit übermittelt
* Irgendwie sollte man ein (einheitliches) Bild vom Leser haben. Negativbeispiel:
  * Einerseits ein Dummy dem man `mdir` erklären muss
  * andererseits kann er Python-Pakete installieren
* Formulierungen
  * keine romanartigen Erzählungen
  * keine seitenlange Installationsanweisungen (besser: Link auf Anleitung im Web)
* Es sollte einen roten Faden durch die Diplomarbeit geben. Dazu erklärt man zuerst die Grundlagen und Inhalte die ein gewöhnlicher Leser braucht um sich in Ihrer Arbeit zurchtzufinden (= zumeist Literaturrrecherche). Danach bauen Sie auf diesen Gurndlagen ihren praktischen Teil auf, welche dann zu einem Ergebnis führt. Aus diesem grund ist die DA üblicherweise wie folgt gegliedert:
  * Aufgabenstellung
  * Grundlagen
  * Praktischer Teil
  * Zusammenfassung
* Bei unterschiedlichen Teilaufgabenstellungen (die womöglich aufeinander aufbauen) ist die Reihenfolge der Ausarbeitungen so zu wählen dass der Leser diesen roten Faden nicht verliert ... Grundlagen zuerst !

### Quellcode

* mit Java/Python/*-doc
* Sinnvolle Namen verwenden
* Coderichtlinien einhalten
  * Klassen haben einen Outlline Kommentar (JavaDoc) in dem der Name des Autors und der Zweck der Klasse beschrieben wird
  * Funktionen / Methoden haben einen Outline Kommentar (JavaDoc) in dem beschrieben wird was die Methode macht und was die Parameter sowie der Rückgabetyp bedeuten
  * Gross & Kleinschreibung beachten
    * (final) Konstanten: GROSSBUCHSTABEN
    * packages: immer.mit.kleinbuchstaben
    * variablen: klein
    * Klassen: ErsterBuchstabeGross
  * Benennungen:
    * Funktionen: anhand eines verbs
    * Getter / Setter verwenden
    * Klassen: Einzahl
* Die Kennzeichnung von Quellcode in Markdown darf keine Unterstriche beinhalten

### Listings

* Wenige (aber dafür aussagekräftige) Listings verwenden
* am besten ohne Kommentare
  * dafür mit Erklärung im Text darüber oder darunter
  * mit Verweis auf File
* UML Diagramme als Ergänzung

### Bilder

* Machen Sie Grafiken in guter Auflösung (> 72 dpi) und Achten Sie darauf dass diese nicht zu viel oder zu wenig Inhalt zeigen weil die Grafiken auf max. Seitenbreite skaliert werden können und dann entweder zu klein oder zu pixelig erscheinen.
* Orientieren sie die Grafiken so wie sie selbige in der Arbeit haben wollen. Wenn sie Grafiken um 90 Grad drehen, dann drehen Sie alle Grafiken in die selbe Richtung !
* Quellenangaben nicht vergessen (bei selbst erstellen Bildern: 'Eigene Darstellung')
* Manchmal treten bei der Verwendung von Bildern Probleme auf. Hier ein best-of:
  * Die extension von Bildern ist wichtig! `MeinBild.drawio.png` kann verhindern das eine DA korrekt baut, während `MeinBild-drawio.png` zu passenden Ergebnissen führt.
  * Umlaute in Dateinamen verhinden auf manchen Maschinen das die Bilder eingefügt werden -> Also keine verwenden.
  * Groß und Kleinschreibung der Dateinamen ist wichtig. Am besten verwenden Sie nur kleinschreibung mit Bindestrichen.

### Quellenangaben

* alle Tools inkl. Versionsnummer
* alle Zitate bzw. übernommene Meinungen
* Datenblätter usw. können auch zitiert werden

### Künstliche Intlligenz

Es ist prinzipiell erlaubt KI im Rahmen der Diplomarbeit einzusetzen. Welche Art der KI, sowie für welchen Einsatzzweck sie diese verwendet haben halten Sie vollständig in der `metadata.yaml` fest, und diese Information unterschreiben Sie auch mit der eidesstattlichen Erklärung. Man unterscheidet dabei prinzipiell zwei Use-cases:

1. **Verwendung als Werkzeug (ähnlich einer Autokorrektur)**: In diesem Fall haben Sie selbst das Wissen generiert / recherchiert und es ist nicht notwendig das speziell in der Arbeit zu erwähnen. Dieser Anwendungsfall ist vergleichbar mit der Verwendung eines Taschenrechners. Dieser wird nur als Werkzeug verwendet und trägt inhaltlich nichts zur DA bei.
2. **Verwendung zur generierung von Wissen**: Sofern Sie irgendeine KI als Informationsquelle verwenden (also um neues Wissen zu generieren, aus Quellen zusammenzufassen o.Ä.), dann ist das in der Diplomarbeit speziell kennzuzeichnen. Dazu **müssen** Sie den kompletten Chatverlauf inkl. Prompt und Folgekorrespondenz als PDF an die Diplomarbeit anhängen, und diese korrekt in der `.bib` Datei als `@unpublished` referenzieren. Es ist dabei wichtig das der `short-prompt` (also eine art Kurzzusammenfassung des Prompts) in der metadata.yaml mit jenem im im finalen Literaturverzeichnis übereinstimmt. Ein Beispiel wie dabei vorzugehen ist, ist im DA Template enthalten.  

Code-generierungstools wie z.B.*Github Copilot* stellen hier einen Grenzfall dar. Sofern Sie Copilot dazu verwenden um z.B. Quellcode formatieren zu lassen oder kleine / lokale Anpassungen (im Sinne von Boilerplate code, oder Getter/Setter generierung) dann bedarf es zwar einer Nennung in der metadata.yaml, aber keine Nennung als Quelle. Sobald Sie mit dieser oder einer vergleichbaren KI neue Inhalte oer Ansätze generieren, ist die wie oben beschrieben zu dokumentieren.

Im Zweifelsfall behandeln Sie KI generierte Inhalte immer so als ob neues Wissen damit generiert wurde.

## Checkliste

Abschließend noch eine kurze Liste der wichtigsten Punkte, an denen erfahrungsgemäß die häufigsten Fehler auftreten. Diese Punkte bilden auch die Grundlage der routine-mäßigen Formbegutachtung an Universitäten.

* Titelseite: Länge des Titels (Zeilenumbrüche), Name, Studiengang, Datum -> Ausbessern in metadata.yaml
* Erklärung: vollständig mit Unterschrift.
* Inhaltsverzeichnis: balancierte Struktur, Tiefe, Länge der Überschriften.
* Kurzfassung/Abstract: präzise Zusammenfassung, passende Länge, gleiche Inhalte und Struktur.
* Überschriften:Länge, Stil, Aussagekraft.
* Typographie: sauberes Schriftbild, keine manuellen Abstände zwischen Absätzen oder Einrückungen, keine überlangen Zeilen, Hervorhebungen, Schriftgröße, Platzierung von Fußnoten.
* Interpunktion: Binde- und Gedankenstriche richtig gesetzt, Abstände nach Punkten (v. a. nach Abkürzungen)
* Abbildungen: Qualität der Grafiken und Bilder, Schriftgröße und -typ in Abbildungen, Platzierung von Abbildungen und Tabellen, Captions. Sind alle Abbildungen (und Tabellen) im Text referenziert?
* Gleichungen/Formeln:mathem. Elemente auch im Fließtext richtig gesetzt, explizite Gleichungen richtig verwendet, Verwendung von mathem. Symbolen.
* Quellenangaben:Zitate richtig referenziert, Seiten- oder Kapitelangaben bei gedruckten Medien
* Literaturverzeichnis: mehrfach zitierte Quellen nur einmal angeführt, Art der Publikation muss in jedem Fall klar sein, konsistente Einträge, Online-Quellen(URLs) sauber angeführt inkl. letztem Betrachtungszeitpunkt
* Sonstiges: ungültige Querverweise (??), Anhang, Papiergröße der PDF-Datei (A4=8.27×11.69Zoll), Druckgröße und -qualität.

**Authors:** Günther Hutter, Marko Schrempf
