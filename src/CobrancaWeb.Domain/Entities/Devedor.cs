namespace CobrancaWeb.Domain.Entities;

public sealed class Devedor : BaseEntity
{
    public string Nome { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public List<Contrato> Contratos { get; set; } = [];
}
