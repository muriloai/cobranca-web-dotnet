using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class NegociacaoConfiguration : IEntityTypeConfiguration<Negociacao>
{
    public void Configure(EntityTypeBuilder<Negociacao> builder)
    {
        builder.ToTable("Negociacoes", table =>
        {
            table.HasCheckConstraint("CK_Negociacoes_Valor", "[ValorAcordado] > 0");
            table.HasCheckConstraint("CK_Negociacoes_Desconto", "[DescontoPercentual] BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_Negociacoes_Parcelas", "[QuantidadeParcelas] BETWEEN 1 AND 36");
            table.HasCheckConstraint("CK_Negociacoes_Status", "[Status] IN (1, 2, 3)");
        });
        builder.HasKey(item => item.Id).HasName("PK_Negociacoes");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.ValorAcordado).HasPrecision(18, 2);
        builder.Property(item => item.DescontoPercentual).HasPrecision(5, 2);
        builder.Property(item => item.Status).HasConversion<int>();
        builder.HasIndex(item => new { item.ContratoId, item.Status })
            .HasDatabaseName("IX_Negociacoes_Contrato_Status");
        builder.HasIndex(item => item.ContratoId)
            .IsUnique().HasFilter("[Status] = 1")
            .HasDatabaseName("UX_Negociacoes_Contrato_Ativa");
        builder.HasOne(item => item.Contrato).WithMany(item => item.Negociacoes)
            .HasForeignKey(item => item.ContratoId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Negociacoes_Contratos");
    }
}
