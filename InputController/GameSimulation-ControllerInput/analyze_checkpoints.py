"""
Analysiert Checkpoints, um herauszufinden, wo das Training instabil wurde.
Untersucht den log_std-Parameter der Policy, um Explosionen zu erkennen.
"""
import os
import numpy as np
from stable_baselines3 import PPO

def analyze_checkpoint(model_path: str) -> dict:
    """
    Analysiert die Policy-Parameter eines Checkpoints.

    Args:
        model_path: Pfad zur Checkpoint-Datei (.zip)

    Returns:
        Dictionary mit Analyseergebnissen (Standardabweichungen, Status)
    """
    try:
        model = PPO.load(model_path, device="cpu")

        # Log_std-Parameter abrufen (steuert Aktions-Zufälligkeit)
        log_std = model.policy.log_std.detach().numpy()
        std = np.exp(log_std)

        # Testvorhersage durchführen
        dummy_obs = {"obs": np.random.uniform(-1, 1, (1, 20)).astype(np.float32)}
        action, _ = model.predict(dummy_obs, deterministic=True)

        return {
            "path": model_path,
            "log_std_mean": float(np.mean(log_std)),
            "log_std_max": float(np.max(log_std)),
            "std_mean": float(np.mean(std)),
            "std_max": float(np.max(std)),
            "action_valid": not (np.isnan(action).any() or np.isinf(action).any()),
            "status": "OK" if np.mean(std) < 10 else "UNSTABLE"
        }
    except Exception as e:
        return {"path": model_path, "error": str(e), "status": "ERROR"}

def main():
    """
    Hauptfunktion: Durchsucht Checkpoint-Verzeichnis und analysiert alle gespeicherten Modelle
    auf Trainingsstabilität.
    """
    checkpoint_dir = "logs/foosball/ppo_foosball_checkpoints"

    # Alle Checkpoints sammeln
    checkpoints = []
    for f in os.listdir(checkpoint_dir):
        if f.endswith(".zip"):
            try:
                steps = int(f.split("_")[-2])
                checkpoints.append((steps, os.path.join(checkpoint_dir, f)))
            except:
                pass

    checkpoints.sort()  # Nach Schritten sortieren (älteste zuerst)

    print("Analysiere Checkpoints auf Stabilität...\n")
    print(f"{'Schritte':>12} | {'std_mean':>12} | {'std_max':>12} | Status")
    print("-" * 60)

    last_stable = None
    first_unstable = None

    # Checkpoints sampeln (alle 500k Schritte zur Beschleunigung)
    sampled = [(s, p) for s, p in checkpoints if s % 500000 == 0 or s < 500000]

    for steps, path in sampled:
        result = analyze_checkpoint(path)

        if "error" in result:
            print(f"{steps:>12,} | FEHLER: {result['error'][:30]}")
        else:
            status = result["status"]
            std_mean = result["std_mean"]
            std_max = result["std_max"]

            # Formatierung basierend auf Größenordnung
            if std_mean > 1e6:
                std_str = f"{std_mean:.2e}"
                max_str = f"{std_max:.2e}"
            else:
                std_str = f"{std_mean:.4f}"
                max_str = f"{std_max:.4f}"

            print(f"{steps:>12,} | {std_str:>12} | {max_str:>12} | {status}")

            if status == "OK" and result["action_valid"]:
                last_stable = (steps, path)
            elif status == "UNSTABLE" and first_unstable is None:
                first_unstable = (steps, path)

    print("\n" + "=" * 60)

    if last_stable:
        print(f"\nLETZTER STABILER CHECKPOINT: {last_stable[0]:,} Schritte")
        print(f"  Pfad: {last_stable[1]}")
        print(f"\n  Um diesen als ONNX zu exportieren:")
        print(f'  python export_onnx.py --model_path "{last_stable[1]}"')
        print(f"\n  Um Training hiervon fortzusetzen:")
        print(f'  python train_foosball.py --resume_model_path "{last_stable[1]}" --learning_rate 0.0001')

    if first_unstable:
        print(f"\nERSTER INSTABILER CHECKPOINT: {first_unstable[0]:,} Schritte")

if __name__ == "__main__":
    main()
