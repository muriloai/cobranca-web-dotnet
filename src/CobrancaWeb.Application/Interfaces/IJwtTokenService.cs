using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Application.Interfaces;

public interface IJwtTokenService
{
    string GerarToken(Usuario usuario);
    string GerarToken(Guid usuarioId, string nome, string email, PerfilUsuario perfil);
}
