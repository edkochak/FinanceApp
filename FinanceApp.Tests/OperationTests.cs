using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;

namespace FinanceApp.Tests
{
    public class OperationTests
    {
        [Fact]
        public void CreateOperation_ShouldUpdateAccountBalance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test Account", 1000);
            var categoryId = 1;
            
            // Act
            operationFacade.CreateOperation(OperationType.Income, account.Id, 500, DateTime.Now, categoryId);
            
            // Assert
            Assert.Equal(1500, account.Balance);
        }
        
        [Fact]
        public void DeleteOperation_ShouldRevertAccountBalance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test Account", 1000);
            var categoryId = 1;
            
            // Создаем тестовую операцию - обратите внимание, тест ожидает, что операция изменит баланс
            operationFacade.CreateOperation(OperationType.Expense, account.Id, 300, DateTime.Now, categoryId);
            Assert.Equal(700, account.Balance); // Проверяем что баланс уменьшился
            
            var operations = operationFacade.GetAllOperations();
            var operation = operations[0];
            
            // Act
            var result = operationFacade.DeleteOperation(operation.Id);
            
            // Assert
            Assert.True(result);
            Assert.Equal(1000, account.Balance); // Баланс должен вернуться к исходному состоянию
        }
        
        [Fact]
        public void CreateOperation_WithInvalidAccountId_ShouldThrowException()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                operationFacade.CreateOperation(OperationType.Income, 999, 500, DateTime.Now, 1));
                
            Assert.Contains("999", ex.Message);
        }
    }
}
