import struct
import select
import threading
import json
from collections import defaultdict
import paho.mqtt.client as mqtt

MOUSE_PATHS = [
    "/dev/input/mouse0",
    "/dev/input/mouse1",
    "/dev/input/mouse2",
    "/dev/input/mouse3",
]

BROKER_HOST = "10.0.0.5"
BROKER_PORT = 1883
TOPIC_PREFIX = "digikicker/mouse"

state = defaultdict(lambda: (0, 128, 128))
state_lock = threading.Lock()
dirty = set()

client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id="raspberry-mice")
client.connect(BROKER_HOST, BROKER_PORT, keepalive=60)
client.loop_start()

def read_mice(files):
    """Mäuse per select() nicht-blockierend einlesen."""
    while True:
        readable, _, _ = select.select(files, [], [], 0.1)
        for f in readable:
            idx = files.index(f)
            data = f.read(3)
            if len(data) == 3:
                button, x, y = struct.unpack('BBB', data)
                with state_lock:
                    state[idx] = (button, x, y)
                    dirty.add(idx)
def report():
    """Nur geänderte Zustände publishen, danach State zurücksetzen."""
    while True:
        with state_lock:
            to_send = {idx: state[idx] for idx in dirty}
            dirty.clear()
            for idx in to_send:
                state[idx] = (0, 128, 128)

        for idx, (button, x, y) in to_send.items():
            payload = json.dumps({
                "id":     idx,
                "button": button,
                "x":      x,
                "y":      y
            })
            topic = f"{TOPIC_PREFIX}/{idx}"
            client.publish(topic, payload, qos=0)

        threading.Event().wait(0.02)

files = [open(p, "rb") for p in MOUSE_PATHS]
reader   = threading.Thread(target=read_mice, args=(files,), daemon=True)
reporter = threading.Thread(target=report,    daemon=True)
reader.start()
reporter.start()
reader.join()