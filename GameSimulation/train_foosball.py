"""
DigiKicker Foosball RL Training Script

Verwendung:
    # Training im Godot Editor (dieses Skript starten, dann F5 in Godot mit RLTraining.tscn)
    python train_foosball.py

    # Training mit exportierter Executable
    python train_foosball.py --env_path DigiKicker/DigiKicker.exe

    # Training mit Visualisierung
    python train_foosball.py --env_path DigiKicker/DigiKicker.exe --viz

    # Schnelleres Training (8-fache Beschleunigung)
    python train_foosball.py --env_path DigiKicker/DigiKicker.exe --speedup 8

    # Training von Checkpoint fortsetzen
    python train_foosball.py --resume_model_path logs/foosball/model.zip

    # Export zu ONNX für Godot-Inferenz
    python train_foosball.py --resume_model_path logs/foosball/model.zip --onnx_export_path model.onnx --timesteps 0

WICHTIG: --use_sde für stabiles Training verwenden! Ohne diese Option kann die Policy-Standardabweichung explodieren.
"""

import argparse
import os
import pathlib
from typing import Callable

import numpy as np
from stable_baselines3 import PPO
from stable_baselines3.common.callbacks import BaseCallback, CheckpointCallback
from stable_baselines3.common.vec_env.vec_monitor import VecMonitor

from godot_rl.wrappers.onnx.stable_baselines_export import export_model_as_onnx
from godot_rl.wrappers.stable_baselines_wrapper import StableBaselinesGodotEnv


class StabilityMonitorCallback(BaseCallback):
    """
    Überwacht die Trainingsstabilität und warnt vor explodierender Policy-Standardabweichung.
    Ermöglicht außerdem frühzeitiges Stoppen bei zu hoher KL-Divergenz.
    """

    def __init__(self, check_freq: int = 1000, max_kl: float = 0.1, verbose: int = 1):
        """
        Initialisiert den Stabilitätsmonitor-Callback.

        Args:
            check_freq: Prüfintervall in Trainingsschritten
            max_kl: Maximale erlaubte KL-Divergenz
            verbose: Ausführlichkeitsgrad der Ausgabe
        """
        super().__init__(verbose)
        self.check_freq = check_freq
        self.max_kl = max_kl
        self.last_warning_step = 0

    def _on_step(self) -> bool:
        """
        Wird bei jedem Trainingsschritt aufgerufen und prüft die Stabilität.

        Returns:
            True, um das Training fortzusetzen, False zum Abbrechen
        """
        if self.n_calls % self.check_freq == 0:
            # Policy-Standardabweichung prüfen (nur ohne SDE relevant)
            if hasattr(self.model.policy, 'log_std'):
                log_std = self.model.policy.log_std.detach().cpu().numpy()
                std = np.exp(log_std)
                std_mean = np.mean(std)
                std_max = np.max(std)

                # Warnung ausgeben, wenn Standardabweichung zu hoch wird
                if std_mean > 5.0 and self.n_calls - self.last_warning_step > 10000:
                    print(f"\nWARNUNG: Policy-Standardabweichung ist hoch! Mittelwert={std_mean:.2f}, Maximum={std_max:.2f}")
                    print("   Verwenden Sie --use_sde für stabileres Training.")
                    self.last_warning_step = self.n_calls

                # Metriken in TensorBoard protokollieren
                self.logger.record("train/policy_std_mean", std_mean)
                self.logger.record("train/policy_std_max", std_max)

        return True


def parse_args():
    """
    Parst die Kommandozeilenargumente für das RL-Training.

    Returns:
        Tuple aus geparsten Argumenten und unbekannten Argumenten
    """
    parser = argparse.ArgumentParser(
        description="Trainiert einen PPO-Agenten für DigiKicker Foosball",
        allow_abbrev=False
    )
    parser.add_argument(
        "--env_path",
        default=None,
        type=str,
        help="Pfad zur exportierten Godot-Executable. Leer lassen für Editor-Training.",
    )
    parser.add_argument(
        "--experiment_dir",
        default="logs/foosball_v2",
        type=str,
        help="Verzeichnis für TensorBoard-Logs und Checkpoints.",
    )
    parser.add_argument(
        "--experiment_name",
        default="ppo_foosball",
        type=str,
        help="Experimentname für Logging.",
    )
    parser.add_argument(
        "--seed",
        type=int,
        default=42,
        help="Zufallsseed für Reproduzierbarkeit."
    )
    parser.add_argument(
        "--resume_model_path",
        default=None,
        type=str,
        help="Pfad zum .zip-Modell zum Fortsetzen des Trainings.",
    )
    parser.add_argument(
        "--save_model_path",
        default=None,
        type=str,
        help="Pfad zum Speichern des finalen Modells (.zip).",
    )
    parser.add_argument(
        "--save_checkpoint_frequency",
        default=50000,
        type=int,
        help="Checkpoint alle N Schritte speichern. Auf 0 setzen zum Deaktivieren.",
    )
    parser.add_argument(
        "--onnx_export_path",
        default=None,
        type=str,
        help="ONNX-Modell für Godot-Inferenz exportieren.",
    )
    parser.add_argument(
        "--timesteps",
        default=1_000_000,
        type=int,
        help="Gesamtanzahl Trainingsschritte.",
    )
    parser.add_argument(
        "--inference",
        action="store_true",
        help="Inferenz statt Training ausführen.",
    )
    parser.add_argument(
        "--viz",
        action="store_true",
        help="Spielfenster während des Trainings anzeigen.",
    )
    parser.add_argument(
        "--speedup",
        default=1,
        type=int,
        help="Physik-Beschleunigungsfaktor (1-8 empfohlen).",
    )
    parser.add_argument(
        "--n_parallel",
        default=1,
        type=int,
        help="Anzahl paralleler Umgebungen.",
    )
    parser.add_argument(
        "--learning_rate",
        default=1e-4,
        type=float,
        help="Lernrate (Standard: 1e-4, niedrigere Werte erhöhen Stabilität).",
    )
    parser.add_argument(
        "--n_steps",
        default=512,
        type=int,
        help="Schritte pro Rollout (pro Umgebung).",
    )
    parser.add_argument(
        "--batch_size",
        default=128,
        type=int,
        help="Größe der Minibatches.",
    )
    parser.add_argument(
        "--n_epochs",
        default=5,
        type=int,
        help="Anzahl der Epochen pro Update.",
    )
    parser.add_argument(
        "--use_sde",
        action="store_true",
        default=True,
        help="Verwendet State-Dependent Exploration (empfohlen für Stabilität).",
    )
    parser.add_argument(
        "--no_sde",
        action="store_true",
        help="Deaktiviert State-Dependent Exploration.",
    )
    parser.add_argument(
        "--lr_schedule",
        choices=["constant", "linear"],
        default="linear",
        help="Lernraten-Zeitplan (linearer Abfall empfohlen).",
    )

    return parser.parse_known_args()


def linear_schedule(initial_value: float) -> Callable[[float], float]:
    """
    Erstellt einen linearen Lernraten-Zeitplan, der gegen Ende des Trainings auf 0 abfällt.

    Args:
        initial_value: Initiale Lernrate zu Beginn des Trainings

    Returns:
        Funktion, die basierend auf verbleibendem Fortschritt die aktuelle Lernrate berechnet
    """
    def func(progress_remaining: float) -> float:
        return progress_remaining * initial_value
    return func


def main():
    """
    Hauptfunktion für das RL-Training: Erstellt die Umgebung, initialisiert das Modell
    und führt Training oder Inferenz aus.
    """
    args, _ = parse_args()

    # SDE-Flag auswerten
    use_sde = args.use_sde and not args.no_sde

    print("=" * 60)
    print("DigiKicker Foosball RL Training (v2 - Stabil)")
    print("=" * 60)

    # Argumente validieren
    if args.inference and args.resume_model_path is None:
        raise ValueError("--inference erfordert --resume_model_path")

    # Pfade einrichten
    checkpoint_dir = os.path.join(args.experiment_dir, f"{args.experiment_name}_checkpoints")

    if args.save_checkpoint_frequency > 0 and os.path.isdir(checkpoint_dir):
        print(f"Warnung: Checkpoint-Verzeichnis existiert bereits: {checkpoint_dir}")
        print("Checkpoints könnten überschrieben werden.")

    # Godot-Umgebung erstellen
    print(f"\nErstelle Umgebung...")
    print(f"  env_path: {args.env_path or 'Im-Editor'}")
    print(f"  speedup: {args.speedup}x")
    print(f"  n_parallel: {args.n_parallel}")

    env = StableBaselinesGodotEnv(
        env_path=args.env_path,
        show_window=args.viz,
        seed=args.seed,
        n_parallel=args.n_parallel,
        speedup=args.speedup
    )
    env = VecMonitor(env)

    print(f"  observation_space: {env.observation_space}")
    print(f"  action_space: {env.action_space}")

    # Lernrate (optional mit Zeitplan)
    lr = args.learning_rate
    if args.lr_schedule == "linear":
        lr = linear_schedule(args.learning_rate)
        print(f"  Verwende linearen LR-Zeitplan: {args.learning_rate} -> 0")

    # Modell erstellen oder laden
    if args.resume_model_path is None:
        print(f"\nErstelle neues PPO-Modell...")
        print(f"  learning_rate: {args.learning_rate}")
        print(f"  n_steps: {args.n_steps}")
        print(f"  batch_size: {args.batch_size}")
        print(f"  n_epochs: {args.n_epochs}")
        print(f"  use_sde: {use_sde}")

        # Policy-Parameter für stabiles Training
        policy_kwargs = dict(
            log_std_init=-1.0,  # Startet mit niedrigerer Standardabweichung (exp(-1) ≈ 0.37)
            net_arch=dict(pi=[256, 256], vf=[256, 256]),  # Getrennte Policy- und Value-Netzwerke
        )

        model = PPO(
            "MultiInputPolicy",
            env,
            learning_rate=lr,
            n_steps=args.n_steps,
            batch_size=args.batch_size,
            n_epochs=args.n_epochs,
            gamma=0.99,
            gae_lambda=0.95,
            clip_range=0.2,
            ent_coef=0.005,
            vf_coef=0.5,
            max_grad_norm=0.5,
            use_sde=use_sde,
            sde_sample_freq=4 if use_sde else -1,  # Rauschen alle 4 Schritte neu sampeln
            target_kl=0.03,  # Updates frühzeitig stoppen bei zu hoher KL-Divergenz
            verbose=1,
            tensorboard_log=args.experiment_dir,
            policy_kwargs=policy_kwargs,
        )
    else:
        print(f"\nLade Modell von: {args.resume_model_path}")
        model = PPO.load(
            args.resume_model_path,
            env=env,
            tensorboard_log=args.experiment_dir,
            learning_rate=lr,
        )

    # Inferenz oder Training ausführen
    if args.inference:
        print(f"\nFühre Inferenz für {args.timesteps} Schritte aus...")
        obs = env.reset()
        for i in range(args.timesteps):
            action, _state = model.predict(obs, deterministic=True)
            obs, reward, done, info = env.step(action)
            if i % 1000 == 0:
                print(f"  Schritt {i}/{args.timesteps}")
    else:
        print(f"\nStarte Training für {args.timesteps} Zeitschritte...")

        callbacks = []

        # Stabilitätsmonitor hinzufügen
        stability_callback = StabilityMonitorCallback(check_freq=1000)
        callbacks.append(stability_callback)

        if args.save_checkpoint_frequency > 0:
            print(f"  Checkpoints werden gespeichert in: {checkpoint_dir}")
            checkpoint_callback = CheckpointCallback(
                save_freq=max(args.save_checkpoint_frequency // env.num_envs, 1),
                save_path=checkpoint_dir,
                name_prefix=args.experiment_name,
            )
            callbacks.append(checkpoint_callback)

        try:
            model.learn(
                total_timesteps=args.timesteps,
                tb_log_name=args.experiment_name,
                callback=callbacks if callbacks else None,
                progress_bar=True,
            )
        except KeyboardInterrupt:
            print("\nTraining vom Benutzer unterbrochen.")
        finally:
            # Modell speichern
            if args.save_model_path:
                save_path = pathlib.Path(args.save_model_path).with_suffix(".zip")
                print(f"\nSpeichere Modell nach: {save_path}")
                model.save(save_path)
            else:
                # Automatisches Speichern
                auto_save_path = os.path.join(args.experiment_dir, f"{args.experiment_name}_final.zip")
                print(f"\nAutomatisches Speichern des Modells nach: {auto_save_path}")
                model.save(auto_save_path)

            # ONNX exportieren
            if args.onnx_export_path:
                onnx_path = pathlib.Path(args.onnx_export_path).with_suffix(".onnx")
                print(f"Exportiere ONNX nach: {onnx_path}")
                export_model_as_onnx(model, str(onnx_path))

            # Umgebung schließen (Verbindungsfehler bei Ctrl+C ignorieren)
            print("Schließe Umgebung...")
            try:
                env.close()
            except (ConnectionResetError, BrokenPipeError, OSError):
                pass  # Godot bereits geschlossen, ignorieren

    print("\nFertig!")


if __name__ == "__main__":
    main()
