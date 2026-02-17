using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Category.UpdateCategory
{
    public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper, ILogger<UpdateCategoryCommand> logger) : IRequestHandler<UpdateCategoryCommand, bool>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateCategoryCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<bool> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetCategoryByNameAsync(request.updateCategoryRequest.Name);
                if (existingCategory != null)
                {
                    var updatedCategory = _mapper.Map(request.updateCategoryRequest, existingCategory);
                    await _categoryRepository.UpdateCategoryAsync(updatedCategory);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Category with Name {CategoryName} not found", request.updateCategoryRequest.Name);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating category with Name {CategoryName}", request.updateCategoryRequest.Name);
                throw;
            }
        }
    }
}
