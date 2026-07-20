import onnxruntime as ort

from utils.preprocessing import preprocess
from utils.postprocessing import postprocess


class YOLODetector:

    def __init__(
        self,
        model_path,
        classes,
        conf_threshold=0.5,
        input_size=640
    ):

        self.classes = classes
        self.conf_threshold = conf_threshold
        self.input_size = input_size

        self.session = ort.InferenceSession(
            model_path,
            providers=["CPUExecutionProvider"]
        )

        self.input_name = self.session.get_inputs()[0].name

    def preprocess(self, frame):
        return preprocess(
            frame,
            self.input_size
        )

    def infer(self, frame):

        image = self.preprocess(frame)

        outputs = self.session.run(
            None,
            {self.input_name: image}
        )

        results = postprocess(
            predictions=outputs[0],
            classes=self.classes,
            frame_shape=frame.shape,
            input_size=self.input_size,
            conf_threshold=self.conf_threshold,
            iou_threshold=0.45
        )

        return results