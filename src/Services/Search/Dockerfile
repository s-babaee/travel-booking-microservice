FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Services/Search/Search.Api/Search.Api.csproj src/Services/Search/Search.Api/
RUN dotnet restore src/Services/Search/Search.Api/Search.Api.csproj

COPY . .
RUN dotnet publish src/Services/Search/Search.Api/Search.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Search.Api.dll"]
