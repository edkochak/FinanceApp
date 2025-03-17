#!/bin/bash

echo "Очистка файлов сборки и временных файлов..."

# Удаление папок и файлов сборки .NET
find . -type d -name "bin" -exec rm -rf {} +
find . -type d -name "obj" -exec rm -rf {} +
find . -type d -name "TestResults" -exec rm -rf {} +

# Удаление файлов кэша и логов Visual Studio Code
find . -name "*.suo" -delete
find . -name "*.user" -delete
find . -name "*.userprefs" -delete
find . -name "*.vs" -delete
find . -name "*.vscode" -delete
find . -name ".DS_Store" -delete
find . -name "*~" -delete
find . -name ".gitignore.swp" -delete

# Удаление временных файлов покрытия кода
find . -name "*.coverage" -delete
find . -name "*.coveragexml" -delete
find . -name "coverage.json" -delete
find . -name "coverage.cobertura.xml" -delete

# Удаление файлов, создаваемых при отладке
find . -name "*.dbmdl" -delete
find . -name "*.dbproj.schemaview" -delete
find . -name "*.jfm" -delete
find . -name "*.pfx" -delete
find . -name "*.publishsettings" -delete

# Проверка целостности решения (не удаляем файлы .csproj и .sln)
echo "Проверка файлов решения..."
find . -name "*.csproj" | wc -l
find . -name "*.sln" | wc -l

# Удаление latex temp files
find . -name "*.aux" -delete
find . -name "*.log" -delete
find . -name "*.out" -delete
find . -name "*.toc" -delete

echo "Очистка завершена."
echo "Чтобы сделать скрипт исполняемым, выполните команду: chmod +x clean.sh"
