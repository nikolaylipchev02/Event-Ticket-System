using UserService.Application;
using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure;

public class UserRepository : IUserRepository {
    readonly UserServiceDbContext _userServiceDbContext;

    public UserRepository(UserServiceDbContext userServiceDbContext) {
        _userServiceDbContext = userServiceDbContext;
    }

    public async Task Add(User user, CancellationToken cancellationToken) {
        _userServiceDbContext.Users.Add(user);
        await _userServiceDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<User?> GetByEmail(string email, CancellationToken cancellationToken) {
        return await _userServiceDbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}