using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Application.Validators;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using CobrancaWeb.Domain.Interfaces;
using FluentValidation;

namespace CobrancaWeb.Application.Services;

public sealed class NegociacaoService(
    INegociacaoRepository negociacaoRepository,
    IContratoRepository contratoRepository,
    IValidator<NegociacaoInputDto> negociacaoValidator) : INegociacaoService
{
    public async Task<NegociacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var negociacao = await negociacaoRepository.ObterAsync(id, cancellationToken);
        return negociacao is null ? null : MapearParaDto(negociacao);
    }

    public async Task<SimulacaoNegociacaoDto> SimularAsync(
        Guid contratoId,
        decimal descontoPercentual,
        int parcelas,
        bool jurosCompostos = false,
        DateTime? dataReferencia = null,
        CancellationToken cancellationToken = default)
    {
        var contrato = await contratoRepository.ObterAsync(contratoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contrato com ID '{contratoId}' não foi encontrado.");

        if (contrato.Status != StatusContrato.EmAberto)
        {
            throw new InvalidOperationException("Apenas contratos em aberto podem ser negociados.");
        }

        if (parcelas < 1 || parcelas > 36)
        {
            throw new ArgumentOutOfRangeException(nameof(parcelas), "A quantidade de parcelas deve ser entre 1 e 36.");
        }

        if (descontoPercentual < 0 || descontoPercentual > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(descontoPercentual), "O desconto percentual deve estar entre 0% e 100%.");
        }

        var dataRef = (dataReferencia ?? DateTime.UtcNow).Date;
        var diasAtraso = contrato.CalcularDiasAtraso(dataRef);

        if (!NegociacaoValidator.ValidarRegraDesconto(diasAtraso, descontoPercentual))
        {
            throw new InvalidOperationException(
                $"Para dívidas com menos de {NegociacaoValidator.LimiteDiasAtrasoDescontoEspecial} dias de atraso, o desconto máximo permitido é de {NegociacaoValidator.DescontoMaximoMenos90Dias}%.");
        }

        var valorCorrigido = contrato.CalcularValorCorrigido(dataRef, jurosCompostos);
        var valorDesconto = Math.Round(valorCorrigido * (descontoPercentual / 100m), 2, MidpointRounding.AwayFromZero);
        var valorFinal = Math.Round(valorCorrigido - valorDesconto, 2, MidpointRounding.AwayFromZero);

        var valorParcelaBase = Math.Round(valorFinal / parcelas, 2, MidpointRounding.AwayFromZero);
        var diferencaCentavos = valorFinal - (valorParcelaBase * parcelas);

        var parcelasEstimadas = new List<ParcelaSimuladaDto>(parcelas);
        var primeiroVencimento = dataRef.AddDays(30);

        for (var i = 1; i <= parcelas; i++)
        {
            var valorParcela = (i == 1) ? valorParcelaBase + diferencaCentavos : valorParcelaBase;
            var vencimento = primeiroVencimento.AddMonths(i - 1);
            parcelasEstimadas.Add(new ParcelaSimuladaDto(i, vencimento, valorParcela));
        }

        return new SimulacaoNegociacaoDto(
            contrato.Id,
            contrato.Numero,
            contrato.Devedor?.Nome,
            contrato.ValorOriginal,
            diasAtraso,
            valorCorrigido,
            descontoPercentual,
            valorDesconto,
            valorFinal,
            parcelas,
            valorParcelaBase,
            parcelasEstimadas);
    }

    public async Task<NegociacaoDto> EfetivarAsync(NegociacaoInputDto dto, CancellationToken cancellationToken = default)
    {
        await negociacaoValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var contrato = await contratoRepository.ObterAsync(dto.ContratoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contrato com ID '{dto.ContratoId}' não foi encontrado.");

        if (contrato.Status != StatusContrato.EmAberto)
        {
            throw new InvalidOperationException("Apenas contratos em aberto podem ser negociados.");
        }

        var hoje = DateTime.UtcNow.Date;
        var diasAtraso = contrato.CalcularDiasAtraso(hoje);

        if (!NegociacaoValidator.ValidarRegraDesconto(diasAtraso, dto.DescontoPercentual))
        {
            throw new InvalidOperationException(
                $"Para dívidas com menos de {NegociacaoValidator.LimiteDiasAtrasoDescontoEspecial} dias de atraso, o desconto máximo permitido é de {NegociacaoValidator.DescontoMaximoMenos90Dias}%.");
        }

        var valorCorrigido = contrato.CalcularValorCorrigido(hoje, dto.UsarJurosCompostos);
        var valorDesconto = Math.Round(valorCorrigido * (dto.DescontoPercentual / 100m), 2, MidpointRounding.AwayFromZero);
        var valorAcordado = Math.Round(valorCorrigido - valorDesconto, 2, MidpointRounding.AwayFromZero);

        var negociacao = new Negociacao
        {
            Id = Guid.NewGuid(),
            ContratoId = contrato.Id,
            Contrato = contrato,
            DataAcordo = DateTime.UtcNow,
            ValorAcordado = valorAcordado,
            DescontoPercentual = dto.DescontoPercentual,
            QuantidadeParcelas = dto.QuantidadeParcelas
        };

        var valorParcelaBase = Math.Round(valorAcordado / dto.QuantidadeParcelas, 2, MidpointRounding.AwayFromZero);
        var diferencaCentavos = valorAcordado - (valorParcelaBase * dto.QuantidadeParcelas);
        var primeiroVencimento = dto.PrimeiroVencimento?.Date ?? hoje.AddDays(30);

        for (var i = 1; i <= dto.QuantidadeParcelas; i++)
        {
            var valor = (i == 1) ? valorParcelaBase + diferencaCentavos : valorParcelaBase;
            var vencimento = primeiroVencimento.AddMonths(i - 1);

            negociacao.Parcelas.Add(new ParcelaPagamento
            {
                Id = Guid.NewGuid(),
                NegociacaoId = negociacao.Id,
                Numero = i,
                Vencimento = vencimento,
                Valor = valor
            });
        }

        contrato.MarcarComoNegociado();

        await negociacaoRepository.AdicionarAsync(negociacao, cancellationToken);
        await negociacaoRepository.SalvarAsync(cancellationToken);

        return MapearParaDto(negociacao);
    }

    public async Task<NegociacaoDto> RegistrarPagamentoParcelaAsync(
        Guid negociacaoId,
        Guid parcelaId,
        DateTime pagoEm,
        CancellationToken cancellationToken = default)
    {
        var negociacao = await negociacaoRepository.ObterAsync(negociacaoId, cancellationToken)
            ?? throw new KeyNotFoundException($"Negociação com ID '{negociacaoId}' não foi encontrada.");

        negociacao.RegistrarPagamento(parcelaId, pagoEm);

        await negociacaoRepository.SalvarAsync(cancellationToken);

        return MapearParaDto(negociacao);
    }

    private static NegociacaoDto MapearParaDto(Negociacao negociacao)
    {
        var parcelas = negociacao.Parcelas
            .OrderBy(p => p.Numero)
            .Select(p => new ParcelaDto(
                p.Id,
                p.Numero,
                p.Vencimento,
                p.Valor,
                p.PagoEm))
            .ToList();

        return new NegociacaoDto(
            negociacao.Id,
            negociacao.ContratoId,
            negociacao.Contrato?.Numero,
            negociacao.Contrato?.Devedor?.Nome,
            negociacao.DataAcordo,
            negociacao.ValorAcordado,
            negociacao.DescontoPercentual,
            negociacao.QuantidadeParcelas,
            negociacao.Status,
            parcelas);
    }
}
