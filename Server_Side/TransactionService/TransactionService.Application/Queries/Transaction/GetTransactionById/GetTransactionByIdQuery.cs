using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Transaction.GetTransactionById
{
    public record GetTransactionByIdQuery(Guid TransactionId) : IRequest<TransactionDto?>;
}
