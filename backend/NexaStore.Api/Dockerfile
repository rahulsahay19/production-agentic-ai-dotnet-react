FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["global.json", "."]
COPY ["backend/NexaStore.Api/NexaStore.Api.csproj", "backend/NexaStore.Api/"]
COPY ["backend/NexaStore.Application/NexaStore.Application.csproj", "backend/NexaStore.Application/"]
COPY ["backend/NexaStore.Domain/NexaStore.Domain.csproj", "backend/NexaStore.Domain/"]
COPY ["backend/NexaStore.Infrastructure/NexaStore.Infrastructure.csproj", "backend/NexaStore.Infrastructure/"]
RUN dotnet restore "backend/NexaStore.Api/NexaStore.Api.csproj"

COPY backend backend
RUN dotnet publish "backend/NexaStore.Api/NexaStore.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "NexaStore.Api.dll"]
