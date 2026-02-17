using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Category.GetCategoryByUser
{
    public record GetCategoryByUserQuery(Guid UserId) : IRequest<IEnumerable<CategoryDto>>;
}

