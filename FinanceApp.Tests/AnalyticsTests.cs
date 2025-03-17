using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Collections.Generic;

namespace FinanceApp.Tests
{
    public class AnalyticsTests
    {
        [Fact]
        public void CalculateIncomeExpenseDifference_ShouldReturnCorrectValue()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount(1, "Test Account", 1000);
            var incomeCategoryId = 1;
            var expenseCategoryId = 2;
            
            var startDate = DateTime.Now.AddDays(-10);
            var endDate = DateTime.Now;
            
            operationFacade.CreateOperation(1, OperationType.Income, account.Id, 500, startDate.AddDays(1), incomeCategoryId);
            operationFacade.CreateOperation(2, OperationType.Income, account.Id, 300, startDate.AddDays(2), incomeCategoryId);
            operationFacade.CreateOperation(3, OperationType.Expense, account.Id, 200, startDate.AddDays(3), expenseCategoryId);
            operationFacade.CreateOperation(4, OperationType.Expense, account.Id, 100, startDate.AddDays(4), expenseCategoryId);
            
            // Act
            var difference = analyticsFacade.CalculateIncomeExpenseDifference(startDate, endDate);
            
            // Assert
            Assert.Equal(500, difference); // (500 + 300) - (200 + 100) = 500
        }
        
        [Fact]
        public void GetExpenseByCategory_ShouldGroupCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount(1, "Test Account", 1000);
            
            var foodCategory = categoryFacade.CreateCategory(1, CategoryType.Expense, "Еда");
            var transportCategory = categoryFacade.CreateCategory(2, CategoryType.Expense, "Транспорт");
            
            operationFacade.CreateOperation(1, OperationType.Expense, account.Id, 150, DateTime.Now.AddDays(-2), foodCategory.Id);
            operationFacade.CreateOperation(2, OperationType.Expense, account.Id, 250, DateTime.Now.AddDays(-1), foodCategory.Id);
            operationFacade.CreateOperation(3, OperationType.Expense, account.Id, 100, DateTime.Now, transportCategory.Id);
            
            // Act
            var expensesByCategory = analyticsFacade.GetExpenseByCategory();
            
            // Assert
            Assert.Equal(2, expensesByCategory.Count);
            Assert.Equal(400, expensesByCategory["Еда"]);
            Assert.Equal(100, expensesByCategory["Транспорт"]);
        }
        
        [Fact]
        public void GetAverageOperationAmount_ShouldCalculateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount(1, "Test Account", 1000);
            var categoryId = 1;
            
            operationFacade.CreateOperation(1, OperationType.Income, account.Id, 100, DateTime.Now.AddDays(-3), categoryId);
            operationFacade.CreateOperation(2, OperationType.Income, account.Id, 200, DateTime.Now.AddDays(-2), categoryId);
            operationFacade.CreateOperation(3, OperationType.Income, account.Id, 300, DateTime.Now.AddDays(-1), categoryId);
            
            // Act
            decimal average = analyticsFacade.GetAverageOperationAmount(OperationType.Income);
            
            // Assert
            Assert.Equal(200, average);
        }
    }
}
