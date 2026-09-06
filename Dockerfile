FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore ECommerce.sln
RUN dotnet publish src/Api/ECommerce.Api/ECommerce.Api.csproj --no-restore --configuration Release --output /app/publish --no-self-contained

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.Api.dll"]
