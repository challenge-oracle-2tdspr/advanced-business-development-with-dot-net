FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AgroTech.Worker.Readings/AgroTech.Worker.Readings.csproj", "AgroTech.Worker.Readings/"]
COPY ["AgroTech.Contracts/AgroTech.Contracts.csproj", "AgroTech.Contracts/"]

RUN dotnet restore "AgroTech.Worker.Readings/AgroTech.Worker.Readings.csproj"

COPY . .

WORKDIR "/src/AgroTech.Worker.Readings"
RUN dotnet build "AgroTech.Worker.Readings.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AgroTech.Worker.Readings.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroTech.Worker.Readings.dll"]