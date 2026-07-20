from flask import Flask, request, render_template, jsonify
from src.pipeline.predict_pipeline import CustomData, PredictPipeline
from src.logger import logging
from src.exception import CustomException
import sys

application = Flask(__name__)
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
            
            # Get number of recommendations (default 3)
            top_n = int(request.form.get('top_n', 3))

            # Validate and extract form data
            data = CustomData(
                Temperature=float(request.form.get('Temperature')),
                Moisture=float(request.form.get('Moisture')),
                Rainfall=float(request.form.get('Rainfall')),
                PH=float(request.form.get('PH')),
                Nitrogen=float(request.form.get('Nitrogen')),
                Phosphorous=float(request.form.get('Phosphorous')),
                Potassium=float(request.form.get('Potassium')),
                Carbon=float(request.form.get('Carbon')),
                Soil=str(request.form.get('Soil')),
                Crop=str(request.form.get('Crop'))
            )
            
            logging.info(f"Input data - Temperature:{data.Temperature}, Moisture:{data.Moisture}, Rainfall:{data.Rainfall}, PH:{data.PH}, Nitrogen:{data.Nitrogen}, Phosphorous:{data.Phosphorous}, Potassium:{data.Potassium}, Carbon:{data.Carbon}, Soil:{data.Soil}, Crop:{data.Crop}")
            
            pred_df = data.get_data_as_dataframe()
            logging.info("Data converted to dataframe successfully")

            predict_pipeline = PredictPipeline()
            results = predict_pipeline.predict(pred_df, top_n=top_n)
            
            logging.info(f"Prediction successful. Top {len(results)} crops recommended")
            results_json = [{"name": fertilizer, "probability": round(prob, 2)} for fertilizer, prob in results]
            
            print("Results to send:", results_json)

            return jsonify(results_json)

        except ValueError as ve:
            error_msg = "Error: Please enter valid values for all fields."
            logging.error(f"ValueError in prediction: {ve}")
            return jsonify({"error": error_msg})
            
        except Exception as e:
            error_msg = "Error: An unexpected error occurred. Please try again."
            logging.error(f"Unexpected error in prediction: {str(e)}")
            return jsonify({"error": error_msg})

    return render_template('home.html')
    
if __name__ == "__main__":
    logging.info("Flask application started")
    app.run(host="0.0.0.0", debug=True)
