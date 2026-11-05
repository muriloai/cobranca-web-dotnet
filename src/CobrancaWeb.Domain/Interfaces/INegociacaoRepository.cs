using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface INegociacaoRepository
{
    Task<Negociacao?> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task EfetivarAsync(Negociacao negociacao, DateTime primeiroVencimento, CancellationToken cancellationToken = default);
    Task RegistrarPagamentoAsync(Guid negociacaoId, Guid parcelaId, DateTime pagoEm, CancellationToken cancellationToken = default);
}
