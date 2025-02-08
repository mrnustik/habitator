FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["Habitator.API/Habitator.API.csproj", "Habitator.API/"]
RUN dotnet restore "Habitator.API/Habitator.API.csproj" -a $TARGETARCH

COPY . .
RUN dotnet publish "Habitator.API/Habitator.API.csproj" -c $BUILD_CONFIGURATION -a $TARGETARCH -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
EXPOSE 8080
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Habitator.API.dll"]