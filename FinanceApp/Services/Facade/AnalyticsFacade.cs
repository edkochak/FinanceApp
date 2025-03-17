using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Domain;

namespace FinanceApp.Services.Facade
{
    public class AnalyticsFacade
    {
        private readonly OperationFacade _operationFacade;
        private readonly CategoryFacade _categoryFacade;

        public AnalyticsFacade(OperationFacade operationFacade, CategoryFacade categoryFacade)
        {
            _operationFacade = operationFacade ?? throw new ArgumentNullException(nameof(operationFacade));
            _categoryFacade = categoryFacade ?? throw new ArgumentNullException(nameof(categoryFacade));
        }

        // Подсчет разницы доходов и расходов за период
        public decimal CalculateIncomeExpenseDifference(DateTime start, DateTime end)
        {
            var totalIncome = _operationFacade.GetIncomeTotal(start, end);
            var totalExpense = _operationFacade.GetExpenseTotal(start, end);
            return totalIncome - totalExpense;
        }

        // Группировка доходов по категориям
        public Dictionary<string, decimal> GetIncomeByCategory(DateTime? start = null, DateTime? end = null)
        {
            var categoryAmounts = _operationFacade.GetOperationsGroupedByCategory(OperationType.Income, start, end);
            var result = new Dictionary<string, decimal>();
            
            foreach (var categoryAmount in categoryAmounts)
            {
                var category = _categoryFacade.GetCategory(categoryAmount.Key);
                if (category != null)
                    result[category.Name] = categoryAmount.Value;
            }
            
            return result;
        }

        // Группировка расходов по категориям
        public Dictionary<string, decimal> GetExpenseByCategory(DateTime? start = null, DateTime? end = null)
        {
            var categoryAmounts = _operationFacade.GetOperationsGroupedByCategory(OperationType.Expense, start, end);
            var result = new Dictionary<string, decimal>();
            
            foreach (var categoryAmount in categoryAmounts)
            {
                var category = _categoryFacade.GetCategory(categoryAmount.Key);
                if (category != null)
                    result[category.Name] = categoryAmount.Value;
            }
            
            return result;
        }

        // Дополнительная аналитика: средний размер операции по типу
        public decimal GetAverageOperationAmount(OperationType type, DateTime? start = null, DateTime? end = null)
        {
            var operations = _operationFacade.GetAllOperations();
            var query = operations.Where(o => o.Type == type);
            
            if (start.HasValue)
                query = query.Where(o => o.Date >= start.Value);
            
            if (end.HasValue)
                query = query.Where(o => o.Date <= end.Value);
            
            var filteredOperations = query.ToList();
            return filteredOperations.Any() ? filteredOperations.Average(o => o.Amount) : 0;
        }
    }
}
