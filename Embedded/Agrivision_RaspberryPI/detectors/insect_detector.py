from detectors.detector_base import YOLODetector


INSECT_CLASSES = [
    'Africanized Honey Bees',
    'Aphids',
    'Armyworms',
    'Brown Marmorated Stink Bugs',
    'Cabbage Loopers',
    'Citrus-Canker',
    'Colorado Potato Beetles',
    'Corn Borers',
    'Corn Earworms',
    'Fall armyworm',
    'Fruit Flies',
    'Spider Mites',
    'Thrips',
    'Tomato Hornworms',
    'Western Corn Rootworms'
]


class InsectDetector(YOLODetector):
    def __init__(self):
        super().__init__(
            "models/farm_insects.onnx",
            INSECT_CLASSES,
            0.7
        )