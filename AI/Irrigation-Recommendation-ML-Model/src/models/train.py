import os
import mlflow
import mlflow.sklearn 
from sklearn.compose import ColumnTransformer
from xgboost import XGBClassifier
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import StandardScaler, OneHotEncoder
from src.data.preprocess import split_data
from src.utils.utils import save_object
from sklearn.metrics import accuracy_score, confusion_matrix, classification_report, recall_score

def train_model():
    X_train, X_test, y_train, y_test = split_data("data/raw/irrigation_prediction.csv", target_col="Irrigation_Need")
    
    categorical_cols = X_train.select_dtypes(include=['object']).columns.tolist()
    numerical_cols = X_train.select_dtypes(include=['number']).columns.tolist()

    preprocessor = ColumnTransformer(
        transformers=[
            ('num', StandardScaler(), numerical_cols),
            ('cat', OneHotEncoder(handle_unknown='ignore'), categorical_cols) 
        ])
    
    model_pipeline = Pipeline(steps=[
        ('preprocessor', preprocessor),
        ('classifier', XGBClassifier(
            n_estimators=300,
            learning_rate=0.1,
            max_depth=6,
            random_state=42,
            n_jobs=-1,
            eval_metric='logloss'))
    ])
    
    with mlflow.start_run():
        # Train the model
        model_pipeline.fit(X_train, y_train)
        preds = model_pipeline.predict(X_test)
        
        acc = accuracy_score(y_test, preds)
        rec = recall_score(y_test, preds, average='weighted')
        con_mat = confusion_matrix(y_test, preds)
        
        # Get classification report as a dictionary for clean JSON logging
        report_dict = classification_report(y_test, preds, output_dict=True)
        
        # Log parameters, metrics, and dictionaries
        mlflow.log_param("model_type", "XGBClassifier")
        mlflow.log_metric("accuracy", acc)
        mlflow.log_metric("recall", rec)
        mlflow.log_dict({"confusion_matrix": con_mat.tolist()}, "confusion_matrix.json")
        mlflow.log_dict(report_dict, "classification_report.json")
        
        # Log datasets so they appear in the MLflow UI
        train_ds = mlflow.data.from_pandas(X_train)
        mlflow.log_input(train_ds, context="training")
        test_ds = mlflow.data.from_pandas(X_test)
        mlflow.log_input(test_ds, context="testing")
        
        # Log the model pipeline itself in MLflow
        mlflow.sklearn.log_model(model_pipeline, "model")
        
        print(f"Model Trained. Accuracy: {acc:.4f}, Recall: {rec:.4f}")
        
    # Save the pipeline locally as a .pkl file
    os.makedirs("models", exist_ok=True)
    save_object(model_pipeline, "models/xgb_model.pkl")

train_model()