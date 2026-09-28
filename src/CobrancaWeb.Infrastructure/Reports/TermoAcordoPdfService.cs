using System.Globalization;
using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CobrancaWeb.Infrastructure.Reports;

public sealed class TermoAcordoPdfService : ITermoAcordoPdfService
{
    private static readonly CultureInfo CulturaBr = new("pt-BR");

    static TermoAcordoPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GerarTermoAcordoPdf(Negociacao negociacao)
    {
        ArgumentNullException.ThrowIfNull(negociacao);

        var devedor = negociacao.Contrato?.Devedor;
        var devedorNome = devedor?.Nome ?? "Não informado";
        var devedorDoc = devedor?.Documento ?? "Não informado";
        var devedorEmail = devedor?.Email;
        var devedorTel = devedor?.Telefone;
        var valorOriginal = negociacao.Contrato?.ValorOriginal ?? negociacao.ValorAcordado;
        var diasAtraso = negociacao.Contrato?.CalcularDiasAtraso(negociacao.DataAcordo) ?? 0;

        var parcelasDto = negociacao.Parcelas
            .OrderBy(p => p.Numero)
            .Select(p => new ParcelaDto(p.Id, p.Numero, p.Vencimento, p.Valor, p.PagoEm))
            .ToList();

        var negociacaoDto = new NegociacaoDto(
            negociacao.Id,
            negociacao.ContratoId,
            negociacao.Contrato?.Numero,
            devedorNome,
            negociacao.DataAcordo,
            negociacao.ValorAcordado,
            negociacao.DescontoPercentual,
            negociacao.QuantidadeParcelas,
            negociacao.Status,
            parcelasDto);

        return GerarTermoAcordoPdf(
            negociacaoDto,
            devedorNome,
            devedorDoc,
            devedorEmail,
            devedorTel,
            valorOriginal,
            diasAtraso);
    }

    public byte[] GerarTermoAcordoPdf(
        NegociacaoDto negociacao,
        string devedorNome,
        string devedorDocumento,
        string? devedorEmail,
        string? devedorTelefone,
        decimal valorOriginal,
        int diasAtraso)
    {
        ArgumentNullException.ThrowIfNull(negociacao);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36, Unit.Point);
                page.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken3));

                page.Header().Element(c => ComporCabecalho(c, negociacao));
                page.Content().Element(c => ComporConteudo(c, negociacao, devedorNome, devedorDocumento, devedorEmail, devedorTelefone, valorOriginal, diasAtraso));
                page.Footer().Element(ComporRodape);
            });
        });

        return documento.GeneratePdf();
    }

    private static void ComporCabecalho(IContainer container, NegociacaoDto negociacao)
    {
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(10).Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("COBRANÇA WEB").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                col.Item().Text("Assessoria em Recuperação de Ativos e Crédito").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("CNPJ: 10.234.567/0001-89 | Av. Paulista, 1000 - São Paulo, SP").FontSize(8).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(180).Column(col =>
            {
                col.Item().AlignRight().Text($"ACORDO Nº {negociacao.Id.ToString()[..8].ToUpperInvariant()}").FontSize(11).Bold();
                col.Item().AlignRight().Text($"Emissão: {negociacao.DataAcordo:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Darken1);
                col.Item().AlignRight().Text($"Status: {negociacao.Status}").FontSize(8).Bold().FontColor(Colors.Green.Darken2);
            });
        });
    }

    private static void ComporConteudo(
        IContainer container,
        NegociacaoDto negociacao,
        string devedorNome,
        string devedorDoc,
        string? devedorEmail,
        string? devedorTel,
        decimal valorOriginal,
        int diasAtraso)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Título
            col.Item().AlignCenter().PaddingBottom(10).Text("INSTRUMENTO PARTICULAR DE CONFISSÃO E PARCELAMENTO DE DÍVIDA")
                .FontSize(11).Bold().FontColor(Colors.Blue.Darken4);

            // Qualificação das Partes
            col.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(c =>
            {
                c.Item().Text("1. QUALIFICAÇÃO DAS PARTES").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                c.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("CREDOR: ").Bold();
                    t.Span("CobrançaWeb Assessoria Financeira e Cobrança Ltda., inscrita no CNPJ sob o nº 10.234.567/0001-89.");
                });
                c.Item().PaddingTop(2).Text(t =>
                {
                    t.Span("DEVEDOR(A): ").Bold();
                    t.Span($"{devedorNome}, inscrito(a) sob o CPF/CNPJ nº {devedorDoc}");
                    if (!string.IsNullOrWhiteSpace(devedorEmail))
                    {
                        t.Span($", e-mail: {devedorEmail}");
                    }
                    if (!string.IsNullOrWhiteSpace(devedorTel))
                    {
                        t.Span($", telefone: {devedorTel}");
                    }
                    t.Span(".");
                });
            });

            // Resumo da Dívida e Condições
            col.Item().PaddingTop(10).Background(Colors.Grey.Lighten5).Padding(8).Column(c =>
            {
                c.Item().Text("2. DISCRIMINAÇÃO DO DÉBITO E CONDIÇÕES ACORDADAS").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
                c.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Column(sub =>
                    {
                        sub.Item().Text($"Contrato de Origem: {negociacao.NumeroContrato ?? "CT-001"}").FontSize(8);
                        sub.Item().Text($"Valor Original: {valorOriginal.ToString("C", CulturaBr)}").FontSize(8);
                        sub.Item().Text($"Tempo em Atraso: {diasAtraso} dias").FontSize(8);
                    });

                    r.RelativeItem().Column(sub =>
                    {
                        sub.Item().Text($"Desconto Concedido: {negociacao.DescontoPercentual:F2}%").FontSize(8).FontColor(Colors.Green.Darken3).Bold();
                        sub.Item().Text($"Valor Total Acordado: {negociacao.ValorAcordado.ToString("C", CulturaBr)}").FontSize(9).Bold();
                        sub.Item().Text($"Condição: {negociacao.QuantidadeParcelas} parcela(s)").FontSize(8);
                    });
                });
            });

            // Tabela de Parcelamento
            col.Item().PaddingTop(12).Text("3. CRONOGRAMA DE VENCIMENTO DAS PARCELAS").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignCenter().Text("Parcela").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignCenter().Text("Vencimento").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignRight().Text("Valor").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(4).AlignCenter().Text("Situação").FontColor(Colors.White).Bold();
                });

                foreach (var parcela in negociacao.Parcelas)
                {
                    var situacao = parcela.PagoEm.HasValue
                        ? $"Pago em {parcela.PagoEm.Value:dd/MM/yyyy}"
                        : "A Vencer";

                    var corSituacao = parcela.PagoEm.HasValue
                        ? Colors.Green.Darken2
                        : Colors.Grey.Darken2;

                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{parcela.Numero}/{negociacao.QuantidadeParcelas}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{parcela.Vencimento:dd/MM/yyyy}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(parcela.Valor.ToString("C", CulturaBr));
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(situacao).FontColor(corSituacao).Bold();
                }

                // Linha Total
                table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("TOTAL ACORDADO:").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(negociacao.ValorAcordado.ToString("C", CulturaBr)).Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("");
            });

            // Cláusulas e Termos Jurídicos
            col.Item().PaddingTop(10).Text("4. DISPOSIÇÕES GERAIS E CLÁUSULAS CONTRATUAIS").FontSize(9).Bold().FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(2).Text(
                "CLÁUSULA 1ª — Pelo presente instrumento particular, o(a) DEVEDOR(A) confessa e reconhece formalmente a certeza, liquidez e exigibilidade do débito supramencionado.\n" +
                "CLÁUSULA 2ª — O adimplemento pontual de cada parcela nas datas estipuladas confere quitação transitória da referida parcela, e a quitação integral somente ocorrerá com o pagamento de todas as parcelas.\n" +
                "CLÁUSULA 3ª — O inadimplemento de qualquer das parcelas por prazo superior a 10 (dez) dias corridos implicará a rescisão imediata deste acordo, com vencimento antecipado do saldo remanescente, perda integral dos descontos concedidos e retomada das cobranças judiciais e extrajudiciais.\n" +
                "CLÁUSULA 4ª — Para dirimir quaisquer litígios decorrentes deste instrumento, as partes elegem expressamente o Foro da Comarca da Capital do Estado de São Paulo."
            ).FontSize(7.5f).FontColor(Colors.Grey.Darken2);

            // Local e Data
            col.Item().PaddingTop(14).AlignCenter().Text($"São Paulo, {negociacao.DataAcordo.ToString("dd 'de' MMMM 'de' yyyy", CulturaBr)}.").FontSize(8);

            // Bloco de Assinaturas
            col.Item().PaddingTop(24).Row(r =>
            {
                r.RelativeItem().PaddingHorizontal(20).Column(c =>
                {
                    c.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(3).AlignCenter().Text(devedorNome).FontSize(8).Bold();
                    c.Item().AlignCenter().Text($"CPF/CNPJ: {devedorDoc}").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    c.Item().AlignCenter().Text("DEVEDOR(A)").FontSize(7).Bold().FontColor(Colors.Blue.Darken3);
                });

                r.RelativeItem().PaddingHorizontal(20).Column(c =>
                {
                    c.Item().BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(3).AlignCenter().Text("COBRANÇA WEB ASSESSORIA").FontSize(8).Bold();
                    c.Item().AlignCenter().Text("Departamento de Cobrança e Acordos").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    c.Item().AlignCenter().Text("CREDOR / ASSESSORIA").FontSize(7).Bold().FontColor(Colors.Blue.Darken3);
                });
            });
        });
    }

    private static void ComporRodape(IContainer container)
    {
        container.BorderTop(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text(t =>
            {
                t.Span("CobrançaWeb — Sistema Integrado de Recuperação de Ativos. Termo emitido digitalmente.").FontSize(7).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(100).AlignRight().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        });
    }
}
