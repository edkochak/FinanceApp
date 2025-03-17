using FinanceApp.Services.Facade;

namespace FinanceApp.Services.Command
{
    public class UpdateBalanceCommand : ICommand
    {
        private readonly BankAccountFacade _facade;
        private readonly int _accountId;
        private readonly decimal _amount;

        public UpdateBalanceCommand(BankAccountFacade facade, int accountId, decimal amount)
        {
            _facade = facade;
            _accountId = accountId;
            _amount = amount;
        }

        public void Execute()
        {
            _facade.UpdateBalance(_accountId, _amount);
        }
    }
}
