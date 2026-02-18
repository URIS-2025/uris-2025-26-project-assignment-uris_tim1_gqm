FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service project
COPY ["services/qgm-goal-service/QGMGoalService.API/QGMGoalService.API.csproj", "services/qgm-goal-service/QGMGoalService.API/"]

RUN dotnet restore "services/qgm-goal-service/QGMGoalService.API/QGMGoalService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/qgm-goal-service/ services/qgm-goal-service/

WORKDIR "/src/services/qgm-goal-service/QGMGoalService.API"
RUN dotnet publish "QGMGoalService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QGMGoalService.API.dll"]
