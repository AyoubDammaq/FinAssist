using FluentValidation;

namespace TransactionService.Application.Commands.Category.AddCategory
{
    public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public AddCategoryCommandValidator() { 
            RuleFor(x => x.addCategoryRequest.Name)
                .NotEmpty().WithMessage("Le nom de la catégorie est requis.")
                .MaximumLength(100).WithMessage("Le nom de la catégorie ne peut pas dépasser 100 caractères.");
        }
    }
}
