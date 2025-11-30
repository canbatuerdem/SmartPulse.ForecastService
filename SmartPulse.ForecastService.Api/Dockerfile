FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SmartPulse.ForecastService.Api/SmartPulse.ForecastService.Api.csproj", "SmartPulse.ForecastService.Api/"]
COPY ["SmartPulse.ForecastService.Service/SmartPulse.ForecastService.Service.csproj", "SmartPulse.ForecastService.Service/"]
COPY ["SmartPulse.ForecastService.Repository/SmartPulse.ForecastService.Repository.csproj", "SmartPulse.ForecastService.Repository/"]

RUN dotnet restore "SmartPulse.ForecastService.Api/SmartPulse.ForecastService.Api.csproj"

COPY . .
WORKDIR "/src/SmartPulse.ForecastService.Api"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartPulse.ForecastService.Api.dll"]
