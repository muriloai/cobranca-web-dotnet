namespace CobrancaWeb.Application.DTOs;

public sealed record DashboardDto(
    long ContratosInadimplentes,
    decimal ValorInadimplente,
    decimal ValorRecuperado,
    long AcordosAtivos,
    decimal TaxaRecuperacaoPercentual
);
