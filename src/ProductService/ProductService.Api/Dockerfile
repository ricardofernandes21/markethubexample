FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /repo

# Copy project files for layer-cached restore
COPY src/ProductService/ProductService.Api/ProductService.Api.csproj src/ProductService/ProductService.Api/
COPY src/ProductService/ProductService.Application/ProductService.Application.csproj src/ProductService/ProductService.Application/
COPY src/ProductService/ProductService.Domain/ProductService.Domain.csproj src/ProductService/ProductService.Domain/
COPY src/ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj src/ProductService/ProductService.Infrastructure/
COPY shared/Contracts/Contracts.csproj shared/Contracts/

RUN dotnet restore src/ProductService/ProductService.Api/ProductService.Api.csproj

# Copy source
COPY src/ProductService/ProductService.Api/ src/ProductService/ProductService.Api/
COPY src/ProductService/ProductService.Application/ src/ProductService/ProductService.Application/
COPY src/ProductService/ProductService.Domain/ src/ProductService/ProductService.Domain/
COPY src/ProductService/ProductService.Infrastructure/ src/ProductService/ProductService.Infrastructure/
COPY shared/Contracts/ shared/Contracts/

RUN dotnet publish src/ProductService/ProductService.Api/ProductService.Api.csproj \
    -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]
