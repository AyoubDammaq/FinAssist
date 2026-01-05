using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Commands.DeleteUser
{
    public class DeleteUserCommandHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<DeleteUserCommand, Unit>
    {
        public readonly IUserRepository _userRepository = userRepository;
        public readonly IMapper _mapper = mapper;

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetById(request.Id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"Utilisateur avec l'ID {request.Id} non trouvé.");
                }
                await _userRepository.Delete(user);
                return Unit.Value;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Une erreur est survenue lors de la suppression de l'utilisateur.", ex);
            }
        }


    }
}
