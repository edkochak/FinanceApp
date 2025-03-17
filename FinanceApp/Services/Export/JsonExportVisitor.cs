using System.Text.Json;
using FinanceApp.Domain;

namespace FinanceApp.Services.Export
{
    public class JsonExportVisitor : IVisitor
    {
        private readonly List<object> _entities = new List<object>();

        public void Visit(BankAccount account)
        {
            _entities.Add(new 
            {
                Type = "BankAccount",
                account.Id,
                account.Name,
                account.Balance
            });
        }

        public void Visit(Category category)
        {
            _entities.Add(new
            {
                Type = "Category",
                category.Id,
                CategoryType = category.Type.ToString(),
                category.Name
            });
        }

        public void Visit(Operation operation)
        {
            _entities.Add(new
            {
                Type = "Operation",
                operation.Id,
                OperationType = operation.Type.ToString(),
                operation.BankAccountId,
                operation.CategoryId,
                operation.Amount,
                Date = operation.Date.ToString("yyyy-MM-dd HH:mm:ss"),
                operation.Description
            });
        }

        public string GetJsonResult()
        {
            return JsonSerializer.Serialize(_entities, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}