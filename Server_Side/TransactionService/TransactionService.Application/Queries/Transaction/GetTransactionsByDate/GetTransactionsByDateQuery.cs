using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByDate
{
    public record GetTransactionsByDateQuery(DateTime Date) : IRequest<IEnumerable<TransactionDto>>;
}
