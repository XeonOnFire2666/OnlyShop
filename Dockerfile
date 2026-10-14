# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Копируем только csproj-файлы для кеширования restore
COPY ["OnlyShop/OnlyShop.csproj", "OnlyShop/"]
COPY ["OnlyShop.Adapters/OnlyShop.Adapters.csproj", "OnlyShop.Adapters/"]
COPY ["OnlyShop.Application/OnlyShop.Application.csproj", "OnlyShop.Application/"]
COPY ["OnlyShop.Core/OnlyShop.Core.csproj", "OnlyShop.Core/"]
COPY ["OnlyShop.Infrastructure/OnlyShop.Infrastructure.csproj", "OnlyShop.Infrastructure/"]

RUN dotnet restore "OnlyShop/OnlyShop.csproj"

COPY . .

WORKDIR /src/OnlyShop
# Без --no-restore: publish сам пересобирает граф зависимостей с учётом всех файлов
RUN dotnet publish "OnlyShop.csproj" -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OnlyShop.dll"]