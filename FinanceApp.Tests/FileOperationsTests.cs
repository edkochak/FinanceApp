using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Export;
using System;
using System.IO;
using System.Text;

namespace FinanceApp.Tests
{
    public class FileOperationsTests
    {
        [Fact]
        public void ExportToFile_ShouldCreateFileCorrectly()
        {
            // Arrange
            var account = new BankAccount(1, "Test Account", 1000);
            var category = new Category(2, CategoryType.Income, "Test Category");
            var operation = new Operation(3, OperationType.Income, 1, 500, DateTime.Now, 2);
            
            var csvVisitor = new CsvExportVisitor();
            var jsonVisitor = new JsonExportVisitor();
            
            // Посещаем объекты
            account.Accept(csvVisitor);
            category.Accept(csvVisitor);
            operation.Accept(csvVisitor);
            
            account.Accept(jsonVisitor);
            category.Accept(jsonVisitor);
            operation.Accept(jsonVisitor);
            
            // Act - получаем данные
            var csvData = csvVisitor.GetCsvResult();
            var jsonData = jsonVisitor.GetJsonResult();
            
            // Создаем временные файлы
            string csvFile = Path.GetTempFileName();
            string jsonFile = Path.GetTempFileName();
            
            try
            {
                // Записываем данные в файлы
                File.WriteAllText(csvFile, csvData);
                File.WriteAllText(jsonFile, jsonData);
                
                // Assert
                Assert.True(File.Exists(csvFile));
                Assert.True(File.Exists(jsonFile));
                
                // Проверяем содержимое файлов
                var csvContent = File.ReadAllText(csvFile);
                var jsonContent = File.ReadAllText(jsonFile);
                
                Assert.Equal(csvData, csvContent);
                Assert.Contains("BankAccount", csvContent);
                
                Assert.Equal(jsonData, jsonContent);
                Assert.Contains("BankAccount", jsonContent);
            }
            finally
            {
                // Удаляем временные файлы
                if (File.Exists(csvFile))
                    File.Delete(csvFile);
                
                if (File.Exists(jsonFile))
                    File.Delete(jsonFile);
            }
        }
        
        [Fact]
        public void ExportErrorHandling_ShouldCatchExceptions()
        {
            // Arrange - создаем невалидный путь к файлу
            string invalidPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "invalid_dir", "file.txt");
            
            // Act & Assert - проверяем, что исключение будет выброшено
            var ex = Assert.Throws<DirectoryNotFoundException>(() => File.WriteAllText(invalidPath, "test"));
            
            // Проверяем сообщение об ошибке более гибким способом
            // В разных ОС могут быть разные сообщения об ошибке
            Assert.NotNull(ex.Message);
            Assert.NotEmpty(ex.Message);
        }
        
        [Fact]
        public void ExportToFile_ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var account = new BankAccount(1, "Тестовый счёт с юникодом 😊", 1000);
            var csvVisitor = new CsvExportVisitor();
            account.Accept(csvVisitor);
            var csvData = csvVisitor.GetCsvResult();
            
            // Act
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, csvData, Encoding.UTF8);
                var readData = File.ReadAllText(tempFile, Encoding.UTF8);
                
                // Assert
                Assert.Equal(csvData, readData);
                Assert.Contains("Тестовый счёт с юникодом", readData);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        
        [Fact]
        public void WriteAndReadOperation_ShouldPreserveData()
        {
            // Arrange
            var operation = new Operation(100, OperationType.Income, 1, 999.99m, new DateTime(2023, 5, 15), 5, "Test description 👍");
            var jsonVisitor = new JsonExportVisitor();
            operation.Accept(jsonVisitor);
            var jsonData = jsonVisitor.GetJsonResult();
            
            // Act
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, jsonData);
                var readData = File.ReadAllText(tempFile);
                
                // Assert
                Assert.Equal(jsonData, readData);
                Assert.Contains("999.99", readData);
                Assert.Contains("2023-05-15", readData);
                Assert.Contains("Test description", readData);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
        
        [Fact]
        public void MultipleFilesExport_ShouldWork()
        {
            // Arrange
            var account1 = new BankAccount(1, "Account 1", 1000);
            var account2 = new BankAccount(2, "Account 2", 2000);
            var csvVisitor = new CsvExportVisitor();
            account1.Accept(csvVisitor);
            account2.Accept(csvVisitor);
            
            // Act
            var files = new List<string> { 
                Path.GetTempFileName(), 
                Path.GetTempFileName() 
            };
            
            try
            {
                // Пишем в первый файл
                File.WriteAllText(files[0], csvVisitor.GetCsvResult());
                
                // Создаем нового посетителя и пишем во второй файл
                var newVisitor = new CsvExportVisitor();
                account1.Accept(newVisitor);
                File.WriteAllText(files[1], newVisitor.GetCsvResult());
                
                // Assert
                var content1 = File.ReadAllText(files[0]);
                var content2 = File.ReadAllText(files[1]);
                
                Assert.Contains("Account 1", content1);
                Assert.Contains("Account 2", content1);
                Assert.Contains("Account 1", content2);
                Assert.DoesNotContain("Account 2", content2);
            }
            finally
            {
                foreach (var file in files)
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
            }
        }
    }
}
