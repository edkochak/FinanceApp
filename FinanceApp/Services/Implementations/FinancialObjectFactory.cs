using System;
using FinanceApp.Domain;

namespace FinanceApp.Services.Implementations
{
    public class FinancialObjectFactory
    {
        private int _nextAccountId = 1;
        private int _nextCategoryId = 1;
        private int _nextOperationId = 1;

        public BankAccount CreateBankAccount(string name, decimal initialBalance, AccountType type = AccountType.Checking)
        {
            if (initialBalance < 0)
                throw new ArgumentException("Начальный баланс не может быть отрицательным", nameof(initialBalance));
            
            return new BankAccount(_nextAccountId++, name, initialBalance, type);
        }

        public BankAccount CreateBankAccount(int id, string name, decimal initialBalance, AccountType type = AccountType.Checking)
        {
            return new BankAccount(id, name, initialBalance, type);
        }

        public Category CreateCategory(CategoryType type, string name)
        {
            return new Category(_nextCategoryId++, type, name);
        }

        public Category CreateCategory(int id, CategoryType type, string name)
        {
            return new Category(id, type, name);
        }

        public Operation CreateOperation(OperationType type, int bankAccountId, decimal amount, DateTime date, int categoryId, string description = "")
        {
            return new Operation(_nextOperationId++, type, bankAccountId, amount, date, categoryId, description);
        }

        public Operation CreateOperation(int id, OperationType type, int bankAccountId, decimal amount, DateTime date, int categoryId, string description = "")
        {
            return new Operation(id, type, bankAccountId, amount, date, categoryId, description);
        }
    }
}
