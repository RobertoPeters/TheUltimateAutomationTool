FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
EXPOSE 3080
ENV ASPNETCORE_URLS="http://+:3080"
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY src/Tuat/DeployLinux/ .
RUN mkdir Settings
USER root
RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*
USER $APP_UID
ENTRYPOINT ["dotnet", "Tuat.dll"]