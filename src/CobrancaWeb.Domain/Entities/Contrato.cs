using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Domain.Entities;

public sealed class Contrato : BaseEntity
{
    public Guid DevedorId { get; set; }
    public Devedor? Devedor { get; set; }
    public string Numero { get; set; } = string.Empty;
    public decimal ValorOriginal { get; set; }
    public DateTime Vencimento { get; set; }
    public decimal TaxaJurosMensal { get; set; } = 0.01m;
    public StatusContrato Status { get; private set; } = StatusContrato.EmAberto;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public List<Negociacao> Negociacoes { get; set; } = [];
    public List<Acionamento> Acionamentos { get; set; } = [];

    public int CalcularDiasAtraso(DateTime dataReferencia)
    {
        return Math.Max(0, (dataReferencia.Date - Vencimento.Date).Days);
    }

    public decimal CalcularValorCorrigido(DateTime dataReferencia, bool jurosCompostos = false)
    {
        if (ValorOriginal <= 0 || TaxaJurosMensal < 0 || TaxaJurosMensal > 1)
        {
            throw new InvalidOperationException("Os dados financeiros do contrato são inválidos.");
        }

        // Um mês financeiro equivale a 30 dias; a aplicação pode optar por juros simples ou compostos.
        var meses = CalcularDiasAtraso(dataReferencia) / 30m;
        var fator = jurosCompostos
            ? (decimal)Math.Pow(1 + (double)TaxaJurosMensal, (double)meses)
            : 1 + TaxaJurosMensal * meses;

        return Math.Round(ValorOriginal * fator, 2, MidpointRounding.AwayFromZero);
    }

    public void MarcarComoNegociado()
    {
        if (Status != StatusContrato.EmAberto)
        {
            throw new InvalidOperationException("Somente contratos em aberto podem ser negociados.");
        }

        Status = StatusContrato.Negociado;
    }

    public void Liquidar()
    {
        if (Status != StatusContrato.Negociado)
        {
            throw new InvalidOperationException("Somente contratos negociados podem ser liquidados.");
        }

        Status = StatusContrato.Liquidado;
    }
}
