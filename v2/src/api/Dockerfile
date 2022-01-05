FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 5000 5001

ARG NEW_RELIC_LICENSE_KEY
ARG NEW_RELIC_APP_NAME
ARG NEWRELIC_LOG_LEVEL=off

ENV ASPNETCORE_URLS=http://+:5000;https://+5001;

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

RUN mkdir -p /app/newrelic/logs

# copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["src/api/Foundant.Core.Api.csproj", "src/api/"]
RUN dotnet restore "src/api/Foundant.Core.Api.csproj"

# copy everything else and build
COPY . .
WORKDIR "/src/src/api"
RUN dotnet build "Foundant.Core.Api.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Foundant.Core.Api.csproj" -c Release -o /app/publish

# build runtime image
FROM base AS final

USER root
# install the agent
# from https://medium.com/imaginelearning/using-new-relic-with-docker-for-monitoring-net-core-applications-8ab57b0883aa

RUN mkdir /usr/local/newrelic-netcore20-agent \
    && cd /usr/local \
    && export NEW_RELIC_DOWNLOAD_URI=https://download.newrelic.com/$(wget -qO - "https://nr-downloads-main.s3.amazonaws.com/?delimiter=/&prefix=dot_net_agent/latest_release/newrelic-netcore20-agent" | grep -E -o 'dot_net_agent/latest_release/newrelic-netcore20-agent_[[:digit:]]{1,3}(\.[[:digit:]]{1,3}){3}_amd64\.tar\.gz') \
    && echo "Downloading: $NEW_RELIC_DOWNLOAD_URI into $(pwd)" \
    && wget -O - "$NEW_RELIC_DOWNLOAD_URI" | gzip -dc | tar xf -

# Enable the agent
# from https://medium.com/imaginelearning/using-new-relic-with-docker-for-monitoring-net-core-applications-8ab57b0883aa
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-netcore20-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-netcore20-agent/libNewRelicProfiler.so \
    NEW_RELIC_LICENSE_KEY=$NEW_RELIC_LICENSE_KEY \
    NEW_RELIC_APP_NAME=$NEW_RELIC_APP_NAME \
    NEWRELIC_LOG_LEVEL=$NEWRELIC_LOG_LEVEL=${NEWRELIC_LOG_LEVEL} \
    NEWRELIC_LOG_DIRECTORY=/app/newrelic/logs

USER appuser

# copy the app from the build image
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Foundant.Core.Api.dll"]
