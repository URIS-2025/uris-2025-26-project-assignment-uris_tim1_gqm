FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared libraries
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
COPY ["shared/Shared.Contracts/Shared.Contracts.csproj", "shared/Shared.Contracts/"]
# Copy service projects
COPY ["services/audit-service/AuditService.Domain/AuditService.Domain.csproj", "services/audit-service/AuditService.Domain/"]
COPY ["services/audit-service/AuditService.Application/AuditService.Application.csproj", "services/audit-service/AuditService.Application/"]
COPY ["services/audit-service/AuditService.Infrastructure/AuditService.Infrastructure.csproj", "services/audit-service/AuditService.Infrastructure/"]
COPY ["services/audit-service/AuditService.API/AuditService.API.csproj", "services/audit-service/AuditService.API/"]

RUN dotnet restore "services/audit-service/AuditService.API/AuditService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/audit-service/ services/audit-service/

WORKDIR "/src/services/audit-service/AuditService.API"
RUN dotnet publish "AuditService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuditService.API.dll"]
