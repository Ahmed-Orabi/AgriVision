import os
import sys
from dataclasses import dataclass
from sklearn.ensemble import (
    RandomForestClassifier, 
    BaggingClassifier, 
    GradientBoostingClassifier, 
    AdaBoostClassifier
    )
from sklearn.naive_bayes import GaussianNB
from sklearn.svm import SVC
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score
from sklearn.neighbors import KNeighborsClassifier
from sklearn.tree import DecisionTreeClassifier, ExtraTreeClassifier
from src.exception import CustomException
from src.logger import logging
from src.utils import save_object, evaluate_models


@dataclass
class ModelTrainerConfig:
    train_model_file_path: str = os.path.join("artifacts", 'model.pkl')
    min_accuracy_threshold: float = 0.75  # Configurable threshold


class ModelTrainer:
    def __init__(self, min_accuracy_threshold: float = None):
        self.model_trainer_config = ModelTrainerConfig()
        
        # Allow custom threshold if provided
        if min_accuracy_threshold is not None:
            self.model_trainer_config.min_accuracy_threshold = min_accuracy_threshold
            logging.info(f"Using custom accuracy threshold: {min_accuracy_threshold}")

    def init_model_trainer(self, train_arr, test_arr):
        try:
            logging.info("Splitting train and test input data")
            x_train, y_train, x_test, y_test=(
                train_arr[:,:-1],
                train_arr[:,-1].astype(int),
                test_arr[:,:-1],
                test_arr[:,-1].astype(int)
            )

            models = {
                'LogisticRegression': LogisticRegression(),
                'GaussianNB':GaussianNB(),
                'SVC':SVC(),
                'KNeighborsClassifier':KNeighborsClassifier(),
                'DecisionTreeClassifier':DecisionTreeClassifier(),
                'ExtraTreeClassifier':ExtraTreeClassifier(),
                'RandomForestClassifier':RandomForestClassifier(),
                'BaggingClassifier':BaggingClassifier(),
                'GradientBoostingClassifier':GradientBoostingClassifier(),
                'AdaBoostClassifier':AdaBoostClassifier()
            }

            params = {
                'LogisticRegression': {
                    'C': [0.1, 1, 10, 100],
                    'solver': ['lbfgs']
                },
                'GaussianNB': {},
                'SVC': {
                    'C': [0.1, 1, 10],
                    'kernel': ['linear', 'rbf'],
                    'gamma': ['scale', 'auto']
                },
                'KNeighborsClassifier': {
                    'n_neighbors': [3, 5, 7, 9, 11],
                    'weights': ['uniform', 'distance']
                },
                'DecisionTreeClassifier': {
                    'criterion': ['gini', 'entropy'],
                    'max_depth': [None, 10, 20, 30],
                    'min_samples_split': [2, 5, 10]
                },
                'ExtraTreeClassifier': {
                    'criterion': ['gini', 'entropy'],
                    'max_depth': [None, 10, 20, 30],
                    'min_samples_split': [2, 5, 10]
                },
                'RandomForestClassifier': {
                    'n_estimators': [32, 64, 128, 256],
                    'criterion': ['gini', 'entropy'],
                    'max_depth': [None, 10, 20]
                },
                'BaggingClassifier': {
                    'n_estimators': [10, 50, 100, 200],
                    'max_samples': [0.5, 0.7, 1.0]
                },
                'GradientBoostingClassifier': {
                    'learning_rate': [0.1, 0.05, 0.01],
                    'n_estimators': [64, 128, 256],
                    'subsample': [0.7, 0.8, 0.9, 1.0]
                },
                'AdaBoostClassifier': {
                    'n_estimators': [32, 64, 128, 256],
                    'learning_rate': [0.1, 0.5, 1.0]
                }
            }

            logging.info("Starting model evaluation with GridSearchCV...")
            model_report, trained_models = evaluate_models(
                X_train=x_train, 
                y_train=y_train, 
                X_test= x_test, 
                y_test= y_test, 
                models=models,
                params=params
                )
            
            best_model_name = max(model_report, key=model_report.get)
            best_model_score = model_report[best_model_name]
            best_model = trained_models[best_model_name]

            if best_model_score < self.model_trainer_config.min_accuracy_threshold:
                logging.warning(f"Best model accuracy ({best_model_score:.4f}) is below threshold ({self.model_trainer_config.min_accuracy_threshold})")
                raise CustomException(f"No model meets the minimum accuracy threshold of {self.model_trainer_config.min_accuracy_threshold}")
            
            logging.info(f"Best model found: '{best_model_name}' with accuracy: {best_model_score:.4f}")
            
            logging.info("All model scores:")
            for model_name, score in sorted(model_report.items(), key=lambda x: x[1], reverse=True):
                logging.info(f"  {model_name}: {score:.4f}")

            save_object(
                file_path=self.model_trainer_config.train_model_file_path,
                obj=best_model
            )
            logging.info(f"Best model saved to: {self.model_trainer_config.train_model_file_path}")

            return best_model_score


        except Exception as e:
            raise CustomException(e, sys)
