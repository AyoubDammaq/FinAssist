using FluentValidation;

namespace TransactionService.Application.Commands.Transaction.AddTransaction
{
    public class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
    {
        public AddTransactionCommandValidator() 
        {
            RuleFor(x => x.addTransactionRequest.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");
        }
    }
}
