using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Application.DTOs;

public sealed record ContratoDto(
    Guid Id,
    Guid DevedorId,
    string? DevedorNome,
    string? DevedorDocumento,
    string Numero,
    decimal ValorOriginal,
    DateTime Vencimento,
    decimal TaxaJurosMensal,
    StatusContrato Status,
    int DiasAtraso,
    decimal ValorAtualizado,
    DateTime CriadoEm
);

public sealed record CriarContratoDto(
    Guid DevedorId,
    string Numero,
    decimal ValorOriginal,
    DateTime Vencimento,
    decimal TaxaJurosMensal
);

public sealed record AtualizacaoFinanceiraDto(
    Guid ContratoId,
    string NumeroContrato,
    int DiasAtraso,
    decimal TaxaJurosMensal,
    decimal ValorOriginal,
    decimal ValorJuros,
    decimal ValorTotalAtualizado,
    DateTime DataCalculo,
    bool JurosCompostos
);
