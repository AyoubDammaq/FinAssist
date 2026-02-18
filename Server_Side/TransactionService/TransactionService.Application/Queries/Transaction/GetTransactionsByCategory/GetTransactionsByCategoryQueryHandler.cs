using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Transaction.GetTransactionsByCategory
{
    public class GetTransactionsByCategoryQueryHandler(ITransactionRepository transactionRepository, IMapper mapper, ILogger<GetTransactionsByCategoryQuery> logger) : IRequestHandler<GetTransactionsByCategoryQuery, IEnumerable<TransactionDto>>
    {
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetTransactionsByCategoryQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<TransactionDto>> Handle(GetTransactionsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var transactions = await _transactionRepository.SearchTransactionsAsync(t => t.UserId == request.UserId && t.CategoryId == request.CategoryId);
            if (transactions == null || !transactions.Any())
            {
                _logger.LogWarning("No transactions found for user {UserId} and category {CategoryId}", request.UserId, request.CategoryId);
                return Enumerable.Empty<TransactionDto>();
            }
            return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
        }
    }
}