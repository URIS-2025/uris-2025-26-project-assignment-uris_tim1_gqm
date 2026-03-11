FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service projects
COPY ["services/department-service/DepartmentService.Domain/DepartmentService.Domain.csproj", "services/department-service/DepartmentService.Domain/"]
COPY ["services/department-service/DepartmentService.Application/DepartmentService.Application.csproj", "services/department-service/DepartmentService.Application/"]
COPY ["services/department-service/DepartmentService.Infrastructure/DepartmentService.Infrastructure.csproj", "services/department-service/DepartmentService.Infrastructure/"]
COPY ["services/department-service/DepartmentService.API/DepartmentService.API.csproj", "services/department-service/DepartmentService.API/"]

RUN dotnet restore "services/department-service/DepartmentService.API/DepartmentService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/department-service/ services/department-service/

WORKDIR "/src/services/department-service/DepartmentService.API"
RUN dotnet publish "DepartmentService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DepartmentService.API.dll"]
