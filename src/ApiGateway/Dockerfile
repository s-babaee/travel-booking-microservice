FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ApiGateway/ApiGateway.csproj src/ApiGateway/
RUN dotnet restore src/ApiGateway/ApiGateway.csproj

COPY . .
RUN dotnet publish src/ApiGateway/ApiGateway.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
