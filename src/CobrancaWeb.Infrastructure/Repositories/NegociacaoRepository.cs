using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
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

    public async Task AdicionarAsync(Negociacao negociacao, CancellationToken cancellationToken = default)
    {
        await Context.Negociacoes.AddAsync(negociacao, cancellationToken);
    }
}
