using Xunit;
using FinanceApp.Services.Facade;
using FinanceApp.Services.Implementations;

namespace FinanceApp.Tests
{
    public class BasicTests
    {
        [Fact]
        public void Factory_ShouldCreateBankAccount()
        {
            // ...existing code...
            var factory = new FinancialObjectFactory();
            var acc = factory.CreateBankAccount(1, "Test", 100);
            Assert.Equal(1, acc.Id);
            Assert.Equal(100, acc.Balance);
        }

        [Fact]
        public void BankAccountFacade_ShouldUpdateBalance()
        {
            // ...existing code...
            var factory = new FinancialObjectFactory();
            var facade = new BankAccountFacade(factory);
            var acc = facade.CreateAccount(1, "Test", 100);
            facade.UpdateBalance(1, 50);
            Assert.Equal(150, acc.Balance);
        }
    }
}
