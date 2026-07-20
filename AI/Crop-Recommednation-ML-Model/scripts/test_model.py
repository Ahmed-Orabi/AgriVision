import sys
import joblib
import pandas as pd
from src.exception import CustomException
from src.pipeline.predict_pipeline import CustomData, PredictPipeline


new_data = pd.DataFrame({
    'N': [22, 108],
    'P': [68, 94],
    'K': [16, 47],
    'temperature': [27.70496805, 27.35911627],
    'humidity': [63.20915034, 84.54625006],
    'ph': [7.74672376, 6.387431382999999],
    'rainfall': [37.46160727, 90.81250457]  
})

predict_pipeline = PredictPipeline()
results_array = predict_pipeline.predict(new_data)

# Print the predicted prices
print(f"Predicted Prices: {results_array}")