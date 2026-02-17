using MediatR;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Category.AddCategory
{
    public record AddCategoryCommand(AddCategoryRequest addCategoryRequest) : IRequest<bool>;
}
