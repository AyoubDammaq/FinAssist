    using AuthService.Domain.Interfaces;
    using AutoMapper;
    using MediatR;
    using Microsoft.Extensions.Logging;

    namespace AuthService.Application.Commands.UpdateProfile
    {
        public class UpdateProfileCommandHandler(IUserRepository userRepository, IMapper mapper, ILogger<UpdateProfileCommandHandler> logger) : IRequestHandler<UpdateProfileCommand, Unit>
        {
            public readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            public readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            private readonly ILogger<UpdateProfileCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("Début de la mise à jour du profil utilisateur avec l'ID {UserId}.", request.ProfileDto.Id);
                    var user = await _userRepository.GetById(request.ProfileDto.Id);
                    if (user == null)
                    {
                        _logger.LogWarning("Utilisateur avec l'ID {UserId} non trouvé.", request.ProfileDto.Id);
                        throw new KeyNotFoundException($"User with ID {request.ProfileDto.Id} not found.");
                    }
                    _mapper.Map(request.ProfileDto, user);
                    await _userRepository.Update(user);
                    _logger.LogInformation("Profil utilisateur avec l'ID {UserId} mis à jour avec succès.", request.ProfileDto.Id);
                    return Unit.Value;
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogError(ex, "Erreur lors de la mise à jour du profil utilisateur : utilisateur non trouvé (ID {UserId}).", request.ProfileDto.Id);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Une erreur est survenue lors de la mise à jour du profil utilisateur (ID {UserId}).", request.ProfileDto.Id);
                    throw new ApplicationException("An error occurred while updating the user profile.", ex);
                }
            }
        }
    }
