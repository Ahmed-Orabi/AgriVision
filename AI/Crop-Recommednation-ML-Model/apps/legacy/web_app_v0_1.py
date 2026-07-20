import os
from flask import Flask, request, render_template
from src.pipeline.predict_pipeline import CustomData, PredictPipeline
from src.logger import logging
from src.exception import CustomException
import sys

TEMPLATE_DIR = os.path.join(os.path.dirname(__file__), "..", "templates")
application = Flask(__name__, template_folder=TEMPLATE_DIR)
app = application

@app.route('/')
def index():
    logging.info("Landing page accessed")
    return render_template('index.html')

@app.route('/predict')
def home_page():
    logging.info("Prediction page accessed")
    return render_template('home.html')

@app.route('/submit_prediction', methods=['GET', 'POST'])
def predict_datapoint():
    if request.method == 'POST':
        try:
            logging.info("Prediction request received")
            
            # Validate and extract form data
            data = CustomData(
                N=float(request.form.get('N')),
                P=float(request.form.get('P')),
                K=float(request.form.get('K')),
                temperature=float(request.form.get('temperature')),
                humidity=float(request.form.get('humidity')),
                ph=float(request.form.get('ph')),
                rainfall=float(request.form.get('rainfall'))
            )
            
            logging.info(f"Input data - N:{data.N}, P:{data.P}, K:{data.K}, Temp:{data.temperature}, Humidity:{data.humidity}, pH:{data.ph}, Rainfall:{data.rainfall}")
            
            pred_df = data.get_data_as_dataframe()
            logging.info("Data converted to dataframe successfully")

            predict_pipeline = PredictPipeline()
            results_array = predict_pipeline.predict(pred_df)
            
            logging.info(f"Prediction successful: {results_array[0]}")
            return render_template('home.html', results=results_array[0])

        except ValueError as ve:
            error_msg = "Error: Please enter valid numeric values for all fields."
            logging.error(f"ValueError in prediction: {ve}")
            return render_template('home.html', results=error_msg)
            
        except Exception as e:
            error_msg = "Error: An unexpected error occurred. Please try again."
            logging.error(f"Unexpected error in prediction: {str(e)}")
            raise CustomException(e, sys)

    return render_template('home.html')
    
if __name__ == "__main__":
    logging.info("Flask application started")
    app.run(host="0.0.0.0", debug=True)
