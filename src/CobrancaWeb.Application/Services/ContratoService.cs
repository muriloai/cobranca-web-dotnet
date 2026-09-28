using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;

namespace CobrancaWeb.Application.Services;

public sealed class ContratoService(
    IContratoRepository contratoRepository,
    IDevedorRepository devedorRepository) : IContratoService
{
    public async Task<ContratoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepository.ObterAsync(id, cancellationToken);
        return contrato is null ? null : MapearParaDto(contrato, DateTime.UtcNow);
    }

    public async Task<IReadOnlyList<ContratoDto>> ObterPorDevedorAsync(Guid devedorId, CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.UtcNow;
        var contratos = await contratoRepository.PorDevedorAsync(devedorId, cancellationToken);
        return contratos.Select(c => MapearParaDto(c, hoje)).ToList();
    }

    public async Task<ContratoDto> CriarAsync(CriarContratoDto dto, CancellationToken cancellationToken = default)
    {
        var devedor = await devedorRepository.ObterAsync(dto.DevedorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Devedor com ID '{dto.DevedorId}' não foi encontrado.");

        if (string.IsNullOrWhiteSpace(dto.Numero))
        {
            throw new ArgumentException("O número do contrato é obrigatório.", nameof(dto));
        }

        if (dto.ValorOriginal <= 0)
        {
            throw new ArgumentException("O valor original deve ser maior que zero.", nameof(dto));
        }

        if (dto.TaxaJurosMensal < 0 || dto.TaxaJurosMensal > 1)
        {
            throw new ArgumentException("A taxa de juros mensal deve estar entre 0.00 e 1.00 (ex: 0.015 para 1,5% ao mês).", nameof(dto));
        }

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            DevedorId = dto.DevedorId,
            Devedor = devedor,
            Numero = dto.Numero.Trim(),
            ValorOriginal = dto.ValorOriginal,
            Vencimento = dto.Vencimento.Date,
            TaxaJurosMensal = dto.TaxaJurosMensal,
            CriadoEm = DateTime.UtcNow
        };

        await contratoRepository.AdicionarAsync(contrato, cancellationToken);
        await contratoRepository.SalvarAsync(cancellationToken);

        return MapearParaDto(contrato, DateTime.UtcNow);
    }

    public async Task<AtualizacaoFinanceiraDto> CalcularAtualizacaoAsync(
        Guid contratoId,
        DateTime? dataReferencia = null,
        bool jurosCompostos = false,
        CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepository.ObterAsync(contratoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contrato com ID '{contratoId}' não foi encontrado.");

        var dataRef = (dataReferencia ?? DateTime.UtcNow).Date;
        var diasAtraso = contrato.CalcularDiasAtraso(dataRef);
        var valorAtualizado = contrato.CalcularValorCorrigido(dataRef, jurosCompostos);
        var valorJuros = Math.Max(0, valorAtualizado - contrato.ValorOriginal);

        return new AtualizacaoFinanceiraDto(
            contrato.Id,
            contrato.Numero,
            diasAtraso,
            contrato.TaxaJurosMensal,
            contrato.ValorOriginal,
            valorJuros,
            valorAtualizado,
            dataRef,
            jurosCompostos);
    }

    private static ContratoDto MapearParaDto(Contrato contrato, DateTime dataReferencia)
    {
        var diasAtraso = contrato.CalcularDiasAtraso(dataReferencia);
        var valorAtualizado = contrato.CalcularValorCorrigido(dataReferencia, false);

        return new ContratoDto(
            contrato.Id,
            contrato.DevedorId,
            contrato.Devedor?.Nome,
            contrato.Devedor?.Documento,
            contrato.Numero,
            contrato.ValorOriginal,
            contrato.Vencimento,
            contrato.TaxaJurosMensal,
            contrato.Status,
            diasAtraso,
            valorAtualizado,
            contrato.CriadoEm);
    }
}
