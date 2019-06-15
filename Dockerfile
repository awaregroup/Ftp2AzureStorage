FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build-env
WORKDIR /app

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine3.9
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AwareGroup.Ftp2AzureStorage.dll"]