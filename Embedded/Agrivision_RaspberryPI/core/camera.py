import cv2
import threading


class Camera:
    def __init__(self, camera_id=0):
        self.cap = cv2.VideoCapture(camera_id)

        self.frame = None
        self.running = True

        self.thread = threading.Thread(
            target=self.update,
            daemon=True
        )

        self.thread.start()

    def update(self):
        while self.running:
            ret, frame = self.cap.read()

            if ret:
                self.frame = frame

    def get_frame(self):
        return self.frame

    def release(self):
        self.running = False
        self.cap.release()