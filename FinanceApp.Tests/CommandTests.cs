using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Command;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.IO;
using FinanceApp.Util; // <-- Добавлено

namespace FinanceApp.Tests
{
    public class CommandTests
    {
        [Fact]
        public void UpdateBalanceCommand_ShouldUpdateBalance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var account = accountFacade.CreateAccount(1, "Test", 100);
            
            var command = new UpdateBalanceCommand(accountFacade, account.Id, 50);
            
            // Act
            command.Execute();
            
            // Assert
            Assert.Equal(150, account.Balance);
        }
        
        [Fact]
        public void CreateOperationCommand_ShouldCreateOperation()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 100);
            var categoryId = 1;
            
            var command = new CreateOperationCommand(
                operationFacade, 
                OperationType.Income, 
                account.Id, 
                200, 
                categoryId);
            
            // Act
            command.Execute();
            
            // Assert
            Assert.Equal(300, account.Balance);
            Assert.Single(operationFacade.GetAllOperations());
        }
        
        [Fact]
        public void TimeMeasureDecorator_ShouldExecuteCommand()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var account = accountFacade.CreateAccount(1, "Test", 100);
            
            ICommand command = new UpdateBalanceCommand(accountFacade, account.Id, 50);
            ICommand decoratedCommand = new TimeMeasureDecorator(command);
            
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                decoratedCommand.Execute();
                
                // Assert
                Assert.Equal(150, account.Balance);
            }
            finally
            {
                // Восстановить Console.Out
                Console.SetOut(originalOutput);
            }
        }

        [Fact]
        public void DeleteOperationCommand_ShouldDeleteOperation()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 100);
            var categoryId = 1;
            
            // Создаем операцию через фасад, чтобы она обновила баланс
            var operation = operationFacade.CreateOperation(OperationType.Expense, account.Id, 30, DateTime.Now, categoryId);
            Assert.Single(operationFacade.GetAllOperations());
            Assert.Equal(70, account.Balance);
            
            var command = new DeleteOperationCommand(operationFacade, operation.Id);
            
            // Act
            command.Execute();
            
            // Assert
            Assert.Empty(operationFacade.GetAllOperations());
            Assert.Equal(100, account.Balance); // Баланс должен вернуться к исходному состоянию
        }
    }
}
