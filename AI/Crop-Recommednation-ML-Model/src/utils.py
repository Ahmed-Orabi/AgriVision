import os
import sys
import dill
import numpy as np
import pandas as pd
from src.exception import CustomException
from sklearn.metrics import accuracy_score
from sklearn.model_selection import GridSearchCV


def save_object(file_path, obj):
    try:
        dir_path= os.path.dirname(file_path)
        os.makedirs(dir_path, exist_ok=True)

        with open(file_path, 'wb') as file_obj:
            dill.dump(obj, file_obj)

    except Exception as e:
        raise CustomException(e, sys)
    

def load_object(file_path):
    try:
        with open(file_path, 'rb') as file_obj:
            return dill.load(file_obj)
        
    except Exception as e:
        raise CustomException(e, sys)
   

def evaluate_models(X_train, y_train, X_test, y_test, models, params):
    try:
        report = {}
        best_models = {}

        for model_name, model in models.items():
            para = params[model_name]
            gscv = GridSearchCV(model, para, cv=3, n_jobs=-1, scoring='accuracy')
            gscv.fit(X_train, y_train)

            best_model_from_cv = gscv.best_estimator_

            y_test_pred = best_model_from_cv.predict(X_test)
            test_model_score = accuracy_score(y_test, y_test_pred)

            report[model_name] = (test_model_score)
            best_models[model_name] = best_model_from_cv

        return report, best_models

    except Exception as e:
        raise CustomException(e,sys)