using System;
using System.Text.Json;

namespace FinanceApp.Services.Import
{
    public class JsonImport : ImportTemplate
    {
        protected override object ParseData(string fileContent)
        {
            // Проверяем на пустую строку
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                Console.WriteLine("Ошибка парсинга JSON: пустая строка");
                return null;
            }
            
            // Используем JsonDocument для парсинга JSON
            try
            {
                using var doc = JsonDocument.Parse(fileContent);
                return doc.RootElement;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка парсинга JSON: {ex.Message}");
                return null;
            }
        }

        protected override void ProcessData(object data)
        {
            if (data == null) return;
            
            var element = (JsonElement)data;
            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    string type = "";
                    if (item.TryGetProperty("Type", out var typeProperty))
                    {
                        type = typeProperty.GetString();
                    }
                    
                    Console.WriteLine($"Импорт JSON объекта: {type}");
                }
            }
            else
            {
                Console.WriteLine("JSON-файл не содержит массива объектов");
            }
        }
    }
}
