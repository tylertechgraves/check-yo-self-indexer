#!/bin/bash

cd ./check-yo-self-indexer || exit
dotnet publish -c Debug
docker build --build-arg Configuration=Debug -t check-yo-self-indexer:1.0.0 .
cd .. || exit