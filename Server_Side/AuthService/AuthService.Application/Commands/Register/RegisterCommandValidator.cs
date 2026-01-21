using FluentValidation;

namespace AuthService.Application.Commands.Register
{
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.registerRequestDto)
                .NotNull();

            When(x => x.registerRequestDto is not null, () =>
            {
                RuleFor(x => x.registerRequestDto.UserName)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.registerRequestDto.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .MaximumLength(256);

                RuleFor(x => x.registerRequestDto.Password)
                    .NotEmpty()
                    .MinimumLength(6)
                    .MaximumLength(256);

                RuleFor(x => x.registerRequestDto.ConfirmPassword)
                    .NotEmpty()
                    .Equal(x => x.registerRequestDto.Password)
                    .WithMessage("La confirmation du mot de passe ne correspond pas.");
            });
        }
    }
}
