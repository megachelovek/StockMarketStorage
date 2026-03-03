FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY mock/MockWebSocketServer/MockWebSocketServer.csproj .
RUN dotnet restore MockWebSocketServer.csproj
COPY mock/MockWebSocketServer/ .
RUN dotnet publish MockWebSocketServer.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MockWebSocketServer.dll"]
