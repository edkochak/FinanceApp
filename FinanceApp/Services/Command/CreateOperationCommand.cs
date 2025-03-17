using System;
using FinanceApp.Domain;
using FinanceApp.Services.Facade;

namespace FinanceApp.Services.Command
{
    public class CreateOperationCommand : ICommand
    {
        private readonly OperationFacade _facade;
        private readonly OperationType _type;
        private readonly int _accountId;
        private readonly decimal _amount;
        private readonly int _categoryId;
        private readonly string _description;

        public CreateOperationCommand(
            OperationFacade facade,
            OperationType type,
            int accountId,
            decimal amount,
            int categoryId,
            string description = "")
        {
            _facade = facade;
            _type = type;
            _accountId = accountId;
            _amount = amount;
            _categoryId = categoryId;
            _description = description;
        }

        public void Execute()
        {
            _facade.CreateOperation(_type, _accountId, _amount, DateTime.Now, _categoryId, _description);
        }
    }
}
