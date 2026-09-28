using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class DevedorRepository(AppDbContext context) : RepositoryBase(context), IDevedorRepository
{
    public Task<Devedor?> ObterAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Devedores.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
    }

    public Task<Devedor?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        return Context.Devedores.FirstOrDefaultAsync(item => item.Documento == documento, cancellationToken);
    }

    public async Task<IReadOnlyList<Devedor>> BuscarAsync(string? termo, CancellationToken cancellationToken = default)
    {
        var query = Context.Devedores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(termo))
        {
            var busca = termo.Trim();
            query = query.Where(item => item.Nome.Contains(busca) || item.Documento.Contains(busca));
        }

        return await query.OrderBy(item => item.Nome).Take(100).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(Devedor devedor, CancellationToken cancellationToken = default)
    {
        await Context.Devedores.AddAsync(devedor, cancellationToken);
    }
}
