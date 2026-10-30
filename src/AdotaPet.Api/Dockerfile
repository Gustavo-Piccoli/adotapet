# Build context: repository root (see docker-compose.yml)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props .editorconfig ./
COPY src/ src/
RUN dotnet publish src/AdotaPet.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# Seed\photos is copied to the publish folder by AdotaPet.Infrastructure.csproj
ENTRYPOINT ["dotnet", "AdotaPet.Api.dll"]
