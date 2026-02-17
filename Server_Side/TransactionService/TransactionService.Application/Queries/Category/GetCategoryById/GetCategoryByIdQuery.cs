using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Queries.Category.GetCategoryById
{
    public record GetCategoryByIdQuery(Guid CategoryId) : IRequest<CategoryDto?>;
}
