using CobrancaWeb.Infrastructure.Data;

namespace CobrancaWeb.Infrastructure.Repositories;

public abstract class RepositoryBase(AppDbContext context)
{
    protected AppDbContext Context { get; } = context;

    public async Task SalvarAsync(CancellationToken cancellationToken = default)
    {
        await Context.SaveChangesAsync(cancellationToken);
    }
}
