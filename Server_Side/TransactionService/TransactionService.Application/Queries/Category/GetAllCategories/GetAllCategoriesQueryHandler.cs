using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TransactionService.Application.DTOs;
using TransactionService.Domain.Interfaces;

namespace TransactionService.Application.Queries.Category.GetAllCategories
{
    public class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, ILogger<GetAllCategoriesQuery> logger) : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>> 
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetAllCategoriesQuery> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _categoryRepository.GetAllCategoriesAsync();
                return _mapper.Map<IEnumerable<CategoryDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all categories");
                throw;
            }
        }
    }
}
