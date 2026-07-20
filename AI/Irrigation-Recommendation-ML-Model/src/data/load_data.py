import pandas as pd
import os

def load_data(file_path: str) -> pd.DataFrame:
    """
    Load data from a CSV file ==> DataFrame.

    Args:
        file_path (str): The path to the CSV file.
        
    Returns:
        pd.DataFrame: The loaded data as a pandas DataFrame.
    """
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"File not found: {file_path}")
    
    df = pd.read_csv(file_path)
    
    columns_drop = ['Organic_Carbon', 'Sunlight_Hours', 'Wind_Speed_kmh', 'Irrigation_Type', 'Water_Source', 'Mulching_Used', 'Previous_Irrigation_mm', 'Region']
    df.drop(columns=columns_drop, inplace=True)
    return df

# file_path = "data/raw/irrigation_prediction.csv"
# df = load_data(file_path)
# categorical_columns = df.select_dtypes(include=['object', 'category']).columns
# for col in categorical_columns:
#     print(f"--- Column: {col} ---")
    
#     # Get the unique values
#     unique_vals = df[col].unique()
    
#     # Print them out
#     print(f"Unique values ({len(unique_vals)} total): {unique_vals}\n")
# print(df.head())
# print(df.columns.tolist())
# print(df.shape)
# print(df.info())

########################################################################################################################################################################################################
# --- Column: gSoil_Type ---
# Unique values (4 total): ['Clay' 'Silt' 'Sandy' 'Loamy']

# --- Column: Crop_Type ---
# Unique values (6 total): ['Wheat' 'Maize' 'Cotton' 'Rice' 'Sugarcane' 'Potato']

# --- Column: Crop_Growth_Stage ---
# Unique values (4 total): ['Vegetative' 'Flowering' 'Harvest' 'Sowing']

# --- Column: Season ---
# Unique values (3 total): ['Rabi' 'Zaid' 'Kharif']

# --- Column: Irrigation_Need ---
# Unique values (3 total): ['Low' 'Medium' 'High']
########################################################################################################################################################################################################

#  gSoil_Type  Soil_pH  Soil_Moisture  Electrical_Conductivity  Temperature_C  Humidity  Rainfall_mm Crop_Type Crop_Growth_Stage  Season  Field_Area_hectare Irrigation_Need
# 0       Clay     6.14          36.48                     2.17          21.90     31.19      1167.70     Wheat        Vegetative    Rabi                4.73             Low
# 1       Silt     6.41          50.56                     0.23          36.50     26.01       831.28     Maize         Flowering    Zaid               12.22          Medium
# 2      Sandy     7.71          40.07                     2.18          41.83     76.41      1844.45    Cotton           Harvest    Rabi                5.52             Low
# 3       Clay     5.96          12.75                     0.40          37.22     43.32       306.26     Wheat            Sowing  Kharif                1.43          Medium
# 4       Clay     7.76          18.58                     2.52          22.38     86.44      1875.63    Cotton            Sowing    Zaid                2.52          Medium
########################################################################################################################################################################################################
# ['gSoil_Type', 'Soil_pH', 'Soil_Moisture', 'Electrical_Conductivity', 'Temperature_C', 'Humidity', 'Rainfall_mm', 'Crop_Type', 'Crop_Growth_Stage', 'Season', 'Field_Area_hectare', 'Irrigation_Need']
########################################################################################################################################################################################################
# (10000, 12)
########################################################################################################################################################################################################
# <class 'pandas.core.frame.DataFrame'>
# RangeIndex: 10000 entries, 0 to 9999
# Data columns (total 12 columns):
#  #   Column                   Non-Null Count  Dtype  
# ---  ------                   --------------  -----  
#  0   gSoil_Type               10000 non-null  object 
#  1   Soil_pH                  10000 non-null  float64
#  2   Soil_Moisture            10000 non-null  float64
#  3   Electrical_Conductivity  10000 non-null  float64
#  4   Temperature_C            10000 non-null  float64
#  5   Humidity                 10000 non-null  float64
#  6   Rainfall_mm              10000 non-null  float64
#  7   Crop_Type                10000 non-null  object 
#  8   Crop_Growth_Stage        10000 non-null  object 
#  9   Season                   10000 non-null  object 
#  10  Field_Area_hectare       10000 non-null  float64
#  11  Irrigation_Need          10000 non-null  object 
# dtypes: float64(7), object(5)
# memory usage: 937.6+ KB
########################################################################################################################################################################################################