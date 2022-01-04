FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/svc/WeatherMicroService/WeatherMicroService.csproj", "src/svc/WeatherMicroService/"]
RUN dotnet restore "src/svc/WeatherMicroService/WeatherMicroService.csproj"

# copy everything else and build
COPY . .
WORKDIR "/src/src/svc/WeatherMicroService/"
RUN dotnet build "WeatherMicroService.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "WeatherMicroService.csproj" -c Release -o /app/publish

# build runtime image
FROM base AS final

# copy the app from the build image
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WeatherMicroService.dll"]
