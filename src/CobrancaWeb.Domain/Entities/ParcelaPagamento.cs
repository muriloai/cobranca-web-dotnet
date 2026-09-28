namespace CobrancaWeb.Domain.Entities;

public sealed class ParcelaPagamento : BaseEntity
{
    public Guid NegociacaoId { get; set; }
    public Negociacao? Negociacao { get; set; }
    public int Numero { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal Valor { get; set; }
    public DateTime? PagoEm { get; private set; }

    public void RegistrarPagamento(DateTime pagoEm)
    {
        if (pagoEm == default)
        {
            throw new ArgumentException("A data do pagamento é obrigatória.", nameof(pagoEm));
        }

        if (PagoEm is not null)
        {
            throw new InvalidOperationException("A parcela já foi paga.");
        }

        PagoEm = pagoEm;
    }
}
