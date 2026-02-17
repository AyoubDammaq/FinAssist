using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Transaction.UpdateTransaction
{
    public class UpdateTransactionCommandHandler(ITransactionRepository transactionRepository, IMapper mapper, ILogger<UpdateTransactionCommandHandler> logger) : IRequestHandler<UpdateTransactionCommand, bool>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateTransactionCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingTransaction = await _transactionRepository.GetTransactionByIdAsync(request.updateTransactionRequest.Id);
                if (existingTransaction != null)
                {
                    var updatedTransaction = _mapper.Map(request.updateTransactionRequest, existingTransaction);
                    await _transactionRepository.UpdateTransactionAsync(updatedTransaction);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found", request.updateTransactionRequest.Id);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating transaction with ID {TransactionId}", request.updateTransactionRequest.Id);
                throw;
            }
        }
    }
}
