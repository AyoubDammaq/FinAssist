using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using DnsClient;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text.Encodings.Web;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace AuthService.Application.Services
{
    public sealed class EmailManagment(IConfiguration configuration, IUserRepository userRepository, ILogger<EmailManagment> logger, LookupClient? dnsClient = null) : IEmailManagment
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly LookupClient _dnsClient = dnsClient ?? new LookupClient();
        private readonly ILogger<EmailManagment> _logger = logger;

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("toEmail required", nameof(toEmail));

            var host = _configuration["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host missing");
            var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"] ?? username;
            var fromName = _configuration["Smtp:FromName"] ?? "AuthService";

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(fromName, fromEmail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject ?? string.Empty;
            msg.Body = new TextPart("html") { Text = body ?? string.Empty };

            // Choose StartTls for 587, SslOnConnect for 465
            var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            // Optional: small timeout in ms
            const int connectTimeoutMs = 20000;

            try
            {
                using var client = new SmtpClient
                {
                    // Optional: set a local domain if your env needs it
                    // LocalDomain = "your.local.domain"
                };

                client.Timeout = connectTimeoutMs;

                // Connect
                await client.ConnectAsync(host, port, socketOptions, cancellationToken).ConfigureAwait(false);

                // Authenticate if credentials provided
                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    // If using App Password it will work as normal password
                    await client.AuthenticateAsync(username, password, cancellationToken).ConfigureAwait(false);
                }

                await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Email sent to {To}", toEmail);
                return true;
            }
            catch (SocketException sockEx)
            {
                _logger.LogError(sockEx, "SocketException sending email to {To} (SocketErrorCode={Code})", toEmail, sockEx.SocketErrorCode);
                return false;
            }
            catch (SslHandshakeException sslEx)
            {
                _logger.LogError(sslEx, "TLS handshake failed sending email to {To}", toEmail);
                return false;
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Authentication failed sending email to {To} (check username/app-password)", toEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending email to {To}", toEmail);
                return false;
            }
        }

        public Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("L'adresse e-mail destinataire est requise.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(resetToken))
                throw new ArgumentException("Le jeton de réinitialisation est requis.", nameof(resetToken));

            var baseUrl = _configuration["App:PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("Configuration manquante: `App:PublicBaseUrl` (ex: https://app.monsite.com).");

            var tokenEncoded = UrlEncoder.Default.Encode(resetToken);
            var emailEncoded = UrlEncoder.Default.Encode(toEmail);

            var resetLink = $"{baseUrl.TrimEnd('/')}/reset-password?email={emailEncoded}&token={tokenEncoded}";

            var subject = "Réinitialisation de votre mot de passe";
            var body =
                $"""
                 <p>Bonjour,</p>
                 <p>Vous avez demandé la réinitialisation de votre mot de passe.</p>
                 <p>Cliquez sur ce lien pour continuer :</p>
                 <p><a href="{resetLink}">Réinitialiser mon mot de passe</a></p>
                 <p>Ou copiez-collez ce lien dans votre navigateur :<br>{resetLink}</p>
                 <p>Si vous n'êtes pas à l'origine de cette demande, ignorez cet e-mail.</p>
                 """;

            return SendEmailAsync(toEmail, subject, body, cancellationToken);
        }

        public async Task<bool> CheckEmailValidation(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // 1) Syntaxe
            try
            {
                var normalized = new MailAddress(email.Trim()).Address;
                email = normalized;
            }
            catch
            {
                return false;
            }

            // 2) Domaine
            var atIndex = email.LastIndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
                return false;

            var domain = email[(atIndex + 1)..];
            if (domain.Length > 255)
                return false;

            // 3) MX / A / AAAA lookup via DnsClient
            try
            {
                var queryResult = await _dnsClient.QueryAsync(domain, QueryType.MX).ConfigureAwait(false);
                var hasMx = queryResult.Answers.MxRecords().Any();

                if (!hasMx)
                {
                    var aResult = await _dnsClient.QueryAsync(domain, QueryType.A).ConfigureAwait(false);
                    var aaaaResult = await _dnsClient.QueryAsync(domain, QueryType.AAAA).ConfigureAwait(false);

                    var hasA = aResult.Answers.ARecords().Any() || aaaaResult.Answers.AaaaRecords().Any();

                    if (!hasA)
                    {
                        // domaine probablement non-routable pour le mail
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            // 4) Existence du compte
            var normalizedEmailForLookup = email.Trim().ToLowerInvariant();

            try
            {
                var user = await _userRepository.GetByEmail(normalizedEmailForLookup).ConfigureAwait(false);
                return user != null;
            }
            catch
            {
                return false;
            }
        }
    }
}