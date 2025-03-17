using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using FinanceApp.Services.Command;
using System;
using System.IO;
using FinanceApp.Util;

namespace FinanceApp.Tests
{
    public class UseCaseTests
    {
        [Fact]
        public void TimeMeasureDecorator_OutputFormat_ShouldBeCorrect()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var account = accountFacade.CreateAccount("Test", 1000);
            
            ICommand command = new UpdateBalanceCommand(accountFacade, account.Id, 500);
            ICommand decoratedCommand = new TimeMeasureDecorator(command, "TestCommand");
            
            var originalOutput = Console.Out;
            var stringWriter = new NonClosingStringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                decoratedCommand.Execute();
                
                // Assert
                var output = stringWriter.ToString();
                Assert.Matches("Время выполнения команды TestCommand: \\d+ мс", output);
                Assert.Equal(1500, account.Balance);
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }
        
        [Fact]
        public void NestedTimeMeasureDecorators_ShouldWorkCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var account = accountFacade.CreateAccount("Test", 1000);
            
            ICommand command = new UpdateBalanceCommand(accountFacade, account.Id, 500);
            ICommand innerDecorated = new TimeMeasureDecorator(command, "Inner");
            ICommand outerDecorated = new TimeMeasureDecorator(innerDecorated, "Outer");
            
            // Перехватываем вывод в консоль
            var originalOutput = Console.Out;
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);
            
            try
            {
                // Act
                outerDecorated.Execute();
                
                // Assert
                var output = stringWriter.ToString();
                Assert.Contains("Время выполнения команды Inner", output);
                Assert.Contains("Время выполнения команды Outer", output);
                Assert.Equal(1500, account.Balance);
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }
        
        [Fact]
        public void RetrieveAccountFromCache_ShouldReturnSameInstance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var proxy = new Services.Proxy.BankAccountProxy();
            
            // Act
            var originalAccount = accountFacade.CreateAccount("Cached Account", 1000);
            proxy.Save(originalAccount);
            
            // Изменим баланс исходного аккаунта
            originalAccount.UpdateBalance(500);
            
            // Получим аккаунт из кэша
            var cachedAccount = proxy.GetById(originalAccount.Id);
            
            // Assert
            Assert.NotNull(cachedAccount);
            Assert.Same(originalAccount, cachedAccount);
            Assert.Equal(1500, cachedAccount.Balance);
        }
        
        [Fact]
        public void MultipleOperationCommands_ShouldExecuteInOrder()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount("Test", 1000);
            var category = categoryFacade.CreateCategory(CategoryType.Income, "Test");
            
            var commands = new List<ICommand>
            {
                new CreateOperationCommand(operationFacade, OperationType.Income, account.Id, 100, category.Id),
                new CreateOperationCommand(operationFacade, OperationType.Expense, account.Id, 50, category.Id),
                new CreateOperationCommand(operationFacade, OperationType.Income, account.Id, 200, category.Id),
                new CreateOperationCommand(operationFacade, OperationType.Expense, account.Id, 150, category.Id)
            };
            
            // Act
            foreach (var command in commands)
            {
                command.Execute();
            }
            
            // Assert
            Assert.Equal(1100, account.Balance); // 1000 + 100 - 50 + 200 - 150 = 1100
            Assert.Equal(4, operationFacade.GetAllOperations().Count);
        }
        
        [Fact]
        public void AccountTypeEnum_ShouldBeUsedCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            
            // Act
            var checkingAccount = accountFacade.CreateAccount("Checking", 1000, AccountType.Checking);
            var savingsAccount = accountFacade.CreateAccount("Savings", 2000, AccountType.Savings);
            var creditAccount = accountFacade.CreateAccount("Credit", 3000, AccountType.Credit);
            
            // Assert
            Assert.Equal(AccountType.Checking, checkingAccount.Type);
            Assert.Equal(AccountType.Savings, savingsAccount.Type);
            Assert.Equal(AccountType.Credit, creditAccount.Type);
        }
    }
}
