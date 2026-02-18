FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service project
COPY ["services/access-service/AccessService.API/AccessService.API.csproj", "services/access-service/AccessService.API/"]

RUN dotnet restore "services/access-service/AccessService.API/AccessService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/access-service/ services/access-service/

WORKDIR "/src/services/access-service/AccessService.API"
RUN dotnet publish "AccessService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AccessService.API.dll"]
