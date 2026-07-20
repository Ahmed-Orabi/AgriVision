from pydantic import BaseModel
class FertilizerInputSchema(BaseModel):
    Temperature: float
    Moisture: float
    Rainfall: float
    PH: float
    Nitrogen: float
    Phosphorous: float
    Potassium: float
    Carbon: float
    Soil: str
    Crop: str
    
