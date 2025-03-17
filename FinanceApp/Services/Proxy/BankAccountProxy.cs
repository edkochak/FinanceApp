using FinanceApp.Domain;

namespace FinanceApp.Services.Proxy
{
    public interface IBankAccountProxy
    {
        BankAccount? GetById(int id);
        BankAccount Save(BankAccount account);
        bool Remove(int id);
        // ...existing code...
    }

    public class BankAccountProxy : IBankAccountProxy
    {
        private readonly Dictionary<int, BankAccount> _cache = new Dictionary<int, BankAccount>();

        public BankAccount? GetById(int id)
        {
            // ...existing code...
            _cache.TryGetValue(id, out var acct);
            return acct;
        }

        public BankAccount Save(BankAccount account)
        {
            // ...existing code...
            _cache[account.Id] = account;
            return account;
        }
        
        public bool Remove(int id)
        {
            return _cache.Remove(id);
        }
    }
}
