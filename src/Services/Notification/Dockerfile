FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.csproj src/BuildingBlocks/
COPY src/Services/Notification/Notification.Worker.csproj src/Services/Notification/
RUN dotnet restore src/Services/Notification/Notification.Worker.csproj

COPY . .
RUN dotnet publish src/Services/Notification/Notification.Worker.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Notification.Worker.dll"]
