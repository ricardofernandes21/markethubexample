FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY src/AmazonConnector/AmazonConnector.csproj src/AmazonConnector/
COPY shared/Contracts/Contracts.csproj shared/Contracts/

RUN dotnet restore src/AmazonConnector/AmazonConnector.csproj

COPY src/AmazonConnector/ src/AmazonConnector/
COPY shared/Contracts/ shared/Contracts/

RUN dotnet publish src/AmazonConnector/AmazonConnector.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AmazonConnector.dll"]
