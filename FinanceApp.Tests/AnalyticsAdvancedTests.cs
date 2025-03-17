using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Tests
{
    public class AnalyticsAdvancedTests
    {
        [Fact]
        public void AnalyticsFacade_ShouldHandleEmptyData()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var startDate = DateTime.Now.AddDays(-7);
            var endDate = DateTime.Now;
            
            // Act
            var difference = analyticsFacade.CalculateIncomeExpenseDifference(startDate, endDate);
            var incomesByCategory = analyticsFacade.GetIncomeByCategory(startDate, endDate);
            var expensesByCategory = analyticsFacade.GetExpenseByCategory(startDate, endDate);
            var avgIncome = analyticsFacade.GetAverageOperationAmount(OperationType.Income, startDate, endDate);
            var avgExpense = analyticsFacade.GetAverageOperationAmount(OperationType.Expense, startDate, endDate);
            
            // Assert
            Assert.Equal(0, difference);
            Assert.Empty(incomesByCategory);
            Assert.Empty(expensesByCategory);
            Assert.Equal(0, avgIncome);
            Assert.Equal(0, avgExpense);
        }
        
        [Fact]
        public void AnalyticsFacade_WithDeletedCategory_ShouldHandleGracefully()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount("Test", 1000);
            var category = categoryFacade.CreateCategory(CategoryType.Income, "Deleted Category");
            
            // Создаем операцию
            operationFacade.CreateOperation(OperationType.Income, account.Id, 500, DateTime.Now, category.Id);
            
            // Удаляем категорию
            categoryFacade.DeleteCategory(category.Id);
            
            // Act
            var incomesByCategory = analyticsFacade.GetIncomeByCategory();
            
            // Assert
            Assert.Empty(incomesByCategory); // Категория удалена, поэтому она не должна появиться в результатах
        }

        [Fact]
        public void AnalyticsFacade_DateRangeFiltering_ShouldWorkCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount("Test", 1000);
            var categoryIncome = categoryFacade.CreateCategory(CategoryType.Income, "Income");
            var categoryExpense = categoryFacade.CreateCategory(CategoryType.Expense, "Expense");
            
            // Даты для тестирования
            var pastDate = new DateTime(2022, 1, 1);
            var futureDate = new DateTime(2025, 1, 1);
            var now = DateTime.Now;
            
            // Операции в разное время
            operationFacade.CreateOperation(OperationType.Income, account.Id, 100, pastDate, categoryIncome.Id);
            operationFacade.CreateOperation(OperationType.Income, account.Id, 200, now, categoryIncome.Id);
            operationFacade.CreateOperation(OperationType.Income, account.Id, 300, futureDate, categoryIncome.Id);
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 50, pastDate, categoryExpense.Id);
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 150, now, categoryExpense.Id);
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 250, futureDate, categoryExpense.Id);
            
            // Act - только прошлые операции
            var pastDifference = analyticsFacade.CalculateIncomeExpenseDifference(pastDate.AddDays(-1), pastDate.AddDays(1));
            var pastIncomes = analyticsFacade.GetIncomeByCategory(pastDate.AddDays(-1), pastDate.AddDays(1));
            var pastExpenses = analyticsFacade.GetExpenseByCategory(pastDate.AddDays(-1), pastDate.AddDays(1));
            
            // Act - только будущие операции
            var futureDifference = analyticsFacade.CalculateIncomeExpenseDifference(futureDate.AddDays(-1), futureDate.AddDays(1));
            
            // Assert
            Assert.Equal(50, pastDifference); // 100 - 50 = 50
            Assert.Single(pastIncomes);
            Assert.Equal(100, pastIncomes["Income"]);
            Assert.Single(pastExpenses);
            Assert.Equal(50, pastExpenses["Expense"]);
            
            Assert.Equal(50, futureDifference); // 300 - 250 = 50
        }
        
        [Fact]
        public void AnalyticsFacade_MultipleOperationsPerCategory_ShouldAggregateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);
            
            var account = accountFacade.CreateAccount("Test", 1000);
            var foodCategory = categoryFacade.CreateCategory(CategoryType.Expense, "Food");
            var salaryCategory = categoryFacade.CreateCategory(CategoryType.Income, "Salary");
            
            // Несколько операций одной категории
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 100, DateTime.Now, foodCategory.Id, "Breakfast");
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 200, DateTime.Now, foodCategory.Id, "Lunch");
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 300, DateTime.Now, foodCategory.Id, "Dinner");
            
            operationFacade.CreateOperation(OperationType.Income, account.Id, 5000, DateTime.Now, salaryCategory.Id, "Monthly");
            operationFacade.CreateOperation(OperationType.Income, account.Id, 1000, DateTime.Now, salaryCategory.Id, "Bonus");
            
            // Act
            var expensesByCategory = analyticsFacade.GetExpenseByCategory();
            var incomesByCategory = analyticsFacade.GetIncomeByCategory();
            
            // Assert
            Assert.Single(expensesByCategory);
            Assert.Equal(600, expensesByCategory["Food"]); // 100 + 200 + 300
            
            Assert.Single(incomesByCategory);
            Assert.Equal(6000, incomesByCategory["Salary"]); // 5000 + 1000
        }
    }
}
