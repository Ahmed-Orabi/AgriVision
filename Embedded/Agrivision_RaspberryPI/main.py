import cv2

from core.camera import Camera

from detectors.disease_detector import DiseaseDetector
from detectors.insect_detector import InsectDetector
from detectors.growth_detector import GrowthDetector

from core.fusion import fuse_results
from utils.drawing import draw_detections


camera = Camera()

disease_model = DiseaseDetector()
insect_model = InsectDetector()
growth_model = GrowthDetector()

while True:

    frame = camera.get_frame()

    if frame is None:
        continue

    # Inference
    disease = disease_model.infer(frame)
    insects = insect_model.infer(frame)
    growth = growth_model.infer(frame)

    # Fusion
    result = fuse_results(
        disease,
        insects,
        growth
    )

    print(result)

    # Draw detections
    frame = draw_detections(
        frame,
        disease,
        (0, 255, 0)      # Green
    )

    frame = draw_detections(
        frame,
        insects,
        (0, 0, 255)      # Red
    )

    frame = draw_detections(
        frame,
        growth,
        (255, 0, 0)      # Blue
    )

    # Draw fusion summary
    y = 30

    if result["plant"]:
        cv2.putText(
            frame,
            f"Plant: {result['plant']}",
            (10, y),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.7,
            (255, 255, 255),
            2
        )
        y += 30

    if result["disease"]:
        cv2.putText(
            frame,
            f"Disease: {result['disease']}",
            (10, y),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.7,
            (255, 255, 255),
            2
        )
        y += 30

    if result["insect"]:
        cv2.putText(
            frame,
            f"Insect: {result['insect']}",
            (10, y),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.7,
            (255, 255, 255),
            2
        )
        y += 30

    if result["growth_stage"]:
        cv2.putText(
            frame,
            f"Stage: {result['growth_stage']}",
            (10, y),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.7,
            (255, 255, 255),
            2
        )

    cv2.imshow(
        "AgriVision",
        frame
    )

    key = cv2.waitKey(1)

    if key == 27:
        break

camera.release()
cv2.destroyAllWindows()