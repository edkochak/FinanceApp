using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Domain;
using FinanceApp.Services.Implementations;

namespace FinanceApp.Services.Facade
{
    public class OperationFacade
    {
        private readonly Dictionary<int, Operation> _operations = new Dictionary<int, Operation>();
        private readonly FinancialObjectFactory _factory;
        private readonly BankAccountFacade _accountFacade;

        public OperationFacade(FinancialObjectFactory factory, BankAccountFacade accountFacade)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _accountFacade = accountFacade ?? throw new ArgumentNullException(nameof(accountFacade));
        }

        public Operation CreateOperation(OperationType type, int bankAccountId, decimal amount, DateTime date, int categoryId, string description = "")
        {
            // Проверяем существование счета
            var account = _accountFacade.GetAccount(bankAccountId);
            if (account == null)
                throw new ArgumentException($"Счет с ID {bankAccountId} не существует", nameof(bankAccountId));

            var operation = _factory.CreateOperation(type, bankAccountId, amount, date, categoryId, description);
            _operations[operation.Id] = operation;

            // Обновляем баланс счета
            decimal balanceChange = type == OperationType.Income ? amount : -amount;
            _accountFacade.UpdateBalance(bankAccountId, balanceChange);

            return operation;
        }

        // Для тестов
        public Operation CreateOperation(int id, OperationType type, int bankAccountId, decimal amount, DateTime date, int categoryId, string description = "")
        {
            var operation = _factory.CreateOperation(id, type, bankAccountId, amount, date, categoryId, description);
            _operations[operation.Id] = operation;
            return operation;
        }

        public Operation? GetOperation(int id)
        {
            return _operations.TryGetValue(id, out var operation) ? operation : null;
        }

        public List<Operation> GetAllOperations()
        {
            return _operations.Values.ToList();
        }

        public List<Operation> GetOperationsByAccount(int accountId)
        {
            return _operations.Values.Where(o => o.BankAccountId == accountId).ToList();
        }

        public List<Operation> GetOperationsByCategory(int categoryId)
        {
            return _operations.Values.Where(o => o.CategoryId == categoryId).ToList();
        }

        public List<Operation> GetOperationsByDateRange(DateTime start, DateTime end)
        {
            return _operations.Values.Where(o => o.Date >= start && o.Date <= end).ToList();
        }

        public bool DeleteOperation(int id)
        {
            if (!_operations.TryGetValue(id, out var operation))
                return false;

            // Возвращаем баланс счета к предыдущему состоянию
            decimal balanceChange = operation.Type == OperationType.Income ? -operation.Amount : operation.Amount;
            _accountFacade.UpdateBalance(operation.BankAccountId, balanceChange);

            return _operations.Remove(id);
        }

        public decimal GetIncomeTotal(DateTime? start = null, DateTime? end = null)
        {
            var query = _operations.Values
                .Where(o => o.Type == OperationType.Income);

            if (start.HasValue)
                query = query.Where(o => o.Date >= start.Value);

            if (end.HasValue)
                query = query.Where(o => o.Date <= end.Value);

            return query.Sum(o => o.Amount);
        }

        public decimal GetExpenseTotal(DateTime? start = null, DateTime? end = null)
        {
            var query = _operations.Values
                .Where(o => o.Type == OperationType.Expense);

            if (start.HasValue)
                query = query.Where(o => o.Date >= start.Value);

            if (end.HasValue)
                query = query.Where(o => o.Date <= end.Value);

            return query.Sum(o => o.Amount);
        }

        public Dictionary<int, decimal> GetOperationsGroupedByCategory(OperationType type, DateTime? start = null, DateTime? end = null)
        {
            var query = _operations.Values
                .Where(o => o.Type == type);

            if (start.HasValue)
                query = query.Where(o => o.Date >= start.Value);

            if (end.HasValue)
                query = query.Where(o => o.Date <= end.Value);

            return query
                .GroupBy(o => o.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));
        }
    }
}
