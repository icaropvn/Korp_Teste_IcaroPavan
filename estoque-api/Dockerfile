# Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENV ConnectionStrings__Default="Host=estoque-db;Port=5432;Database=estoque_db;Username=postgres;Password=postgres"

EXPOSE 8080

ENTRYPOINT ["dotnet", "estoque-api.dll"]