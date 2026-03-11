FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service projects
COPY ["services/goal-service/GoalService.Domain/GoalService.Domain.csproj", "services/goal-service/GoalService.Domain/"]
COPY ["services/goal-service/GoalService.Infrastructure/GoalService.Infrastructure.csproj", "services/goal-service/GoalService.Infrastructure/"]
COPY ["services/goal-service/GoalService.Application/GoalService.Application.csproj", "services/goal-service/GoalService.Application/"]
COPY ["services/goal-service/GoalService.API/GoalService.API.csproj", "services/goal-service/GoalService.API/"]

RUN dotnet restore "services/goal-service/GoalService.API/GoalService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/goal-service/ services/goal-service/

WORKDIR "/src/services/goal-service/GoalService.API"
RUN dotnet publish "GoalService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GoalService.API.dll"]
