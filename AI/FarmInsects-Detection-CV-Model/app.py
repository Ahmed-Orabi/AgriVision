from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
from ultralytics import YOLO
import cv2
import numpy as np
import io
import base64
from PIL import Image

app = FastAPI(title="Farm Insect Detection API")

model = YOLO("best.pt") 

@app.get("/")
def read_root():
    return {"message": "YOLOv8 Farm Insect Detection API is running!"}

@app.post("/predict/")
async def predict(file: UploadFile = File(...)):
    file_bytes = await file.read()
    nparr = np.frombuffer(file_bytes, np.uint8)
    img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)
    
    if img is None:
        return JSONResponse(status_code=400, content={"message": "bad photo"})

    # 2. Make Prediction
    results = model.predict(source=img, conf=0.3, device= 'cpu',)
    
    predictions = []
    annotated_img_base64 = ""
    
    for r in results:
        boxes = r.boxes
        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0].tolist()
            cls_id = int(box.cls[0])
            cls_name = model.names[cls_id]
            
            predictions.append({
                "class_name": cls_name,
                "bbox": [int(x1), int(y1), int(x2), int(y2)]
            })
            
        annotated_img = r.plot(conf=False)
        
        _, buffer = cv2.imencode('.jpg', annotated_img)
        annotated_img_base64 = base64.b64encode(buffer).decode('utf-8')
        return {
        "predictions": predictions,
        "annotated_image": f"data:image/jpeg;base64,{annotated_img_base64}"
    }