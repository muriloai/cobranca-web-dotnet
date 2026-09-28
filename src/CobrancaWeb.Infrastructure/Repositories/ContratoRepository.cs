using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class ContratoRepository(AppDbContext context) : RepositoryBase(context), IContratoRepository
{
    public Task<Contrato?> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Contratos.Include(item => item.Devedor)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Contrato>> PorDevedorAsync(Guid devedorId, CancellationToken cancellationToken = default)
    {
        return await Context.Contratos.AsNoTracking()
            .Where(item => item.DevedorId == devedorId)
            .OrderBy(item => item.Vencimento)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Contrato contrato, CancellationToken cancellationToken = default)
    {
        await Context.Contratos.AddAsync(contrato, cancellationToken);
    }
}
