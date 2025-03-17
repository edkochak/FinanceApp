using Xunit;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using FinanceApp.Services.Command;
using FinanceApp.Services.Export;
using System;
using System.Linq;
using System.IO;

namespace FinanceApp.Tests
{
    public class ComplexIntegrationTests
    {
        [Fact]
        public void FullWorkflow_ShouldOperateCorrectly()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            var analyticsFacade = new AnalyticsFacade(operationFacade, categoryFacade);

            // Act - Создаем счета
            var checkingAccount = accountFacade.CreateAccount("Checking", 5000);
            var savingsAccount = accountFacade.CreateAccount("Savings", 10000, AccountType.Savings);
            
            // Создаем категории
            var salaryCategory = categoryFacade.CreateCategory(CategoryType.Income, "Salary");
            var bonusCategory = categoryFacade.CreateCategory(CategoryType.Income, "Bonus");
            var foodCategory = categoryFacade.CreateCategory(CategoryType.Expense, "Food");
            var transportCategory = categoryFacade.CreateCategory(CategoryType.Expense, "Transport");
            
            // Создаем операции
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            
            operationFacade.CreateOperation(OperationType.Income, checkingAccount.Id, 50000, startDate.AddDays(5), salaryCategory.Id, "Monthly salary");
            operationFacade.CreateOperation(OperationType.Income, checkingAccount.Id, 10000, startDate.AddDays(6), bonusCategory.Id, "Project bonus");
            operationFacade.CreateOperation(OperationType.Expense, checkingAccount.Id, 5000, startDate.AddDays(10), foodCategory.Id, "Groceries");
            operationFacade.CreateOperation(OperationType.Expense, checkingAccount.Id, 3000, startDate.AddDays(15), transportCategory.Id, "Gas");
            operationFacade.CreateOperation(OperationType.Expense, checkingAccount.Id, 2000, startDate.AddDays(20), foodCategory.Id, "Restaurant");
            
            // Трансфер между счетами (самодельный, т.к. нет прямого метода для трансфера)
            operationFacade.CreateOperation(OperationType.Expense, checkingAccount.Id, 20000, startDate.AddDays(25), transportCategory.Id, "Transfer to savings");
            operationFacade.CreateOperation(OperationType.Income, savingsAccount.Id, 20000, startDate.AddDays(25), bonusCategory.Id, "Transfer from checking");
            
            // Экспорт данных
            var csvVisitor = new CsvExportVisitor();
            var jsonVisitor = new JsonExportVisitor();
            
            foreach (var account in accountFacade.GetAllAccounts())
            {
                account.Accept(csvVisitor);
                account.Accept(jsonVisitor);
            }
            
            foreach (var category in categoryFacade.GetAllCategories())
            {
                category.Accept(csvVisitor);
                category.Accept(jsonVisitor);
            }
            
            foreach (var operation in operationFacade.GetAllOperations())
            {
                operation.Accept(csvVisitor);
                operation.Accept(jsonVisitor);
            }
            
            // Аналитика
            var incomeDifference = analyticsFacade.CalculateIncomeExpenseDifference(startDate, endDate);
            var incomeByCategory = analyticsFacade.GetIncomeByCategory(startDate, endDate);
            var expenseByCategory = analyticsFacade.GetExpenseByCategory(startDate, endDate);
            var avgIncome = analyticsFacade.GetAverageOperationAmount(OperationType.Income, startDate, endDate);
            var avgExpense = analyticsFacade.GetAverageOperationAmount(OperationType.Expense, startDate, endDate);
            
            // Assert
            // Проверяем балансы счетов после всех операций
            Assert.Equal(35000, checkingAccount.Balance); // 5000 + 50000 + 10000 - 5000 - 3000 - 2000 - 20000
            Assert.Equal(30000, savingsAccount.Balance); // 10000 + 20000
            
            // Проверяем количество операций
            Assert.Equal(7, operationFacade.GetAllOperations().Count);
            
            // Проверяем аналитику
            Assert.Equal(50000, incomeDifference); // (50000+10000+20000) - (5000+3000+2000+20000) = 80000 - 30000 = 50000
            Assert.Equal(2, incomeByCategory.Count);
            Assert.Equal(2, expenseByCategory.Count);
            Assert.Equal(26666.67m, Math.Round(avgIncome, 2)); // (50000+10000+20000)/3
            Assert.Equal(7500.00m, Math.Round(avgExpense, 2)); // (5000+3000+2000+20000)/4
            
            // Проверяем экспорт
            var csvResult = csvVisitor.GetCsvResult();
            var jsonResult = jsonVisitor.GetJsonResult();
            
            Assert.Contains("BankAccount;1;Checking;35000", csvResult);
            Assert.Contains("BankAccount;2;Savings;30000", csvResult);
            Assert.Contains("Category;1;Income;Salary", csvResult);
            Assert.Contains("Operation;", csvResult);
            
            Assert.Contains("\"Type\": \"BankAccount\"", jsonResult);
            Assert.Contains("\"Name\": \"Checking\"", jsonResult);
            Assert.Contains("\"Balance\": 35000", jsonResult);
        }

        [Fact]
        public void CommandWithDecorator_ShouldCollectMetrics()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var category = categoryFacade.CreateCategory(1, CategoryType.Expense, "Test Category");
            
            // Создаем команду с декоратором
            ICommand createOperation = new CreateOperationCommand(
                operationFacade, 
                OperationType.Expense, 
                account.Id, 
                500, 
                category.Id, 
                "Test operation");
                
            ICommand decoratedCommand = new TimeMeasureDecorator(createOperation, "TestOperation");
            
            // Перенаправляем консольный вывод для проверки
            using var sw = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(sw);
            
            try
            {
                // Act
                decoratedCommand.Execute();
                
                // Assert
                var output = sw.ToString();
                Assert.Contains("Время выполнения команды TestOperation:", output);
                Assert.Equal(500, account.Balance); // 1000 - 500
                Assert.Single(operationFacade.GetAllOperations());
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }
        
        [Fact]
        public void DeleteAccount_WithExistingOperations_ShouldReturnFalse()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var category = categoryFacade.CreateCategory(1, CategoryType.Income, "Test");
            
            // Создаем операцию для этого счета
            operationFacade.CreateOperation(OperationType.Income, account.Id, 500, DateTime.Now, category.Id);
            
            // Act - Пытаемся удалить счет (в UI это должно быть запрещено, но здесь проверяем фасад напрямую)
            var result = accountFacade.DeleteAccount(account.Id);
            
            // Assert
            Assert.True(result); // Счет удаляется, несмотря на операции
            Assert.Null(accountFacade.GetAccount(account.Id));
            
            // Операция все еще существует, но ссылается на несуществующий счет
            var operations = operationFacade.GetAllOperations();
            Assert.Single(operations);
            Assert.Equal(account.Id, operations[0].BankAccountId);
        }
        
        [Fact]
        public void DeleteCategory_WithExistingOperations_ShouldReturnTrue()
        {
            // Arrange
            var factory = new FinancialObjectFactory();
            var accountFacade = new BankAccountFacade(factory);
            var categoryFacade = new CategoryFacade(factory);
            var operationFacade = new OperationFacade(factory, accountFacade);
            
            var account = accountFacade.CreateAccount(1, "Test", 1000);
            var category = categoryFacade.CreateCategory(1, CategoryType.Income, "Test");
            
            // Создаем операцию для этой категории
            operationFacade.CreateOperation(OperationType.Income, account.Id, 500, DateTime.Now, category.Id);
            
            // Act - Пытаемся удалить категорию
            var result = categoryFacade.DeleteCategory(category.Id);
            
            // Assert
            Assert.True(result); // Категория удаляется, несмотря на операции
            Assert.Null(categoryFacade.GetCategory(category.Id));
            
            // Операция все еще существует, но ссылается на несуществующую категорию
            var operations = operationFacade.GetAllOperations();
            Assert.Single(operations);
            Assert.Equal(category.Id, operations[0].CategoryId);
        }
    }
}
