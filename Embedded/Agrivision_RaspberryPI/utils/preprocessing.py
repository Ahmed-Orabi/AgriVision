import cv2
import numpy as np


def preprocess(frame, input_size=640):

    image = cv2.resize(
        frame,
        (input_size, input_size)
    )

    image = cv2.cvtColor(
        image,
        cv2.COLOR_BGR2RGB
    )

    image = image.astype(np.float32) / 255.0

    image = np.transpose(
        image,
        (2, 0, 1)
    )

    image = np.expand_dims(
        image,
        axis=0
    )

    return image