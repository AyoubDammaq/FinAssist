using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Category.DeleteCategory
{
    public class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, ILogger<DeleteCategoryCommand> logger) : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly ILogger<DeleteCategoryCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<bool> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(request.categoryId);
                if (category != null)
                {
                    await _categoryRepository.DeleteCategoryAsync(request.categoryId);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", request.categoryId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting category with ID {CategoryId}", request.categoryId);
                throw;
            }
        }
    }
}
