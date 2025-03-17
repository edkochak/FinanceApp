using FinanceApp.Services.Facade;

namespace FinanceApp.Services.Command
{
    public class DeleteOperationCommand : ICommand
    {
        private readonly OperationFacade _facade;
        private readonly int _operationId;

        public DeleteOperationCommand(OperationFacade facade, int operationId)
        {
            _facade = facade;
            _operationId = operationId;
        }

        public void Execute()
        {
            _facade.DeleteOperation(_operationId);
        }
    }
}
