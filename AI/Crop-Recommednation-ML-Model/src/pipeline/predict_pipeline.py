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
                    crop_name = str(label_encoder.inverse_transform([int(idx)])[0])
                    probability = float(pred_proba[idx]) * 100.0 
                    probability = round(probability, 2) 
                    results.append((crop_name, probability))
                    logging.info(f"Prediction: {crop_name} with {probability:.2f}% confidence")
                
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
        'N': (0, 140),
        'P': (5, 145),
        'K': (5, 205),
        'temperature': (8, 45),
        'humidity': (14, 100),
        'ph': (3.5, 9.9),
        'rainfall': (20, 300)
    }
    
    def __init__(
            self,
            N: float,
            P: float,
            K: float,
            temperature: float,
            humidity: float,
            ph: float,
            rainfall: float
            ):
        
        # Validate inputs before assignment
        self._validate_input('N', N)
        self._validate_input('P', P)
        self._validate_input('K', K)
        self._validate_input('temperature', temperature)
        self._validate_input('humidity', humidity)
        self._validate_input('ph', ph)
        self._validate_input('rainfall', rainfall)
        
        self.N = N
        self.P = P
        self.K = K
        self.temperature = temperature
        self.humidity = humidity
        self.ph = ph
        self.rainfall = rainfall
        
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
                "N": self.N,
                "P": self.P,
                "K": self.K,
                "temperature": self.temperature,
                "humidity": self.humidity,
                "ph": self.ph,
                "rainfall": self.rainfall
            }

            return pd.DataFrame(custom_data_input_dict, index=[0])
        
        except Exception as e:
            raise CustomException(e, sys)