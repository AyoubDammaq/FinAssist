using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Category.GetAllCategories
{
    public record GetAllCategoriesQuery() : IRequest<IEnumerable<CategoryDto>>;
}
