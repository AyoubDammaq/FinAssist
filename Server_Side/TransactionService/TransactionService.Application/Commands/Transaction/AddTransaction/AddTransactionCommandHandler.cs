using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Transaction.AddTransaction
{
    public class AddTransactionCommandHandler(ICategoryRepository categoryRepository, ITransactionRepository transactionRepository, IMapper mapper, ILogger<AddTransactionCommandHandler> logger) : IRequestHandler<AddTransactionCommand, bool>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly ITransactionRepository _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddTransactionCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var categoryExists = await _categoryRepository.GetCategoryByIdAsync(request.addTransactionRequest.CategoryId);
                if (categoryExists == null)
                {
                    _logger.LogWarning("Category with ID {CategoryId} does not exist.", request.addTransactionRequest.CategoryId);
                    return false;
                }

                var transaction = _mapper.Map<Domain.Entities.Transaction>(request.addTransactionRequest);
                await _transactionRepository.AddTransactionAsync(transaction);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding transaction for user with ID {UserId}", request.addTransactionRequest.UserId);
                throw;
            }
        }
    }
}