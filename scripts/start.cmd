@echo off

cd ..\RankCalculator\
start dotnet run

cd ..\nats-server\
start nats-server.exe

cd ..\Valuator\
start dotnet run --urls "http://0.0.0.0:5001"
start dotnet run --urls "http://0.0.0.0:5002"

cd C:\nginx\nginx-1.25.4
start C:\nginx\nginx-1.25.4\conf\nginx.conf