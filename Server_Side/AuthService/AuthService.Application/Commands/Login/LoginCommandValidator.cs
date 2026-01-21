using FluentValidation;

namespace AuthService.Application.Commands.Login
{
    public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.loginRequestDto)
                .NotNull();

            When(x => x.loginRequestDto is not null, () =>
            {
                RuleFor(x => x.loginRequestDto.Email)
                    .NotEmpty()
                    .EmailAddress()
                    .MaximumLength(256);

                RuleFor(x => x.loginRequestDto.Password)
                    .NotEmpty()
                    .MinimumLength(8)
                    .MaximumLength(1024);
            });
        }
    }
}