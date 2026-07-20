FROM python:3.9-slim

# Metadata
LABEL maintainer="AgriVision AI Team"
LABEL description="Irrigation Recommendation ML API"

# System dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Working directory
WORKDIR /app

# Install Python dependencies first (layer-cache friendly)
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy project source
COPY src/ ./src/
COPY models/ ./models/
# Expose API port
EXPOSE 8000

# Run the FastAPI app with Uvicorn
CMD ["uvicorn", "src.app.main:app", "--host", "0.0.0.0", "--port", "8000"]