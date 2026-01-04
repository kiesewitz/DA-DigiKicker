"""
Untersucht die Struktur eines ONNX-Modells und testet die Inferenz.
"""
import onnx
import onnxruntime as ort
import numpy as np

MODEL_PATH = "DigiKicker/models/foosball_ai.onnx"

print("=" * 60)
print("ONNX-Modell-Inspektion")
print("=" * 60)

# Modell mit ONNX laden
print("\n1. Lade Modell mit onnx...")
try:
    model = onnx.load(MODEL_PATH)
    print(f"   IR-Version: {model.ir_version}")
    print(f"   Opset-Version: {model.opset_import[0].version}")
except Exception as e:
    print(f"   Fehler: {e}")

# Inspektion mit ONNX Runtime
print("\n2. Lade mit ONNX Runtime...")
try:
    session = ort.InferenceSession(MODEL_PATH)

    print("\n   EINGABEN:")
    for inp in session.get_inputs():
        print(f"   - Name: {inp.name}")
        print(f"     Form: {inp.shape}")
        print(f"     Typ: {inp.type}")

    print("\n   AUSGABEN:")
    for out in session.get_outputs():
        print(f"   - Name: {out.name}")
        print(f"     Form: {out.shape}")
        print(f"     Typ: {out.type}")

    # Inferenz testen
    print("\n3. Teste Inferenz...")
    inputs = {}
    for inp in session.get_inputs():
        # Dummy-Eingabe erstellen
        shape = [1 if isinstance(d, str) else d for d in inp.shape]
        if 'float' in inp.type.lower():
            inputs[inp.name] = np.zeros(shape, dtype=np.float32)
        else:
            inputs[inp.name] = np.zeros(shape, dtype=np.float32)
        print(f"   Eingabe '{inp.name}': Form={shape}")

    outputs = session.run(None, inputs)
    print(f"\n   Ausgabe-Formen: {[o.shape for o in outputs]}")
    print("   Inferenz erfolgreich!")

except Exception as e:
    print(f"   Fehler: {e}")
    import traceback
    traceback.print_exc()

print("\n" + "=" * 60)
