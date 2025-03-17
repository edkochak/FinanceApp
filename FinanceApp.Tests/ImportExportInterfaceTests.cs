using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Export;
using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace FinanceApp.Tests
{
    public class ImportExportInterfaceTests
    {
        [Fact]
        public void IVisitor_CsvImplementation_ShouldVisitAllEntityTypes()
        {
            // Arrange
            IVisitor visitor = new CsvExportVisitor();
            var account = new BankAccount(1, "Test Account", 1000);
            var category = new Category(2, CategoryType.Income, "Test Category");
            var operation = new Operation(3, OperationType.Income, 1, 500, DateTime.Now, 2);
            
            // Act
            account.Accept(visitor);
            category.Accept(visitor);
            operation.Accept(visitor);
            
            // Assert
            var result = ((CsvExportVisitor)visitor).GetCsvResult();
            
            // Проверяем, что каждый тип сущности был корректно обработан
            Assert.Contains("BankAccount;1;Test Account;1000", result);
            Assert.Contains("Category;2;Income;Test Category", result);
            Assert.Contains("Operation;3;Income;1;500;2", result);
        }
        
        [Fact]
        public void JsonExportVisitor_ShouldExportAllProperties()
        {
            // Arrange
            var visitor = new JsonExportVisitor();
            var account = new BankAccount(1, "Test Account", 1000, AccountType.Credit);
            var category = new Category(2, CategoryType.Expense, "Bills");
            var operation = new Operation(3, OperationType.Expense, 1, 300, new DateTime(2023, 3, 15), 2, "Monthly payment");
            
            // Act
            account.Accept(visitor);
            category.Accept(visitor);
            operation.Accept(visitor);
            var json = visitor.GetJsonResult();
            
            // Assert - Десериализуем и проверяем свойства
            var jsonDoc = JsonDocument.Parse(json);
            var elements = jsonDoc.RootElement.EnumerateArray().ToArray();
            
            Assert.Equal(3, elements.Length);
            
            var accountElement = elements[0];
            var categoryElement = elements[1];
            var operationElement = elements[2];
            
            // Проверка свойств account
            Assert.Equal("BankAccount", accountElement.GetProperty("Type").GetString());
            Assert.Equal(1, accountElement.GetProperty("Id").GetInt32());
            Assert.Equal("Test Account", accountElement.GetProperty("Name").GetString());
            Assert.Equal(1000, accountElement.GetProperty("Balance").GetDecimal());
            
            // Проверка свойств category
            Assert.Equal("Category", categoryElement.GetProperty("Type").GetString());
            Assert.Equal(2, categoryElement.GetProperty("Id").GetInt32());
            Assert.Equal("Expense", categoryElement.GetProperty("CategoryType").GetString());
            Assert.Equal("Bills", categoryElement.GetProperty("Name").GetString());
            
            // Проверка свойств operation
            Assert.Equal("Operation", operationElement.GetProperty("Type").GetString());
            Assert.Equal(3, operationElement.GetProperty("Id").GetInt32());
            Assert.Equal("Expense", operationElement.GetProperty("OperationType").GetString());
            Assert.Equal(1, operationElement.GetProperty("BankAccountId").GetInt32());
            Assert.Equal(300, operationElement.GetProperty("Amount").GetDecimal());
            Assert.Equal(2, operationElement.GetProperty("CategoryId").GetInt32());
            Assert.Equal("Monthly payment", operationElement.GetProperty("Description").GetString());
        }
        
        [Fact]
        public void IVisitable_Implementation_ShouldAcceptVisitors()
        {
            // Arrange
            var account = new BankAccount(1, "Test Account", 1000);
            var mockVisitor = new MockVisitor();
            
            // Act
            account.Accept(mockVisitor);
            
            // Assert
            Assert.True(mockVisitor.AccountVisited);
            Assert.False(mockVisitor.CategoryVisited);
            Assert.False(mockVisitor.OperationVisited);
            Assert.Equal(account, mockVisitor.LastAccount);
        }
        
        // Mock класс для тестирования интерфейса IVisitor
        private class MockVisitor : IVisitor
        {
            public bool AccountVisited { get; private set; }
            public bool CategoryVisited { get; private set; }
            public bool OperationVisited { get; private set; }
            public BankAccount? LastAccount { get; private set; }
            public Category? LastCategory { get; private set; }
            public Operation? LastOperation { get; private set; }
            
            public void Visit(BankAccount account)
            {
                AccountVisited = true;
                LastAccount = account;
            }
            
            public void Visit(Category category)
            {
                CategoryVisited = true;
                LastCategory = category;
            }
            
            public void Visit(Operation operation)
            {
                OperationVisited = true;
                LastOperation = operation;
            }
        }
    }
}
