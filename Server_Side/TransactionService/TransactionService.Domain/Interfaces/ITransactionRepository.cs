using System.Linq.Expressions;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Interfaces
{
    public interface ITransactionRepository
    {
        Task AddTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Guid transactionId);
        Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
        Task<IEnumerable<Transaction>> GetTransactionsByUserIdAsync(Guid userId);
        Task<IEnumerable<Transaction>> SearchTransactionsAsync(Expression<Func<Transaction, bool>> predicate);
    }
}
