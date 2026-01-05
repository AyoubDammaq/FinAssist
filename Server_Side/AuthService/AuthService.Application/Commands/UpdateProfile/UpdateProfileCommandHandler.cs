using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<UpdateProfileCommand, Unit>
    {
        public readonly IUserRepository _userRepository = userRepository;
        public readonly IMapper _mapper = mapper;

        public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetById(request.ProfileDto.Id);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User with ID {request.ProfileDto.Id} not found.");
                }
                _mapper.Map(request.ProfileDto, user);
                await _userRepository.Update(user);
                return Unit.Value;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while updating the user profile.", ex);
            }
        }
    }
}
