FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ARG Configuration=Release
WORKDIR /app
EXPOSE 8080
COPY bin/$Configuration/netcoreapp3.1/linux-musl-x64/publish .
ENV USER=armadillo
ENV UID=12345
ENV GID=23456
RUN addgroup -g "$GID"  -S "$USER" && \
    adduser --disabled-password --gecos "" --ingroup "$USER" --no-create-home --uid "$UID" "$USER"
ENV urls "http://*:8080"
ENTRYPOINT ["dotnet", "check-yo-self-indexer.dll"]