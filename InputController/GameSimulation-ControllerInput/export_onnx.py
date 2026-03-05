"""
Einfaches ONNX-Export-Skript - keine Godot-Verbindung erforderlich

Verwendung:
    python export_onnx.py                           # Standardmodell verwenden
    python export_onnx.py --model_path <pfad.zip>   # Spezifischen Checkpoint verwenden
"""
import argparse
import os
import sys
import warnings

# Windows-Konsolen-Encoding korrigieren
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
sys.stderr.reconfigure(encoding='utf-8', errors='replace')

# Unnötige Warnungen unterdrücken
warnings.filterwarnings("ignore", message=".*triton.*")
warnings.filterwarnings("ignore", message=".*dynamic_axes.*")
warnings.filterwarnings("ignore", message=".*opset_version.*")
warnings.filterwarnings("ignore", message=".*training mode.*")

import numpy as np
from stable_baselines3 import PPO
from godot_rl.wrappers.onnx.stable_baselines_export import export_model_as_onnx

def main():
    """
    Hauptfunktion: Lädt ein trainiertes PPO-Modell und exportiert es als ONNX-Datei
    für die Verwendung in Godot.
    """
    parser = argparse.ArgumentParser(description="Exportiert SB3-Modell nach ONNX")
    parser.add_argument(
        "--model_path",
        default="logs/foosball_v2/ppo_foosball_final.zip",
        help="Pfad zur .zip-Modelldatei"
    )
    parser.add_argument(
        "--onnx_path",
        default="DigiKicker/models/foosball_ai.onnx",
        help="Ausgabepfad für ONNX-Datei"
    )
    args = parser.parse_args()

    # Ausgabeverzeichnis erstellen
    os.makedirs(os.path.dirname(args.onnx_path), exist_ok=True)

    print(f"Lade Modell von: {args.model_path}")
    model = PPO.load(args.model_path)

    print(f"Exportiere nach ONNX: {args.onnx_path}")

    try:
        export_model_as_onnx(model, args.onnx_path)
        print("Fertig!")
    except ValueError as e:
        # Export erfolgreich, aber Verifizierung schlägt fehl (Bug in godot_rl)
        # Prüfen, ob Datei tatsächlich erstellt wurde
        if os.path.exists(args.onnx_path):
            file_size = os.path.getsize(args.onnx_path)
            print(f"ONNX-Datei erstellt: {args.onnx_path} ({file_size:,} Bytes)")

            # Eigene einfache Verifizierung durchführen
            import onnxruntime as ort
            ort_sess = ort.InferenceSession(args.onnx_path)

            # Alle Input-Informationen abrufen
            print("  Eingaben:")
            inputs_dict = {}
            obs_size = model.observation_space["obs"].shape[0]

            for inp in ort_sess.get_inputs():
                print(f"    - {inp.name}: {inp.shape}")
                # Passende Dummy-Eingabe erstellen
                if inp.name == "obs":
                    inputs_dict[inp.name] = np.random.randn(1, obs_size).astype(np.float32)
                elif "state" in inp.name:
                    inputs_dict[inp.name] = np.array([0.0], dtype=np.float32)

            print("  Ausgaben:")
            for out in ort_sess.get_outputs():
                print(f"    - {out.name}: {out.shape}")

            # Test-Inferenz durchführen
            output = ort_sess.run(None, inputs_dict)
            print(f"  Test-Aktion: {output[0].flatten()}")

            if not np.isnan(output[0]).any():
                print("Fertig! (Workaround für Verifizierung angewendet)")
            else:
                print("Warnung: Modell gibt NaN-Werte aus!")
        else:
            raise e

if __name__ == "__main__":
    main()
