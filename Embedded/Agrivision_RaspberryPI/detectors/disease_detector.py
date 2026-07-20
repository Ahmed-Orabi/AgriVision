from detectors.detector_base import YOLODetector


DISEASE_CLASSES = [
    'Chili___Anthracnose_fruit',
    'Chili___Bacterial_leaf_spot',
    'Chili___Healthy_fruit',
    'Chili___Healthy_leaf',
    'Chili___Mosaic_virus_leaf',
    'Eggplant___Cercospora_leaf_spot',
    'Eggplant___Colorado_potato_beetle',
    'Eggplant___Fruit_rot',
    'Eggplant___Healthy_fruit',
    'Eggplant___Healthy_leaf',
    'Potato___Alternaria_solani_leaf',
    'Potato___Common_scab_fruit',
    'Potato___Healthy_fruit',
    'Potato___Healthy_leaf',
    'Potato___Phytopthora_infestans_leaf',
    'Tomato___Antrhacnose_fruit',
    'Tomato___Bacterial_spot_leaf',
    'Tomato___Early_blight_leaf',
    'Tomato___Healthy_fruit',
    'Tomato___Healthy_leaf',
    'Tomato___Late_blight_leaf',
    'Tomato___Leaf_mold',
    'Tomato___Tomato_yellow_leaf_curl_virus'
]


class DiseaseDetector(YOLODetector):
    def __init__(self):
        super().__init__(
            "models/plant_village_disease.onnx",
            DISEASE_CLASSES,
            0.6
        )