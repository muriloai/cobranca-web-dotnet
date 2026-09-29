using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace CobrancaWeb.LegacyMvc.Controllers
{
    /// <summary>
    /// MÓDULO LEGADO: ConsultaCobrançaController (Simulação do Sistema Antigo)
    /// Tecnologias legadas: ASP.NET MVC 4/5, ADO.NET puro (SqlConnection, SqlDataReader), .NET Framework 4.6.
    /// 
    /// Dívida Técnica Identificada:
    /// - Regras financeiras e de negócio acopladas diretamente na Controller.
    /// - Ausência de camada de Domínio, Injeção de Dependência e validações com FluentValidation.
    /// - Consultas SQL com strings embutidas no código C#.
    /// - Tratamento manual de transações ADO.NET (SqlTransaction).
    /// </summary>
    public class ConsultaCobrancaController
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["CobrancaLegadoDb"]?.ConnectionString
            ?? "Server=(localdb)\\mssqllocaldb;Database=CobrancaWeb;Trusted_Connection=True;";

        // GET: /ConsultaCobranca/Index
        public dynamic Index(string termoBusca)
        {
            var listaContratos = new List<ContratoLegadoModel>();

            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = @"
                    SELECT c.Id, c.Numero, c.ValorOriginal, c.Vencimento, c.TaxaJurosMensal, c.Status,
                           d.Nome AS DevedorNome, d.Documento AS DevedorDocumento, d.Telefone
                    FROM dbo.Contratos c
                    INNER JOIN dbo.Devedores d ON c.DevedorId = d.Id
                    WHERE c.Status = 1 "; // 1 = Em Aberto

                if (!string.IsNullOrWhiteSpace(termoBusca))
                {
                    sql += " AND (d.Nome LIKE @termo OR d.Documento LIKE @termo OR c.Numero LIKE @termo)";
                }

                sql += " ORDER BY c.Vencimento ASC";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(termoBusca))
                    {
                        cmd.Parameters.AddWithValue("@termo", "%" + termoBusca.Trim() + "%");
                    }

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var vencimento = Convert.ToDateTime(reader["Vencimento"]);
                            var valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                            var taxaMensal = Convert.ToDecimal(reader["TaxaJurosMensal"]);

                            // REGRA DE NEGÓCIO EMBUTIDA: Cálculo de dias de atraso e juros simples
                            var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);
                            var meses = diasAtraso / 30.0m;
                            var valorAtualizado = Math.Round(valorOriginal * (1 + (taxaMensal * meses)), 2);

                            listaContratos.Add(new ContratoLegadoModel
                            {
                                ContratoId = (Guid)reader["Id"],
                                NumeroContrato = reader["Numero"].ToString(),
                                NomeDevedor = reader["DevedorNome"].ToString(),
                                Documento = reader["DevedorDocumento"].ToString(),
                                Telefone = reader["Telefone"]?.ToString(),
                                ValorOriginal = valorOriginal,
                                Vencimento = vencimento,
                                DiasAtraso = diasAtraso,
                                ValorAtualizado = valorAtualizado
                            });
                        }
                    }
                }
            }

            return listaContratos;
        }

        // POST: /ConsultaCobranca/SimularAcordo (chamado via AJAX pelo jQuery)
        public dynamic SimularAcordo(Guid contratoId, decimal descontoPercentual, int parcelas)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var sql = "SELECT ValorOriginal, Vencimento, TaxaJurosMensal, Status FROM dbo.Contratos WHERE Id = @Id";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", contratoId);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return new { Sucesso = false, Mensagem = "Contrato não encontrado." };
                        }

                        var status = Convert.ToInt32(reader["Status"]);
                        if (status != 1)
                        {
                            return new { Sucesso = false, Mensagem = "Somente contratos em aberto podem ser negociados." };
                        }

                        var valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                        var vencimento = Convert.ToDateTime(reader["Vencimento"]);
                        var taxa = Convert.ToDecimal(reader["TaxaJurosMensal"]);

                        var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);
                        var meses = diasAtraso / 30.0m;
                        var valorCorrigido = Math.Round(valorOriginal * (1 + (taxa * meses)), 2);

                        // REGRA DE NEGÓCIO VITAL EXTRAÍDA: Trava de desconto para dívidas recentes
                        if (diasAtraso < 90 && descontoPercentual > 30.0m)
                        {
                            return new
                            {
                                Sucesso = false,
                                Mensagem = "POLÍTICA DE CRÉDITO: Para contratos com menos de 90 dias de atraso, o desconto máximo permitido é de 30%."
                            };
                        }

                        var valorDesconto = Math.Round(valorCorrigido * (descontoPercentual / 100.0m), 2);
                        var valorAcordo = valorCorrigido - valorDesconto;
                        var valorParcela = Math.Round(valorAcordo / parcelas, 2);

                        return new
                        {
                            Sucesso = true,
                            DiasAtraso = diasAtraso,
                            ValorOriginal = valorOriginal,
                            ValorCorrigido = valorCorrigido,
                            DescontoPercentual = descontoPercentual,
                            ValorAcordo = valorAcordo,
                            Parcelas = parcelas,
                            ValorParcela = valorParcela
                        };
                    }
                }
            }
        }

        // POST: /ConsultaCobranca/EfetivarAcordo (Transação manual ADO.NET)
        public dynamic EfetivarAcordo(Guid contratoId, decimal descontoPercentual, int quantidadeParcelas)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Ler o contrato dentro da transação
                        decimal valorOriginal;
                        DateTime vencimento;
                        decimal taxa;

                        var sqlSelect = "SELECT ValorOriginal, Vencimento, TaxaJurosMensal, Status FROM dbo.Contratos WITH (UPDLOCK) WHERE Id = @Id";
                        using (var cmdSelect = new SqlCommand(sqlSelect, conn, tx))
                        {
                            cmdSelect.Parameters.AddWithValue("@Id", contratoId);
                            using (var reader = cmdSelect.ExecuteReader())
                            {
                                if (!reader.Read())
                                    throw new InvalidOperationException("Contrato não encontrado.");

                                if (Convert.ToInt32(reader["Status"]) != 1)
                                    throw new InvalidOperationException("Contrato já foi negociado ou liquidado.");

                                valorOriginal = Convert.ToDecimal(reader["ValorOriginal"]);
                                vencimento = Convert.ToDateTime(reader["Vencimento"]);
                                taxa = Convert.ToDecimal(reader["TaxaJurosMensal"]);
                            }
                        }

                        // 2. Aplicar cálculo
                        var diasAtraso = Math.Max(0, (DateTime.Today - vencimento.Date).Days);
                        if (diasAtraso < 90 && descontoPercentual > 30.0m)
                        {
                            throw new InvalidOperationException("Desconto superior ao teto de 30% para menos de 90 dias de atraso.");
                        }

                        var valorCorrigido = Math.Round(valorOriginal * (1 + (taxa * (diasAtraso / 30.0m))), 2);
                        var valorFinal = Math.Round(valorCorrigido * (1 - (descontoPercentual / 100.0m)), 2);

                        // 3. Inserir Negociação
                        var negociacaoId = Guid.NewGuid();
                        var sqlInsertNeg = @"
                            INSERT INTO dbo.Negociacoes (Id, ContratoId, DataAcordo, ValorAcordado, DescontoPercentual, QuantidadeParcelas, Status)
                            VALUES (@Id, @ContratoId, @DataAcordo, @ValorAcordado, @DescontoPercentual, @QuantidadeParcelas, 1)";

                        using (var cmdNeg = new SqlCommand(sqlInsertNeg, conn, tx))
                        {
                            cmdNeg.Parameters.AddWithValue("@Id", negociacaoId);
                            cmdNeg.Parameters.AddWithValue("@ContratoId", contratoId);
                            cmdNeg.Parameters.AddWithValue("@DataAcordo", DateTime.UtcNow);
                            cmdNeg.Parameters.AddWithValue("@ValorAcordado", valorFinal);
                            cmdNeg.Parameters.AddWithValue("@DescontoPercentual", descontoPercentual);
                            cmdNeg.Parameters.AddWithValue("@QuantidadeParcelas", quantidadeParcelas);
                            cmdNeg.ExecuteNonQuery();
                        }

                        // 4. Inserir Parcelas com rateio
                        var valorParcelaBase = Math.Round(valorFinal / quantidadeParcelas, 2);
                        var ajusteCentavos = valorFinal - (valorParcelaBase * quantidadeParcelas);

                        for (var i = 1; i <= quantidadeParcelas; i++)
                        {
                            var valor = (i == 1) ? valorParcelaBase + ajusteCentavos : valorParcelaBase;
                            var sqlInsertParc = @"
                                INSERT INTO dbo.ParcelasPagamento (Id, NegociacaoId, Numero, Vencimento, Valor)
                                VALUES (@Id, @NegociacaoId, @Numero, @Vencimento, @Valor)";

                            using (var cmdParc = new SqlCommand(sqlInsertParc, conn, tx))
                            {
                                cmdParc.Parameters.AddWithValue("@Id", Guid.NewGuid());
                                cmdParc.Parameters.AddWithValue("@NegociacaoId", negociacaoId);
                                cmdParc.Parameters.AddWithValue("@Numero", i);
                                cmdParc.Parameters.AddWithValue("@Vencimento", DateTime.Today.AddDays(30 * i));
                                cmdParc.Parameters.AddWithValue("@Valor", valor);
                                cmdParc.ExecuteNonQuery();
                            }
                        }

                        // 5. Atualizar Contrato para 'Negociado' (Status = 2)
                        var sqlUpdateContrato = "UPDATE dbo.Contratos SET Status = 2 WHERE Id = @Id";
                        using (var cmdUpdate = new SqlCommand(sqlUpdateContrato, conn, tx))
                        {
                            cmdUpdate.Parameters.AddWithValue("@Id", contratoId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        tx.Commit();

                        return new { Sucesso = true, NegociacaoId = negociacaoId, ValorAcordado = valorFinal };
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        return new { Sucesso = false, Mensagem = ex.Message };
                    }
                }
            }
        }
    }

    public class ContratoLegadoModel
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
}
