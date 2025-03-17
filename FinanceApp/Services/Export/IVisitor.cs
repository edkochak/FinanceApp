using FinanceApp.Domain;

namespace FinanceApp.Services.Export
{
    public interface IVisitor
    {
        void Visit(BankAccount account);
        void Visit(Category category);
        void Visit(Operation operation);
    }
}
