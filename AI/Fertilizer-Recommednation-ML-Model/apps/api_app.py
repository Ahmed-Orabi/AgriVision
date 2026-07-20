from fastapi import FastAPI, HTTPException, Query, Request
from src.pipeline.predict_pipeline import PredictPipeline
from src.logger import logging
from src.exception import CustomException
import sys
import pandas as pd
from src.validate_data import FertilizerInputSchema
import warnings
warnings.filterwarnings("ignore")
app = FastAPI(title="Fertilizer Recommendation API",
            description="API for predicting the best fertilizer based on soil and crop data",)

@app.get('/home_page')
def home():
    logging.info("Landing page accessed")
    return {
        "message":"ORABI RUNNING SUCCESSFULLY ON HOME PAGE"
    }

@app.post("/submit_prediction")
def predict(
    input_data: FertilizerInputSchema, 
    top_n: int = Query(3, description="Number of top predictions to return")
):
    try:
        data_dict = input_data.model_dump() 
        
        data = pd.DataFrame([data_dict])

        predict_pipeline = PredictPipeline()
        prediction_names = predict_pipeline.predict(data, top_n=top_n)

        results_json = [{"name": fertilizer, "probability": round(prob, 2)} for fertilizer, prob in prediction_names]
        print("Results to send:", results_json)
        
        return results_json
    
    except ValueError as ve:
        logging.error(f"ValueError in prediction: {ve}")
        # In FastAPI, use HTTPException to return standard HTTP error codes
        raise HTTPException(status_code=400, detail=str(ve))

    except Exception as e:
        logging.error(f"Unexpected error in prediction: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))
