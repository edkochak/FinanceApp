#!/bin/bash
echo "Запуск тестов с измерением покрытия"

# Очистка предыдущих результатов покрытия
rm -rf ./FinanceApp.Tests/TestResults/

# Запускаем тесты с явным указанием форматов покрытия и выходного файла
dotnet test --collect:"XPlat Code Coverage" --results-directory:"./FinanceApp.Tests/TestResults" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# Находим файл с результатами покрытия
COVERAGE_FILE=$(find ./FinanceApp.Tests/TestResults -name "coverage.cobertura.xml" -type f)

if [ -f "$COVERAGE_FILE" ]; then
    echo "Файл с результатами найден: $COVERAGE_FILE"
    echo "Суммарное покрытие кода:"
    
    # Получаем покрытие строк
    LINE_RATE=$(grep -o 'line-rate="[0-9.]*"' $COVERAGE_FILE | head -1 | cut -d'"' -f2)
    if [ -n "$LINE_RATE" ]; then
        COVERAGE_PERCENT=$(awk "BEGIN {printf \"%.2f%%\", $LINE_RATE * 100}")
        echo "Покрытие строк кода: $COVERAGE_PERCENT"
        
        # Проверяем, достигли ли нужного покрытия
        if (( $(echo "$LINE_RATE >= 0.65" | bc -l) )); then
            echo "🎉 Поздравляем! Покрытие кода превышает необходимые 65%"
        else
            echo "⚠️ Внимание: покрытие кода ниже требуемых 65%. Текущее значение: $COVERAGE_PERCENT"
        fi
    else
        echo "Не удалось извлечь информацию о покрытии из файла $COVERAGE_FILE"
    fi
    
    # Генерация HTML отчета
    REPORT_DIR="./FinanceApp.Tests/TestResults/report"
    mkdir -p $REPORT_DIR
    
    echo "Генерация HTML отчета..."
    
    # Счетчик строк для классов
    echo "<html><head><title>Отчет о покрытии кода</title>" > $REPORT_DIR/index.html
    echo "<style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        tr:nth-child(even) { background-color: #f2f2f2; }
        th { background-color: #4CAF50; color: white; }
        .good { color: green; }
        .bad { color: red; }
        .warning { color: orange; }
    </style></head><body>" >> $REPORT_DIR/index.html
    
    echo "<h1>Отчет о покрытии кода</h1>" >> $REPORT_DIR/index.html
    echo "<p>Общее покрытие: <span class='$([ $(echo "$LINE_RATE < 0.65" | bc -l) -eq 1 ] && echo "bad" || echo "good")'>$COVERAGE_PERCENT</span></p>" >> $REPORT_DIR/index.html
    
    echo "<h2>Покрытие по классам:</h2>" >> $REPORT_DIR/index.html
    echo "<table><tr><th>Класс</th><th>Покрытие строк</th></tr>" >> $REPORT_DIR/index.html
    
    # Извлекаем информацию о покрытии по классам
    grep -A 1 '<class name=' $COVERAGE_FILE | grep -v -- "--" | paste - - | 
    sed 's/<class name=\"\(.*\)\" filename.*line-rate=\"\([0-9.]*\)\".*/\1;\2/g' | 
    while IFS=';' read -r classname linerate; do
        coverage_pct=$(awk "BEGIN {printf \"%.2f%%\", $linerate * 100}")
        css_class=$([ $(echo "$linerate < 0.5" | bc -l) -eq 1 ] && echo "bad" || ([ $(echo "$linerate < 0.75" | bc -l) -eq 1 ] && echo "warning" || echo "good"))
        echo "<tr><td>$classname</td><td class='$css_class'>$coverage_pct</td></tr>" >> $REPORT_DIR/index.html
    done
    
    echo "</table></body></html>" >> $REPORT_DIR.index.html
    
    echo "Отчет сгенерирован в $REPORT_DIR/index.html"
    
    # Открываем отчет в браузере
    if [[ "$OSTYPE" == "darwin"* ]]; then
        open $REPORT_DIR/index.html || echo "Не удалось открыть отчет автоматически"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        xdg-open $REPORT_DIR/index.html || echo "Не удалось открыть отчет автоматически"
    fi
else
    echo "Файл с результатами покрытия не найден."
    echo "Поиск выполнялся в: ./FinanceApp.Tests/TestResults/"
    find ./FinanceApp.Tests/TestResults -type f | sort
fi
