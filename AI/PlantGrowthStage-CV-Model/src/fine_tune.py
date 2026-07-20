# pyright: reportPrivateImportUsage=false
# pyright: reportOptionalMemberAccess=false
from ultralytics import YOLO
import torch, shutil, argparse
from pathlib import Path

BASE_DIR        = Path("/teamspace/studios/this_studio/AI/PlantGrowthStage-CV-Model")
WEIGHTS_DIR     = BASE_DIR / "models/weights"
GENERAL_WEIGHTS = WEIGHTS_DIR / "general_best.pt"
DATA_YAML       = BASE_DIR / "data/combined_v3/data.yaml"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--resume", action="store_true")
    args = parser.parse_args()

    if not GENERAL_WEIGHTS.exists():
        print(f"General weights not found: {GENERAL_WEIGHTS}")
        return

    if args.resume:
        last = BASE_DIR / "runs/specific/final_model_v2/weights/last.pt"
        if last.exists():
            print(f"Resuming from: {last}")
            model = YOLO(str(last))
        else:
            print("last.pt not found, start from beginning")
            model = YOLO(str(GENERAL_WEIGHTS))
    else:
        model = YOLO(str(GENERAL_WEIGHTS))

    print(f"GPU: {torch.cuda.get_device_name(0)}")
    print(f"VRAM: {torch.cuda.get_device_properties(0).total_memory / 1e9:.1f} GB")
    print(f"Data: {DATA_YAML}")

    results = model.train(
        data      = str(DATA_YAML),
        epochs    = 75,
        batch     = 64,
        imgsz     = 640,
        patience  = 25,
        device    = 0,
        project   = str(BASE_DIR / "runs/specific"),
        name      = "final_model_v2",
        exist_ok  = True,
        plots     = True,
        save      = True,
        workers   = 4,
        cache     = False,
        amp       = True,
        freeze    = 10,
        lr0       = 0.001,
        resume    = args.resume,
        verbose   = True,
        flipud    = 0.5,
        degrees   = 15.0,
        hsv_s     = 0.3,
        mosaic    = 1.0,
    )

    best = BASE_DIR / "runs/specific/final_model_v2/weights/best.pt"
    dest = WEIGHTS_DIR / "final_model_v2_best.pt"
    if best.exists():
        shutil.copy(best, dest)
        print(f"\nBest weights saved : {dest}")

    print(f"\nResults:")
    print(f"mAP50:    {results.results_dict.get('metrics/mAP50(B)', 'N/A'):.4f}")
    print(f"mAP50-95: {results.results_dict.get('metrics/mAP50-95(B)', 'N/A'):.4f}")

if __name__ == "__main__":
    main()
