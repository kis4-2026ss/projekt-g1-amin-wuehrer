## Results
* Dynamische Hindernisse können zur Laufzeit erzeugt werden
* Der Generator erstellt Hindernis Parameter wie Breite, Höhe, Abstand und Position
* Die Validierung prüft vor dem Spawn, ob ein Hindernis physikalisch schaffbar ist
* Unfaire oder unmögliche Hindernisse werden verworfen, bevor der Spieler sie sieht
* Nur erfolgreich validierte Hindernisse werden als Asteroiden mit Mesh und Collider gespawnt
* Prozedurale Geometrie erzeugt viele unterschiedliche Asteroiden, ohne viele fertige Assets zu benötigen
* Schwierigkeit kann über Geschwindigkeit, Difficulty Wert und Spawn Muster angepasst werden

## Learnings
* Wir hatten nicht die Ressourcen, um das ML Modell wirklich gut und zuverlässig zu trainieren
* Das Weltraum Runner Konzept bietet mehr Möglichkeiten zur Level Generierung als ein klassisches Dino Run Spiel
* Scaling wird vor allem dann zum Problem, wenn komplexere Texturen, 3D Modelle oder sehr viele unterschiedliche Texturen und Objekte verwendet werden
* Je komplexer die generierten Assets sind, desto schwieriger wird es, eine einheitliche Qualität, passende Auflösung und konsistente Darstellung im Spiel sicherzustellen
* Gute KI Level Generierung braucht nicht nur Erzeugung, sondern auch Kontrolle, Tests und Optimierung
