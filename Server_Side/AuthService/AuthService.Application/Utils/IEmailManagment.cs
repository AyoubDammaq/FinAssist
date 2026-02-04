namespace AuthService.Application.Utils
{
    public interface IEmailManagment
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
        Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
        Task<bool> SendPasswordChangedEmail(string toEmail, CancellationToken cancellationToken = default);
        Task<bool> CheckEmailValidation(string email, CancellationToken cancellationToken = default);
    }
}
