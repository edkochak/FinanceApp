using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FinanceApp.Tests
{
    public class OperationFacadeTests
    {
        [Fact]
        public void CreateOperation_ShouldAddToCollectionAndUpdateBalance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount("Test Account", 1000);
            var categoryId = 1;
            
            // Act
            var operation = operationFacade.CreateOperation(
                OperationType.Income, account.Id, 500, DateTime.Now, categoryId, "Test");
            
            // Assert
            Assert.NotNull(operation);
            Assert.Equal(1500, account.Balance);
            
            var operations = operationFacade.GetAllOperations();
            Assert.Single(operations);
            Assert.Equal(operation, operations.First());
        }
        
        [Fact]
        public void GetOperationsByAccount_ShouldReturnCorrectOperations()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account1 = accountFacade.CreateAccount(1, "Account 1", 1000);
            var account2 = accountFacade.CreateAccount(2, "Account 2", 1000);
            var categoryId = 1;
            
            operationFacade.CreateOperation(1, OperationType.Income, account1.Id, 100, DateTime.Now, categoryId);
            operationFacade.CreateOperation(2, OperationType.Income, account1.Id, 200, DateTime.Now, categoryId);
            operationFacade.CreateOperation(3, OperationType.Income, account2.Id, 300, DateTime.Now, categoryId);
            
            // Act
            var account1Operations = operationFacade.GetOperationsByAccount(account1.Id);
            var account2Operations = operationFacade.GetOperationsByAccount(account2.Id);
            var nonExistentAccountOperations = operationFacade.GetOperationsByAccount(999);
            
            // Assert
            Assert.Equal(2, account1Operations.Count);
            Assert.Single(account2Operations);
            Assert.Empty(nonExistentAccountOperations);
            
            Assert.All(account1Operations, op => Assert.Equal(account1.Id, op.BankAccountId));
            Assert.All(account2Operations, op => Assert.Equal(account2.Id, op.BankAccountId));
        }
        
        [Fact]
        public void GetOperation_ShouldReturnCorrectOperation()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var categoryId = 1;
            
            operationFacade.CreateOperation(101, OperationType.Income, account.Id, 100, DateTime.Now, categoryId, "Test 1");
            operationFacade.CreateOperation(102, OperationType.Expense, account.Id, 200, DateTime.Now, categoryId, "Test 2");
            
            // Act
            var op1 = operationFacade.GetOperation(101);
            var op2 = operationFacade.GetOperation(102);
            var opNonExistent = operationFacade.GetOperation(999);
            
            // Assert
            Assert.NotNull(op1);
            Assert.NotNull(op2);
            Assert.Null(opNonExistent);
            
            Assert.Equal(101, op1.Id);
            Assert.Equal("Test 1", op1.Description);
            Assert.Equal(OperationType.Income, op1.Type);
            
            Assert.Equal(102, op2.Id);
            Assert.Equal("Test 2", op2.Description);
            Assert.Equal(OperationType.Expense, op2.Type);
        }
        
        [Fact]
        public void DeleteOperation_NonExistent_ShouldReturnFalse()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            // Act
            var result = operationFacade.DeleteOperation(999);
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void GetOperationsGroupedByCategory_WithNoOperations_ShouldReturnEmptyDictionary()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            // Act
            var result = operationFacade.GetOperationsGroupedByCategory(OperationType.Income);
            
            // Assert
            Assert.Empty(result);
        }
        
        [Fact]
        public void GetIncomeTotal_WithNoOperations_ShouldReturnZero()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            // Act
            var total = operationFacade.GetIncomeTotal();
            
            // Assert
            Assert.Equal(0, total);
        }
        
        [Fact]
        public void GetExpenseTotal_WithNoOperations_ShouldReturnZero()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            // Act
            var total = operationFacade.GetExpenseTotal();
            
            // Assert
            Assert.Equal(0, total);
        }
    }
}
