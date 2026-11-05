using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class NegociacaoRepository(AppDbContext context) : RepositoryBase(context), INegociacaoRepository
{
    public Task<Negociacao?> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Negociacoes
            .Include(item => item.Parcelas)
            .Include(item => item.Contrato)
            .ThenInclude(contrato => contrato!.Devedor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task EfetivarAsync(
        Negociacao negociacao,
        DateTime primeiroVencimento,
        CancellationToken cancellationToken = default)
    {
        return ExecutarProcedureAsync(
            "dbo.sp_EfetivarNegociacao",
            new
            {
                NegociacaoId = negociacao.Id,
                negociacao.ContratoId,
                negociacao.DataAcordo,
                negociacao.ValorAcordado,
                negociacao.DescontoPercentual,
                negociacao.QuantidadeParcelas,
                PrimeiroVencimento = primeiroVencimento.Date
            },
            cancellationToken);
    }

    public Task RegistrarPagamentoAsync(
        Guid negociacaoId,
        Guid parcelaId,
        DateTime pagoEm,
        CancellationToken cancellationToken = default)
    {
        return ExecutarProcedureAsync(
            "dbo.sp_RegistrarPagamentoParcela",
            new { NegociacaoId = negociacaoId, ParcelaId = parcelaId, PagoEm = pagoEm },
            cancellationToken);
    }

    private async Task ExecutarProcedureAsync(string nome, object parametros, CancellationToken cancellationToken)
    {
        var connection = Context.Database.GetDbConnection();
        var fecharConexao = connection.State != ConnectionState.Open;
        if (fecharConexao)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await connection.ExecuteAsync(new CommandDefinition(
                nome,
                parametros,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken));
        }
        finally
        {
            if (fecharConexao)
            {
                await connection.CloseAsync();
            }
        }
    }
}
