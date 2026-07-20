from flask import Flask, request, jsonify
from src.pipeline.predict_pipeline import PredictPipeline
from src.logger import logging
from src.exception import CustomException
import sys
import pandas as pd


application = Flask(__name__)
app = application

@app.route('/home_page')
def home():
    logging.info("Landing page accessed")
    return "Run successfully!....."

@app.route('/submit_prediction', methods=['POST'])
def predict():
    try:
        json_data = request.get_json()
        data = pd.DataFrame(json_data)

        predict_pipeline = PredictPipeline()
        prediction_names = predict_pipeline.predict(data)

        return jsonify({"Prediction": prediction_names.tolist()})
    
    except ValueError as ve:
        error_msg = "Error: Please enter valid numeric values for all fields."
        logging.error(f"ValueError in prediction: {ve}")
        return jsonify({"error": error_msg})

    except Exception as e:
        error_msg = "Error: An unexpected error occurred. Please try again."
        logging.error(f"Unexpected error in prediction: {str(e)}")
        raise CustomException(e, sys)

if __name__ == "__main__":
    logging.info("Flask application started")
    app.run(debug=True)