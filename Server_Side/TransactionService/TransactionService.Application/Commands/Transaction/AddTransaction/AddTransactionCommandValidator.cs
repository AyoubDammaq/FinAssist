using FluentValidation;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Transaction.AddTransaction
{
    public class AddTransactionCommandValidator : AbstractValidator<AddTransactionRequest>
    {
        public AddTransactionCommandValidator() 
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
