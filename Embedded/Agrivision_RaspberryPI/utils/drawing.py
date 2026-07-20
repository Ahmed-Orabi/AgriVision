import cv2


def draw_detections(
    frame,
    detections,
    color=(0, 255, 0)
):

    for det in detections:

        x, y, w, h = det["bbox"]

        cv2.rectangle(
            frame,
            (x, y),
            (x + w, y + h),
            color,
            2
        )

        label = (
            f"{det['class_name']} "
            f"{det['confidence']:.2f}"
        )

        cv2.putText(
            frame,
            label,
            (x, y - 10),
            cv2.FONT_HERSHEY_SIMPLEX,
            0.6,
            color,
            2
        )

    return frame