using System;
using System.Collections.Generic;
using System.Linq;
using FinanceApp.Domain;
using FinanceApp.Services.Implementations;

namespace FinanceApp.Services.Facade
{
    public class CategoryFacade
    {
        private readonly Dictionary<int, Category> _categories = new Dictionary<int, Category>();
        private readonly FinancialObjectFactory _factory;

        public CategoryFacade(FinancialObjectFactory factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Category CreateCategory(CategoryType type, string name)
        {
            var category = _factory.CreateCategory(type, name);
            _categories[category.Id] = category;
            return category;
        }

        // Для тестов
        public Category CreateCategory(int id, CategoryType type, string name)
        {
            var category = _factory.CreateCategory(id, type, name);
            _categories[category.Id] = category;
            return category;
        }

        public Category? GetCategory(int id)
        {
            return _categories.TryGetValue(id, out var category) ? category : null;
        }

        public List<Category> GetAllCategories()
        {
            return _categories.Values.ToList();
        }

        public List<Category> GetCategoriesByType(CategoryType type)
        {
            return _categories.Values.Where(c => c.Type == type).ToList();
        }

        public bool DeleteCategory(int id)
        {
            return _categories.Remove(id);
        }

        public bool UpdateName(int id, string name)
        {
            if (!_categories.TryGetValue(id, out var category))
                return false;

            category.UpdateName(name);
            return true;
        }
    }
}
