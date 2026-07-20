# AgriVision BackEnd

ASP.NET Core Web API backend for the AgriVision platform.

## Overview
This service provides:
- Authentication and authorization (JWT + Google OAuth)
- Farm and field management
- AI crop and fertilizer recommendations
- Plant and insect detection integrations
- Weather and reverse geocoding endpoints
- Stripe billing and subscription flows
- Real-time notifications via SignalR

The solution follows a layered architecture and targets `.NET 9`.

## Solution Structure
- `AgriVision.Domain`: entities, enums, value objects
- `AgriVision.Application`: DTOs and service interfaces
- `AgriVision.Infrastructure`: EF Core, Identity/JWT, external integrations, SignalR notifier
- `AgriVision.API`: controllers, DI configuration, middleware, Swagger
- `AgriVision.Tests`: test project

Solution file: `BackEnd.sln`

## Tech Stack
- ASP.NET Core Web API (`net9.0`)
- Entity Framework Core + SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- Google External Login
- Stripe (`Stripe.net`)
- SignalR
- Swagger / OpenAPI

## Local Run
### Prerequisites
- .NET SDK 9.0
- SQL Server or LocalDB

### Commands
```bash
dotnet restore BackEnd.sln
dotnet run --project AgriVision.API
```

Default local URLs (from `launchSettings.json`):
- configured in `AgriVision.API/Properties/launchSettings.json`

Swagger:
- available at `/swagger`

SignalR hub:
- `/hubs/notifications`

## Configuration
Main config file: `AgriVision.API/appsettings.json`

Important sections:
- `ConnectionStrings:DefaultConnection`
- `Jwt:*`
- `Cors:AllowedOrigins`
- `Frontend:BaseUrl`
- `EmailSettings:*`
- `Authentication:Google:*`
- `AI:CropRecommendation:BaseUrl`
- `AI:FertilizerRecommendation:BaseUrl`
- `ComputerVision:*`
- `HuggingFaceSpaces:ApiKey`
- `Weather:*`
- `Geocoding:*`
- `Stripe:*`

### Secret Management
Do not store production secrets in source control. Use environment variables or user-secrets.

Example:
```bash
dotnet user-secrets init --project AgriVision.API
dotnet user-secrets set "Jwt:Key" "<your-secret-key>" --project AgriVision.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your-connection-string>" --project AgriVision.API
```

## Database
Migrations are applied automatically on startup in `Program.cs` via:
- `db.Database.Migrate()`

Seeded lookup data:
- Roles: `Admin`, `Farmer`
- Subscription plans: `Free`, `Basic`, `Advanced`

## Authentication and Authorization
Authentication schemes:
- `Bearer` (JWT)
- `Google`

Most business endpoints require authentication.
Public endpoints include weather, geocoding, frontend config, and Stripe webhook.

## API Routes (v1)
### Auth (`/api/v1/auth`)
- `POST /register`
- `POST /login`
- `POST /logout`
- `GET /me`
- `GET /confirm-email`
- `POST /resend-email-verification`
- `POST /forgot-password`
- `GET /reset-password-confirm`
- `POST /reset-password`
- `GET /google-login`
- `GET /google-callback`

### Users (`/api/v1/users`) [Authorized]
- `GET /me`
- `PUT /me`

### Farms (`/api/v1/farms`) [Authorized]
- `GET /`
- `GET /{id:guid}`
- `GET /{id:guid}/state`
- `POST /`
- `PUT /{id:guid}`
- `DELETE /{id:guid}`

### AI (`/api/v1/ai`) [Authorized]
- `POST /crop-recommendation`
- `GET /crop-recommendation/history`
- `POST /fertilizer-recommendation`
- `GET /fertilizer-recommendation/history`

### Plant Detection (`/api/v1/PlantDetection`) [Authorized]
- `POST /detect`
- `GET /getDetections`
- `GET /getDetections/{detectionId}`

### Insect Detection (`/api/v1/InsectDetection`) [Authorized]
- `POST /detect`
- `GET /getDetections`
- `GET /getDetections/{detectionId}`

### Notifications (`/api/v1/notifications`) [Authorized]
- `GET /`

### Billing (`/api/v1/billing`)
- `POST /checkout-session` [Authorized]
- `POST /checkout-session/{sessionId}/complete` [Authorized]
- `GET /payments` [Authorized]
- `POST /cancel-subscription` [Authorized]
- `POST /stripe/webhook` [Public]

### Weather (`/api/v1/weather`) [Public]
- `GET /?latitude={lat}&longitude={lon}`

### Geocode (`/api/v1/geocode`) [Public]
- `GET /reverse?latitude={lat}&longitude={lon}`

### Config (`/api/v1/config`) [Public]
- `GET /frontend`

## Testing
```bash
dotnet test BackEnd.sln
```

## CORS
Policy name: `CorsPolicy`

Configured from `Cors:AllowedOrigins`. If empty, fallback defaults are:
- local development origins from API startup defaults

Credentials are enabled.

## Docker
A `Dockerfile` exists, but it currently needs alignment with this repository:
- Solution file copied in Dockerfile is `AgriVision.sln`, while repo uses `BackEnd.sln`
- Docker images are `dotnet 8`, while the project targets `.NET 9`

Update Dockerfile before production deployment.
