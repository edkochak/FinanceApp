using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Command;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using FinanceApp.Services.Export;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FinanceApp.Tests
{
    public class MoreTests
    {
        [Fact]
        public void BankAccount_ShouldThrowOnEmptyName()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => factory.CreateBankAccount("", 100));
            Assert.Throws<ArgumentException>(() => factory.CreateBankAccount("  ", 100));
            Assert.Throws<ArgumentException>(() => factory.CreateBankAccount(null, 100));
        }
        
        [Fact]
        public void Category_ShouldThrowOnEmptyName()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => factory.CreateCategory(CategoryType.Income, ""));
            Assert.Throws<ArgumentException>(() => factory.CreateCategory(CategoryType.Expense, "  "));
            Assert.Throws<ArgumentException>(() => factory.CreateCategory(CategoryType.Income, null));
        }
        
        [Fact]
        public void Operation_ShouldThrowOnNegativeAmount()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                factory.CreateOperation(OperationType.Income, 1, -100, DateTime.Now, 1));
            Assert.Throws<ArgumentException>(() => 
                factory.CreateOperation(OperationType.Expense, 1, 0, DateTime.Now, 1));
        }
        
        [Fact]
        public void AnalyticsFacade_ShouldCalculateStats()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            // Act - запрашиваем аналитику по пустым данным
            var avgIncome = analyticsFacade.GetAverageOperationAmount(OperationType.Income);
            var avgExpense = analyticsFacade.GetAverageOperationAmount(OperationType.Expense);
            var incomesByCategory = analyticsFacade.GetIncomeByCategory();
            var expensesByCategory = analyticsFacade.GetExpenseByCategory();
            
            // Assert
            Assert.Equal(0, avgIncome);
            Assert.Equal(0, avgExpense);
            Assert.Empty(incomesByCategory);
            Assert.Empty(expensesByCategory);
        }
        
        [Fact]
        public void YamlImport_ShouldProcessCorrectly()
        {
            // Arrange
            var importer = new Services.Import.YamlImport();
            var importMethod = typeof(Services.Import.YamlImport)
                .GetMethod("ImportFile", BindingFlags.Public | BindingFlags.Instance);
            var parseMethod = typeof(Services.Import.YamlImport)
                .GetMethod("ParseData", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Создаём временный YAML-файл
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, @"---
type: BankAccount
name: Test Account
balance: 1000
---
type: Category
name: Test Category
");
                
                // Перехватываем вывод Console
                var originalOutput = Console.Out;
                var stringWriter = new StringWriter();
                Console.SetOut(stringWriter);
                
                try
                {
                    // Act & Assert - на парсинг и импорт
                    var yamlContent = File.ReadAllText(tempFile);
                    var data = parseMethod.Invoke(importer, new object[] { yamlContent });
                    Assert.NotNull(data);
                    
                    // Проверим что не выбрасывается исключение при импорте
                    var exception = Record.Exception(() => importMethod.Invoke(importer, new object[] { tempFile }));
                    Assert.Null(exception);
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }
    }
}
