using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Commands.Category.AddCategory
{
    public class AddCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper, ILogger<AddCategoryCommand> logger) : IRequestHandler<AddCategoryCommand, bool>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<AddCategoryCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> Handle(AddCategoryCommand request, CancellationToken cancellationToken)
        {
            // Implementation for adding a category
            try
            {
                var categoryExists = await _categoryRepository.CategoryExistsAsync(request.addCategoryRequest.Name);
                if (!categoryExists)
                {
                    var category = _mapper.Map<Domain.Entities.Category>(request.addCategoryRequest);
                    await _categoryRepository.AddCategoryAsync(category);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Category with name {CategoryName} already exists", request.addCategoryRequest.Name);
                    return false;
                }
            }   
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding category");
                throw;
            }
        }
    }
}
