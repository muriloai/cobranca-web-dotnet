namespace CobrancaWeb.Domain.Reports;

public sealed record DevedorResumo(
    Guid DevedorId,
    string Nome,
    string Documento,
    long ContratosEmAtraso,
    decimal ValorInadimplente,
    decimal ValorRecuperado);
