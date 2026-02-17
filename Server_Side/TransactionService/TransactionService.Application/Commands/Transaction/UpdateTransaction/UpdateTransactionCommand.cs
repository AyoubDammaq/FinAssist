using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Transaction.UpdateTransaction
{
    public record UpdateTransactionCommand(UpdateTransactionRequest updateTransactionRequest) : IRequest<bool>;
}
