# Build context: repository root (see docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Restore layer: only project files, so source edits do not re-download packages
COPY Directory.Build.props .editorconfig ./
COPY src/AdotPet.Domain/AdotPet.Domain.csproj src/AdotPet.Domain/
COPY src/AdotPet.Application/AdotPet.Application.csproj src/AdotPet.Application/
COPY src/AdotPet.Infrastructure/AdotPet.Infrastructure.csproj src/AdotPet.Infrastructure/
COPY src/AdotPet.Api/AdotPet.Api.csproj src/AdotPet.Api/
RUN dotnet restore src/AdotPet.Api/AdotPet.Api.csproj
COPY src/ src/
RUN dotnet publish src/AdotPet.Api -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Seed\photos is copied to the publish folder by AdotPet.Infrastructure.csproj
# Run as non-root (APP_UID is defined by the aspnet image); the photos directory must be
# writable by it. A named volume mounted here inherits this ownership on first use.
RUN mkdir -p /data/photos && chown -R $APP_UID /data/photos
USER $APP_UID
ENTRYPOINT ["dotnet", "AdotPet.Api.dll"]
