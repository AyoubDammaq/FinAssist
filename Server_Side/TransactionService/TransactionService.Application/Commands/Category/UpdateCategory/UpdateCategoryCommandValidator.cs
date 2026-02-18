using FluentValidation;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Category.UpdateCategory
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryRequest>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name cannot exceed 100 characters.");
        }
    }
}
