using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Domain.Entities;

namespace UserService.Application.Authentication;

public class JwtTokenService : IJwtTokenService {
    
    readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options) {
        _options = options.Value;
    }

    public string CreateToken(User user) {
        List<Claim> claims = [
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (ClaimTypes.Name, user.Name),
                new (ClaimTypes.Role, user.Role.ToString())
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_options.Key));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}
