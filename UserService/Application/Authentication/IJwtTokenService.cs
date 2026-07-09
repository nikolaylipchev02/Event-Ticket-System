using UserService.Domain.Entities;

namespace UserService.Application.Authentication;

public interface IJwtTokenService {
    public string CreateToken(User user);
}
