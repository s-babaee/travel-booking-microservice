FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/BuildingBlocks/BuildingBlocks.Contracts/BuildingBlocks.Contracts.csproj src/BuildingBlocks/BuildingBlocks.Contracts/
COPY src/BuildingBlocks/BuildingBlocks.csproj src/BuildingBlocks/
COPY src/Services/Hotel/Hotel.Api.csproj src/Services/Hotel/
RUN dotnet restore src/Services/Hotel/Hotel.Api.csproj

COPY . .
RUN dotnet publish src/Services/Hotel/Hotel.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
VOLUME ["/app/wwwroot/uploads"]
ENTRYPOINT ["dotnet", "Hotel.Api.dll"]
