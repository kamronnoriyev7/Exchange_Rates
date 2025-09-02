# 1️⃣ Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Dockerfile papkada .csproj bor, shuning uchun uni to‘g‘ridan nusxalaymiz
COPY Exchange_Rates.Server.csproj ./
RUN dotnet restore Exchange_Rates.Server.csproj

# Barcha kodni nusxalash
COPY . .
RUN dotnet build Exchange_Rates.Server.csproj -c Release -o /app/build

# 2️⃣ Publish stage
FROM build AS publish
RUN dotnet publish Exchange_Rates.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# 3️⃣ Run stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Exchange_Rates.Server.dll"]
