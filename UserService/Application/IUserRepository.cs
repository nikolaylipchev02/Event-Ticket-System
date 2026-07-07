using UserService.Domain.Entities;

namespace UserService.Application;

public interface IUserRepository {

    public Task Add(User user);

    public Task<User?> GetByEmail(string email);

}
