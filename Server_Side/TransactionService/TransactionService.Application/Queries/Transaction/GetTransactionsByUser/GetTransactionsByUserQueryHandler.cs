using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByUser
{
    public class GetTransactionsByUserQueryHandler(ITransactionRepository transactionRepository, IMapper mapper, ILogger<GetTransactionsByUserQueryHandler> logger) : IRequestHandler<GetTransactionsByUserQuery, IEnumerable<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetTransactionsByUserQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsByUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var transactions = await _transactionRepository.SearchTransactionsAsync(t => t.UserId == request.UserId);
                return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving transactions for user {UserId}", request.UserId);
                throw;
            }
        }
    }
}