using Xunit;
using FinanceApp.Services.Import;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace FinanceApp.Tests
{
    public class ImportTests
    {
        [Fact]
        public void CsvImport_ShouldProcessCorrectly()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "BankAccount;1;Test;1000\nCategory;2;Income;Salary");
                var importer = new CsvImport();
                
                // Act - для тестирования используем приватные методы через рефлексию
                var parseDataMethod = typeof(CsvImport).GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
                var processDataMethod = typeof(CsvImport).GetMethod("ProcessData", BindingFlags.NonPublic | BindingFlags.Instance);
                
                // Проверяем, что методы были найдены
                if (parseDataMethod == null || processDataMethod == null)
                {
                    Assert.Fail("Методы ParseData или ProcessData не найдены в классе CsvImport");
                    return;
                }
                
                var fileContent = File.ReadAllText(tempFile);
                var parsedData = parseDataMethod.Invoke(importer, new object[] { fileContent });
                
                // Assert
                Assert.NotNull(parsedData);
                var lines = parsedData as string[];
                Assert.NotNull(lines);
                Assert.Equal(2, lines.Length);
                Assert.Equal("BankAccount;1;Test;1000", lines[0]);
                
                // Проверяем что вызов ProcessData не выбрасывает исключений
                Assert.Null(Record.Exception(() => processDataMethod.Invoke(importer, new object[] { parsedData })));
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        
        [Fact]
        public void JsonImport_ShouldHandleEmptyInput()
        {
            // Arrange
            var importer = new JsonImport();
            var parseDataMethod = typeof(JsonImport).GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Проверяем, что метод был найден
            if (parseDataMethod == null)
            {
                Assert.Fail("Метод ParseData не найден в классе JsonImport");
                return;
            }
            
            // Act - вызываем метод и проверяем результат
            var result = parseDataMethod.Invoke(importer, new object[] { "" });
            
            // Assert - для пустой строки метод возвращает null
            Assert.Null(result);
        }
    }
}
