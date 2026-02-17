using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Category.UpdateCategory
{
    public record UpdateCategoryCommand(UpdateCategoryRequest updateCategoryRequest) : IRequest<bool>;
}
