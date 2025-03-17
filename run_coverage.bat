@echo off
echo Запуск тестов с измерением покрытия

REM Очистка предыдущих результатов
if exist .\FinanceApp.Tests\TestResults rmdir /s /q .\FinanceApp.Tests\TestResults

REM Запускаем тесты с покрытием
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./FinanceApp.Tests/TestResults" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

REM Создаем директорию для отчета
set REPORT_DIR=.\FinanceApp.Tests\TestResults\report
mkdir %REPORT_DIR% 2>nul

REM Находим файл с покрытием
for /r .\FinanceApp.Tests\TestResults %%f in (coverage.cobertura.xml) do (
    echo Файл с результатами: %%f
    echo Проверка установки reportgenerator...
    
    REM Проверяем, установлен ли reportgenerator
    where reportgenerator >nul 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo Установка reportgenerator...
        dotnet tool install -g dotnet-reportgenerator-globaltool
    )
    
    echo Генерация подробного отчета...
    reportgenerator -reports:"%%f" -targetdir:%REPORT_DIR% -reporttypes:Html
    
    echo Отчет сгенерирован в %REPORT_DIR%
    start "" %REPORT_DIR%\index.html
    goto :done
)

echo Файл с результатами покрытия не найден.
echo Поиск выполнялся в: .\FinanceApp.Tests\TestResults\

:done
