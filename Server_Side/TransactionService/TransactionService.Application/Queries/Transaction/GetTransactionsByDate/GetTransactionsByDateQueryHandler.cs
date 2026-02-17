using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByDate
{
    public class GetTransactionsByDateQueryHandler(ITransactionRepository transactionRepository, IMapper mapper, ILogger<GetTransactionsByDateQueryHandler> logger) : IRequestHandler<GetTransactionsByDateQuery, IEnumerable<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetTransactionsByDateQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsByDateQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var transactions = await _transactionRepository.SearchTransactionsAsync(t => t.CreatedAt.Date == request.Date.Date);
                return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving transactions for date {Date}", request.Date);
                throw;
            }
        }
    }
}