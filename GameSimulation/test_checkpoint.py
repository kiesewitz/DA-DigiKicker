"""
Skript zum Testen, ob ein Checkpoint-Modell gültige (nicht-NaN) Ausgaben produziert.
Verwendung: python test_checkpoint.py <checkpoint_pfad>
"""
import sys
import numpy as np
from stable_baselines3 import PPO

def test_checkpoint(model_path: str) -> bool:
    """
    Testet, ob ein Modell-Checkpoint gültige Ausgaben produziert.

    Args:
        model_path: Pfad zur Checkpoint-Datei

    Returns:
        True, wenn das Modell gültige Aktionen ausgibt, sonst False
    """
    print(f"\nTeste: {model_path}")

    try:
        model = PPO.load(model_path, device="cpu")
        print("  Modell erfolgreich geladen")

        # Dummy-Observation erstellen (20 Floats gemäß FoosballAIController)
        dummy_obs = {
            "obs": np.random.uniform(-1, 1, (1, 20)).astype(np.float32)
        }

        # Aktion vorhersagen
        action, _ = model.predict(dummy_obs, deterministic=True)

        # Auf NaN in Aktion prüfen
        if np.isnan(action).any():
            print(f"  FEHLGESCHLAGEN: Aktion enthält NaN: {action}")
            return False

        if np.isinf(action).any():
            print(f"  FEHLGESCHLAGEN: Aktion enthält Inf: {action}")
            return False

        print(f"  OK: Aktion = {action.flatten()}")
        return True

    except Exception as e:
        print(f"  FEHLER: {e}")
        return False

def main():
    """
    Hauptfunktion: Testet entweder einen spezifischen Checkpoint oder durchsucht
    das Checkpoint-Verzeichnis nach funktionierenden Modellen.
    """
    if len(sys.argv) > 1:
        # Spezifischen Checkpoint testen
        test_checkpoint(sys.argv[1])
    else:
        # Mehrere Checkpoints testen (neueste zu älteste)
        import os
        checkpoint_dir = "logs/foosball/ppo_foosball_checkpoints"

        # Alle Checkpoints sammeln
        checkpoints = []
        for f in os.listdir(checkpoint_dir):
            if f.endswith(".zip"):
                # Schrittzahl extrahieren
                try:
                    steps = int(f.split("_")[-2])
                    checkpoints.append((steps, os.path.join(checkpoint_dir, f)))
                except:
                    pass

        # Nach Schritten sortieren (neueste zuerst)
        checkpoints.sort(reverse=True)

        print("Teste Checkpoints von neuesten zu ältesten...\n")

        # Testen, bis wir einen funktionierenden finden
        working_checkpoints = []
        for steps, path in checkpoints:
            if test_checkpoint(path):
                working_checkpoints.append((steps, path))
                print(f"\n  Funktionierenden Checkpoint gefunden bei {steps:,} Schritten!")
                # Ein paar mehr testen, um den besten zu finden
                if len(working_checkpoints) >= 3:
                    break

        if working_checkpoints:
            print("\n" + "="*60)
            print("FUNKTIONIERENDE CHECKPOINTS (neueste zuerst):")
            for steps, path in working_checkpoints:
                print(f"  {steps:>12,} Schritte: {path}")
            print("\nEmpfehlung: Verwenden Sie den neuesten funktionierenden Checkpoint")
            print(f"  python export_onnx.py --model_path \"{working_checkpoints[0][1]}\"")
        else:
            print("\nKeine funktionierenden Checkpoints gefunden!")

if __name__ == "__main__":
    main()
