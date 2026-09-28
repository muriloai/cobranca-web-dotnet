using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
