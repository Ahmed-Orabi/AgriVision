import os
import sys
import pandas as pd
import numpy as np
from src.exception import CustomException
from src.utils import load_object
from src.logger import logging

class PredictPipeline:
    def __init__(self):
        pass

    def predict(self, features, top_n=1):
        try:
            model_path = 'artifacts/model.pkl'
            preprocessor_path = 'artifacts/preprocessor.pkl'
            label_encoder_path = 'artifacts/label_encoder.pkl'

            logging.info("Loading model, preprocessor, and label encoder...")
            model = load_object(file_path=model_path)
            preprocessor = load_object(file_path=preprocessor_path)
            label_encoder = load_object(file_path=label_encoder_path)

            logging.info("Transforming input features...")
            data_scaled = preprocessor.transform(features)
            
            # Get probabilities for all classes
            logging.info("Calculating prediction probabilities...")
            try:
                # Try to get probabilities (works for most classifiers)
                pred_proba = model.predict_proba(data_scaled)[0]
                
                # Get top N predictions
                top_indices = np.argsort(pred_proba)[::-1][:top_n]
                
                results = []
                for idx in top_indices:
                    ferti_name = str(label_encoder.inverse_transform([int(idx)])[0])
                    probability = float(pred_proba[idx]) * 100.0 
                    probability = round(probability, 2) 
                    results.append((ferti_name, probability))
                    logging.info(f"Prediction: {ferti_name} with {probability:.2f}% confidence")
                
                return results
                
            except AttributeError as ae:
                # If model doesn't support predict_proba, use regular predict
                logging.warning(f"Model doesn't support probability predictions: {ae}")
                preds_code = model.predict(data_scaled).astype(int)
                preds_name = label_encoder.inverse_transform(preds_code)
                return [(str(preds_name[0]), 100.0)]
            except Exception as inner_e:
                logging.error(f"Error in prediction: {inner_e}")
                raise inner_e
        
        except Exception as e:
            raise CustomException(e, sys)
    
    def predict_single(self, features):

        results = self.predict(features, top_n=1)
        return [results[0][0]]  # Return only the crop name
    

class CustomData:
    # Define validation ranges
    VALIDATION_RANGES = {
        'Temperature': (4, 57),
        'Moisture': (0.2, 0.8),
        'Rainfall': (0, 420),
        'PH': (3, 10),
        'Nitrogen':(35, 100),
        'Phosphorous': (0, 178),
        'Potassium': (0, 162),
        'Carbon': (0, 3.5),
    }
    
    def __init__(
            self,
            Temperature: float,
            Moisture: float,
            Rainfall: float,
            PH: float,
            Nitrogen: float,
            Phosphorous: float,
            Potassium: float,
            Carbon: float,
            Soil: str,
            Crop:str
            ):
        
        # Validate inputs before assignment
        self._validate_input('Temperature', Temperature)
        self._validate_input('Moisture', Moisture)
        self._validate_input('Rainfall', Rainfall)
        self._validate_input('PH', PH)
        self._validate_input('Nitrogen', Nitrogen)
        self._validate_input('Phosphorous', Phosphorous)
        self._validate_input('Potassium', Potassium)
        self._validate_input('Carbon', Carbon)
        self._validate_input('Soil', Soil)
        self._validate_input('Crop', Crop)
        
        self.Temperature = Temperature
        self.Moisture = Moisture
        self.Rainfall = Rainfall
        self.PH = PH
        self.Nitrogen = Nitrogen
        self.Phosphorous = Phosphorous
        self.Potassium = Potassium
        self.Carbon = Carbon
        self.Soil = Soil
        self.Crop = Crop
        
        logging.info("Input validation passed for all parameters")

    def _validate_input(self, field_name: str, value: float):
        """Validate if the input value is within acceptable range"""
        try:
            # Check if value is numeric
            value = float(value)
            
            # Check if value is within range
            min_val, max_val = self.VALIDATION_RANGES[field_name]
            
            if value < min_val or value > max_val:
                error_msg = f"{field_name} value ({value}) is out of acceptable range [{min_val}, {max_val}]"
                logging.warning(error_msg)
                raise ValueError(error_msg)
                
        except ValueError as ve:
            raise CustomException(ve, sys)
        except Exception as e:
            raise CustomException(e, sys)

    def get_data_as_dataframe(self):
        try:
            custom_data_input_dict = {
                "Temperature": self.Temperature,
                "Moisture": self.Moisture,
                "Rainfall": self.Rainfall,
                "PH": self.PH,
                "Nitrogen": self.Nitrogen,
                "Phosphorous": self.Phosphorous,
                "Potassium": self.Potassium,
                "Carbon": self.Carbon,
                "Soil": self.Soil,
                "Crop": self.Crop
            }

            return pd.DataFrame(custom_data_input_dict, index=[0])
        
        except Exception as e:
            raise CustomException(e, sys)