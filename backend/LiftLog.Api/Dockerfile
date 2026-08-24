FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY LiftLog.Lib/LiftLog.Lib.csproj LiftLog.Lib/
COPY LiftLog.Api/LiftLog.Api.csproj LiftLog.Api/
RUN dotnet restore LiftLog.Api/LiftLog.Api.csproj

COPY . .
RUN dotnet publish LiftLog.Api/LiftLog.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "LiftLog.Api.dll"]
