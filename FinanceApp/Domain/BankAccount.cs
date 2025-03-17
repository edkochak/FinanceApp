using System;

namespace FinanceApp.Domain
{
    public enum AccountType
    {
        Checking,
        Savings,
        Credit
    }

    public partial class BankAccount
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public decimal Balance { get; private set; }
        public AccountType Type { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public BankAccount(int id, string name, decimal balance, AccountType type = AccountType.Checking)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя счета не может быть пустым", nameof(name));

            Id = id;
            Name = name;
            Balance = balance;
            Type = type;
            CreatedAt = DateTime.Now;
        }

        public void UpdateBalance(decimal amount)
        {
            Balance += amount;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя счета не может быть пустым", nameof(name));
            
            Name = name;
        }

        public void Accept(Services.Export.IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
