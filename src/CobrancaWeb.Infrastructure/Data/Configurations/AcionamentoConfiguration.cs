using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class AcionamentoConfiguration : IEntityTypeConfiguration<Acionamento>
{
    public void Configure(EntityTypeBuilder<Acionamento> builder)
    {
        builder.ToTable("Acionamentos", table =>
            table.HasCheckConstraint("CK_Acionamentos_Tipo", "[Tipo] IN (1, 2, 3)"));
        builder.HasKey(item => item.Id).HasName("PK_Acionamentos");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Tipo).HasConversion<int>();
        builder.Property(item => item.Descricao).HasMaxLength(1000).IsRequired();
        builder.HasIndex(item => new { item.ContratoId, item.RealizadoEm })
            .IsDescending(false, true).HasDatabaseName("IX_Acionamentos_Contrato_Data");
        builder.HasOne(item => item.Contrato).WithMany(item => item.Acionamentos)
            .HasForeignKey(item => item.ContratoId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Acionamentos_Contratos");
        builder.HasOne(item => item.Usuario).WithMany()
            .HasForeignKey(item => item.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Acionamentos_Usuarios");
    }
}
