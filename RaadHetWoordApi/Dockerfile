# Gebruik een officiële .NET runtime als basisimage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaadHetWoordApi/RaadHetWoordApi.csproj", "RaadHetWoordApi/"]
RUN dotnet restore "RaadHetWoordApi/RaadHetWoordApi.csproj"
COPY . .
WORKDIR "/src/RaadHetWoordApi"
RUN dotnet build -c Release -o /app/build

# Publish image
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage: runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaadHetWoordApi.dll"]
