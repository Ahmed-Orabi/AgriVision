from fastapi import FastAPI, HTTPException
from src.utils.validate_data import IrrigationInputSchema
from src.serving.inference import predict_irrigation_need

app = FastAPI(
    title="Irrigation ML Recommendation System API",
    description="""API for predicting irrigation needs based on input features using a trained ML model. 
                The model is trained to classify irrigation needs into categories such as 
                (low, medium, high) --> (0, 1, 2)
                based on various environmental and soil parameters.""",
    version="1.0.0"
)

@app.get("/")
def read_root():
    return {
        "message": "Welcome to the Irrigation ML Recommendation System API! Use the /predict endpoint to get irrigation recommendations."
        }
    
@app.post("/predict")
def predict(input_data: IrrigationInputSchema):
    try:
        prediction = predict_irrigation_need(input_data)
        return {
            "message": "Irrigation need prediction successful!",
            "input_received": input_data.dict(),
            "predicted_irrigation_need": prediction}
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {str(e)}")
