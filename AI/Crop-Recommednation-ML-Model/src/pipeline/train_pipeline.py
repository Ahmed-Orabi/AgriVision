import sys
from src.logger import logging
from src.exception import CustomException
from src.components.data_ingestion import DataIngestion
from src.components.data_transformation import DataTransformation
from src.components.model_trainer import ModelTrainer

if __name__ == "__main__":
    logging.info("=" * 70)
    logging.info("Starting the training pipeline...")
    logging.info("=" * 70)
    
    try:
        logging.info("STEP 1: Initiating Data Ingestion...")
        data_ingestion = DataIngestion()
        train_data_path, test_data_path = data_ingestion.initiate_data_ingestion()
        logging.info(f"Data Ingestion completed")
        logging.info(f"Train path: {train_data_path}")
        logging.info(f"Test path: {test_data_path}")

        logging.info("\nSTEP 2: Initiating Data Transformation...")
        data_transformation = DataTransformation()
        train_arr, test_arr, _ = data_transformation.init_data_transformation(train_data_path, test_data_path)
        logging.info(f"Data Transformation completed")
        logging.info(f"Train array shape: {train_arr.shape}")
        logging.info(f"Test array shape: {test_arr.shape}")

        logging.info("\nSTEP 3: Initiating Model Training...")
        
        model_trainer = ModelTrainer(min_accuracy_threshold=0.75)
        
        best_model_score = model_trainer.init_model_trainer(train_arr, test_arr)
        logging.info(f"Model Training completed")
        logging.info(f"Best model accuracy: {best_model_score:.4f}")
        
        logging.info("\n" + "=" * 70)
        logging.info("TRAINING PIPELINE FINISHED SUCCESSFULLY")
        logging.info("=" * 70)

    except Exception as e:
        logging.error("=" * 70)
        logging.error(f"ERROR OCCURRED IN THE TRAINING PIPELINE")
        logging.error(f"Error details: {str(e)}")
        logging.error("=" * 70)
        raise CustomException(e, sys)