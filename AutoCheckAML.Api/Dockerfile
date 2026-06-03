# ===========================================
# Dockerfile - BackEnd (AutoCheckAML.Api)
# .NET 10 Web API + SQLite
# ===========================================

# --- Etapa 1: Build / Publish ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el csproj primero para cachear dependencias
COPY AutoCheckAML.Api.csproj .
RUN dotnet restore

# Copiar el resto del código y publicar
COPY . .
RUN dotnet publish -c Release -o /app/publish

# --- Etapa 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Crear carpeta de datos para SQLite y dar permisos
RUN mkdir -p /app/data && chmod 777 /app/data

# Copiar el publicado
COPY --from=build /app/publish .

# Puerto expuesto por la API
EXPOSE 5000

# Entorno
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AutoCheckAML.Api.dll"]
