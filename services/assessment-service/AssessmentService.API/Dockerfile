FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library
COPY ["shared/Shared.HMAC/Shared.HMAC.csproj", "shared/Shared.HMAC/"]
# Copy service project
COPY ["services/assessment-service/AssessmentService.API/AssessmentService.API.csproj", "services/assessment-service/AssessmentService.API/"]

RUN dotnet restore "services/assessment-service/AssessmentService.API/AssessmentService.API.csproj"

# Copy everything else
COPY shared/ shared/
COPY services/assessment-service/ services/assessment-service/

WORKDIR "/src/services/assessment-service/AssessmentService.API"
RUN dotnet publish "AssessmentService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AssessmentService.API.dll"]
