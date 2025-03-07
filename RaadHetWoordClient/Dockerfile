# Gebruik de officiële .NET SDK om te bouwen
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopieer alleen de projectbestanden om dependencies te cachen
COPY *.csproj ./
RUN dotnet restore

# Kopieer alle bestanden en bouw de applicatie
COPY . . 
RUN dotnet publish -c Release -o /app/publish-client

# Gebruik Nginx als webserver voor Blazor WebAssembly
FROM nginx:latest
COPY --from=build /app/publish-client/wwwroot /usr/share/nginx/html

# Expose poort 80
EXPOSE 80
