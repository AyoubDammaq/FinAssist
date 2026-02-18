using FluentValidation;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Category.AddCategory
{
    public class AddCategoryCommandValidator : AbstractValidator<AddCategoryRequest>
    {
        public AddCategoryCommandValidator() { 
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Le nom de la catégorie est requis.")
                .MaximumLength(100).WithMessage("Le nom de la catégorie ne peut pas dépasser 100 caractères.");
        }
    }
}
