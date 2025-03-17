using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Services.Import
{
    public class YamlImport : ImportTemplate
    {
        protected override object ParseData(string fileContent)
        {
            // Простая реализация парсинга YAML
            // Для реального приложения лучше использовать библиотеку типа YamlDotNet
            var result = new List<Dictionary<string, string>>();
            var currentObject = new Dictionary<string, string>();
            bool isInObject = false;
            
            foreach (var line in fileContent.Split('\n'))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;
                
                if (trimmedLine == "---")
                {
                    if (isInObject && currentObject.Count > 0)
                    {
                        result.Add(currentObject);
                        currentObject = new Dictionary<string, string>();
                    }
                    isInObject = true;
                    continue;
                }
                
                if (isInObject && trimmedLine.Contains(":"))
                {
                    var parts = trimmedLine.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        currentObject[key] = value;
                    }
                }
            }
            
            if (isInObject && currentObject.Count > 0)
                result.Add(currentObject);
            
            return result;
        }

        protected override void ProcessData(object data)
        {
            if (data is List<Dictionary<string, string>> objects)
            {
                foreach (var obj in objects)
                {
                    if (obj.TryGetValue("type", out var type))
                    {
                        Console.WriteLine($"Импорт YAML объекта типа: {type}");
                        foreach (var prop in obj.Where(p => p.Key != "type"))
                        {
                            Console.WriteLine($"  {prop.Key}: {prop.Value}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Импорт YAML объекта без типа");
                    }
                }
            }
        }
    }
}
