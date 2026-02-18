using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Transaction.AddTransaction
{
    public record AddTransactionCommand(AddTransactionRequest addTransactionRequest) : IRequest<bool>;
}
