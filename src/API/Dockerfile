# ── Stage 1: Build ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for layer caching
COPY ["src/API/API.csproj", "src/API/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]

RUN dotnet restore "src/API/API.csproj"

# Copy all source
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "API.csproj" -c Release -o /app/build

# ── Stage 2: Publish ──────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Stage 3: Runtime ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Create logs directory
RUN mkdir -p /app/logs

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "API.dll"]