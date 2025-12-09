using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> Register(User user);
        Task<List<User>> GetAll();
        Task<User?> GetByEmail(string email);
        Task<User?> GetByUsername(string username);
        Task<User?> GetById(Guid id);
        Task Update(User user);
        Task DeleteById(Guid id);
    }
}