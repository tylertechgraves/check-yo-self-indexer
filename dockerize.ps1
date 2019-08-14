Set-Location ./check-yo-self-indexer
dotnet publish -c Debug -r linux-musl-x64
docker build --build-arg Configuration=Debug -t check-yo-self-indexer:1.0.1 .
Set-Location ../