FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service projects
COPY ["services/orchestration-service/OrchestrationService.Domain/OrchestrationService.Domain.csproj", "services/orchestration-service/OrchestrationService.Domain/"]
COPY ["services/orchestration-service/OrchestrationService.Application/OrchestrationService.Application.csproj", "services/orchestration-service/OrchestrationService.Application/"]
COPY ["services/orchestration-service/OrchestrationService.Infrastructure/OrchestrationService.Infrastructure.csproj", "services/orchestration-service/OrchestrationService.Infrastructure/"]
COPY ["services/orchestration-service/OrchestrationService.API/OrchestrationService.API.csproj", "services/orchestration-service/OrchestrationService.API/"]

RUN dotnet restore "services/orchestration-service/OrchestrationService.API/OrchestrationService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/orchestration-service/ services/orchestration-service/

WORKDIR "/src/services/orchestration-service/OrchestrationService.API"
RUN dotnet publish "OrchestrationService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrchestrationService.API.dll"]
