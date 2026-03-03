FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["StockMarketStorage.sln", "./"]
COPY ["src/StockMarketStorage.Domain/StockMarketStorage.Domain.csproj", "src/StockMarketStorage.Domain/"]
COPY ["src/StockMarketStorage.Application/StockMarketStorage.Application.csproj", "src/StockMarketStorage.Application/"]
COPY ["src/StockMarketStorage.Infrastructure/StockMarketStorage.Infrastructure.csproj", "src/StockMarketStorage.Infrastructure/"]
COPY ["src/StockMarketStorage.Host/StockMarketStorage.Host.csproj", "src/StockMarketStorage.Host/"]

RUN dotnet restore "src/StockMarketStorage.Host/StockMarketStorage.Host.csproj"

COPY src/ src/
RUN dotnet build "src/StockMarketStorage.Host/StockMarketStorage.Host.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "StockMarketStorage.Host.dll"]
