using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CobrancaWeb.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    #region Domain.Entities
    public DbSet<Devedor> Devedores => Set<Devedor>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<Negociacao> Negociacoes => Set<Negociacao>();
    public DbSet<ParcelaPagamento> ParcelasPagamento => Set<ParcelaPagamento>();
    public DbSet<Acionamento> Acionamentos => Set<Acionamento>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<FaixaJuros> FaixasJuros => Set<FaixaJuros>();
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Carregando as Configurations de Infra.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
