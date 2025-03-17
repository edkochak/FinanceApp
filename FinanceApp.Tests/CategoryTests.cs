using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using System;
using System.Linq;

namespace FinanceApp.Tests
{
    public class CategoryTests
    {
        [Fact]
        public void Category_UpdateName_ShouldChangeNameProperty()
        {
            // Arrange
            var category = new Category(1, CategoryType.Income, "Old Name");
            
            // Act
            category.UpdateName("New Name");
            
            // Assert
            Assert.Equal("New Name", category.Name);
        }

        [Fact]
        public void Category_UpdateName_ShouldThrowExceptionWhenNameIsEmpty()
        {
            // Arrange
            var category = new Category(1, CategoryType.Expense, "Test");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => category.UpdateName(""));
            Assert.Throws<ArgumentException>(() => category.UpdateName(null));
            Assert.Throws<ArgumentException>(() => category.UpdateName("   "));
        }
        
        [Fact]
        public void CategoryFacade_GetCategoriesByType_ShouldFilterCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new CategoryFacade(factory);
            
            facade.CreateCategory(CategoryType.Income, "Salary");
            facade.CreateCategory(CategoryType.Income, "Investment");
            facade.CreateCategory(CategoryType.Expense, "Food");
            facade.CreateCategory(CategoryType.Expense, "Transport");
            
            // Act
            var incomeCategories = facade.GetCategoriesByType(CategoryType.Income);
            var expenseCategories = facade.GetCategoriesByType(CategoryType.Expense);
            
            // Assert
            Assert.Equal(2, incomeCategories.Count);
            Assert.Equal(2, expenseCategories.Count);
            Assert.All(incomeCategories, cat => Assert.Equal(CategoryType.Income, cat.Type));
            Assert.All(expenseCategories, cat => Assert.Equal(CategoryType.Expense, cat.Type));
        }
        
        [Fact]
        public void CategoryFacade_UpdateName_ShouldChangeCategoryName()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new CategoryFacade(factory);
            
            var category = facade.CreateCategory(1, CategoryType.Income, "Old Name");
            
            // Act
            var result = facade.UpdateName(1, "New Name");
            
            // Assert
            Assert.True(result);
            Assert.Equal("New Name", category.Name);
        }
        
        [Fact]
        public void CategoryFacade_UpdateName_ShouldReturnFalseForNonExistentCategory()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var facade = new CategoryFacade(factory);
            
            // Act
            var result = facade.UpdateName(999, "New Name");
            
            // Assert
            Assert.False(result);
        }
    }
}
