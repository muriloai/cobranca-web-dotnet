using CobrancaWeb.Application.DTOs;

namespace CobrancaWeb.Application.Interfaces;

public interface IDevedorService
{
    Task<DevedorDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DevedorDto?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DevedorDto>> BuscarAsync(string? termo, CancellationToken cancellationToken = default);
    Task<DevedorDto> CriarAsync(CriarDevedorDto dto, CancellationToken cancellationToken = default);
    Task<DevedorDto> AtualizarAsync(Guid id, AtualizarDevedorDto dto, CancellationToken cancellationToken = default);
}
