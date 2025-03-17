using Xunit;
using FinanceApp.Services.Implementations;
using FinanceApp.Domain;
using System;

namespace FinanceApp.Tests
{
    public class FactoryTests
    {
        [Fact]
        public void CreateBankAccount_ShouldIncrementIdAutomatically()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act
            var account1 = factory.CreateBankAccount("Account 1", 100);
            var account2 = factory.CreateBankAccount("Account 2", 200);
            
            // Assert
            Assert.Equal(1, account1.Id);
            Assert.Equal(2, account2.Id);
        }
        
        [Fact]
        public void CreateBankAccount_ShouldRejectNegativeInitialBalance()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => factory.CreateBankAccount("Test", -100));
            Assert.Contains("баланс", ex.Message.ToLower());
        }
        
        [Fact]
        public void CreateCategory_ShouldIncrementIdAutomatically()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act
            var category1 = factory.CreateCategory(CategoryType.Income, "Category 1");
            var category2 = factory.CreateCategory(CategoryType.Expense, "Category 2");
            
            // Assert
            Assert.Equal(1, category1.Id);
            Assert.Equal(2, category2.Id);
        }
        
        [Fact]
        public void CreateOperation_ShouldIncrementIdAutomatically()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            
            // Act
            var operation1 = factory.CreateOperation(OperationType.Income, 1, 100, DateTime.Now, 1);
            var operation2 = factory.CreateOperation(OperationType.Expense, 1, 200, DateTime.Now, 2);
            
            // Assert
            Assert.Equal(1, operation1.Id);
            Assert.Equal(2, operation2.Id);
        }
    }
}
