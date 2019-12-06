FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
ARG Configuration=Debug
WORKDIR /app
EXPOSE 80
COPY bin/$Configuration/netcoreapp3.1/linux-musl-x64/publish .
ENV urls "http://*:80"
ENTRYPOINT ["dotnet", "check-yo-self-indexer.dll"]