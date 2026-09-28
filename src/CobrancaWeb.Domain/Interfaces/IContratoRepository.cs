using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Domain.Interfaces;

public interface IContratoRepository
{
    Task<Contrato?> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Contrato>> PorDevedorAsync(Guid devedorId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default);
    Task SalvarAsync(CancellationToken cancellationToken = default);
}
