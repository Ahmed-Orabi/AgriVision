# Farm Insects Detection (YOLOv8)

Object detection project for dangerous farm insects using YOLOv8.

## What was organized
- Added a clean folder structure (`artifacts`, `configs`, `dataset`, `deploy`, `notebooks`).
- Moved notebook and experiment outputs to dedicated directories.
- Added a stable model location: `artifacts/models/best.pt`.
- Updated deployment app to load model from project root and keep sample media inside `deploy/sample_media`.

## Project Structure
```text
FarmInsects-Detection-CV-Model/
|-- artifacts/
|   |-- models/
|   |   `-- best.pt
|   |-- train_run_01/
|   |-- train_run_02/
|   |-- validation/
|   `-- predictions_samples/
|-- configs/
|   `-- data.yaml
|-- dataset/
|   |-- train/
|   |   |-- images/
|   |   `-- labels/
|   |-- valid/
|   |   |-- images/
|   |   `-- labels/
|   `-- test/
|       |-- images/
|       `-- labels/
|-- deploy/
|   |-- app.py
|   |-- requirements.txt
|   `-- sample_media/   (auto-created when app runs)
|-- notebooks/
|   `-- dangerous-farm-insects-YOLOv8.ipynb
`-- README.md
```

## Setup
```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r deploy/requirements.txt
```

## Dataset Config
Use `configs/data.yaml`:
```yaml
path: ../dataset
train: train/images
val: valid/images
test: test/images
```

Put your dataset in:
- `dataset/train/images`, `dataset/train/labels`
- `dataset/valid/images`, `dataset/valid/labels`
- `dataset/test/images`, `dataset/test/labels`

## Train (example)
```powershell
yolo task=detect mode=train model=yolov8n.pt data=configs/data.yaml epochs=100 imgsz=640 project=artifacts name=train_run_03
```

## Validation (example)
```powershell
yolo task=detect mode=val model=artifacts/models/best.pt data=configs/data.yaml
```

## Inference (example)
```powershell
yolo task=detect mode=predict model=artifacts/models/best.pt source=dataset/test/images save=True project=artifacts name=predict_run_01
```

## Run Gradio App
```powershell
python deploy/app.py
```

Notes:
- The app expects the model at `artifacts/models/best.pt`.
- If you train a new model, copy/update `best.pt` in that path.
