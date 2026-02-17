using FluentValidation;

namespace TransactionService.Application.Commands.Transaction.UpdateTransaction
{
    public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
    {
        public UpdateTransactionCommandValidator() { 
            RuleFor(x => x.updateTransactionRequest.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
