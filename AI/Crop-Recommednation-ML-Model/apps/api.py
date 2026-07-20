from fastapi import FastAPI, HTTPException, Query
from src.pipeline.predict_pipeline import PredictPipeline
from src.logger import logging
from src.exception import CustomException
from src.validate_data import CropInputSchema
import pandas as pd

app = FastAPI(title="Crop Recommendation API")

@app.get('/home_page')
def home():
    return {"message": "ORABI RUNNING SUCCESSFULLY ON HOME PAGE"}

@app.post('/submit_prediction')
def predict(
    input_data: CropInputSchema, 
    top_n: int = Query(3, description="Number of top predictions to return")
):
    try:
        data_dict = input_data.model_dump() 
        
        data = pd.DataFrame([data_dict])

        predict_pipeline = PredictPipeline()
        prediction_names = predict_pipeline.predict(data, top_n=top_n)

        results_json = [{"name": crop, "probability": round(prob, 2)} for crop, prob in prediction_names]
        
        print("Results to send:", results_json)
        
        return results_json
    
    except ValueError as ve:
        error_msg = "Error: Please enter valid numeric values for all fields."
        logging.error(f"ValueError in prediction: {ve}")
        raise HTTPException(status_code=400, detail=error_msg)

    except Exception as e:
        error_msg = "Error: An unexpected error occurred. Please try again."
        logging.error(f"Unexpected error in prediction: {str(e)}")
        raise HTTPException(status_code=500, detail=error_msg)
