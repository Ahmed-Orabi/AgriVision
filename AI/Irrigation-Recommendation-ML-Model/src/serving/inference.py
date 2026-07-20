import os
import pandas as pd
from src.utils.utils import load_object
from src.utils.validate_data import IrrigationInputSchema

model_path = "models/xgb_model.pkl"
encoder_path = 'src/serving/model/target_encoder.pkl'

try:
    model = load_object(model_path)
    target_encoder = load_object(encoder_path)

except FileNotFoundError:
    model = None
    target_encoder = None
    print(f"Model file not found at {model_path}")
    print(f"Target encoder file not found at {encoder_path}")

def predict_irrigation_need(input_data: IrrigationInputSchema) -> str:
    if model is None or target_encoder is None:
        return "Error --> Model and/or target encoder not trained or not found :( !!"
    
    try:
        df = pd.DataFrame([input_data.dict()])
        prediction = model.predict(df)
        text_prediction = target_encoder.inverse_transform(prediction)
        return text_prediction[0]
    
    except Exception as e:
        return f"Prediction error: {str(e)}"