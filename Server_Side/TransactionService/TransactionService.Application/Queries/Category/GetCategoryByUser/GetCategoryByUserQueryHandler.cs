using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Category.GetCategoryByUser
{
    public class GetCategoryByUserQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, ILogger<GetCategoryByUserQueryHandler> logger) : IRequestHandler<GetCategoryByUserQuery, IEnumerable<CategoryDto>>
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetCategoryByUserQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<IEnumerable<CategoryDto>> Handle(GetCategoryByUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _categoryRepository.GetCategoriesByUserIdAsync(request.UserId);
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving categories for user with ID {UserId}", request.UserId);
                throw;
            }
        }
    }
}
