using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace CobrancaWeb.Infrastructure.Auth;

public sealed class JwtTokenService(JwtOptions options) : IJwtTokenService
{
    private readonly JwtOptions _options = options ?? throw new ArgumentNullException(nameof(options));

    public string GerarToken(Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        return GerarToken(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
    }

    public string GerarToken(Guid usuarioId, string nome, string email, PerfilUsuario perfil)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(ClaimTypes.Name, nome),
            new("name", nome),
            new(ClaimTypes.Email, email),
            new("email", email),
            new(ClaimTypes.Role, perfil.ToString()), // "Operador" ou "Supervisor"
            new("role", perfil.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpiracaoEmMinutos),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
