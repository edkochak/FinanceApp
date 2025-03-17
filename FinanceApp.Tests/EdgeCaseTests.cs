using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Linq;

namespace FinanceApp.Tests
{
    public class EdgeCaseTests
    {
        [Fact]
        public void CreateAccount_WithMaximumDecimalValue_ShouldWork()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            // Act
            var account = facade.CreateAccount("Max Balance", decimal.MaxValue);
            
            // Assert
            Assert.Equal(decimal.MaxValue, account.Balance);
        }
        
        [Fact]
        public void CreateOperation_WithVeryLargeValue_ShouldWork()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount("Large Value Test", 0);
            var categoryId = 1;
            
            decimal largeValue = 1_000_000_000_000_000m; // 1 квадриллион
            
            // Act
            var operation = operationFacade.CreateOperation(OperationType.Income, account.Id, largeValue, DateTime.Now, categoryId);
            
            // Assert
            Assert.Equal(largeValue, operation.Amount);
            Assert.Equal(largeValue, account.Balance);
        }
        
        [Fact]
        public void AccountBalance_CanBeZero_ButNotNegative()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount("Zero Balance Test", 100);
            var categoryId = 1;
            
            // Act - Операция, которая полностью обнулит баланс
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 100, DateTime.Now, categoryId);
            
            // Assert
            Assert.Equal(0, account.Balance);
            
            // Act & Assert - Операция с отрицательной суммой должна быть отвергнута
            Assert.Throws<ArgumentException>(() => 
                operationFacade.CreateOperation(OperationType.Expense, account.Id, -50, DateTime.Now, categoryId));
        }
        
        [Fact]
        public void AccountWithLongName_ShouldBeHandledCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            string longName = new string('A', 1000); // очень длинное название
            
            // Act
            var account = facade.CreateAccount(longName, 100);
            
            // Assert
            Assert.Equal(longName, account.Name);
            
            // Act - Экспорт должен работать корректно с очень длинными названиями
            var visitor = new Services.Export.CsvExportVisitor();
            account.Accept(visitor);
            var result = visitor.GetCsvResult();
            
            // Assert
            Assert.Contains(longName, result);
        }
        
        [Fact]
        public void DateTimeEdgeCases_ShouldBeHandledCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, new CategoryFacade(factory));
            
            var account = accountFacade.CreateAccount("Date Test", 1000);
            var categoryId = 1;
            
            // Крайние даты
            var minDate = new DateTime(1, 1, 1); // Минимальная дата в DateTime
            var maxDate = new DateTime(9999, 12, 31); // Максимальная дата в DateTime
            
            // Act
            var operation1 = operationFacade.CreateOperation(
                OperationType.Income, account.Id, 100, minDate, categoryId, "Min date");
                
            var operation2 = operationFacade.CreateOperation(
                OperationType.Income, account.Id, 200, maxDate, categoryId, "Max date");
                
            // Act - аналитика за весь возможный период
            var totalIncome = analyticsFacade.CalculateIncomeExpenseDifference(minDate, maxDate);
            
            // Assert
            Assert.Equal(minDate, operation1.Date);
            Assert.Equal(maxDate, operation2.Date);
            Assert.Equal(300, totalIncome);
        }
    }
}
