using System;

namespace FinanceApp.Domain
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public partial class Category
    {
        public int Id { get; private set; }
        public CategoryType Type { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Category(int id, CategoryType type, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя категории не может быть пустым", nameof(name));

            Id = id;
            Type = type;
            Name = name;
            CreatedAt = DateTime.Now;
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Имя категории не может быть пустым", nameof(name));
            
            Name = name;
        }

        public void Accept(Services.Export.IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
