using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Repositories;

public sealed class UsuarioRepository(AppDbContext context) : RepositoryBase(context), IUsuarioRepository
{
    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailNormalizado = email.Trim().ToLowerInvariant();
        return Context.Usuarios.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNormalizado, cancellationToken);
    }

    public async Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await Context.Usuarios.AddAsync(usuario, cancellationToken);
    }
}
