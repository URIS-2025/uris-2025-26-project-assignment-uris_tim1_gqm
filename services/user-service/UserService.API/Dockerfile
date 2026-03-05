FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared libraries
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
COPY ["shared/Shared.Contracts/Shared.Contracts.csproj", "shared/Shared.Contracts/"]
COPY ["shared/Shared.Auth/Shared.Auth.csproj", "shared/Shared.Auth/"]
# Copy service projects
COPY ["services/user-service/UserService.Domain/UserService.Domain.csproj", "services/user-service/UserService.Domain/"]
COPY ["services/user-service/UserService.Application/UserService.Application.csproj", "services/user-service/UserService.Application/"]
COPY ["services/user-service/UserService.Infrastructure/UserService.Infrastructure.csproj", "services/user-service/UserService.Infrastructure/"]
COPY ["services/user-service/UserService.API/UserService.API.csproj", "services/user-service/UserService.API/"]

RUN dotnet restore "services/user-service/UserService.API/UserService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/user-service/ services/user-service/

WORKDIR "/src/services/user-service/UserService.API"
RUN dotnet publish "UserService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UserService.API.dll"]
