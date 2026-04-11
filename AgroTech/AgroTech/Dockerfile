FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["AgroTech/AgroTech.csproj", "AgroTech/"]
COPY ["AgroTech.Contracts/AgroTech.Contracts.csproj", "AgroTech.Contracts/"]

RUN dotnet restore "AgroTech/AgroTech.csproj"

COPY . .

WORKDIR "/src/AgroTech"
RUN dotnet build "AgroTech.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AgroTech.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AgroTech.dll"]