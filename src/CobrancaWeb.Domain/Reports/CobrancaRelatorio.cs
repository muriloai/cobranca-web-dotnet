using CobrancaWeb.Domain.Enums;

namespace CobrancaWeb.Domain.Reports;

public sealed record CobrancaRelatorio(
    Guid Id,
    DateTime RealizadoEm,
    TipoAcionamento Tipo,
    string Descricao,
    string Operador,
    string Devedor,
    string Contrato);
