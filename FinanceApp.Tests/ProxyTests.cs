using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Proxy;
using System;

namespace FinanceApp.Tests
{
    public class ProxyTests
    {
        [Fact]
        public void BankAccountProxy_ShouldCacheAccounts()
        {
            // Arrange
            var proxy = new BankAccountProxy();
            var account = new BankAccount(1, "Test Account", 1000);
            
            // Act
            proxy.Save(account);
            var retrievedAccount = proxy.GetById(1);
            
            // Assert
            Assert.NotNull(retrievedAccount);
            Assert.Equal(1, retrievedAccount.Id);
            Assert.Equal("Test Account", retrievedAccount.Name);
            Assert.Equal(1000, retrievedAccount.Balance);
        }
        
        [Fact]
        public void BankAccountProxy_ShouldReturnNullForNonExistentAccount()
        {
            // Arrange
            var proxy = new BankAccountProxy();
            
            // Act
            var retrievedAccount = proxy.GetById(999);
            
            // Assert
            Assert.Null(retrievedAccount);
        }

        [Fact]
        public void BankAccountProxy_ShouldRemoveAccount()
        {
            // Arrange
            var proxy = new BankAccountProxy();
            var account = new BankAccount(1, "Test Account", 1000);
            proxy.Save(account);
            
            // Act
            var result = proxy.Remove(1);
            var retrievedAccount = proxy.GetById(1);
            
            // Assert
            Assert.True(result);
            Assert.Null(retrievedAccount);
        }
    }
}
