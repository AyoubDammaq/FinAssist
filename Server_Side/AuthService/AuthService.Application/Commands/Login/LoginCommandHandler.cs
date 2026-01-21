using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AuthService.Application.Utils;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Commands.Login
{
    public class LoginCommandHandler(IUserRepository userRepository, IPasswordManagement passwordManagement, ITokenManagement tokenManagement, IMapper mapper) : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        public readonly IUserRepository _userRepository = userRepository;   
        public readonly IPasswordManagement _passwordManagement = passwordManagement;
        public readonly ITokenManagement _tokenManagement = tokenManagement;
        public readonly IMapper _mapper = mapper;

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByEmail(request.loginRequestDto.Email);
                var isPasswordValid = user != null && await _passwordManagement.VerifyPassword(request.loginRequestDto.Password, user.PasswordHash, user);
                if (user == null || !isPasswordValid)
                {
                    throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe incorrect.");
                }
                var token = await _tokenManagement.GenerateToken(user);
                var refreshToken = await _tokenManagement.GenerateRefreshToken();
                return new LoginResponseDto
                {
                    AccessToken = token,
                    RefreshToken = refreshToken
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Une erreur est survenue lors de la tentative de connexion.", ex);
            }
        }
    }
}
