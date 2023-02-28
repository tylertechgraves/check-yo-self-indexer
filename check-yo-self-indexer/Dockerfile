FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine3.17
ARG Configuration=Release
WORKDIR /app
EXPOSE 8080
COPY bin/$Configuration/net7.0/publish .
ENV USER=armadillo
ENV UID=12345
ENV GID=23456
RUN addgroup -g "$GID"  -S "$USER" && \
    adduser --disabled-password --gecos "" --ingroup "$USER" --no-create-home --uid "$UID" "$USER"
ENV ASPNETCORE_URLS "http://*:8080"
ENTRYPOINT ["dotnet", "check-yo-self-indexer.dll"]