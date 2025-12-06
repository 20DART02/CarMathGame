# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY CarMathGame.csproj .
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install required system dependencies for PostgreSQL
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    curl \
    libgssapi-krb5-2 \
    libssl3 \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy from build stage
COPY --from=build /app/publish .

# Set environment for Railway
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "CarMathGame.dll"]