from pydantic import BaseModel
class IrrigationInputSchema(BaseModel):
    gSoil_Type: str
    Soil_pH: float
    Soil_Moisture: float
    Electrical_Conductivity: float
    Temperature_C: float
    Humidity: float
    Rainfall_mm: float
    Crop_Type: str
    Crop_Growth_Stage: str
    Season: str
    Field_Area_hectare: float
    
