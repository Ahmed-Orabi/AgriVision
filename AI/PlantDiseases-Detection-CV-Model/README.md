# Plant Diseases Detection (YOLOv8)

Computer vision project for plant disease detection using YOLOv8, with a training notebook and a Gradio deployment app.

## Project Structure

```text
PlantDiseases-Detection-CV-Model/
|-- artifacts/                     # Training/validation/prediction outputs
|   |-- train/
|   |-- train2/
|   |-- train3/
|   |   |-- weights/
|   |       |-- best.pt
|   |       |-- last.pt
|   |-- val/
|   |-- predict/
|-- configs/
|   |-- data.yaml                  # Dataset config (YOLO format)
|-- deploy/
|   |-- app.py                     # Gradio app
|   |-- requirements.txt           # Deployment dependencies
|-- notebooks/
|   |-- plant-village-detection-YOLOv8.ipynb
|-- requirements.txt               # Training/experimentation dependencies
|-- kaggle.json                    # Kaggle credentials (do not commit in public repos)
```

## Features

- Train an object detection model with Ultralytics YOLOv8.
- Inspect validation metrics and plots under `artifacts/`.
- Run image/video inference from a Gradio UI.

## Setup

### 1) Clone and create environment

```bash
git clone <your-repo-url>
cd PlantDiseases-Detection-CV-Model
python -m venv .venv
```

Windows:

```bash
.venv\Scripts\activate
```

Linux/Mac:

```bash
source .venv/bin/activate
```

### 2) Install dependencies

```bash
pip install -r requirements.txt
pip install -r deploy/requirements.txt
```

## Dataset Config

Config file:

- `configs/data.yaml`

Important: current paths inside this file point to Google Drive (`/content/drive/...`).
Before local training, update `train`, `val`, and `test` to match your local dataset paths.

## Train (YOLOv8 CLI)

```bash
yolo task=detect mode=train model=yolov8n.pt data=configs/data.yaml epochs=50 imgsz=640 project=artifacts name=train_custom
```

## Inference (CLI)

```bash
yolo task=detect mode=predict model=artifacts/train3/weights/best.pt source=path/to/image_or_video
```

## Run Gradio App

```bash
python deploy/app.py
```

The app loads model weights from:

- `artifacts/train3/weights/best.pt`

## Notes

- The repository was reorganized so generated outputs live under `artifacts/`.
- Add `kaggle.json` to `.gitignore` if the repo is public.
- If you move model weights, update the path in `deploy/app.py`.
