using System.Data;
using CobrancaWeb.Domain.Enums;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Domain.Reports;
using CobrancaWeb.Infrastructure.Data;
using Dapper;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class RelatorioDapperRepository(DbConnectionFactory connectionFactory) : IRelatorioRepository
{
    public async Task<DashboardResumo> DashboardAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = connectionFactory.Criar();
        using var resultados = await connection.QueryMultipleAsync(new CommandDefinition(
            "dbo.sp_DashboardResumo",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        // A procedure retorna três conjuntos, nesta ordem: inadimplência, recuperação e acordos.
        var inadimplencia = await resultados.ReadSingleAsync<InadimplenciaRow>();
        var recuperacao = await resultados.ReadSingleAsync<RecuperacaoRow>();
        var acordos = await resultados.ReadSingleAsync<AcordosRow>();

        return new DashboardResumo(
            inadimplencia.ContratosInadimplentes,
            inadimplencia.ValorInadimplente,
            recuperacao.ValorRecuperado,
            acordos.AcordosAtivos);
    }

    public async Task<IReadOnlyList<CobrancaRelatorio>> CobrancasAsync(
        DateTime inicio, DateTime fim, CancellationToken cancellationToken = default)
    {
        if (inicio == default || fim == default || fim.Date < inicio.Date)
        {
            throw new ArgumentException("Informe um período válido para o relatório.");
        }

        await using var connection = connectionFactory.Criar();
        var linhas = await connection.QueryAsync<CobrancaRow>(new CommandDefinition(
            "dbo.sp_RelatorioCobrancas",
            new { Inicio = inicio.Date, Fim = fim.Date },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return linhas.Select(linha => new CobrancaRelatorio(
            linha.Id,
            linha.RealizadoEm,
            (TipoAcionamento)linha.Tipo,
            linha.Descricao,
            linha.Operador,
            linha.Devedor,
            linha.Contrato)).ToList();
    }

    public async Task<IReadOnlyList<DevedorResumo>> ResumoPorDevedorAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            WITH Inadimplencia AS (
                SELECT c.DevedorId, COUNT_BIG(*) AS ContratosEmAtraso,
                       SUM(c.ValorOriginal) AS ValorInadimplente
                FROM dbo.Contratos c
                WHERE c.Status = 1 AND c.Vencimento < CONVERT(date, SYSUTCDATETIME())
                GROUP BY c.DevedorId
            ), Recuperacao AS (
                SELECT c.DevedorId, SUM(p.Valor) AS ValorRecuperado
                FROM dbo.Contratos c
                JOIN dbo.Negociacoes n ON n.ContratoId = c.Id
                JOIN dbo.ParcelasPagamento p ON p.NegociacaoId = n.Id
                WHERE p.PagoEm IS NOT NULL
                GROUP BY c.DevedorId
            )
            SELECT TOP (100) d.Id AS DevedorId, d.Nome, d.Documento,
                   COALESCE(i.ContratosEmAtraso, 0) AS ContratosEmAtraso,
                   COALESCE(i.ValorInadimplente, 0) AS ValorInadimplente,
                   COALESCE(r.ValorRecuperado, 0) AS ValorRecuperado
            FROM dbo.Devedores d
            LEFT JOIN Inadimplencia i ON i.DevedorId = d.Id
            LEFT JOIN Recuperacao r ON r.DevedorId = d.Id
            WHERE i.DevedorId IS NOT NULL OR r.DevedorId IS NOT NULL
            ORDER BY COALESCE(i.ValorInadimplente, 0) DESC, d.Nome;
            """;

        await using var connection = connectionFactory.Criar();
        var linhas = await connection.QueryAsync<DevedorResumoRow>(new CommandDefinition(
            sql, cancellationToken: cancellationToken));

        return linhas.Select(linha => new DevedorResumo(
            linha.DevedorId,
            linha.Nome,
            linha.Documento,
            linha.ContratosEmAtraso,
            linha.ValorInadimplente,
            linha.ValorRecuperado)).ToList();
    }

    private sealed class InadimplenciaRow
    {
        public long ContratosInadimplentes { get; set; }
        public decimal ValorInadimplente { get; set; }
    }

    private sealed class RecuperacaoRow
    {
        public decimal ValorRecuperado { get; set; }
    }

    private sealed class AcordosRow
    {
        public long AcordosAtivos { get; set; }
    }

    private sealed class CobrancaRow
    {
        public Guid Id { get; set; }
        public DateTime RealizadoEm { get; set; }
        public int Tipo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
        public string Devedor { get; set; } = string.Empty;
        public string Contrato { get; set; } = string.Empty;
    }

    private sealed class DevedorResumoRow
    {
        public Guid DevedorId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public long ContratosEmAtraso { get; set; }
        public decimal ValorInadimplente { get; set; }
        public decimal ValorRecuperado { get; set; }
    }
}
