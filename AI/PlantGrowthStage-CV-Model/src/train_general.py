# pyright: reportPrivateImportUsage=false
from ultralytics import YOLO
import torch, os, shutil, yaml, argparse

# Paths
BASE_DIR    = os.path.expanduser("~/PlantGrowthStage-CV-Model")
DATA_DIR    = os.path.join(BASE_DIR, "data", "general")
WEIGHTS_DIR = os.path.join(BASE_DIR, "models", "weights")
os.makedirs(WEIGHTS_DIR, exist_ok=True)

# Fix data.yaml paths
yaml_path = os.path.join(DATA_DIR, "data.yaml")
with open(yaml_path, "r") as f:
    cfg = yaml.safe_load(f)

cfg["train"] = os.path.join(DATA_DIR, "train", "images")
cfg["val"]   = os.path.join(DATA_DIR, "valid", "images")
cfg["test"]  = os.path.join(DATA_DIR, "test",  "images")

with open(yaml_path, "w") as f:
    yaml.dump(cfg, f, allow_unicode=True)

print("data.yaml updated")

# Args
parser = argparse.ArgumentParser()
parser.add_argument('--resume', action='store_true', help='Resume training from last checkpoint')
args = parser.parse_args()

# Training config
MODEL_SIZE = "yolov8n.pt"
EPOCHS     = 50
BATCH      = 16
IMG_SIZE   = 640
PATIENCE   = 15

def main():
    print(f"\n GPU: {torch.cuda.get_device_name(0)}")
    print(f"   VRAM: {torch.cuda.get_device_properties(0).total_memory / 1e9:.1f} GB")

    if args.resume:
        # checkpoint
        last_weights = os.path.join(BASE_DIR, "runs", "general", "train", "weights", "last.pt")
        if not os.path.exists(last_weights):
            print("No checkpoint, Start from beginning")
            model = YOLO(MODEL_SIZE)
        else:
            print(f"Resuming from: {last_weights}")
            model = YOLO(last_weights)
    else:
        model = YOLO(MODEL_SIZE)

    results = model.train(
        data       = yaml_path,
        epochs     = EPOCHS,
        batch      = BATCH,
        imgsz      = IMG_SIZE,
        patience   = PATIENCE,
        device     = 0,
        project    = os.path.join(BASE_DIR, "runs", "general"),
        name       = "train",
        exist_ok   = True,
        plots      = True,
        save       = True,
        workers    = 4,
        cache      = False,
        amp        = True,
        resume     = args.resume,
        verbose    = True,
    )

    # Save best weights
    best = os.path.join(BASE_DIR, "runs", "general", "train", "weights", "best.pt")
    dest = os.path.join(WEIGHTS_DIR, "general_best.pt")
    if os.path.exists(best):
        shutil.copy(best, dest)
        print(f"\n Best weights saved → {dest}")

if __name__ == "__main__":
    main()