using CobrancaWeb.Application.DTOs;

namespace CobrancaWeb.Application.Interfaces;

public interface INegociacaoService
{
    Task<NegociacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SimulacaoNegociacaoDto> SimularAsync(
        Guid contratoId,
        decimal descontoPercentual,
        int parcelas,
        bool jurosCompostos = false,
        DateTime? dataReferencia = null,
        CancellationToken cancellationToken = default);
    Task<NegociacaoDto> EfetivarAsync(NegociacaoInputDto dto, CancellationToken cancellationToken = default);
    Task<NegociacaoDto> RegistrarPagamentoParcelaAsync(
        Guid negociacaoId,
        Guid parcelaId,
        DateTime pagoEm,
        CancellationToken cancellationToken = default);
}
