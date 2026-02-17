using MediatR;

namespace TransactionService.Application.Commands.Category.DeleteCategory
{
    public record DeleteCategoryCommand(Guid categoryId) : IRequest<bool>;
}
