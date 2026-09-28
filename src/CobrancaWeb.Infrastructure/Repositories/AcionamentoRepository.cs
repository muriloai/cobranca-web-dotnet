using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class AcionamentoRepository(AppDbContext context) : RepositoryBase(context), IAcionamentoRepository
{
    public async Task<IReadOnlyList<Acionamento>> PorContratoAsync(Guid contratoId, CancellationToken cancellationToken = default)
    {
        return await Context.Acionamentos.AsNoTracking()
            .Where(item => item.ContratoId == contratoId)
            .OrderByDescending(item => item.RealizadoEm)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Acionamento acionamento, CancellationToken cancellationToken = default)
    {
        await Context.Acionamentos.AddAsync(acionamento, cancellationToken);
    }
}
