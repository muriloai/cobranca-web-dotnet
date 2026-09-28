using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class ParcelaConfiguration : IEntityTypeConfiguration<ParcelaPagamento>
{
    public void Configure(EntityTypeBuilder<ParcelaPagamento> builder)
    {
        // Uma parcela precisa ter número e valor positivos.
        builder.ToTable("ParcelasPagamento", table =>
        {
            table.HasCheckConstraint("CK_Parcelas_Numero", "[Numero] > 0");
            table.HasCheckConstraint("CK_Parcelas_Valor", "[Valor] > 0");
        });
        builder.HasKey(item => item.Id).HasName("PK_ParcelasPagamento");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Vencimento).HasColumnType("date");
        builder.Property(item => item.Valor).HasPrecision(18, 2);

        // O número da parcela é único dentro de cada negociação.
        builder.HasIndex(item => new { item.NegociacaoId, item.Numero })
            .IsUnique().HasDatabaseName("UQ_Parcelas_NegociacaoNumero");

        builder.HasIndex(item => new { item.Vencimento, item.PagoEm })
            // O valor incluído evita leitura extra nas consultas de pagamentos.
            .IncludeProperties(item => item.Valor)
            .HasDatabaseName("IX_Parcelas_Vencimento");

        builder.HasOne(item => item.Negociacao).WithMany(item => item.Parcelas)
            .HasForeignKey(item => item.NegociacaoId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Parcelas_Negociacoes");
    }
}
