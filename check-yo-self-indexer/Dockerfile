FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
ARG Configuration=Debug
WORKDIR /app
EXPOSE 80
COPY bin/$Configuration/netcoreapp2.2/publish .
ENV urls "http://*:80"
ENTRYPOINT ["dotnet", "check-yo-self-indexer.dll"]