# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# .csproj nusxalash
COPY *.csproj ./
RUN dotnet restore

# Barcha kodni nusxalash
COPY . .

# Build qilish
RUN dotnet build -c Release -o /app/build

# 2️⃣ Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# 3️⃣ Run stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Exchange_Rates.Server.dll"]
