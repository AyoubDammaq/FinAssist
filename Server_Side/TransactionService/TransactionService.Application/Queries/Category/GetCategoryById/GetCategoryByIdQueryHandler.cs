using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Category.GetCategoryById
{
    public class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, ILogger<GetCategoryByIdQueryHandler> logger) : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _categoryRepository.GetCategoryByIdAsync(request.CategoryId);
                if (category != null)
                {
                    return _mapper.Map<CategoryDto>(category);
                }
                else
                {
                    _logger.LogWarning("Category with ID {CategoryId} not found", request.CategoryId);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving category with ID {CategoryId}", request.CategoryId);
                throw;
            }
        }
    }
}
