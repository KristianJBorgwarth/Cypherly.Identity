FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cypherly.Identity.API/Cypherly.Identity.API.csproj", "Cypherly.Identity.API/"]
COPY ["Cypherly.Identity.Application/Cypherly.Identity.Application.csproj", "Cypherly.Identity.Application/"]
COPY ["Cypherly.Identity.Domain/Cypherly.Identity.Domain.csproj", "Cypherly.Identity.Domain/"]
COPY ["Cypherly.Common.Messaging/Cypherly.Common.Messaging.csproj", "Cypherly.Common.Messaging/"]
COPY ["Cypherly.Identity.Infrastructure/Cypherly.Identity.Infrastructure.csproj", "Cypherly.Identity.Infrastructure/"]
RUN dotnet restore "Cypherly.Identity.API/Cypherly.Identity.API.csproj"
COPY . .
WORKDIR "/src/Cypherly.Identity.API"
RUN dotnet build "./Cypherly.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cypherly.Identity.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cypherly.Identity.API.dll"]
