FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiere alle Projektdateien
COPY ["Directory.Build.props", "./"]
COPY ["styleguide.ruleset", "./"]
COPY ["src/ProductCatalog/ProductCatalog.Api/ProductCatalog.Api.csproj", "src/ProductCatalog/ProductCatalog.Api/"]
COPY ["src/ProductCatalog/ProductCatalog.Application/ProductCatalog.Application.csproj", "src/ProductCatalog/ProductCatalog.Application/"]
COPY ["src/ProductCatalog/ProductCatalog.Domain/ProductCatalog.Domain.csproj", "src/ProductCatalog/ProductCatalog.Domain/"]
COPY ["src/ProductCatalog/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj", "src/ProductCatalog/ProductCatalog.Infrastructure/"]

# Restore 
RUN dotnet restore "src/ProductCatalog/ProductCatalog.Api/ProductCatalog.Api.csproj"

# Kopiere alles andere
COPY . .

# Build
WORKDIR "/src"
RUN dotnet build "src/ProductCatalog/ProductCatalog.Api/ProductCatalog.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "src/ProductCatalog/ProductCatalog.Api/ProductCatalog.Api.csproj" -c Release -o /app/publish

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "ProductCatalog.Api.dll"]