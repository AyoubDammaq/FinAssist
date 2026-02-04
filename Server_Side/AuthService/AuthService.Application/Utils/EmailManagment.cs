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
        private readonly ILogger<EmailManagment> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("L'adresse e-mail destinataire est vide ou nulle.");
                throw new ArgumentException("toEmail required", nameof(toEmail));
            }

            var host = _configuration["Smtp:Host"];
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogError("Configuration SMTP: Host manquant.");
                throw new InvalidOperationException("Smtp:Host missing");
            }

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

            var socketOptions = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            const int connectTimeoutMs = 20000;

            try
            {
                using var client = new SmtpClient();
                client.Timeout = connectTimeoutMs;

                await client.ConnectAsync(host, port, socketOptions, cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                {
                    await client.AuthenticateAsync(username, password, cancellationToken).ConfigureAwait(false);
                }

                await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Email envoyé à {To}", toEmail);
                return true;
            }
            catch (SocketException sockEx)
            {
                _logger.LogError(sockEx, "Erreur de socket lors de l'envoi de l'e-mail à {To} (SocketErrorCode={Code})", toEmail, sockEx.SocketErrorCode);
                return false;
            }
            catch (SslHandshakeException sslEx)
            {
                _logger.LogError(sslEx, "Échec de la négociation TLS lors de l'envoi de l'e-mail à {To}", toEmail);
                return false;
            }
            catch (AuthenticationException authEx)
            {
                _logger.LogError(authEx, "Échec de l'authentification lors de l'envoi de l'e-mail à {To} (vérifiez le nom d'utilisateur/mot de passe)", toEmail);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'envoi de l'e-mail à {To}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    _logger.LogError("L'adresse e-mail destinataire est requise.");
                    throw new ArgumentException("L'adresse e-mail destinataire est requise.", nameof(toEmail));
                }
                if (string.IsNullOrWhiteSpace(resetToken))
                {
                    _logger.LogError("Le jeton de réinitialisation est requis.");
                    throw new ArgumentException("Le jeton de réinitialisation est requis.", nameof(resetToken));
                }

                var baseUrl = _configuration["App:PublicBaseUrl"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    _logger.LogError("Configuration manquante: `App:PublicBaseUrl`.");
                    throw new InvalidOperationException("Configuration manquante: `App:PublicBaseUrl` (ex: https://app.monsite.com).");
                }

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

                return await SendEmailAsync(toEmail, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'e-mail de réinitialisation à {To}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordChangedEmail(string toEmail, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    _logger.LogError("L'adresse e-mail destinataire est requise.");
                    throw new ArgumentException("L'adresse e-mail destinataire est requise.", nameof(toEmail));
                }
                var subject = "Confirmation de changement de mot de passe";
                var body =
                    $"""
                     <p>Bonjour,</p>
                     <p>Votre mot de passe a été modifié avec succès.</p>
                     <p>Si vous n'êtes pas à l'origine de ce changement, veuillez contacter le support immédiatement.</p>
                     """;
                return await SendEmailAsync(toEmail, subject, body, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'e-mail de confirmation de changement de mot de passe à {To}", toEmail);
                return false;
            }
        }

        public async Task<bool> CheckEmailValidation(string email, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    _logger.LogWarning("L'e-mail fourni est vide ou nul.");
                    return false;
                }

                // 1) Syntaxe
                try
                {
                    var normalized = new MailAddress(email.Trim()).Address;
                    email = normalized;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Échec de la validation de la syntaxe de l'e-mail: {Email}", email);
                    return false;
                }

                // 2) Domaine
                var atIndex = email.LastIndexOf('@');
                if (atIndex < 0 || atIndex == email.Length - 1)
                {
                    _logger.LogWarning("Le domaine de l'e-mail est invalide: {Email}", email);
                    return false;
                }

                var domain = email[(atIndex + 1)..];
                if (domain.Length > 255)
                {
                    _logger.LogWarning("Le domaine de l'e-mail est trop long: {Domain}", domain);
                    return false;
                }

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
                            _logger.LogWarning("Le domaine n'a pas d'enregistrements MX/A/AAAA: {Domain}", domain);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Échec de la résolution DNS pour le domaine: {Domain}", domain);
                    return false;
                }

                // 4) Existence du compte
                var normalizedEmailForLookup = email.Trim().ToLowerInvariant();

                try
                {
                    var user = await _userRepository.GetByEmail(normalizedEmailForLookup).ConfigureAwait(false);
                    return user != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la vérification de l'existence du compte pour l'e-mail: {Email}", normalizedEmailForLookup);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la validation de l'e-mail: {Email}", email);
                return false;
            }
        }
    }
}