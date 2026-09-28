using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class DevedorConfiguration : IEntityTypeConfiguration<Devedor>
{
    public void Configure(EntityTypeBuilder<Devedor> builder)
    {
        builder.ToTable("Devedores", table =>
            table.HasCheckConstraint("CK_Devedores_Documento", "LEN([Documento]) IN (11, 14) AND [Documento] NOT LIKE '%[^0-9]%'"));
        builder.HasKey(item => item.Id).HasName("PK_Devedores");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Nome).HasMaxLength(180).IsRequired();
        builder.Property(item => item.Documento).HasMaxLength(14).IsUnicode(false).IsRequired();
        builder.Property(item => item.Email).HasMaxLength(254);
        builder.Property(item => item.Telefone).HasMaxLength(20).IsUnicode(false);
        builder.Property(item => item.Endereco).HasMaxLength(300);
        builder.Property(item => item.CriadoEm).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasIndex(item => item.Documento).IsUnique().HasDatabaseName("UQ_Devedores_Documento");
    }
}
