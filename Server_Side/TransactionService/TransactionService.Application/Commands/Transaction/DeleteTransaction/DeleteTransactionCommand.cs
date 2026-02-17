using MediatR;

namespace TransactionService.Application.Commands.Transaction.DeleteTransaction
{
    public record DeleteTransactionCommand(Guid TransactionId) : IRequest<bool>;
}
