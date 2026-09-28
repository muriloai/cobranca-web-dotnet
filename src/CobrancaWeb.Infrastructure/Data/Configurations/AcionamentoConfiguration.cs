using CobrancaWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CobrancaWeb.Infrastructure.Data.Configurations;

public sealed class AcionamentoConfiguration : IEntityTypeConfiguration<Acionamento>
{
    public void Configure(EntityTypeBuilder<Acionamento> builder)
    {
        // O tipo é armazenado como inteiro e limitado aos valores do enum.
        builder.ToTable("Acionamentos", table =>
            table.HasCheckConstraint("CK_Acionamentos_Tipo", "[Tipo] IN (1, 2, 3)"));
        builder.HasKey(item => item.Id).HasName("PK_Acionamentos");
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Tipo).HasConversion<int>();
        builder.Property(item => item.Descricao).HasMaxLength(1000).IsRequired();

        // O histórico de contatos é consultado por contrato, do mais recente ao mais antigo.
        builder.HasIndex(item => new { item.ContratoId, item.RealizadoEm })
            .IsDescending(false, true).HasDatabaseName("IX_Acionamentos_Contrato_Data");

        builder.HasOne(item => item.Contrato).WithMany(item => item.Acionamentos)
            .HasForeignKey(item => item.ContratoId)
            // Nenhum contato deve ser removido por exclusão automática do contrato.
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Acionamentos_Contratos");

        builder.HasOne(item => item.Usuario).WithMany()
            .HasForeignKey(item => item.UsuarioId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_Acionamentos_Usuarios");
    }
}
