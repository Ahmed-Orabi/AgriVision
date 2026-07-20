from detectors.detector_base import YOLODetector


GROWTH_CLASSES = [
    "Tomato_Early_Vegetative",
    "Tomato_Flowering_Initiation",
    "Tomato_Unripe",
    "Tomato_Ripe",
    "Cotton_Early_Stage",
    "Cotton_Mid_Stage",
    "Cotton_Late_Stage"
]


class GrowthDetector(YOLODetector):
    def __init__(self):
        super().__init__(
            "models/plant_growth_stage.onnx",
            GROWTH_CLASSES,
            0.6
        )