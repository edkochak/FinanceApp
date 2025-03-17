using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Domain;
using FinanceApp.Services.Implementations;

namespace FinanceApp.Services.Facade
{
    public class BankAccountFacade
    {
        private readonly Dictionary<int, BankAccount> _accounts = new Dictionary<int, BankAccount>();
        private readonly FinancialObjectFactory _factory;

        public BankAccountFacade(FinancialObjectFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public BankAccount CreateAccount(string name, decimal initialBalance, AccountType type = AccountType.Checking)
        {
            var account = _factory.CreateBankAccount(name, initialBalance, type);
            _accounts[account.Id] = account;
            return account;
        }

        // Для тестов
        public BankAccount CreateAccount(int id, string name, decimal initialBalance, AccountType type = AccountType.Checking)
        {
            var account = _factory.CreateBankAccount(id, name, initialBalance, type);
            _accounts[account.Id] = account;
            return account;
        }

        public BankAccount? GetAccount(int id)
        {
            return _accounts.TryGetValue(id, out var account) ? account : null;
        }

        public List<BankAccount> GetAllAccounts()
        {
            return _accounts.Values.ToList();
        }

        public bool DeleteAccount(int id)
        {
            return _accounts.Remove(id);
        }

        public bool UpdateBalance(int id, decimal amount)
        {
            if (!_accounts.TryGetValue(id, out var account))
                return false;

            account.UpdateBalance(amount);
            return true;
        }

        public bool UpdateName(int id, string name)
        {
            if (!_accounts.TryGetValue(id, out var account))
                return false;

            account.UpdateName(name);
            return true;
        }

        public decimal GetTotalBalance()
        {
            return _accounts.Values.Sum(a => a.Balance);
        }
    }
}
