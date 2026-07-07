using UserService.Domain.Entities;

namespace UserService.Application;

public interface IUserRepository {

    public Task<User?> GetByEmail(string email);

}
