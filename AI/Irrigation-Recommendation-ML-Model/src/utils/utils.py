import joblib
import os

def save_object(obj, filepath):

    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    joblib.dump(obj, filepath)
    print(f"Object saved successfully to {filepath}")

def load_object(filepath):

    if not os.path.exists(filepath):
        raise FileNotFoundError(f"File not found: {filepath}")
    return joblib.load(filepath)