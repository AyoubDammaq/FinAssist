using AuthService.Application.DTOs;
using AuthService.Application.Exceptions;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.Register
{
    public class RegisterCommandHandler(IUserRepository userRepository, IPasswordManagement passwordManagement, IMapper mapper, ILogger<RegisterCommandHandler> logger) : IRequestHandler<RegisterCommand, RegisterResponseDto>
    {
        public readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        public readonly IPasswordManagement _passwordManagement = passwordManagement ?? throw new ArgumentNullException(nameof(passwordManagement));
        public readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        public readonly ILogger<RegisterCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début du processus d'enregistrement pour l'utilisateur : {Email}", request.registerRequestDto.Email);

            var existingUser = await _userRepository.GetByEmail(request.registerRequestDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentative d'enregistrement avec un email déjà utilisé : {Email}", request.registerRequestDto.Email);
                throw new ApplicationException($"Le nom d'utilisateur {request.registerRequestDto.Email} est déjà pris.");
            }

            var isStrong = await _passwordManagement.IsPasswordStrong(request.registerRequestDto.Password);
            if (!isStrong)
            {
                _logger.LogWarning("Mot de passe faible fourni pour l'email : {Email}", request.registerRequestDto.Email);
                throw new WeakPasswordException();
            }

            try
            {
                var user = _mapper.Map<User>(request.registerRequestDto);
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, request.registerRequestDto.Password);
                await _userRepository.Register(user);
                var responseDto = _mapper.Map<RegisterResponseDto>(user);
                _logger.LogInformation("Utilisateur enregistré avec succès : {Email}", user.Email);
                return responseDto;
            }
            catch (WeakPasswordException)
            {
                _logger.LogError("Erreur WeakPasswordException lors de l'enregistrement de l'utilisateur : {Email}", request.registerRequestDto.Email);
                throw; 
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Erreur ApplicationException lors de l'enregistrement de l'utilisateur : {Email}", request.registerRequestDto.Email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'enregistrement de l'utilisateur : {Email}", request.registerRequestDto.Email);
                throw new ApplicationException("Une erreur est survenue lors de l'enregistrement de l'utilisateur.", ex);
            }
        }
    }
}
