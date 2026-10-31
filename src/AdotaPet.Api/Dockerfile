# Build context: repository root (see docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Restore layer: only project files, so source edits do not re-download packages
COPY Directory.Build.props .editorconfig ./
COPY src/AdotaPet.Domain/AdotaPet.Domain.csproj src/AdotaPet.Domain/
COPY src/AdotaPet.Application/AdotaPet.Application.csproj src/AdotaPet.Application/
COPY src/AdotaPet.Infrastructure/AdotaPet.Infrastructure.csproj src/AdotaPet.Infrastructure/
COPY src/AdotaPet.Api/AdotaPet.Api.csproj src/AdotaPet.Api/
RUN dotnet restore src/AdotaPet.Api/AdotaPet.Api.csproj
COPY src/ src/
RUN dotnet publish src/AdotaPet.Api -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Seed\photos is copied to the publish folder by AdotaPet.Infrastructure.csproj
# Run as non-root (APP_UID is defined by the aspnet image); the photos directory must be
# writable by it. A named volume mounted here inherits this ownership on first use.
RUN mkdir -p /data/photos && chown -R $APP_UID /data/photos
USER $APP_UID
ENTRYPOINT ["dotnet", "AdotaPet.Api.dll"]
