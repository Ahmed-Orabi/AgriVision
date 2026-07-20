# pyright: reportPrivateImportUsage=false
from ultralytics import YOLO
from pathlib import Path
import shutil

BASE_DIR    = Path("/home/aladdin/AgriVision/AI/PlantGrowthStage-CV-Model")
WEIGHTS     = BASE_DIR / "models/weights/last_model_v2_best.pt"
EXPORTS_DIR = BASE_DIR / "models/exports"
EXPORTS_DIR.mkdir(exist_ok=True)

model = YOLO(str(WEIGHTS))

# ONNX for API and Raspberry Pi
print("Exporting ONNX...")
model.export(
    format   = "onnx",
    imgsz    = 640,
    dynamic  = True,
    simplify = True,
)

onnx_src = WEIGHTS.with_suffix(".onnx")
onnx_dst = EXPORTS_DIR / "last_model_v2_best.onnx"
if onnx_src.exists():
    shutil.copy(onnx_src, onnx_dst)
    print(f"ONNX saved:- {onnx_dst}")

print("\nExport complete!")
for f in EXPORTS_DIR.iterdir():
    print(f"  {f.name} ({f.stat().st_size / 1e6:.1f} MB)")