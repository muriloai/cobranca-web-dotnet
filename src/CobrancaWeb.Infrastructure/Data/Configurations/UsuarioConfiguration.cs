using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        // O perfil usa os códigos do domínio e o banco rejeita outros valores.
        builder.ToTable("Usuarios", table =>
            table.HasCheckConstraint("CK_Usuarios_Perfil", "[Perfil] IN (1, 2)"));
        builder.HasKey(item => item.Id).HasName("PK_Usuarios");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Nome).HasMaxLength(180).IsRequired();
        builder.Property(item => item.Email).HasMaxLength(254).IsRequired();
        builder.Property(item => item.SenhaHash).HasMaxLength(255).IsRequired();
        builder.Property(item => item.Perfil).HasConversion<int>();
        builder.Property(item => item.Ativo).HasDefaultValue(true);
        builder.Property(item => item.CriadoEm).HasDefaultValueSql("SYSUTCDATETIME()");
        // O e-mail identifica o usuário no fluxo de autenticação.
        builder.HasIndex(item => item.Email).IsUnique().HasDatabaseName("UQ_Usuarios_Email");
    }
}
