FROM mcr.microsoft.com/dotnet/core/sdk:2.2-alpine3.9 AS build-env
WORKDIR /app

#copy source and compile main project binaries
COPY ./src/ ./
RUN dotnet publish ./AwareGroup.Ftp2AzureStorage/AwareGroup.Ftp2AzureStorage.csproj -c Release -o /app/out 

#build container
FROM mcr.microsoft.com/dotnet/core/runtime:2.2-alpine3.9
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "AwareGroup.Ftp2AzureStorage.dll"]