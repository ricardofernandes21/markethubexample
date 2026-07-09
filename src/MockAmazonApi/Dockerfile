FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

COPY src/MockAmazonApi/MockAmazonApi.csproj src/MockAmazonApi/

RUN dotnet restore src/MockAmazonApi/MockAmazonApi.csproj

COPY src/MockAmazonApi/ src/MockAmazonApi/

RUN dotnet publish src/MockAmazonApi/MockAmazonApi.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MockAmazonApi.dll"]
