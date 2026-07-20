from src.data.preprocess import split_data
from src.utils.utils import load_object
from sklearn.metrics import classification_report, confusion_matrix, accuracy_score

def evaluate_model():
    X_train, X_test, y_train, y_test = split_data("data/raw/irrigation_prediction.csv", target_col="Irrigation_Need")

    model_pipeline = load_object("models/xgb_model.pkl")
    y_pred = model_pipeline.predict(X_test)

    print("Classification Report:")
    print(classification_report(y_test, y_pred))
    
    print("Confusion Matrix:")
    print(confusion_matrix(y_test, y_pred))
    
    print("Accuracy Score:", accuracy_score(y_test, y_pred))
    
evaluate_model()