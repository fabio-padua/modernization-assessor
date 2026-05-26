# Multi-stage build for the Modernization Assessor orchestrator.
# Build context is the repository root (see docker build args in CI / azure.yaml).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files first to maximize layer caching during restore.
COPY ModernizationAssessor.sln ./
COPY src/Shared/ModernizationAssessor.Shared.csproj src/Shared/
COPY src/Orchestrator/ModernizationAssessor.Orchestrator.csproj src/Orchestrator/
COPY tests/Orchestrator.Tests/ModernizationAssessor.Orchestrator.Tests.csproj tests/Orchestrator.Tests/

RUN dotnet restore src/Orchestrator/ModernizationAssessor.Orchestrator.csproj

# Copy the rest of the sources and publish a self-contained framework-dependent build.
COPY src/ src/
COPY tests/ tests/
COPY samples/ samples/

RUN dotnet publish src/Orchestrator/ModernizationAssessor.Orchestrator.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---- Runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# The aspnet:9.0 image already ships with a non-root `app` user (uid/gid 64198).
# We only need the writable working directories.
RUN mkdir -p /app/out /app/inventory \
    && chown -R app:app /app/out /app/inventory

COPY --from=build /app/publish ./
COPY --from=build /src/samples ./samples

ENV DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    ASPNETCORE_URLS= \
    ASSESSOR_INVENTORY=/app/samples/inventory/contoso.csv \
    ASSESSOR_CUSTOMER="Sample Customer"

USER app

# Default command runs an assessment using the env vars above. CI / job overrides as needed.
ENTRYPOINT ["dotnet", "ModernizationAssessor.Orchestrator.dll"]
CMD ["--inventory", "/app/samples/inventory/contoso.csv", "--customer", "Sample Customer"]
