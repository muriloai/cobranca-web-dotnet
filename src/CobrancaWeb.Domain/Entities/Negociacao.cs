using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Domain.Entities;

public sealed class Negociacao : BaseEntity
{
    public Guid ContratoId { get; set; }
    public Contrato? Contrato { get; set; }
    public DateTime DataAcordo { get; set; } = DateTime.UtcNow;
    public decimal ValorAcordado { get; set; }
    public decimal DescontoPercentual { get; set; }
    public int QuantidadeParcelas { get; set; }
    public StatusNegociacao Status { get; private set; } = StatusNegociacao.Ativa;
    public List<ParcelaPagamento> Parcelas { get; set; } = [];

    public void RegistrarPagamento(Guid parcelaId, DateTime pagoEm)
    {
        if (Status != StatusNegociacao.Ativa)
        {
            throw new InvalidOperationException("A negociação não está ativa.");
        }

        if (Contrato is null)
        {
            throw new InvalidOperationException("O contrato da negociação precisa estar carregado.");
        }

        if (Contrato.Status != StatusContrato.Negociado)
        {
            throw new InvalidOperationException("O contrato não está negociado.");
        }

        var parcela = Parcelas.SingleOrDefault(item => item.Id == parcelaId)
            ?? throw new KeyNotFoundException("Parcela não encontrada.");

        parcela.RegistrarPagamento(pagoEm);

        if (Parcelas.All(item => item.PagoEm is not null))
        {
            Status = StatusNegociacao.Concluida;
            Contrato.Liquidar();
        }
    }
}
