FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service project
COPY ["services/premise-service/PremiseService.API/PremiseService.API.csproj", "services/premise-service/PremiseService.API/"]

RUN dotnet restore "services/premise-service/PremiseService.API/PremiseService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/premise-service/ services/premise-service/

WORKDIR "/src/services/premise-service/PremiseService.API"
RUN dotnet publish "PremiseService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PremiseService.API.dll"]
