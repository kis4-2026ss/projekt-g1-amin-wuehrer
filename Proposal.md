
# Project Proposal: On-the-fly Level-Erstellung und Validierung mit KI

## 1. Goal of the Project

**High-Level Goal:**
Entwicklung eines Endless-Runner-Prototyps, dessen Spielwelt zu 100% on-the-fly durch generative KI-Modelle erstellt wird. Das System verzichtet vollständig auf vorgefertigte Hindernis-Assets (Prefabs). Stattdessen generiert ein KI-Modell permanent neue, einzigartige Kollisionsgeometrien (2D-Polygone), während ein zweiter KI-Agent diese simultan auf Spielbarkeit validiert.

**Validation:**
Das Projekt gilt als erfolgreich, wenn:
*   Das System im laufenden Betrieb neue, nicht-traversierbare Formen generiert, die sich in Höhe, Breite und Komplexität unterscheiden.
*   Der Validierungs-Agent innerhalb von <150ms entscheidet, ob die erfundene Form mit der aktuellen Spieler-Geschwindigkeit physisch überwindbar ist.
*   Ein flüssiger Spielablauf ohne spürbare Latenz durch die Hintergrund-Berechnungen der KI-Inferenz gewährleistet ist.
*   Das Spiel muss für mehrere Minuten Spielbar sein ohne dass ein unüberwindbares Hinderniss kommt.

**System/Feature to develop:**
Ein **"Generative-Adversarial-Validation" (GAV)** Framework innerhalb der Unity Engine. Es besteht aus zwei Kernkomponenten:
1.  **Modul A (Generator AI):** Ein neuronales Netz, das Vertex-Daten für neue 2D-Formen erzeugt.
2.  **Modul B (Validator AI):** Ein Reinforcement Learning Agent, der diese Formen in einer Schatten-Simulation "vorspielt".

**AI Assistance in Development:**
*   **Unity Sentis:** Zur Ausführung des Generator-Modells (ONNX) direkt auf der GPU des Endnutzers.
*   **Unity ML-Agents:** Zur Ausbildung des Validierungs-Agenten in einer beschleunigten Trainingsumgebung.
*   **GitHub Copilot / Gemini:** Zur Unterstützung beim Schreiben der prozeduralen Mesh-Generierungsskripte in C#.

---

## 2. Architecture Diagram (Description)

<img width="5923" height="7135" alt="GAV Framework Validation-2026-05-08-124939" src="https://github.com/user-attachments/assets/febdf13c-df03-48db-8a60-d2b53ff1f5e4" />

---

## 3. Project Plan

| Fokus | Tasks |
| :--- | :--- |
| **Basics & Prototyping** | Aufsetzen des Unity-Projekts; Implementierung des Player Controllers; Entwicklung der C#-Logik zur Mesh-Erzeugung aus Vertex-Arrays. |
| **Generator AI** | Design und Training eines Modells (z. B. VAE), das mathematisch valide 2D-Formen ausgibt; Export nach ONNX und Integration via Unity Sentis. |
| **Validator AI** | Training eines ML-Agenten auf die Bewältigung unbekannter, dynamisch generierter Geometrien; Aufbau der "Shadow-Scene" zur Vorab-Prüfung. |
| **Integration & Optimierung** | Verknüpfung beider KIs; Implementierung von asynchronen Prozessen (`Task.Run`), um Framerate-Einbrüche während der Inferenz zu verhindern. |
