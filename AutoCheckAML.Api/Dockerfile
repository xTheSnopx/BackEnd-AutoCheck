# ===========================================
# Dockerfile - BackEnd (AutoCheckAML.Api)
# .NET 10 Web API + PostgreSQL (Supabase)
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

# Copiar el publicado
COPY --from=build /app/publish .

# Render usa la variable PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT:-5000}

EXPOSE ${PORT:-5000}

ENTRYPOINT ["dotnet", "AutoCheckAML.Api.dll"]
