FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Hoard.Api/Hoard.Api.csproj", "Hoard.Api/"]
COPY ["Hoard.Core/Hoard.Core.csproj", "Hoard.Core/"]
RUN dotnet restore "Hoard.Api/Hoard.Api.csproj"
COPY . .
WORKDIR "/src/Hoard.Api"
RUN dotnet build "./Hoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Hoard.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Hoard.Api.dll"]
