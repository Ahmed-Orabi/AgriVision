import gradio as gr
import cv2
import requests
import os
from pathlib import Path
os.environ['YOLO_CONFIG_DIR'] = '/tmp'

from ultralytics import YOLO

file_urls =[
    'https://www.dropbox.com/scl/fi/hpdcy7wx2s2u6j2j9q7hm/APAS_image-25.jpg?rlkey=nyvfah3dlqud0k0km0r33a2z1&st=ttjweqqg&dl=0',
    'https://www.dropbox.com/scl/fi/xgbgqiawhdb4f6cjz8qip/APAS_image-28.jpg?rlkey=n1984hh3d2uxgkzdk2btfe06f&st=27b9977w&dl=0',
    'https://www.dropbox.com/scl/fi/pc3w5b1xzfvixo8kiz82j/apple.mp4?rlkey=xb1k1mkjrgc8j7ui6q5s74yvl&st=7a9och0v&dl=0'
]

# Download Sample Files data
def download_file(url, savename):
    url = url
    if not os.path.exists(savename):
        file = requests.get(url)
        open(savename, "wb").write(file.content)

DEPLOY_DIR = Path(__file__).resolve().parent
SAMPLE_MEDIA_DIR = DEPLOY_DIR / "sample_media"
SAMPLE_MEDIA_DIR.mkdir(parents=True, exist_ok=True)

for i, url in enumerate(file_urls):
    if 'mp4' in file_urls[i]:
        download_file(file_urls[i], str(SAMPLE_MEDIA_DIR / "video.mp4"))
        
    else:
        download_file(file_urls[i], str(SAMPLE_MEDIA_DIR / f"image_{i}.jpg"))
        
# init model
ROOT_DIR = Path(__file__).resolve().parents[1]
MODEL_PATH = ROOT_DIR / "artifacts" / "models" / "best.pt"
model = YOLO(str(MODEL_PATH))
# input path
path = [[str(SAMPLE_MEDIA_DIR / "image_0.jpg")], [str(SAMPLE_MEDIA_DIR / "image_1.jpg")]]
video_path = [[str(SAMPLE_MEDIA_DIR / "video.mp4")]]
# -----------------------------------------------------------------------------

def show_predictions_image(image_path):
    img = cv2.imread(image_path)
    outputs = model.predict(source=image_path)
    
    names = model.names
    results = outputs[0].cpu().numpy()
    
    for i, det in enumerate(results.boxes.xyxy):
        x1, y1, x2, y2 = map(int, det)
        
        cls_id = int(results.boxes.cls[i])
        label = names[cls_id]
        conf = results.boxes.conf[i]
        text = f"{label} {conf:.2f}"

        cv2.rectangle(img, (x1, y1), (x2, y2), color=(0, 0, 255), thickness=2)
        
        (tw, th), _ = cv2.getTextSize(text, cv2.FONT_HERSHEY_SIMPLEX, 0.6, 1)
        cv2.rectangle(img, (x1, y1 - 20), (x1 + tw, y1), (0, 0, 255), -1)
        
        cv2.putText(img, text, (x1, y1 - 5), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.6, (255, 255, 255), 1)
            
    return cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
# -----------------------------------------------------------------------------
# FOR IMAGES INFERENCES
inputs_image =[
    gr.components.Image(type="filepath", label="Input Image")
]

outputs_image =[
    gr.components.Image(type="numpy", label="Output Image")
]

interface_image = gr.Interface(
    fn=show_predictions_image,
    inputs=inputs_image,
    outputs=outputs_image,
    title="Dangerous Farm Insects Object Detection",
    description="Upload an image to detect objects using YOLOv8 model.",
    examples=path,
    cache_examples=False,
)
# -----------------------------------------------------------------------------
def show_predictions_video(video_path):
    cap = cv2.VideoCapture(video_path)
    
    while(cap.isOpened()):
        ret, frame = cap.read()
        
        if ret:
            outputs = model.predict(source=frame, conf=0.25)
            
            annotated_frame = outputs[0].plot() 
            
            yield cv2.cvtColor(annotated_frame, cv2.COLOR_BGR2RGB)
        else:
            break
            
    cap.release()
            
        
# -----------------------------------------------------------------------------
# FOR VIDEOS INFERENCES
inputs_video =[
    gr.Video(label="Input Video")
]

outputs_video =[
    gr.components.Image(type="numpy", label="Output Video")
]

interface_video = gr.Interface(
    fn=show_predictions_video,
    inputs=inputs_video,
    outputs=outputs_video,
    title="Dangerous Farm Insects Object Detection",
    description="Upload a video to detect objects using YOLOv8 model.",
    examples=video_path,
    cache_examples=False,
)

# -----------------------------------------------------------------------------
# TAB LAYOUT

gr.TabbedInterface(
    [interface_image, interface_video],
    tab_names=["Image Object Detection", "Video Object Detection"],
).queue().launch()
