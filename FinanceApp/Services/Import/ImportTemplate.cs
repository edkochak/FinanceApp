using System;
using System.IO;

namespace FinanceApp.Services.Import
{
    public abstract class ImportTemplate
    {
        public void ImportFile(string path)
        {
            // ...existing code...
            var fileContent = File.ReadAllText(path);
            var parsedData = ParseData(fileContent);
            ProcessData(parsedData);
        }

        protected abstract object ParseData(string fileContent);
        protected abstract void ProcessData(object data);
    }

    public class CsvImport : ImportTemplate
    {
        protected override object ParseData(string fileContent)
        {
            // ...existing code...
            return fileContent.Split('\n');
        }

        protected override void ProcessData(object data)
        {
            // ...existing code...
            var lines = data as string[];
            foreach(var line in lines ?? Array.Empty<string>())
            {
                Console.WriteLine($"Импорт CSV строки: {line}");
                // ...existing code...
            }
        }
    }
}