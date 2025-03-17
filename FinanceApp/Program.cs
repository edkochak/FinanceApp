using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FinanceApp.Domain;
using FinanceApp.Services.Command;
using FinanceApp.Services.Export;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;
using FinanceApp.Services.Import;
using FinanceApp.Services.Proxy;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;
        private static BankAccountFacade _accountFacade;
        private static CategoryFacade _categoryFacade;
        private static OperationFacade _operationFacade;
        private static AnalyticsFacade _analyticsFacade;
        private static IBankAccountProxy _accountProxy;

        public static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            ConfigureServices();
            InitializeDemo();
            
            bool exit = false;
            while (!exit)
            {
                try
                {
                    DisplayMenu();
                    var choice = Console.ReadLine();
                    exit = ProcessMenuChoice(choice);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                }
            }
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();
            
            // Регистрация сервисов в DI-контейнере
            services.AddSingleton<FinancialObjectFactory>();
            services.AddSingleton<BankAccountFacade>();
            services.AddSingleton<CategoryFacade>();
            services.AddSingleton<IBankAccountProxy, BankAccountProxy>();
            services.AddSingleton<OperationFacade>();
            services.AddSingleton<AnalyticsFacade>();
            
            _serviceProvider = services.BuildServiceProvider();
            
            // Получение экземпляров из DI-контейнера
            _accountFacade = _serviceProvider.GetRequiredService<BankAccountFacade>();
            _categoryFacade = _serviceProvider.GetRequiredService<CategoryFacade>();
            _operationFacade = _serviceProvider.GetRequiredService<OperationFacade>();
            _analyticsFacade = _serviceProvider.GetRequiredService<AnalyticsFacade>();
            _accountProxy = _serviceProvider.GetRequiredService<IBankAccountProxy>();
        }

        private static void InitializeDemo()
        {
            // Создаем начальные данные для демонстрации
            var account1 = _accountFacade.CreateAccount("Основной счет", 5000);
            var account2 = _accountFacade.CreateAccount("Сберегательный", 10000, AccountType.Savings);
            
            // Сохраняем в прокси для примера
            _accountProxy.Save(account1);
            _accountProxy.Save(account2);
            
            var catIncome1 = _categoryFacade.CreateCategory(CategoryType.Income, "Зарплата");
            var catIncome2 = _categoryFacade.CreateCategory(CategoryType.Income, "Кэшбэк");
            var catExpense1 = _categoryFacade.CreateCategory(CategoryType.Expense, "Кафе");
            var catExpense2 = _categoryFacade.CreateCategory(CategoryType.Expense, "Здоровье");
            
            // Используем команды для создания операций (с декоратором для измерения времени)
            ICommand createOpCommand1 = new CreateOperationCommand(
                _operationFacade, 
                OperationType.Income, 
                account1.Id, 
                50000, 
                catIncome1.Id, 
                "Зарплата за февраль");
            
            ICommand timedCommand1 = new TimeMeasureDecorator(createOpCommand1, "Создание операции дохода");
            timedCommand1.Execute();
            
            _operationFacade.CreateOperation(OperationType.Income, account1.Id, 500, DateTime.Now.AddDays(-5), catIncome2.Id, "Кэшбэк с карты");
            _operationFacade.CreateOperation(OperationType.Expense, account1.Id, 1200, DateTime.Now.AddDays(-3), catExpense1.Id, "Обед с коллегами");
            _operationFacade.CreateOperation(OperationType.Expense, account1.Id, 3500, DateTime.Now.AddDays(-1), catExpense2.Id, "Лекарства");
            
            Console.WriteLine("Демонстрационные данные созданы успешно!");
            Console.WriteLine("Нажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void DisplayMenu()
        {
            Console.Clear();
            Console.WriteLine("=== ВШЭ-Банк: Учет Финансов ===");
            Console.WriteLine("1. Просмотр счетов");
            Console.WriteLine("2. Создать новый счет");
            Console.WriteLine("3. Просмотр категорий");
            Console.WriteLine("4. Создать новую категорию");
            Console.WriteLine("5. Просмотр операций");
            Console.WriteLine("6. Создать новую операцию");
            Console.WriteLine("7. Аналитика");
            Console.WriteLine("8. Экспорт данных");
            Console.WriteLine("9. Импорт данных (пример)");
            Console.WriteLine("10. Удалить счет");
            Console.WriteLine("11. Удалить категорию");
            Console.WriteLine("12. Удалить операцию");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите опцию: ");
        }

        private static bool ProcessMenuChoice(string choice)
        {
            switch (choice)
            {
                case "1":
                    ShowAccounts();
                    return false;
                case "2":
                    CreateAccount();
                    return false;
                case "3":
                    ShowCategories();
                    return false;
                case "4":
                    CreateCategory();
                    return false;
                case "5":
                    ShowOperations();
                    return false;
                case "6":
                    CreateOperation();
                    return false;
                case "7":
                    ShowAnalytics();
                    return false;
                case "8":
                    ExportData();
                    return false;
                case "9":
                    ImportDataDemo();
                    return false;
                case "10":
                    DeleteAccount();
                    return false;
                case "11":
                    DeleteCategory();
                    return false;
                case "12":
                    DeleteOperation();
                    return false;
                case "0":
                    return true;
                default:
                    Console.WriteLine("Неверный выбор. Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    return false;
            }
        }

        private static void ShowAccounts()
        {
            var accounts = _accountFacade.GetAllAccounts();
            
            Console.Clear();
            Console.WriteLine("=== Список счетов ===");
            foreach (var account in accounts)
            {
                Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance:C}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void CreateAccount()
        {
            Console.Clear();
            Console.WriteLine("=== Создание нового счета ===");
            
            Console.Write("Введите название счета: ");
            var name = Console.ReadLine();
            
            Console.Write("Введите начальный баланс: ");
            if (!decimal.TryParse(Console.ReadLine(), out var balance))
            {
                Console.WriteLine("Неверный формат баланса!");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Выберите тип счета:");
            Console.WriteLine("1. Текущий");
            Console.WriteLine("2. Сберегательный");
            Console.WriteLine("3. Кредитный");
            Console.Write("Ваш выбор: ");
            var typeChoice = Console.ReadLine();
            
            var accountType = typeChoice switch
            {
                "2" => AccountType.Savings,
                "3" => AccountType.Credit,
                _ => AccountType.Checking
            };
            
            var account = _accountFacade.CreateAccount(name, balance, accountType);
            
            // Сохраняем в прокси
            _accountProxy.Save(account);
            
            Console.WriteLine($"Счет '{name}' успешно создан с ID {account.Id}");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowCategories()
        {
            var categories = _categoryFacade.GetAllCategories();
            
            Console.Clear();
            Console.WriteLine("=== Список категорий ===");
            foreach (var category in categories)
            {
                Console.WriteLine($"ID: {category.Id}, Тип: {category.Type}, Название: {category.Name}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void CreateCategory()
        {
            Console.Clear();
            Console.WriteLine("=== Создание новой категории ===");
            
            Console.Write("Введите название категории: ");
            var name = Console.ReadLine();
            
            Console.WriteLine("Выберите тип категории:");
            Console.WriteLine("1. Доход");
            Console.WriteLine("2. Расход");
            Console.Write("Ваш выбор: ");
            var typeChoice = Console.ReadLine();
            
            var categoryType = typeChoice == "1" ? CategoryType.Income : CategoryType.Expense;
            
            var category = _categoryFacade.CreateCategory(categoryType, name);
            Console.WriteLine($"Категория '{name}' успешно создана с ID {category.Id}");
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowOperations()
        {
            var operations = _operationFacade.GetAllOperations();
            
            Console.Clear();
            Console.WriteLine("=== Список операций ===");
            foreach (var operation in operations)
            {
                string type = operation.Type == OperationType.Income ? "Доход" : "Расход";
                Console.WriteLine($"ID: {operation.Id}, Тип: {type}, Сумма: {operation.Amount:C}, Дата: {operation.Date.ToShortDateString()}");
                Console.WriteLine($"  Счет ID: {operation.BankAccountId}, Категория ID: {operation.CategoryId}");
                if (!string.IsNullOrEmpty(operation.Description))
                    Console.WriteLine($"  Описание: {operation.Description}");
                Console.WriteLine();
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void CreateOperation()
        {
            Console.Clear();
            Console.WriteLine("=== Создание новой операции ===");
            
            // Отображаем доступные счета
            var accounts = _accountFacade.GetAllAccounts();
            Console.WriteLine("Доступные счета:");
            foreach (var account in accounts)
            {
                Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance:C}");
            }
            
            Console.Write("\nВыберите ID счета: ");
            if (!int.TryParse(Console.ReadLine(), out var accountId))
            {
                Console.WriteLine("Неверный ID счета!");
                Console.ReadKey();
                return;
            }
            
            // Проверяем существование счета
            var selectedAccount = _accountFacade.GetAccount(accountId);
            if (selectedAccount == null)
            {
                Console.WriteLine("Счет не найден!");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("\nВыберите тип операции:");
            Console.WriteLine("1. Доход");
            Console.WriteLine("2. Расход");
            Console.Write("Ваш выбор: ");
            var typeChoice = Console.ReadLine();
            
            var operationType = typeChoice == "1" ? OperationType.Income : OperationType.Expense;
            
            // Отображаем подходящие категории
            var categoryType = operationType == OperationType.Income ? CategoryType.Income : CategoryType.Expense;
            var categories = _categoryFacade.GetCategoriesByType(categoryType);
            
            Console.WriteLine($"\nДоступные категории типа '{categoryType}':");
            foreach (var category in categories)
            {
                Console.WriteLine($"ID: {category.Id}, Название: {category.Name}");
            }
            
            Console.Write("\nВыберите ID категории: ");
            if (!int.TryParse(Console.ReadLine(), out var categoryId))
            {
                Console.WriteLine("Неверный ID категории!");
                Console.ReadKey();
                return;
            }
            
            // Проверяем существование категории
            var selectedCategory = _categoryFacade.GetCategory(categoryId);
            if (selectedCategory == null)
            {
                Console.WriteLine("Категория не найдена!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("\nВведите сумму операции: ");
            if (!decimal.TryParse(Console.ReadLine(), out var amount))
            {
                Console.WriteLine("Неверный формат суммы!");
                Console.ReadKey();
                return;
            }
            
            Console.Write("\nВведите описание (необязательно): ");
            var description = Console.ReadLine();
            
            // Используем команду и декоратор измерения времени
            ICommand command = new CreateOperationCommand(
                _operationFacade,
                operationType,
                accountId,
                amount,
                categoryId,
                description
            );
            
            ICommand timedCommand = new TimeMeasureDecorator(command, "Создание операции");
            timedCommand.Execute();
            
            Console.WriteLine("\nОперация успешно создана!");
            Console.WriteLine($"Новый баланс счета '{selectedAccount.Name}': {_accountFacade.GetAccount(accountId).Balance:C}");
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ShowAnalytics()
        {
            Console.Clear();
            Console.WriteLine("=== Аналитика ===");
            
            Console.WriteLine("Выберите период:");
            Console.WriteLine("1. Текущий месяц");
            Console.WriteLine("2. Предыдущий месяц");
            Console.WriteLine("3. Произвольный период");
            Console.Write("Ваш выбор: ");
            
            var choice = Console.ReadLine();
            
            DateTime startDate;
            DateTime endDate = DateTime.Now;
            
            switch (choice)
            {
                case "1":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    break;
                case "2":
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays(-1);
                    startDate = new DateTime(endDate.Year, endDate.Month, 1);
                    break;
                case "3":
                    Console.Write("Введите начальную дату (формат: dd.MM.yyyy): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out startDate))
                    {
                        Console.WriteLine("Неверный формат даты!");
                        Console.ReadKey();
                        return;
                    }
                    
                    Console.Write("Введите конечную дату (формат: dd.MM.yyyy): ");
                    if (!DateTime.TryParse(Console.ReadLine(), out endDate))
                    {
                        Console.WriteLine("Неверный формат даты!");
                        Console.ReadKey();
                        return;
                    }
                    break;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Console.ReadKey();
                    return;
            }
            
            Console.Clear();
            Console.WriteLine($"=== Аналитика за период: {startDate.ToShortDateString()} - {endDate.ToShortDateString()} ===\n");
            
            // Разница доходов и расходов
            decimal difference = _analyticsFacade.CalculateIncomeExpenseDifference(startDate, endDate);
            decimal totalIncome = _operationFacade.GetIncomeTotal(startDate, endDate);
            decimal totalExpense = _operationFacade.GetExpenseTotal(startDate, endDate);
            
            Console.WriteLine("--- Общая статистика ---");
            Console.WriteLine($"Всего доходов: {totalIncome:C}");
            Console.WriteLine($"Всего расходов: {totalExpense:C}");
            Console.WriteLine($"Разница (доходы - расходы): {difference:C}");
            Console.WriteLine();
            
            // Группировка доходов по категориям
            var incomeByCategory = _analyticsFacade.GetIncomeByCategory(startDate, endDate);
            Console.WriteLine("--- Доходы по категориям ---");
            foreach (var item in incomeByCategory)
            {
                Console.WriteLine($"{item.Key}: {item.Value:C} ({item.Value / totalIncome:P2})");
            }
            Console.WriteLine();
            
            // Группировка расходов по категориям
            var expenseByCategory = _analyticsFacade.GetExpenseByCategory(startDate, endDate);
            Console.WriteLine("--- Расходы по категориям ---");
            foreach (var item in expenseByCategory)
            {
                Console.WriteLine($"{item.Key}: {item.Value:C} ({item.Value / totalExpense:P2})");
            }
            Console.WriteLine();
            
            // Средний размер операции
            decimal avgIncome = _analyticsFacade.GetAverageOperationAmount(OperationType.Income, startDate, endDate);
            decimal avgExpense = _analyticsFacade.GetAverageOperationAmount(OperationType.Expense, startDate, endDate);
            
            Console.WriteLine("--- Дополнительная статистика ---");
            Console.WriteLine($"Средний размер дохода: {avgIncome:C}");
            Console.WriteLine($"Средний размер расхода: {avgExpense:C}");
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ExportData()
        {
            Console.Clear();
            Console.WriteLine("=== Экспорт данных ===");
            
            Console.WriteLine("Выберите формат экспорта:");
            Console.WriteLine("1. CSV");
            Console.WriteLine("2. JSON");
            Console.Write("Ваш выбор: ");
            
            var choice = Console.ReadLine();
            var format = choice == "2" ? "JSON" : "CSV";
            
            // Создаем соответствующий посетитель
            IVisitor visitor;
            string fileName;
            
            if (format == "JSON")
            {
                visitor = new JsonExportVisitor();
                fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            }
            else
            {
                visitor = new CsvExportVisitor();
                fileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            }
            
            // Посещаем все объекты
            foreach (var account in _accountFacade.GetAllAccounts())
            {
                account.Accept(visitor);
            }
            
            foreach (var category in _categoryFacade.GetAllCategories())
            {
                category.Accept(visitor);
            }
            
            foreach (var operation in _operationFacade.GetAllOperations())
            {
                operation.Accept(visitor);
            }
            
            // Получаем результат
            string result;
            
            if (format == "JSON")
            {
                result = ((JsonExportVisitor)visitor).GetJsonResult();
            }
            else
            {
                result = ((CsvExportVisitor)visitor).GetCsvResult();
            }
            
            // Сохраняем в файл
            try
            {
                File.WriteAllText(fileName, result);
                Console.WriteLine($"Данные успешно экспортированы в файл: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении файла: {ex.Message}");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void ImportDataDemo()
        {
            Console.Clear();
            Console.WriteLine("=== Импорт данных (демонстрация шаблонного метода) ===");
            
            // Создаём временный файл для демонстрации
            string tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllLines(tempFile, new[] {
                    "BankAccount;10;Демо импорта;1000",
                    "Category;20;Income;Импортированная категория"
                });
                
                Console.WriteLine($"Создан временный файл для демонстрации: {tempFile}");
                Console.WriteLine("Содержимое файла:");
                Console.WriteLine(File.ReadAllText(tempFile));
                
                // Использование шаблонного метода для импорта
                var importer = new CsvImport();
                Console.WriteLine("\nВыполняется импорт данных...");
                importer.ImportFile(tempFile);
                
                Console.WriteLine("\nИмпорт завершен. В рамках демонстрации данные не были добавлены в приложение.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при импорте: {ex.Message}");
            }
            finally
            {
                // Удаляем временный файл
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    Console.WriteLine($"Временный файл удален: {tempFile}");
                }
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void DeleteAccount()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление счета ===");
            
            var accounts = _accountFacade.GetAllAccounts();
            if (!accounts.Any())
            {
                Console.WriteLine("Нет доступных счетов для удаления.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Доступные счета:");
            foreach (var account in accounts)
            {
                Console.WriteLine($"ID: {account.Id}, Название: {account.Name}, Баланс: {account.Balance:C}");
            }
            
            Console.Write("\nВведите ID счета для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var accountId))
            {
                Console.WriteLine("Неверный формат ID!");
                Console.ReadKey();
                return;
            }
            
            // Проверяем наличие операций на этом счете
            var accountOperations = _operationFacade.GetOperationsByAccount(accountId);
            if (accountOperations.Any())
            {
                Console.WriteLine($"Невозможно удалить счет, так как с ним связано {accountOperations.Count} операций.");
                Console.WriteLine("Сначала удалите все операции, связанные с этим счетом.");
                Console.ReadKey();
                return;
            }
            
            bool result = _accountFacade.DeleteAccount(accountId);
            if (result)
            {
                Console.WriteLine($"Счет с ID {accountId} успешно удален.");
                
                // Удаляем из прокси
                var proxyAccount = _accountProxy.GetById(accountId);
                if (proxyAccount != null)
                {
                    // Добавим метод удаления из прокси
                    _accountProxy.Remove(accountId);
                }
            }
            else
            {
                Console.WriteLine($"Счет с ID {accountId} не найден.");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void DeleteCategory()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление категории ===");
            
            var categories = _categoryFacade.GetAllCategories();
            if (!categories.Any())
            {
                Console.WriteLine("Нет доступных категорий для удаления.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Доступные категории:");
            foreach (var category in categories)
            {
                Console.WriteLine($"ID: {category.Id}, Тип: {category.Type}, Название: {category.Name}");
            }
            
            Console.Write("\nВведите ID категории для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var categoryId))
            {
                Console.WriteLine("Неверный формат ID!");
                Console.ReadKey();
                return;
            }
            
            // Проверяем наличие операций с этой категорией
            var categoryOperations = _operationFacade.GetOperationsByCategory(categoryId);
            if (categoryOperations.Any())
            {
                Console.WriteLine($"Невозможно удалить категорию, так как с ней связано {categoryOperations.Count} операций.");
                Console.WriteLine("Сначала удалите все операции, связанные с этой категорией.");
                Console.ReadKey();
                return;
            }
            
            bool result = _categoryFacade.DeleteCategory(categoryId);
            if (result)
            {
                Console.WriteLine($"Категория с ID {categoryId} успешно удалена.");
            }
            else
            {
                Console.WriteLine($"Категория с ID {categoryId} не найдена.");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        private static void DeleteOperation()
        {
            Console.Clear();
            Console.WriteLine("=== Удаление операции ===");
            
            var operations = _operationFacade.GetAllOperations();
            if (!operations.Any())
            {
                Console.WriteLine("Нет доступных операций для удаления.");
                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Доступные операции:");
            foreach (var operation in operations)
            {
                string type = operation.Type == OperationType.Income ? "Доход" : "Расход";
                Console.WriteLine($"ID: {operation.Id}, Тип: {type}, Сумма: {operation.Amount:C}, Дата: {operation.Date.ToShortDateString()}");
                Console.WriteLine($"  Счет ID: {operation.BankAccountId}, Категория ID: {operation.CategoryId}");
                if (!string.IsNullOrEmpty(operation.Description))
                    Console.WriteLine($"  Описание: {operation.Description}");
                Console.WriteLine();
            }
            
            Console.Write("\nВведите ID операции для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var operationId))
            {
                Console.WriteLine("Неверный формат ID!");
                Console.ReadKey();
                return;
            }
            
            bool result = _operationFacade.DeleteOperation(operationId);
            if (result)
            {
                Console.WriteLine($"Операция с ID {operationId} успешно удалена.");
            }
            else
            {
                Console.WriteLine($"Операция с ID {operationId} не найдена.");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}
