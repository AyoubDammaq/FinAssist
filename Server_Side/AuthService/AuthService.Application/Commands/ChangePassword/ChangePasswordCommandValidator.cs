using FluentValidation;

namespace AuthService.Application.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.ChangePasswordRequestDto.UserId)
                .NotEmpty();

            RuleFor(x => x.ChangePasswordRequestDto)
                .NotNull();

            When(x => x.ChangePasswordRequestDto is not null, () =>
            {
                RuleFor(x => x.ChangePasswordRequestDto.CurrentPassword)
                    .NotEmpty()
                    .MinimumLength(6)
                    .MaximumLength(256);

                RuleFor(x => x.ChangePasswordRequestDto.NewPassword)
                    .NotEmpty()
                    .MinimumLength(6)
                    .MaximumLength(256);

                RuleFor(x => x.ChangePasswordRequestDto.ConfirmNewPassword)
                    .NotEmpty()
                    .Equal(x => x.ChangePasswordRequestDto.NewPassword)
                    .WithMessage("Les mots de passe ne correspondent pas.");
            });
        }
    }
}
