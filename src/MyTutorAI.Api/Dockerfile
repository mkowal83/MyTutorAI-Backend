# Build context: repository root (see compose.yaml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so the package layer is cached until a project file changes.
COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/MyTutorAI.Domain/MyTutorAI.Domain.csproj src/MyTutorAI.Domain/
COPY src/MyTutorAI.Application/MyTutorAI.Application.csproj src/MyTutorAI.Application/
COPY src/MyTutorAI.Infrastructure/MyTutorAI.Infrastructure.csproj src/MyTutorAI.Infrastructure/
COPY src/MyTutorAI.Api/MyTutorAI.Api.csproj src/MyTutorAI.Api/
RUN dotnet restore src/MyTutorAI.Api/MyTutorAI.Api.csproj

COPY src/ src/
RUN dotnet publish src/MyTutorAI.Api/MyTutorAI.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyTutorAI.Api.dll"]
