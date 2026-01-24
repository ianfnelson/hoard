# syntax=docker/dockerfile:1.7-labs
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hoard.Scheduler/Hoard.Scheduler.csproj", "Hoard.Scheduler/"]
COPY ["Hoard.Core/Hoard.Core.csproj", "Hoard.Core/"]
RUN --mount=type=secret,id=nuget_auth_token \
    dotnet nuget add source "https://nuget.pkg.github.com/ianfnelson/index.json" \
    --name github \
    --username ianfnelson \
    --password "$(cat /run/secrets/nuget_auth_token)" \
    --store-password-in-clear-text
RUN dotnet restore "Hoard.Scheduler/Hoard.Scheduler.csproj"
COPY . .
WORKDIR "/src/Hoard.Scheduler"
RUN dotnet build "./Hoard.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hoard.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hoard.Scheduler.dll"]