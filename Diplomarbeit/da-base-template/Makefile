# Where are the 
SOURCEDIR=./example
STYLEDIR=$(CURDIR)/style
STAGINGDIR=$(CURDIR)/staging

INPUTDIR=$(STAGINGDIR)
OUTPUTDIR=$(STAGINGDIR)

BIBFILE=$(INPUTDIR)/literatur.bib
METADATAFILE = $(INPUTDIR)/metadata.yaml
OUTPUTFILE = $(OUTPUTDIR)/diplomarbeit.pdf
LOGFILE = $(OUTPUTFILE).log
SPELLERRORFILE = $(OUTPUTDIR)/spellcheck-results.txt

SHELL := /bin/bash

help:
	@echo ' 								                                   '
	@echo 'Makefile für Pandoc Markdown Vorlage                                '
	@echo 'für Diplomarbeiten der HTL Leoben                                   '
	@echo ' 								                                   '
	@echo 'Autor: Günther Hutter, Clemens Lauermann und Andreas Pötscher       '
	@echo '       basierend auf der Arbeit von Michael Heinemann               '
	@echo '       sowie auf dem Template der HTL Rennweg3                      '
	@echo ' 								                                   '
	@echo 'Änderungswünsche werden ausschließlich via issue und pullrequest    '
	@echo 'Usage:                                                              '
	@echo '   make pdf                   generate a PDF file  	               '
	@echo '   make spellcheck            checks the output for misspelled words'
	@echo '                                                                    '
	@echo ' 																	


# Helper Targets
build-stage:
    # Kopieren der DA-Quellen in das staging Verzeichnis
	@echo "Source directory = $(SOURCEDIR)"
	@echo "Copying source files into the staging directory"
	@rsync -av $(SOURCEDIR)/ $(STAGINGDIR) \
	--cvs-exclude \
	--exclude=HTLLE-DA-Vorlage \
	--exclude=$(OUTPUTFILE) \
	--exclude=$(LOGFILE) \
	--quiet

    # Style in das staging Verzeichnis mergen
	@echo "Merging style files into the staging directory"
	@rsync -av $(STYLEDIR)/ $(STAGINGDIR)/style --quiet

    # Prüfen auf Dateien mit Unterstrichen im Namen
	@echo "Checking for files containing underscores in $(STAGINGDIR)..."
	@files=$$(find $(STAGINGDIR) -type f -name '*_*'); \
	if [ -n "$$files" ]; then \
		echo "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; \
		echo "WARNING: Found files with underscores:"; \
		echo " ->  $$files"; \
		echo "These files are very likely to cause issues during the build process"; \
		echo "Remove or rename them to build the thesis safely"; \
		echo "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"; \
	fi

compile-output:
    # Bauen der Arbeit aus dem staging verzeichnis heraus
	@echo "Compiling the thesis into: $(OUTPUTFILE)"

    # Builden mit pandoc. Als Basisverzeichnis springen wir ins staging. 
    # Damit sollten alle relativen Links stimmen
	-@cd staging && pandoc "$(STAGINGDIR)"/*.md "$(STAGINGDIR)/style/"*.md *.yaml \
	-o "$(OUTPUTFILE)" \
	--template="$(STAGINGDIR)/style/template.tex" \
    --bibliography="$(BIBFILE)" 2>"$(LOGFILE)" \
	--csl="$(STAGINGDIR)/style/htlle-diplomarbeit.csl" \
	--highlight-style=pygments \
	--listings \
	--metadata link-citations=true \
	-N 

    # Ausgabe der Warnungen 
	@cat $(LOGFILE)

    # Build erfolgreich -> Resultate zurückkopieren
	@rsync -az $(LOGFILE) $(SOURCEDIR)/
	@rsync -az $(OUTPUTFILE) $(SOURCEDIR)/
	
do-spellcheck:

	@echo "Performing a spellcheck on all Markdown files"

    # Überprüft die staging Dateien auf Rechtschreibfehler
	@hunspell -d de_AT,de_AT_frami,en_US -u3 -t "$(STAGINGDIR)"/*.md > "$(SPELLERRORFILE)"

    # Ausgabe der Rechtschreibfehler
	@echo '-------------------8<-------------------------'
	#@cat "$(SPELLERRORFILE)" | sort | uniq | sed 's/^/   /'
	@ls "$(STAGINGDIR)"/*.md | xargs -I {} -r0 echo "Working on "{}" file ..." | xargs -I {} -r0 echo "{}" 

	@echo '------------------->8-------------------------'

    # Spell resultate zurueckkopieren
	@rsync -az $(SPELLERRORFILE) $(SOURCEDIR)/

remove-stage:
    #Remove the staging directory
	@echo "Removing the staging directory"
	@rm -rf $(STAGINGDIR)


# Targets which are intended to be used directly
spellcheck: build-stage do-spellcheck remove-stage

# Erzeugen einer PDF Datei
pdf: build-stage compile-output remove-stage

# Ziel für die Erzeugung einer .tex Datei
tex: OUTPUTFILE := $(OUTPUTFILE).tex
tex: build-stage compile-output remove-stage
 
clean: 
    # Remove the staging directory
	@rm -rf $(STAGINGDIR)

# Special Targets
.PHONY: help pdf clean
