using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;

namespace FinanceApp.Tests
{
    public class BankAccountTests
    {
        [Fact]
        public void BankAccount_UpdateName_ShouldChangeNameProperty()
        {
            // Arrange
            var account = new BankAccount(1, "Old Name", 1000);
            
            // Act
            account.UpdateName("New Name");
            
            // Assert
            Assert.Equal("New Name", account.Name);
        }

        [Fact]
        public void BankAccount_UpdateName_ShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            var account = new BankAccount(1, "Test", 1000);
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => account.UpdateName(""));
            Assert.Throws<ArgumentException>(() => account.UpdateName(null));
            Assert.Throws<ArgumentException>(() => account.UpdateName("   "));
        }
        
        [Fact]
        public void BankAccountFacade_GetAllAccounts_ShouldReturnAllAccounts()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            facade.CreateAccount("Account 1", 100);
            facade.CreateAccount("Account 2", 200);
            facade.CreateAccount("Account 3", 300);
            
            // Act
            var accounts = facade.GetAllAccounts();
            
            // Assert
            Assert.Equal(3, accounts.Count);
        }
        
        [Fact]
        public void BankAccountFacade_GetAccount_ShouldReturnCorrectAccount()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            var account1 = facade.CreateAccount(1, "Account 1", 100);
            var account2 = facade.CreateAccount(2, "Account 2", 200);
            
            // Act
            var result1 = facade.GetAccount(1);
            var result2 = facade.GetAccount(2);
            var result3 = facade.GetAccount(999); // несуществующий
            
            // Assert
            Assert.Equal(account1, result1);
            Assert.Equal(account2, result2);
            Assert.Null(result3);
        }
        
        [Fact]
        public void BankAccountFacade_UpdateName_ShouldChangeAccountName()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            var account = facade.CreateAccount(1, "Old Name", 100);
            
            // Act
            var result = facade.UpdateName(1, "New Name");
            
            // Assert
            Assert.True(result);
            Assert.Equal("New Name", account.Name);
        }
        
        [Fact]
        public void BankAccountFacade_UpdateName_ShouldReturnFalseForNonExistentAccount()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            // Act
            var result = facade.UpdateName(999, "New Name");
            
            // Assert
            Assert.False(result);
        }
        
        [Fact]
        public void BankAccountFacade_GetTotalBalance_ShouldCalculateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            
            facade.CreateAccount("Account 1", 100);
            facade.CreateAccount("Account 2", 200);
            facade.CreateAccount("Account 3", 300);
            
            // Act
            var totalBalance = facade.GetTotalBalance();
            
            // Assert
            Assert.Equal(600, totalBalance);
        }
    }
}
