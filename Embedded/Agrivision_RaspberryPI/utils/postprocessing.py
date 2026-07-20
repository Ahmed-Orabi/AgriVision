import cv2
import numpy as np


def postprocess(
    predictions,
    classes,
    frame_shape,
    input_size=640,
    conf_threshold=0.5,
    iou_threshold=0.45
):

    h0, w0 = frame_shape[:2]

    predictions = predictions.squeeze(0).T

    boxes = []
    scores = []
    class_ids = []

    for pred in predictions:

        x, y, w, h = pred[:4]

        class_scores = pred[4:]

        class_id = np.argmax(class_scores)

        confidence = float(class_scores[class_id])

        if confidence < conf_threshold:
            continue

        x1 = x - w / 2
        y1 = y - h / 2

        scale_x = w0 / input_size
        scale_y = h0 / input_size

        x1 *= scale_x
        y1 *= scale_y

        w *= scale_x
        h *= scale_y

        boxes.append([
            int(x1),
            int(y1),
            int(w),
            int(h)
        ])

        scores.append(confidence)
        class_ids.append(class_id)

    results = []

    if len(boxes) == 0:
        return results

    indices = cv2.dnn.NMSBoxes(
        boxes,
        scores,
        conf_threshold,
        iou_threshold
    )

    if len(indices) == 0:
        return results

    for idx in indices.flatten():

        x, y, w, h = boxes[idx]

        results.append({

            "class_id": class_ids[idx],

            "class_name": classes[
                class_ids[idx]
            ],

            "confidence": scores[idx],

            "bbox": [
                x,
                y,
                w,
                h
            ]
        })

    return results