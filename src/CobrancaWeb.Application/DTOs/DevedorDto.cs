namespace CobrancaWeb.Application.DTOs;

public sealed record DevedorDto(
    Guid Id,
    string Nome,
    string Documento,
    string? Email,
    string? Telefone,
    string? Endereco,
    DateTime CriadoEm,
    IReadOnlyList<ContratoResumoDto> Contratos
);

public sealed record ContratoResumoDto(
    Guid Id,
    string Numero,
    decimal ValorOriginal,
    DateTime Vencimento,
    string Status
);

public sealed record CriarDevedorDto(
    string Nome,
    string Documento,
    string? Email,
    string? Telefone,
    string? Endereco
);

public sealed record AtualizarDevedorDto(
    string Nome,
    string? Email,
    string? Telefone,
    string? Endereco
);
