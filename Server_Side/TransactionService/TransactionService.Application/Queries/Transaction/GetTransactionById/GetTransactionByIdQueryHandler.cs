using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Transaction.GetTransactionById
{
    public class GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository, IMapper mapper, ILogger<GetTransactionByIdQueryHandler> logger) : IRequestHandler<GetTransactionByIdQuery, TransactionDto?>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetTransactionByIdQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<TransactionDto?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var transaction = await _transactionRepository.GetTransactionByIdAsync(request.TransactionId);
                if (transaction != null)
                {
                    return _mapper.Map<TransactionDto>(transaction);
                }
                else
                {
                    _logger.LogWarning("Transaction with ID {TransactionId} not found", request.TransactionId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving transaction with ID {TransactionId}", request.TransactionId);
                throw;
            }
        }
    }
}
