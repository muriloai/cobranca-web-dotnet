using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        // As restrições mantêm valor, taxa e status coerentes mesmo fora da aplicação.
        builder.ToTable("Contratos", table =>
        {
            table.HasCheckConstraint("CK_Contratos_Valor", "[ValorOriginal] > 0");
            table.HasCheckConstraint("CK_Contratos_Taxa", "[TaxaJurosMensal] >= 0 AND [TaxaJurosMensal] <= 1");
            table.HasCheckConstraint("CK_Contratos_Status", "[Status] IN (1, 2, 3)");
        });
        builder.HasKey(item => item.Id).HasName("PK_Contratos");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Numero).HasMaxLength(50).IsRequired();

        // Valores financeiros usam decimal e vencimento guarda apenas a data.
        builder.Property(item => item.ValorOriginal).HasPrecision(18, 2);
        builder.Property(item => item.Vencimento).HasColumnType("date");
        builder.Property(item => item.TaxaJurosMensal).HasPrecision(9, 6).HasDefaultValue(0.01m);
        builder.Property(item => item.Status).HasConversion<int>()
            .HasDefaultValue(StatusContrato.EmAberto)
            .HasSentinel(StatusContrato.EmAberto);
        builder.Property(item => item.CriadoEm).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(item => item.Numero).IsUnique().HasDatabaseName("UQ_Contratos_Numero");

        // Os índices cobrem a busca por devedor e a fila de contratos vencidos.
        builder.HasIndex(item => new { item.DevedorId, item.Status })
            .IncludeProperties(item => new { item.Vencimento, item.ValorOriginal })
            .HasDatabaseName("IX_Contratos_Devedor_Status");
        builder.HasIndex(item => new { item.Status, item.Vencimento })
            .IncludeProperties(item => item.ValorOriginal)
            .HasDatabaseName("IX_Contratos_Status_Vencimento");

        builder.HasOne(item => item.Devedor).WithMany(item => item.Contratos)
            .HasForeignKey(item => item.DevedorId)
            // Excluir um devedor não deve apagar seus contratos de cobrança.
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Contratos_Devedores");
    }
}
