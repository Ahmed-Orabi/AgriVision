import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from src.utils.utils import save_object
from src.data.load_data import load_data

def split_data(file_path: str = "data/raw/irrigation_prediction.csv", target_col: str = "Irrigation_Need"):
    df = load_data(file_path)

    df.dropna(subset=[target_col], inplace=True)
    df.drop_duplicates(inplace=True)

    X = df.drop(columns=[target_col])
    y_raw = df[target_col]

    label_encoder = LabelEncoder()
    y = label_encoder.fit_transform(y_raw)
    save_object(label_encoder, 'src/serving/model/target_encoder.pkl')

    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)
    
    return X_train, X_test, y_train, y_test

# file_path = "data/raw/irrigation_prediction.csv"
# df = load_data(file_path)
# X_train, X_test, y_train, y_test = split_data(df)
# print("Train Shape:", X_train.shape)
# print("Test Shape:", X_test.shape)
# print("Train Shape:", y_train.shape)
# print("Test Shape:", y_test.shape)
# Select the Categorical and Numerical columns from df
# categorical_cols = df.select_dtypes(include=['object']).columns
# numerical_cols = df.select_dtypes(include=['int64','float64']).columns
# print(categorical_cols)
# print(numerical_cols)