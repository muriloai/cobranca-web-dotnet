using CobrancaWeb.Application.DTOs;

namespace CobrancaWeb.Application.Interfaces;

public interface IContratoService
{
    Task<ContratoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContratoDto>> ObterPorDevedorAsync(Guid devedorId, CancellationToken cancellationToken = default);
    Task<ContratoDto> CriarAsync(CriarContratoDto dto, CancellationToken cancellationToken = default);
    Task<AtualizacaoFinanceiraDto> CalcularAtualizacaoAsync(
        Guid contratoId,
        DateTime? dataReferencia = null,
        bool jurosCompostos = false,
        CancellationToken cancellationToken = default);
}
