using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Commands.Register
{
    public class RegisterCommandHandler(IUserRepository userRepository, IPasswordManagement passwordManagement, IMapper mapper) : IRequestHandler<RegisterCommand, RegisterResponseDto>
    {
        public readonly IUserRepository _userRepository = userRepository;
        public readonly IPasswordManagement _passwordManagement = passwordManagement;
        public readonly IMapper _mapper = mapper;

        public async Task<RegisterResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmail(request.registerRequestDto.Email);
            if (existingUser != null)
            {
                throw new ApplicationException($"Le nom d'utilisateur {request.registerRequestDto.Email} est déjà pris.");
            }
            
            try
            {
                var user = _mapper.Map<User>(request.registerRequestDto);
                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, request.registerRequestDto.Password);
                await _userRepository.Register(user);
                var responseDto = _mapper.Map<RegisterResponseDto>(user);
                return responseDto;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Une erreur est survenue lors de l'enregistrement de l'utilisateur.", ex);
            }
        }
    }
}
