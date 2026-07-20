# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and projects
COPY AgriVision.sln .
COPY AgriVision.API/AgriVision.API.csproj AgriVision.API/
COPY AgriVision.Application/AgriVision.Application.csproj AgriVision.Application/
COPY AgriVision.Domain/AgriVision.Domain.csproj AgriVision.Domain/
COPY AgriVision.Infrastructure/AgriVision.Infrastructure.csproj AgriVision.Infrastructure/

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Publish API
RUN dotnet publish AgriVision.API/AgriVision.API.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgriVision.API.dll"]
