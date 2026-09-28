using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class FaixaJurosConfiguration : IEntityTypeConfiguration<FaixaJuros>
{
    public void Configure(EntityTypeBuilder<FaixaJuros> builder)
    {
        builder.ToTable("FaixasJuros", table =>
        {
            table.HasCheckConstraint("CK_FaixasJuros_Dias", "[DiasMinimos] >= 0");
            table.HasCheckConstraint("CK_FaixasJuros_Taxa", "[TaxaMensal] >= 0 AND [TaxaMensal] <= 1");
        });
        builder.HasKey(item => item.DiasMinimos).HasName("PK_FaixasJuros");
        builder.Property(item => item.DiasMinimos).ValueGeneratedNever();
        builder.Property(item => item.TaxaMensal).HasPrecision(9, 6);
    }
}
