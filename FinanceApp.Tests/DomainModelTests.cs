using Xunit;
using FinanceApp.Domain;
using System;

namespace FinanceApp.Tests
{
    public class DomainModelTests
    {
        [Fact]
        public void BankAccount_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var now = DateTime.Now;
            var account = new BankAccount(42, "Savings", 5000.50m, AccountType.Savings);
            
            // Assert
            Assert.Equal(42, account.Id);
            Assert.Equal("Savings", account.Name);
            Assert.Equal(5000.50m, account.Balance);
            Assert.Equal(AccountType.Savings, account.Type);
            Assert.True(account.CreatedAt >= now);
            Assert.True(account.CreatedAt <= DateTime.Now);
        }

        [Fact]
        public void Category_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange & Act
            var now = DateTime.Now;
            var category = new Category(101, CategoryType.Income, "Salary");
            
            // Assert
            Assert.Equal(101, category.Id);
            Assert.Equal(CategoryType.Income, category.Type);
            Assert.Equal("Salary", category.Name);
            Assert.True(category.CreatedAt >= now);
            Assert.True(category.CreatedAt <= DateTime.Now);
        }

        [Fact]
        public void Operation_Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var date = new DateTime(2023, 12, 31);
            
            // Act
            var operation = new Operation(
                201, 
                OperationType.Expense, 
                301, 
                123.45m, 
                date, 
                401, 
                "Christmas shopping");
            
            // Assert
            Assert.Equal(201, operation.Id);
            Assert.Equal(OperationType.Expense, operation.Type);
            Assert.Equal(301, operation.BankAccountId);
            Assert.Equal(123.45m, operation.Amount);
            Assert.Equal(date, operation.Date);
            Assert.Equal(401, operation.CategoryId);
            Assert.Equal("Christmas shopping", operation.Description);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void BankAccount_ConstructorWithInvalidName_ShouldThrow(string invalidName)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new BankAccount(1, (string?)invalidName, 100));
            Assert.Contains("Имя счета", ex.Message);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Category_ConstructorWithInvalidName_ShouldThrow(string invalidName)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Category(1, CategoryType.Income, (string?)invalidName));
            Assert.Contains("Имя категории", ex.Message);
        }
        
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Operation_ConstructorWithInvalidAmount_ShouldThrow(decimal invalidAmount)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => 
                new Operation(1, OperationType.Income, 1, invalidAmount, DateTime.Now, 1));
            Assert.Contains("Сумма операции", ex.Message);
        }
        
        [Fact]
        public void BankAccount_UpdateBalance_ShouldChangeBalance()
        {
            // Arrange
            var account = new BankAccount(1, "Test", 1000);
            
            // Act - Add money
            account.UpdateBalance(500);
            
            // Assert
            Assert.Equal(1500, account.Balance);
            
            // Act - Subtract money
            account.UpdateBalance(-700);
            
            // Assert
            Assert.Equal(800, account.Balance);
        }
        
        [Fact]
        public void Operation_Constructor_WithDefaultDescription_ShouldSetEmptyString()
        {
            // Act
            var operation = new Operation(1, OperationType.Income, 1, 100, DateTime.Now, 1);
            
            // Assert
            Assert.Equal("", operation.Description);
        }
    }
}
