using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByUser
{
    public record GetTransactionsByUserQuery(Guid UserId) : IRequest<IEnumerable<TransactionDto>>;
}
