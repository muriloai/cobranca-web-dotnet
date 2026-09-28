using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface INegociacaoRepository
{
    Task<Negociacao?> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Negociacao negociacao, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
