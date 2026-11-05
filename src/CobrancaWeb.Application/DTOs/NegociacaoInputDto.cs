using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Application.DTOs;

public sealed record NegociacaoInputDto(
    Guid ContratoId,
    decimal DescontoPercentual,
    int QuantidadeParcelas,
    DateTime? PrimeiroVencimento = null,
    bool UsarJurosCompostos = false
);

public sealed record NegociacaoDto(
    Guid Id,
    Guid ContratoId,
    string? NumeroContrato,
    string? DevedorNome,
    DateTime DataAcordo,
    decimal ValorAcordado,
    decimal DescontoPercentual,
    int QuantidadeParcelas,
    StatusNegociacao Status,
    IReadOnlyList<ParcelaDto> Parcelas
);

// Contrato congelado de leitura v1: mantém o total usado pelo consumidor legado.
public sealed record NegociacaoResumoDto(
    Guid Id,
    Guid ContratoId,
    string? NumeroContrato,
    string? DevedorNome,
    DateTime DataAcordo,
    decimal ValorAcordado,
    decimal DescontoPercentual,
    int QuantidadeParcelas,
    StatusNegociacao Status
);

public sealed record ParcelaDto(
    Guid Id,
    int Numero,
    DateTime Vencimento,
    decimal Valor,
    DateTime? PagoEm
);

public sealed record SimulacaoNegociacaoDto(
    Guid ContratoId,
    string NumeroContrato,
    string? DevedorNome,
    decimal ValorOriginal,
    int DiasAtraso,
    decimal ValorCorrigido,
    decimal DescontoPercentual,
    decimal ValorDesconto,
    decimal ValorFinal,
    int QuantidadeParcelas,
    decimal ValorParcela,
    IReadOnlyList<ParcelaSimuladaDto> ParcelasEstimadas
);

public sealed record ParcelaSimuladaDto(
    int Numero,
    DateTime Vencimento,
    decimal Valor
);

public sealed record RegistrarPagamentoDto(
    Guid ParcelaId,
    DateTime? DataPagamento = null
);
