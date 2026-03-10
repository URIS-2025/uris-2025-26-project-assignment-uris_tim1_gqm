FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service projects
COPY ["services/gqm-goal-service/GQMGoalService.API/GQMGoalService.API.csproj", "services/gqm-goal-service/GQMGoalService.API/"]
COPY ["services/gqm-goal-service/GQMGoalService.Application/GQMGoalService.Application.csproj", "services/gqm-goal-service/GQMGoalService.Application/"]
COPY ["services/gqm-goal-service/GQMGoalService.Domain/GQMGoalService.Domain.csproj", "services/gqm-goal-service/GQMGoalService.Domain/"]
COPY ["services/gqm-goal-service/GQMGoalService.Infrastructure/GQMGoalService.Infrastructure.csproj", "services/gqm-goal-service/GQMGoalService.Infrastructure/"]

RUN dotnet restore "services/gqm-goal-service/GQMGoalService.API/GQMGoalService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/gqm-goal-service/ services/gqm-goal-service/

WORKDIR "/src/services/gqm-goal-service/GQMGoalService.API"
RUN dotnet publish "GQMGoalService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GQMGoalService.API.dll"]
