using System;

namespace FinanceApp.Domain
{
    public enum OperationType
    {
        Income,
        Expense
    }

    public partial class Operation
    {
        public int Id { get; private set; }
        public OperationType Type { get; private set; }
        public int BankAccountId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Description { get; private set; }
        public int CategoryId { get; private set; }

        public Operation(int id, OperationType type, int bankAccountId, decimal amount, DateTime date, int categoryId, string description = "")
        {
            if (amount <= 0)
                throw new ArgumentException("Сумма операции должна быть положительной", nameof(amount));

            Id = id;
            Type = type;
            BankAccountId = bankAccountId;
            Amount = amount;
            Date = date;
            CategoryId = categoryId;
            Description = description;
        }

        public void UpdateDescription(string description)
        {
            Description = description ?? string.Empty;
        }

        public void UpdateAmount(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Сумма операции должна быть положительной", nameof(amount));
            
            Amount = amount;
        }

        public void Accept(Services.Export.IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
