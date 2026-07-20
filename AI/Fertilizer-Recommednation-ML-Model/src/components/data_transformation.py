import os
import sys
from dataclasses import dataclass
import pandas as pd
import numpy as np
from sklearn.compose import ColumnTransformer
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import LabelEncoder, OneHotEncoder, RobustScaler
from src.exception import CustomException
from src.logger import logging
from src.utils import save_object


@dataclass
class DataTransformationConfig:
    preprocessor_ogj_file_path = os.path.join('artifacts', "preprocessor.pkl")
    label_encoder_obj_file_path = os.path.join('artifacts', "label_encoder.pkl")


class DataTransformation:
    def __init__(self):
        self.data_transformation_config = DataTransformationConfig()

    def get_data_transformer_obj(self):
        '''This function is for data transformation'''

        try:
            num_columns = ['Temperature', 'Moisture', 'Rainfall', 'PH', 'Nitrogen',
                           'Phosphorous', 'Potassium', 'Carbon']
            cat_columns = ['Soil', 'Crop']

            num_pipeline = Pipeline(
                steps=[
                    ("scaler", RobustScaler())
                ]
            )
            logging.info("Numerical columns scaled successfully")

            cat_pipeline = Pipeline(
                steps=[
                    ("OHE", OneHotEncoder(sparse_output=False, handle_unknown='ignore')),
                    ("StanderScaler", RobustScaler())
                ]
            )
            logging.info("Categorical columns Encoded successfully")

            preprocessor = ColumnTransformer(
                [
                    ("num_pipeline", num_pipeline, num_columns),
                    ("cat_pipeline", cat_pipeline, cat_columns)
                ]
            )

            return preprocessor

        except Exception as e:
            raise CustomException(e, sys)
        
    def init_data_transformation(self, train_path, test_path):
        ''' This function to initiate data transformation '''
        try:
            train_df = pd.read_csv(train_path)
            test_df = pd.read_csv(test_path)
            logging.info("trian and test dataset readed successfully")

            logging.info("Obtaining preprocessing object")

            preprocessing_obj = self.get_data_transformer_obj()

            target_col_name = 'Fertilizer'

            input_feature_train_df = train_df.drop(columns= [target_col_name], axis=1)
            target_feature_train_df = train_df[target_col_name]

            input_feature_test_df = test_df.drop(columns= [target_col_name], axis=1)
            target_feature_test_df = test_df[target_col_name]

            logging.info("Applying preprocessing obj on training and testing DF")

            input_feature_train_arr = preprocessing_obj.fit_transform(input_feature_train_df)
            input_feature_test_arr = preprocessing_obj.transform(input_feature_test_df)

            le = LabelEncoder()
            target_feature_train_arr = le.fit_transform(target_feature_train_df) 
            target_feature_test_arr = le.transform(target_feature_test_df)

            train_arr = np.c_[
                input_feature_train_arr, target_feature_train_arr
            ]
            test_arr = np.c_[
                input_feature_test_arr, target_feature_test_arr
            ]

            logging.info("Saved preprocessing object")

            save_object(
                file_path= self.data_transformation_config.preprocessor_ogj_file_path,
                obj= preprocessing_obj
            )
            save_object(
            file_path= self.data_transformation_config.label_encoder_obj_file_path,
            obj= le
            )

            return (
                train_arr, 
                test_arr, 
                self.data_transformation_config.preprocessor_ogj_file_path
            )
        except Exception as e:
            raise CustomException(e, sys)
