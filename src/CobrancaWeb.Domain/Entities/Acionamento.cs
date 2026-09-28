using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Domain.Entities;

public sealed class Acionamento : BaseEntity
{
    public Guid ContratoId { get; set; }
    public Contrato? Contrato { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoAcionamento Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DateTime RealizadoEm { get; set; } = DateTime.UtcNow;
}
