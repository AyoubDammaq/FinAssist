using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByCategory
{
    public record GetTransactionsByCategoryQuery(Guid UserId, Guid CategoryId) : IRequest<IEnumerable<TransactionDto>>;
}
