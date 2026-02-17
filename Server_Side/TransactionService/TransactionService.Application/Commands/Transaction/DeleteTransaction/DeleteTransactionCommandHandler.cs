using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Transaction.DeleteTransaction
{
    public class DeleteTransactionCommandHandler(ITransactionRepository transactionRepository, ILogger<DeleteTransactionCommand> logger) : IRequestHandler<DeleteTransactionCommand, bool>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly ILogger<DeleteTransactionCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var transaction = await _transactionRepository.GetTransactionByIdAsync(request.TransactionId);
                if (transaction != null)
                {
                    await _transactionRepository.DeleteTransactionAsync(request.TransactionId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found", request.TransactionId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting transaction with ID {TransactionId}", request.TransactionId);
                throw;
            }
        }
    }
}
