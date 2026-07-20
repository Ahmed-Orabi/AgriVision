import os
import sys
import pandas as pd
from src.logger import logging
from dataclasses import dataclass
from src.exception import CustomException
from sklearn.model_selection import train_test_split

@dataclass
class DataIngestionConfig:
    train_data_path: str = os.path.join('artifacts', "train.csv")
    test_data_path: str = os.path.join('artifacts', "test.csv")
    raw_data_path: str = os.path.join('artifacts', "data.csv")

class DataIngestion:
    def __init__(self):
        self.ingestion_config = DataIngestionConfig()

    def initiate_data_ingestion(self):
        logging.info("Entered the data ingestion method")
        try:
            df = pd.read_csv(os.path.join('notebooks', 'data', 'fertilizer_recommendation_dataset.csv'))
            logging.info(f"Read the dataset as a dataframe with shape: {df.shape}")

            os.makedirs(os.path.dirname(self.ingestion_config.train_data_path), exist_ok=True)
            logging.info(f"Created artifacts directory: {os.path.dirname(self.ingestion_config.train_data_path)}")

            df.to_csv(self.ingestion_config.raw_data_path, index=False, header=True)
            logging.info(f"Saved raw data to: {self.ingestion_config.raw_data_path}")
            
            logging.info("Train test split initiated")
            train_set, test_set = train_test_split(df, test_size=0.2, random_state=42)
            logging.info(f"Train set size: {len(train_set)}, Test set size: {len(test_set)}")

            train_set.to_csv(self.ingestion_config.train_data_path, index=False, header=True)
            test_set.to_csv(self.ingestion_config.test_data_path, index=False, header=True)

            logging.info("Ingestion of the data is completed")

            return(
                self.ingestion_config.train_data_path,
                self.ingestion_config.test_data_path,
            )
        except Exception as e:
            raise CustomException(e, sys)
