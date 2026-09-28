namespace CobrancaWeb.Domain.Reports;

public sealed record DashboardResumo(
    long ContratosInadimplentes,
    decimal ValorInadimplente,
    decimal ValorRecuperado,
    long AcordosAtivos);
