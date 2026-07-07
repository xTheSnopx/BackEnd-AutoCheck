# ===========================================
# Dockerfile - BackEnd (AutoCheckAML.Api)
# .NET 10 Web API + PostgreSQL (Supabase)
# ===========================================

# --- Etapa 1: Build / Publish ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el csproj primero para cachear dependencias
COPY AutoCheckAML.Api/AutoCheckAML.Api.csproj ./AutoCheckAML.Api/
RUN dotnet restore ./AutoCheckAML.Api/AutoCheckAML.Api.csproj

# Copiar el resto del código y publicar
COPY AutoCheckAML.Api/ ./AutoCheckAML.Api/
RUN dotnet publish ./AutoCheckAML.Api/AutoCheckAML.Api.csproj -c Release -o /app/publish

# --- Etapa 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copiar el publicado
COPY --from=build /app/publish .

# Render usa la variable PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}

EXPOSE ${PORT:-5000}

ENTRYPOINT ["dotnet", "AutoCheckAML.Api.dll"]
