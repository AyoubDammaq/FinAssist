using FluentValidation;
using TransactionService.Application.DTOs;

namespace TransactionService.Application.Commands.Transaction.UpdateTransaction
{
    public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionRequest>
    {
        public UpdateTransactionCommandValidator() { 
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
