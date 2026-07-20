import sys
import joblib
import pandas as pd
from src.exception import CustomException
from src.pipeline.predict_pipeline import CustomData, PredictPipeline


new_data = pd.DataFrame({
    'Temperature': [25.38, 18.79],
    'Moisture': [0.398843879, 0.457291727],
    'Rainfall': [182.0611454, 38.39551897],
    'PH': [7.049864303, 6.313310604],
    'Nitrogen': [71.22304264, 52.73819251],
    'Phosphorous': [135.1388053, 42.25293593],
    'Potassium': [147.4572035, 52.56910836],
    'Carbon': [0.800911123, 0.894318288],
    'Soil': ['Neutral Soil', 'Acidic Soil'],
    'Crop': ['papaya', 'Peas']
})

predict_pipeline = PredictPipeline()
results_array = predict_pipeline.predict(new_data)

# Print the predicted prices
print(f"Predicted fertilizers: {results_array}")
