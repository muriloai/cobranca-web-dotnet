using CobrancaWeb.Domain.Reports;

namespace CobrancaWeb.Domain.Interfaces;

public interface IRelatorioRepository
{
    Task<DashboardResumo> DashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CobrancaRelatorio>> CobrancasAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DevedorResumo>> ResumoPorDevedorAsync(CancellationToken cancellationToken = default);
}
