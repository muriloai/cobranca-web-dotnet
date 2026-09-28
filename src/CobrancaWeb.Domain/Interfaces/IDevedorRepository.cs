using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface IDevedorRepository
{
    Task<Devedor?> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Devedor?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Devedor>> BuscarAsync(string? termo, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Devedor devedor, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
