using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories
{
    public class CategoryRepository(TransactionDbContext transactionDbContext) : ICategoryRepository
    {
        private readonly TransactionDbContext _transactionDbContext = transactionDbContext ?? throw new ArgumentNullException(nameof(transactionDbContext));

        public async Task AddCategoryAsync(Category category)
        {
            _transactionDbContext.Categories.Add(category);
            await _transactionDbContext.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _transactionDbContext.Categories.Update(category);
            await _transactionDbContext.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _transactionDbContext.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _transactionDbContext.Categories.Remove(category);
                await _transactionDbContext.SaveChangesAsync();
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _transactionDbContext.Categories.FindAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetCategoriesByUserIdAsync(Guid userId)
        {
            return await _transactionDbContext.Categories
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _transactionDbContext.Categories.ToListAsync();
        }
    }
}
