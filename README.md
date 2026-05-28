# sopra05 

* [Sopra Wiki](https://sopranium.de) 
* Mailingliste sopra05@informatik.uni-freiburg.de

## Gruppe

Matteo, Taha, Olivia, Marlene, Marius, Daniel

Gruppentreffen: Dienstag, 12-14 Uhr, SR 02-017, Geb. 052

* Product Owner: Marius, Matteo
* Architektur: Taha, Matteo
* Qualitätssicherung: Taha (Sprint 06)

## Discord

https://discord.gg/NaMaGvaWBc

## Google Docs

https://docs.google.com/document/d/1_kEdXOlSozcvY5AZxWhq__W8vdzq7essS5Nt4PvE67Q/edit?usp=sharing

## Definition of Done

* Das Item ist in Gitea geschlossen.
* Im Item sind die geschätzte und die tatsächliche Arbeitszeit eingetragen.
* Alle für das Item relevanten Dateien sind im aktuellen Stand des remote release Branch integriert.
* Der Tutor hat die Fertigstellung des Items im Sprint Review anhand des aktuellen Standes des remote release Branch bestätigt.
* Alle Gruppenmitglieder sollen vor jedem Sprintmeeting mindestens ein Issue erstellen
* Product Owner merged Pull request um 10:00 Uhr vor dem Meeting

## Hinweise

* Laden Sie keine kompilierten Binärdateien hoch (z.B.: `*.exe`, `*.bin`). Ausname hier sind _nur_ die Archive für die Abgaben.
* Laden Sie keine benutzerspezifischen Dateien hoch (Zum Beispiel `*.suo`, `*.user`)
* Geben Sie jedem Commit eine aussagekräftige Nachricht. Verweisen Sie wenn möglich auf den Issue und sagen Sie was Sie geändert haben.
* Laden Sie keine Änderungen am Programm hoch, die nicht kompillieren (falls es nötig ist kaputten Code zu teilen, benutze einen `wip/<somename>`-branch)
* Ändern sie bei Dateinamen und Ordnern niemals nur die Groß/Kleinschreibug, da dies Git+Windows völlig verwirrt.

---
---
---

# SoPra Do's and Don'ts

## Disclaimer
- Dieser Abschnitt wurde nicht von den Dozenten erstellt und könnte daher unvollständig oder fehlerhaft sein.
- Für exakte Informationen bitte das offizielle <a href="https://sopranium.de/Hauptseite">Wiki</a> verwenden.

## Grundlagen SoPra
- Fehlzeiten gelten nur mit Attest oder offizieller Bescheinigung als entschuldigt. Ohne Nachweis wird die Abwesenheit als unentschuldigt gewertet.
- Der Tutor ist für die organisatorische Betreuung zuständig. Die inhaltliche Arbeit und Koordination übernimmt die Gruppe selbst.
- **Issues zuweisen/Estimation**: Diese erfolgen ausschließlich im Meeting. ~~Ausnahmen sind möglich, z.B.:~~
    - ~~Wenn ein Issue deutlich schneller bearbeitet wurde als geschätzt (z.B. 30 Minuten statt der geplanten 8 Stunden), kann man die Gruppe fragen, ob man ein weiteres Issue aus dem Backlog übernehmen kann.~~
    - ~~Bugs sollten sofort behoben werden. Dafür ist ein entsprechendes Issue anzulegen.~~
    
    ~~Alle Ausnahmen müssen mit einem signifikanten Teil der Gruppe abgesprochen sein.~~

## Arbeitsablauf: Wie arbeitet man?
1. **Issue gründlich lesen**: Versteht das Ziel des Issues. Wenn Unklarheiten bestehen, fragt nach.
2. **Code aktualisieren**: Führt einen Pull durch, um den neuesten Stand zu erhalten (<a href="https://sopranium.de/GitWorkflow">GitWorkflow</a>).
3. **An dem Issue arbeiten**: Bearbeitet das Issue vollständig, bis es fehlerfrei läuft. Es dürfen keine Codefehler (ERRORs) committed oder gepusht werden.
    - Falls ihr nicht weiterkommt, fragt in der Gruppe nach Unterstützung.
4. **Commit**: Commitet mit einer aussagekräftigen Nachricht, die beschreibt, was geändert wurde. In der Commit-Nachricht könnt ihr das zugehörige Issue mit `#Nummer` referenzieren (z.B. `#15`) und es gegebenenfalls mit `closes #15` direkt schließen ([GitWorkflow](https://sopranium.de/GitWorkflow)).
5. **Erneut pullen**: Aktualisiert den Code nach dem Commit, um sicherzustellen, dass keine Konflikte bestehen (<a href="https://sopranium.de/GitWorkflow">GitWorkflow</a>).
6. **Merge-Konflikte lösen**: Falls Konflikte auftreten, behebt diese. (<a href="https://sopranium.de/GitWorkflow">GitWorkflow</a>).
7. **Push**: Push den finalen Code.
8. **Zeit loggen**: Loggt die aufgewendete Zeit im Issue und fügt bei Bedarf einen Kommentar hinzu.
9. **Issue gegebenenfalls schließen**: Schließt das Issue nach erfolgreicher Bearbeitung.
10. **Neue Issues erstellen**: Sollten durch eure Arbeit neue Bugs oder Aufgaben entstanden sein, erstellt dafür neue Issues.

## Wie erstellt man ein Issue?
1. **Überschrift**: Verfasst eine klare, prägnante Überschrift.
2. **Details als Kommentar**: Beschreibt die Kriterien, die erfüllt sein müssen, damit das Issue geschlossen werden kann.
3. **Kein Estimation/Meilenstein**: Diese werden erst im Sprint-Meeting festgelegt, ~~es sei denn, es handelt sich um eine Ausnahme.~~
4. **Backlog**: Alle nicht zugewiesenen Issues bilden das Backlog, das Aufgaben enthält, die in der nächsten Zeit erledigt werden müssen.
5. **Im Meeting**: Hier werden die Schätzungen (Estimation), die zuständige Person und der Meilenstein (Sprint) festgelegt.

## Issue nicht bis zum Meeting erledigt?
- Die betroffene Person erhält Punktabzug, abhängig von den Schätzungen für die anderen zu bearbeitenden Issues.
- Das Issue wird der Person entzogen (sie soll aber trotzdem die geleistete Zeit loggen, bekommt jedoch die Schätzung nicht gutgeschrieben).
- Das Issue wird zurück ins Backlog geschoben.
- Wann ein Issue als abgeschlossen gilt, ist oft eine Ermessensfrage:
    - **Gruppenentscheidung**: Wenn die Gruppe der Meinung ist, dass ein Issue nicht korrekt bearbeitet wurde, kann es als nicht erledigt gewertet werden.
    - **Tutor-Entscheidung**: Ein Issue gilt als unvollständig, wenn:
        - Keine Zeit erfasst wurde.
        - Das Issue nicht geschlossen wurde.
        - Kein aussagekräftiger Kommentar bei Recherche- oder Textaufgaben hinzugefügt wurde.
        - Teile aus dem zu bearbeitenden Issue unbegründet weggelassen wurden.

## Product Owner
- Behält das Backlog und die <a href="https://sopranium.de/Hauptseite">Meilensteine</a> im Blick.
- Erstellt jede Woche neue Issues, die auf dem GDD basieren, und fügt sie dem Backlog hinzu, um den nächsten <a href="https://sopranium.de/Hauptseite">Meilenstein</a> zu erreichen.
- **Vor dem Meeting**:
    - Merged den Master-Branch in den Release-Branch (<a href="https://sopranium.de/Gitea">Gitea</a>).
    - Schließt den aktuellen Sprint-Meilenstein in Gitea.
    - Bereitet den neuesten Build für die Review vor.
- **Im Meeting**:
    - Stellt einen Laptop zur Verfügung um die Änderungen Reviewen zu können
    - Verteilt die Issues an die Gruppe und schätzt zusammen mit der Gruppe den Aufwand (Planning Poker).

## Developer
- Im Meeting muss jeder neue Code, der geschrieben wurde, innerhalb von 5 Minuten im Release-Build gezeigt werden können (Code wird ignoriert).
- Jeder kann Issues unter der Woche erstellen; diese werden jedoch zunächst ins Backlog verschoben.
