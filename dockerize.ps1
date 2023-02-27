Set-Location ./check-yo-self-indexer
dotnet publish -c Debug
docker build --build-arg Configuration=Debug -t check-yo-self-indexer:1.0.0 .
Set-Location ../