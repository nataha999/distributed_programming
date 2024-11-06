@echo off

taskkill /f /IM valuator.exe
taskkill /f /IM nginx.exe
taskkill /f /IM rankCalculator.exe
taskkill /f /IM EventsLogger.exe
taskkill /f /IM nats-server.exe