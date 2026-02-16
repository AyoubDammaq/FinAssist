using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories
{
    public class TransactionRepository(TransactionDbContext transactionDbContext) : ITransactionRepository
    {
        private readonly TransactionDbContext _transactionDbContext = transactionDbContext ?? throw new ArgumentNullException(nameof(transactionDbContext));

        public async Task AddTransactionAsync(Transaction transaction)
        {
            _transactionDbContext.Transactions.Add(transaction);
            await _transactionDbContext.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _transactionDbContext.Transactions.Update(transaction);
            await _transactionDbContext.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(Guid transactionId)
        {
            var transaction = await _transactionDbContext.Transactions.FindAsync(transactionId);
            if (transaction != null)
            {
                _transactionDbContext.Transactions.Remove(transaction);
                await _transactionDbContext.SaveChangesAsync();
            }
        }
        public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId)
        {
            return await _transactionDbContext.Transactions.FindAsync(transactionId);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId)
        {
            return await _transactionDbContext.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> SearchTransactionsAsync(Expression<Func<Transaction, bool>> predicate)
        {
            return await _transactionDbContext.Transactions
                .Where(predicate)
                .ToListAsync();
        }
    }
}
