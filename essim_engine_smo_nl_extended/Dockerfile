FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM esdlmapeditoressim/essim:gridmaster AS essim
WORKDIR /opt

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["essim_engine_smo_nl_extended/essim_engine_smo_nl_extended.csproj", "essim_engine_smo_nl_extended/"]
COPY ["essim_extension_core/essim_extension_core.csproj", "essim_extension_core/"]

RUN dotnet restore "essim_engine_smo_nl_extended/essim_engine_smo_nl_extended.csproj"
RUN dotnet restore "essim_extension_core/essim_extension_core.csproj"
COPY . .
WORKDIR "/src/essim_engine_smo_nl_extended"
RUN dotnet build "essim_engine_smo_nl_extended.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "essim_engine_smo_nl_extended.csproj" -c Release -o /app/publish

FROM base AS final
#### Add Essim components to final image
WORKDIR /opt
COPY --from=essim /opt .
ENV PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin:/opt/jdk/bin
ENV JAVA_VERSION_MAJOR=8
ENV JAVA_VERSION_MINOR=202
ENV JAVA_VERSION_BUILD=08
ENV JAVA_PACKAGE=server-jre
ENV JAVA_JCE=standard
ENV JAVA_HOME=/opt/jdk
ENV GLIBC_REPO=https://github.com/sgerrand/alpine-pkg-glibc
ENV GLIBC_VERSION=2.29-r0
ENV LANG=C.UTF-8
ENV HTTP_SERVER_SCHEME=http
ENV HTTP_SERVER_HOSTNAME=localhost
ENV HTTP_SERVER_PORT=8112
ENV HTTP_SERVER_PATH=essim
ENV MONGODB_HOST=localhost
ENV MONGODB_PORT=27017
ENV GRAFANA_ADMIN_USER=admin
ENV GRAFANA_ADMIN_PASSWORD=admin
ENV GRAFANA_EXTERNAL_URL=http://localhost:3000
ENV INFLUXDB_EXTERNAL_URL=http://localhost:8086
ENV PROFILE_QUERY_TIMEOUT=45
ENV CONSOLE_LOG_LEVEL=info
ENV FILE_LOG_LEVEL=off
ENV LOG_FILE_LOCATION=/tmp
ENV MAX_LOG_FILE_SIZE=10MB
ENV MAX_LOG_HISTORY_DAYS=1
ENV LOG_FILES_SIZE_CAP=10MB
EXPOSE 8112
#### End of ESSIM section
#### Add application environment variables
ENV AWS_ESSIM_QUEUE_URL=https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_NR}/gridmaster_essim_queue
ENV AWS_ESSIM_EXPORT_QUEUE_URL=https://sqs.eu-central-1.amazonaws.com/{AWS_ACCOUNT_NR}/gridmaster_essim_export_queue
ENV AWS_ESSIM_QUEUE_TIMEOUT=30
ENV SIMULATION_START_DATE=2018-12-31T23:00:00+0100
ENV SIMULATION_END_DATE=2019-12-31T22:00:00+0100
ENV SIMULATION_OUTPUT_TYPE=INFLUX
ENV CSV_STORAGE_LOCATION=/opt/output
#### End of application section
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "essim_engine_smo_nl_extended.dll"]