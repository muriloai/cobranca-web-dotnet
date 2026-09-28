using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Devedor> Devedores => Set<Devedor>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Negociacao> Negociacoes => Set<Negociacao>();
    public DbSet<ParcelaPagamento> ParcelasPagamento => Set<ParcelaPagamento>();
    public DbSet<Acionamento> Acionamentos => Set<Acionamento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<FaixaJuros> FaixasJuros => Set<FaixaJuros>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
