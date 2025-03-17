using FinanceApp.Domain;
using System.Text;

namespace FinanceApp.Services.Export
{
    public class CsvExportVisitor : IVisitor
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public void Visit(BankAccount account)
        {
            // ...existing code...
            _sb.AppendLine($"BankAccount;{account.Id};{account.Name};{account.Balance}");
        }

        public void Visit(Category category)
        {
            // ...existing code...
            _sb.AppendLine($"Category;{category.Id};{category.Type};{category.Name}");
        }

        public void Visit(Operation operation)
        {
            // ...existing code...
            _sb.AppendLine($"Operation;{operation.Id};{operation.Type};{operation.BankAccountId};{operation.Amount};{operation.CategoryId}");
        }

        public string GetCsvResult() => _sb.ToString();
    }
}
