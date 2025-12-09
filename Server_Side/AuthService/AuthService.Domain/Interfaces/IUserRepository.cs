using AuthService.Domain.Models;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository
    {
        public Guid register (User user);
        public User? getByEmail (string email);
        public User? getByUsername (string username);
        public User? getById (Guid id);
        public void update (User user);
        public void deleteById (Guid id);
	}
}
 