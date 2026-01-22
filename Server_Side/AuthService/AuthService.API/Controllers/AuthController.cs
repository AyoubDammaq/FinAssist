using AuthService.Application.Commands.ChangePassword;
using AuthService.Application.Commands.DeleteUser;
using AuthService.Application.Commands.ForgotPassword;
using AuthService.Application.Commands.Login;
using AuthService.Application.Commands.Logout;
using AuthService.Application.Commands.RefreshToken;
using AuthService.Application.Commands.Register;
using AuthService.Application.Commands.ResetPassword;
using AuthService.Application.Commands.UpdateProfile;
using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Queries.GetAllUsers;
using AuthService.Application.Queries.GetUserByEmail;
using AuthService.Application.Queries.GetUserById;
using AuthService.Application.Queries.GetUserByUsername;
using AuthService.Application.Queries.Health;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private sealed record ApiError(string Message, string ErrorCode, object? Details = null);

        private IActionResult MapExceptionToResponse(Exception ex)
        {
            return ex switch
            {
                WeakPasswordException => BadRequest(new ApiError(
                    Message: "Mot de passe trop faible. Utilisez au moins 8 caractères avec une majuscule, une minuscule, un chiffre et un caractère spécial.",
                    ErrorCode: "WEAK_PASSWORD"
                )),

                UnauthorizedAccessException => Unauthorized(new ApiError(
                    Message: "Accès non autorisé. Veuillez vous authentifier puis réessayer.",
                    ErrorCode: "AUTH_UNAUTHORIZED"
                )),

                KeyNotFoundException => NotFound(new ApiError(
                    Message: "Ressource introuvable.",
                    ErrorCode: "NOT_FOUND"
                )),

                ApplicationException appEx => BadRequest(new ApiError(
                    Message: appEx.Message,
                    ErrorCode: "BUSINESS_RULE"
                )),

                _ => StatusCode(500, new ApiError(
                    Message: "Une erreur technique est survenue. Veuillez réessayer plus tard.",
                    ErrorCode: "INTERNAL_ERROR"
                ))
            };
        }

        private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                return MapExceptionToResponse(ex);
            }
        }

        // Commandes

        [HttpPost("register")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
            => ExecuteAsync(async () =>
            {
                var command = new RegisterCommand(registerRequestDto);
                var result = await _mediator.Send(command);
                return Ok(result);
            });

        [HttpPost("login")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
            => ExecuteAsync(async () =>
            {
                var command = new LoginCommand(loginRequestDto);
                var result = await _mediator.Send(command);
                return Ok(result);
            });

        [HttpPost("logout")]
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Logout([FromBody] string email)
            => ExecuteAsync(async () =>
            {
                var command = new LogoutCommand(email);
                var result = await _mediator.Send(command);
                return Ok(result);
            });

        [HttpPost("refresh")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto refreshTokenRequestDto)
            => ExecuteAsync(async () =>
            {
                if (refreshTokenRequestDto is null || string.IsNullOrWhiteSpace(refreshTokenRequestDto.RefreshToken))
                {
                    return BadRequest(new ApiError(
                        Message: "Le jeton de rafraîchissement (refresh token) est requis.",
                        ErrorCode: "REFRESH_TOKEN_REQUIRED"
                    ));
                }

                var command = new RefreshTokenCommand(refreshTokenRequestDto);
                var result = await _mediator.Send(command);
                return Ok(result);
            });

        [HttpPost("change-password")]
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto, CancellationToken cancellationToken)
            => ExecuteAsync(async () =>
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new ApiError(
                        Message: "Session invalide. Veuillez vous reconnecter.",
                        ErrorCode: "INVALID_SESSION"
                    ));
                }

                await _mediator.Send(new ChangePasswordCommand(userId, dto), cancellationToken);

                return Ok(new { message = "Mot de passe mis à jour avec succès." });
            });

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken cancellationToken)
            => ExecuteAsync(async () =>
            {
                await _mediator.Send(new ForgotPasswordCommand(dto), cancellationToken);

                // Message volontairement neutre (sécurité : éviter d'indiquer si l'email existe).
                return Ok(new { message = "Si un compte existe pour cet e-mail, un message de réinitialisation a été envoyé." });
            });

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
            => ExecuteAsync(async () =>
            {
                await _mediator.Send(new ResetPasswordCommand(dto), cancellationToken);
                return Ok(new { message = "Mot de passe réinitialisé avec succès. Vous pouvez vous connecter." });
            });

        [HttpPut("profile")]
        [Authorize]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
            => ExecuteAsync(async () =>
            {
                var command = new UpdateProfileCommand(updateProfileDto);
                await _mediator.Send(command);
                return NoContent();
            });

        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> DeleteUser(Guid id)
            => ExecuteAsync(async () =>
            {
                var command = new DeleteUserCommand(id);
                await _mediator.Send(command);
                return NoContent();
            });

        // Requêtes

        [HttpGet("users")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetAllUsers()
            => ExecuteAsync(async () =>
            {
                var query = new GetAllUsersQuery();
                var result = await _mediator.Send(query);
                return Ok(result);
            });

        [HttpGet("users/{id}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetUserById(Guid id)
            => ExecuteAsync(async () =>
            {
                var query = new GetUserByIdQuery(id);
                var result = await _mediator.Send(query);
                return Ok(result);
            });

        [HttpGet("users/username/{username}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetUserByUsername(string username)
            => ExecuteAsync(async () =>
            {
                var query = new GetUserByUsernameQuery(username);
                var result = await _mediator.Send(query);
                return Ok(result);
            });

        [HttpGet("users/email/{email}")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> GetUserByEmail(string email)
            => ExecuteAsync(async () =>
            {
                var query = new GetUserByEmailQuery(email);
                var result = await _mediator.Send(query);
                return Ok(result);
            });

        [HttpGet("health")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public Task<IActionResult> Health()
            => ExecuteAsync(async () =>
            {
                var query = new HealthCommand();
                var result = await _mediator.Send(query);
                return Ok(result);
            });
    }
}