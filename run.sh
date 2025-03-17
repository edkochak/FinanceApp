#!/bin/bash
echo "Используется .NET 9.0"
dotnet build FinanceApp.sln
cd FinanceApp
dotnet run
read -p "Press any key to continue..."
