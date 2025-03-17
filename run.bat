@echo off
echo Используется .NET 9.0
dotnet build FinanceApp.sln
cd FinanceApp
dotnet run
pause
