using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace CobrancaWeb.LegacyMvc.Controllers
{
    public sealed class ConsultaCobrancaController : Controller
    {
        private readonly string _connectionString;

        public ConsultaCobrancaController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["CobrancaLegadoDb"]?.ConnectionString
                ?? throw new InvalidOperationException("A connection string CobrancaLegadoDb não foi configurada.");
        }

        // GET: /ConsultaCobranca/Index
        [HttpGet]
        public ActionResult Index(string termoBusca)
        {
            var listaContratos = new List<ContratoLegadoModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT c.Id, c.Numero, c.ValorOriginal, c.Vencimento, c.TaxaJurosMensal, c.Status,
                           d.Nome AS DevedorNome, d.Documento AS DevedorDocumento, d.Telefone
                    FROM dbo.Contratos c
                    INNER JOIN dbo.Devedores d ON c.DevedorId = d.Id
                    WHERE c.Status = 1 ";

                if (!string.IsNullOrWhiteSpace(termoBusca))
                {
                    sql += " AND (d.Nome LIKE @termo OR d.Documento LIKE @termo OR c.Numero LIKE @termo)";
                }

                sql += " ORDER BY c.Vencimento ASC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(termoBusca))
                    {
                        cmd.Parameters.Add("@termo", SqlDbType.NVarChar, 200).Value = "%" + termoBusca.Trim() + "%";
                    }

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vencimento = Convert.ToDateTime(reader["Vencimento"]);
                            var valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                            var taxaMensal = Convert.ToDecimal(reader["TaxaJurosMensal"]);
                            var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);
                            var meses = diasAtraso / 30.0m;
                            var valorAtualizado = Math.Round(valorOriginal * (1 + (taxaMensal * meses)), 2, MidpointRounding.AwayFromZero);

                            listaContratos.Add(new ContratoLegadoModel
                            {
                                ContratoId = (Guid)reader["Id"],
                                NumeroContrato = Convert.ToString(reader["Numero"]),
                                NomeDevedor = Convert.ToString(reader["DevedorNome"]),
                                Documento = Convert.ToString(reader["DevedorDocumento"]),
                                Telefone = reader["Telefone"] == DBNull.Value ? null : Convert.ToString(reader["Telefone"]),
                                ValorOriginal = valorOriginal,
                                Vencimento = vencimento,
                                DiasAtraso = diasAtraso,
                                ValorAtualizado = valorAtualizado
                            });
                        }
                    }
                }
            }

            return View(listaContratos);
        }

        // GET: /ConsultaCobranca/Negociacao/{id}
        [HttpGet]
        public ActionResult Negociacao(Guid id)
        {
            const string sql = @"
                SELECT n.Id, n.ContratoId, c.Numero AS NumeroContrato, d.Nome AS DevedorNome,
                       n.DataAcordo, n.ValorAcordado, n.DescontoPercentual,
                       n.QuantidadeParcelas, n.Status
                FROM dbo.Negociacoes n
                INNER JOIN dbo.Contratos c ON c.Id = n.ContratoId
                INNER JOIN dbo.Devedores d ON d.Id = c.DevedorId
                WHERE n.Id = @Id";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return HttpNotFound();
                    }

                    var acordo = new NegociacaoLegadoModel
                    {
                        Id = (Guid)reader["Id"],
                        ContratoId = (Guid)reader["ContratoId"],
                        NumeroContrato = Convert.ToString(reader["NumeroContrato"]),
                        DevedorNome = Convert.ToString(reader["DevedorNome"]),
                        DataAcordo = Convert.ToDateTime(reader["DataAcordo"]),
                        ValorAcordado = Convert.ToDecimal(reader["ValorAcordado"]),
                        DescontoPercentual = Convert.ToDecimal(reader["DescontoPercentual"]),
                        QuantidadeParcelas = Convert.ToInt32(reader["QuantidadeParcelas"]),
                        Status = Convert.ToInt32(reader["Status"])
                    };

                    return View(acordo);
                }
            }
        }

        // POST: /ConsultaCobranca/SimularAcordo — formato JSON mantido para legacy-cobranca.js.
        [HttpPost]
        public JsonResult SimularAcordo(Guid contratoId, decimal descontoPercentual, int parcelas)
        {
            if (parcelas < 1 || parcelas > 36 || descontoPercentual < 0 || descontoPercentual > 100)
            {
                return Json(new { Sucesso = false, Mensagem = "Informe de 1 a 36 parcelas e desconto entre 0% e 100%." });
            }

            const string sql = "SELECT ValorOriginal, Vencimento, TaxaJurosMensal, Status FROM dbo.Contratos WHERE Id = @Id";
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = contratoId;
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                    {
                        return Json(new { Sucesso = false, Mensagem = "Contrato não encontrado." });
                    }

                    if (Convert.ToInt32(reader["Status"]) != 1)
                    {
                        return Json(new { Sucesso = false, Mensagem = "Somente contratos em aberto podem ser negociados." });
                    }

                    var valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                    var vencimento = Convert.ToDateTime(reader["Vencimento"]);
                    var taxa = Convert.ToDecimal(reader["TaxaJurosMensal"]);
                    var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);

                    if (diasAtraso < 90 && descontoPercentual > 30m)
                    {
                        return Json(new
                        {
                            Sucesso = false,
                            Mensagem = "POLÍTICA DE CRÉDITO: Para contratos com menos de 90 dias de atraso, o desconto máximo permitido é de 30%."
                        });
                    }

                    var valorCorrigido = Math.Round(valorOriginal * (1 + (taxa * (diasAtraso / 30m))), 2, MidpointRounding.AwayFromZero);
                    var valorDesconto = Math.Round(valorCorrigido * (descontoPercentual / 100m), 2, MidpointRounding.AwayFromZero);
                    var valorAcordo = Math.Round(valorCorrigido - valorDesconto, 2, MidpointRounding.AwayFromZero);
                    var valorParcela = Math.Round(valorAcordo / parcelas, 2, MidpointRounding.AwayFromZero);

                    return Json(new
                    {
                        Sucesso = true,
                        DiasAtraso = diasAtraso,
                        ValorOriginal = valorOriginal,
                        ValorCorrigido = valorCorrigido,
                        DescontoPercentual = descontoPercentual,
                        ValorAcordo = valorAcordo,
                        Parcelas = parcelas,
                        ValorParcela = valorParcela
                    });
                }
            }
        }

        // POST: /ConsultaCobranca/EfetivarAcordo — mantém rota e formato do cliente MVC anterior.
        [HttpPost]
        public JsonResult EfetivarAcordo(Guid contratoId, decimal descontoPercentual, int quantidadeParcelas)
        {
            if (quantidadeParcelas < 1 || quantidadeParcelas > 36 || descontoPercentual < 0 || descontoPercentual > 100)
            {
                return Json(new { Sucesso = false, Mensagem = "Informe de 1 a 36 parcelas e desconto entre 0% e 100%." });
            }

            const string sql = "SELECT ValorOriginal, Vencimento, TaxaJurosMensal, Status FROM dbo.Contratos WHERE Id = @Id";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = contratoId;
                    conn.Open();

                    decimal valorOriginal;
                    DateTime vencimento;
                    decimal taxa;
                    using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (!reader.Read())
                        {
                            return Json(new { Sucesso = false, Mensagem = "Contrato não encontrado." });
                        }

                        if (Convert.ToInt32(reader["Status"]) != 1)
                        {
                            return Json(new { Sucesso = false, Mensagem = "Contrato já foi negociado ou liquidado." });
                        }

                        valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                        vencimento = Convert.ToDateTime(reader["Vencimento"]);
                        taxa = Convert.ToDecimal(reader["TaxaJurosMensal"]);
                    }

                    var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);
                    if (diasAtraso < 90 && descontoPercentual > 30m)
                    {
                        return Json(new { Sucesso = false, Mensagem = "Desconto superior ao teto de 30% para menos de 90 dias de atraso." });
                    }

                    var valorCorrigido = Math.Round(valorOriginal * (1 + (taxa * (diasAtraso / 30m))), 2, MidpointRounding.AwayFromZero);
                    var valorFinal = Math.Round(valorCorrigido * (1 - (descontoPercentual / 100m)), 2, MidpointRounding.AwayFromZero);
                    var negociacaoId = Guid.NewGuid();
                    var dataAcordo = DateTime.UtcNow;
                    var primeiroVencimento = DateTime.Today.AddDays(30);

                    using (var procedure = new SqlCommand("dbo.sp_EfetivarNegociacao", conn))
                    {
                        procedure.CommandType = CommandType.StoredProcedure;
                        procedure.Parameters.Add("@NegociacaoId", SqlDbType.UniqueIdentifier).Value = negociacaoId;
                        procedure.Parameters.Add("@ContratoId", SqlDbType.UniqueIdentifier).Value = contratoId;
                        procedure.Parameters.Add("@DataAcordo", SqlDbType.DateTime2).Value = dataAcordo;
                        AddDecimal(procedure, "@ValorAcordado", 18, 2, valorFinal);
                        AddDecimal(procedure, "@DescontoPercentual", 5, 2, descontoPercentual);
                        procedure.Parameters.Add("@QuantidadeParcelas", SqlDbType.Int).Value = quantidadeParcelas;
                        procedure.Parameters.Add("@PrimeiroVencimento", SqlDbType.Date).Value = primeiroVencimento;
                        procedure.ExecuteNonQuery();
                    }

                    return Json(new { Sucesso = true, NegociacaoId = negociacaoId, ValorAcordado = valorFinal });
                }
            }
            catch (SqlException ex)
            {
                return Json(new { Sucesso = false, Mensagem = ex.Message });
            }
        }

        private static void AddDecimal(SqlCommand command, string name, byte precision, byte scale, decimal value)
        {
            var parameter = command.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = precision;
            parameter.Scale = scale;
            parameter.Value = value;
        }
    }

    public sealed class ContratoLegadoModel
    {
        public Guid ContratoId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public string NomeDevedor { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public decimal ValorOriginal { get; set; }
        public DateTime Vencimento { get; set; }
        public int DiasAtraso { get; set; }
        public decimal ValorAtualizado { get; set; }
    }

    public sealed class NegociacaoLegadoModel
    {
        public Guid Id { get; set; }
        public Guid ContratoId { get; set; }
        public string NumeroContrato { get; set; } = string.Empty;
        public string DevedorNome { get; set; } = string.Empty;
        public DateTime DataAcordo { get; set; }
        public decimal ValorAcordado { get; set; }
        public decimal DescontoPercentual { get; set; }
        public int QuantidadeParcelas { get; set; }
        public int Status { get; set; }
    }
}
