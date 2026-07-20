# AgriVision

AgriVision is a smart agriculture platform that combines:
- A `.NET` Backend API.
- A static frontend built with `HTML/CSS/JavaScript`.
- AI models for crop and fertilizer recommendation.
- Computer vision models for plant disease and insect detection.

The project is split into clear modules so each track can work independently.

## Modules

- `BackEnd/`: `ASP.NET Core` API (`net9.0`) + `Identity` + `JWT` + `Stripe` + `SignalR` + `SQL Server`.
- `FrontEnd/`: User interface (Landing + Authentication + Dashboard).
- `AI/`: ML/CV projects (Flask APIs + Training Pipelines + YOLOv8).
- `Flutter/`: Mobile application track.
- `Embedded/`: Embedded systems / IoT track.
- `Ui Ux/`: UI designs and screenshots.

## Key Features

- Email/password authentication + Google OAuth.
- Farm and field management.
- Crop recommendation.
- Fertilizer recommendation.
- Plant disease detection from images.
- Harmful insect detection from images.
- Real-time notifications via SignalR.
- Subscription and billing via Stripe.
- Weather and reverse geocoding integrations.

## Project Structure

```text
AgriVision/
|-- BackEnd/
|   |-- AgriVision.API/
|   |-- AgriVision.Application/
|   |-- AgriVision.Domain/
|   |-- AgriVision.Infrastructure/
|   `-- AgriVision.Tests/
|-- FrontEnd/
|   |-- Authentication/
|   |-- Dashboard/
|   `-- assets/
|-- AI/
|   |-- Crop-Recommednation-ML-Model/
|   |-- Fertilizer-Recommednation-ML-Model/
|   |-- PlantDiseases-Detection-CV-Model/
|   `-- FarmInsects-Detection-CV-Model/
|-- Flutter/
|-- Embedded/
`-- Ui Ux/
```

## Quick Start (Local)

### 1) Run Backend

Prerequisites:
- `.NET SDK 9.0`
- `SQL Server` or `LocalDB`

Commands:

```bash
cd BackEnd
dotnet restore BackEnd.sln
dotnet run --project AgriVision.API
```

Default URLs:
- `https://localhost:5001`
- `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`

Notes:
- Migrations are applied automatically on startup.
- Seeded subscription plans: `Free`, `Basic`, and `Advanced`.

### 2) Run Frontend

Run the frontend through a local HTTP server (not `file://`).

Example (VS Code Live Server):
- `http://127.0.0.1:5500/FrontEnd/index.html`

The frontend currently expects the API at:
- `https://localhost:5001`

### 3) (Optional) Run AI Services Locally

Each AI model has its own `README` under `AI/`.

Examples:

```bash
cd AI/Crop-Recommednation-ML-Model
pip install -r requirements.txt
python apps/api.py
```

```bash
cd AI/Fertilizer-Recommednation-ML-Model
pip install -r requirements.txt
python -m apps.api_app
```

## Configuration

Main settings file:
- `BackEnd/AgriVision.API/appsettings.json`

Important keys:
- `ConnectionStrings:DefaultConnection`
- `Jwt:*`
- `Cors:AllowedOrigins`
- `Frontend:BaseUrl`
- `Authentication:Google:*`
- `AI:*`
- `ComputerVision:*`
- `Weather:*`
- `Geocoding:*`
- `Stripe:*`

## Important Security Note

The repository currently includes development secrets inside config files.
Before any real deployment:
- Move all secrets to environment variables or a secret manager.
- Rotate all exposed keys.
- Keep only non-sensitive defaults in source control.

## Testing

Backend tests:

```bash
cd BackEnd
dotnet test BackEnd.sln
```

## API Snapshot (v1)

Base route: `/api/v1`

Main groups:
- `/auth`
- `/users`
- `/farms`
- `/ai`
- `/PlantDetection`
- `/InsectDetection`
- `/notifications`
- `/billing`
- `/weather`
- `/geocode`
- `/config`

Use Swagger for full endpoint details.

## Suggested Team Improvements

- Centralize frontend API URLs in one config file instead of hardcoded values.
- Add CI for build, tests, and linting.
- Keep module-level `README` files updated continuously.

## Screenshots

UI screenshots are available in:
- `Ui Ux/Dashboard/`

## License

There is currently no `LICENSE` file.
If the project will be public, add an appropriate license.
