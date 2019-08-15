#!/bin/bash

dotnet publish ../../check-yo-self-indexer/check-yo-self-indexer.csproj -c Release -r linux-musl-x64
dotnet publish ../check-yo-self-indexer-sdk.csproj -o ./out
dotnet pack ../check-yo-self-indexer-sdk.csproj /p:PackageVersion=1.0.7-beta --configuration Debug --include-source --include-symbols --output ./nupkg --version-suffix "beta"