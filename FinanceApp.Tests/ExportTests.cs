using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Export;
using System;
using System.IO;
using System.Text.Json;

namespace FinanceApp.Tests
{
    public class ExportTests
    {
        [Fact]
        public void CsvExportVisitor_ShouldGenerateCorrectOutput()
        {
            // Arrange
            var account = new BankAccount(1, "Тестовый счет", 1000);
            var category = new Category(10, CategoryType.Expense, "Тест");
            var operation = new Operation(100, OperationType.Expense, 1, 500, DateTime.Now, 10);
            
            var visitor = new CsvExportVisitor();
            
            // Act
            account.Accept(visitor);
            category.Accept(visitor);
            operation.Accept(visitor);
            
            var result = visitor.GetCsvResult();
            
            // Assert
            Assert.Contains("BankAccount;1;Тестовый счет;1000", result);
            Assert.Contains("Category;10;Expense;Тест", result);
            Assert.Contains("Operation;100;Expense;1;500;10", result);
        }
        
        [Fact]
        public void JsonExportVisitor_ShouldGenerateCorrectOutput()
        {
            // Arrange
            var account = new BankAccount(1, "Тестовый счет", 1000);
            var visitor = new JsonExportVisitor();
            
            // Act
            account.Accept(visitor);
            var result = visitor.GetJsonResult();
            
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Выведем результат для диагностики
                Console.WriteLine("JSON Result: " + result);
                
                // Assert - проверим каждое поле отдельно
                Assert.Contains("\"Type\"", result);
                Assert.Contains("\"Id\"", result);
                Assert.Contains("\"Name\"", result);
                Assert.Contains("\"Balance\"", result);
                Assert.Contains("BankAccount", result);
                Assert.Contains("1", result);
                Assert.Contains("1000", result);
                
                // Дополнительно проверим, что можем десериализовать результат
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(result);
                var firstItem = jsonElement.EnumerateArray().First();
                
                Assert.Equal("BankAccount", firstItem.GetProperty("Type").GetString());
                Assert.Equal(1, firstItem.GetProperty("Id").GetInt32());
                Assert.Equal("Тестовый счет", firstItem.GetProperty("Name").GetString());
                Assert.Equal(1000, firstItem.GetProperty("Balance").GetDecimal());
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }
    }
}
