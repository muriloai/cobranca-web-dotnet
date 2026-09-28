using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface IAcionamentoRepository
{
    Task<IReadOnlyList<Acionamento>> PorContratoAsync(Guid contratoId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Acionamento acionamento, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
