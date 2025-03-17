using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Linq;

namespace FinanceApp.Tests
{
    public class OperationDetailedTests
    {
        [Fact]
        public void Operation_UpdateDescription_ShouldChangeDescriptionProperty()
        {
            // Arrange
            var operation = new Operation(1, OperationType.Income, 1, 100, DateTime.Now, 1, "Old Description");
            
            // Act
            operation.UpdateDescription("New Description");
            
            // Assert
            Assert.Equal("New Description", operation.Description);
        }

        [Fact]
        public void Operation_UpdateDescription_ShouldHandleNullAsEmptyString()
        {
            // Arrange
            var operation = new Operation(1, OperationType.Income, 1, 100, DateTime.Now, 1, "Description");
            
            // Act
            operation.UpdateDescription(null);
            
            // Assert
            Assert.Equal(string.Empty, operation.Description);
        }
        
        [Fact]
        public void Operation_UpdateAmount_ShouldChangeAmountProperty()
        {
            // Arrange
            var operation = new Operation(1, OperationType.Income, 1, 100, DateTime.Now, 1);
            
            // Act
            operation.UpdateAmount(200);
            
            // Assert
            Assert.Equal(200, operation.Amount);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Operation_UpdateAmount_ShouldThrowExceptionForInvalidValues(decimal invalidAmount)
        {
            // Arrange
            var operation = new Operation(1, OperationType.Income, 1, 100, DateTime.Now, 1);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => operation.UpdateAmount(invalidAmount));
        }
        
        [Fact]
        public void OperationFacade_GetOperationsByDateRange_ShouldFilterCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var categoryId = 1;
            
            var start = new DateTime(2023, 1, 1);
            var middle = new DateTime(2023, 1, 15);
            var end = new DateTime(2023, 1, 31);
            
            operationFacade.CreateOperation(1, OperationType.Income, account.Id, 100, start.AddDays(-1), categoryId);  // До периода
            operationFacade.CreateOperation(2, OperationType.Income, account.Id, 200, start.AddDays(1), categoryId);   // В периоде
            operationFacade.CreateOperation(3, OperationType.Income, account.Id, 300, middle, categoryId);            // В периоде
            operationFacade.CreateOperation(4, OperationType.Income, account.Id, 400, end.AddDays(-1), categoryId);   // В периоде
            operationFacade.CreateOperation(5, OperationType.Income, account.Id, 500, end.AddDays(1), categoryId);    // После периода
            
            // Act
            var operationsInRange = operationFacade.GetOperationsByDateRange(start, end);
            
            // Assert
            Assert.Equal(3, operationsInRange.Count);
            Assert.All(operationsInRange, op => Assert.True(op.Date >= start && op.Date <= end));
            Assert.Contains(operationsInRange, op => op.Id == 2);
            Assert.Contains(operationsInRange, op => op.Id == 3);
            Assert.Contains(operationsInRange, op => op.Id == 4);
        }
        
        [Fact]
        public void OperationFacade_GetIncomeTotal_ShouldCalculateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var categoryId = 1;
            
            var now = DateTime.Now;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);
            
            operationFacade.CreateOperation(1, OperationType.Income, account.Id, 100, yesterday, categoryId);
            operationFacade.CreateOperation(2, OperationType.Income, account.Id, 200, now, categoryId);
            operationFacade.CreateOperation(3, OperationType.Income, account.Id, 300, tomorrow, categoryId);
            operationFacade.CreateOperation(4, OperationType.Expense, account.Id, 500, now, categoryId); // Расход - не должен учитываться
            
            // Act
            var totalAll = operationFacade.GetIncomeTotal();
            var totalToday = operationFacade.GetIncomeTotal(now, now);
            
            // Assert
            Assert.Equal(600, totalAll); // 100 + 200 + 300
            Assert.Equal(200, totalToday); // Только за сегодня
        }
        
        [Fact]
        public void OperationFacade_GetExpenseTotal_ShouldCalculateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var categoryId = 1;
            
            var now = DateTime.Now;
            var yesterday = now.AddDays(-1);
            var tomorrow = now.AddDays(1);
            
            operationFacade.CreateOperation(1, OperationType.Expense, account.Id, 100, yesterday, categoryId);
            operationFacade.CreateOperation(2, OperationType.Expense, account.Id, 200, now, categoryId);
            operationFacade.CreateOperation(3, OperationType.Expense, account.Id, 300, tomorrow, categoryId);
            operationFacade.CreateOperation(4, OperationType.Income, account.Id, 500, now, categoryId); // Доход - не должен учитываться
            
            // Act
            var totalAll = operationFacade.GetExpenseTotal();
            var totalYesterday = operationFacade.GetExpenseTotal(yesterday, yesterday);
            
            // Assert
            Assert.Equal(600, totalAll); // 100 + 200 + 300
            Assert.Equal(100, totalYesterday); // Только за вчера
        }
        
        [Fact]
        public void OperationFacade_GetOperationsGroupedByCategory_ShouldGroupCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var category1Id = 1;
            var category2Id = 2;
            
            operationFacade.CreateOperation(1, OperationType.Expense, account.Id, 100, DateTime.Now, category1Id);
            operationFacade.CreateOperation(2, OperationType.Expense, account.Id, 200, DateTime.Now, category1Id);
            operationFacade.CreateOperation(3, OperationType.Expense, account.Id, 300, DateTime.Now, category2Id);
            operationFacade.CreateOperation(4, OperationType.Income, account.Id, 400, DateTime.Now, category1Id); // Доход - не должен учитываться
            
            // Act
            var groupedExpenses = operationFacade.GetOperationsGroupedByCategory(OperationType.Expense);
            
            // Assert
            Assert.Equal(2, groupedExpenses.Count);
            Assert.Equal(300, groupedExpenses[category1Id]); // 100 + 200
            Assert.Equal(300, groupedExpenses[category2Id]); // 300
        }
    }
}
