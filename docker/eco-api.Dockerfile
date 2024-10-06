FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY src/Host/Host/Host.csproj Host/
COPY src/Core/Application/Application.csproj Application/
COPY src/Core/Domain/Domain.csproj Domain/
COPY src/Core/Shared/Shared.csproj Shared/
COPY src/Infrastructure/Infrastructure/Infrastructure.csproj Infrastructure/
COPY src/Migrators/Migrators.MSSQL/Migrators.MSSQL.csproj Migrators.MSSQL/

RUN dotnet restore "./Host/Host.csproj"

COPY src/. .
WORKDIR "/src/Host"

RUN dotnet build "./Host/Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Host/Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN ls -la /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ECO.WebApi.Host.dll"]
