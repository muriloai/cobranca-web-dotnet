using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CobrancaWeb.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Documento = table.Column<string>(type: "varchar(14)", unicode: false, maxLength: 14, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Telefone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Endereco = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devedores", x => x.Id);
                    table.CheckConstraint("CK_Devedores_Documento", "LEN([Documento]) IN (11, 14) AND [Documento] NOT LIKE '%[^0-9]%'");
                });

            migrationBuilder.CreateTable(
                name: "FaixasJuros",
                columns: table => new
                {
                    DiasMinimos = table.Column<int>(type: "int", nullable: false),
                    TaxaMensal = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaixasJuros", x => x.DiasMinimos);
                    table.CheckConstraint("CK_FaixasJuros_Dias", "[DiasMinimos] >= 0");
                    table.CheckConstraint("CK_FaixasJuros_Taxa", "[TaxaMensal] >= 0 AND [TaxaMensal] <= 1");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.CheckConstraint("CK_Usuarios_Perfil", "[Perfil] IN (1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevedorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ValorOriginal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Vencimento = table.Column<DateTime>(type: "date", nullable: false),
                    TaxaJurosMensal = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false, defaultValue: 0.01m),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.CheckConstraint("CK_Contratos_Status", "[Status] IN (1, 2, 3)");
                    table.CheckConstraint("CK_Contratos_Taxa", "[TaxaJurosMensal] >= 0 AND [TaxaJurosMensal] <= 1");
                    table.CheckConstraint("CK_Contratos_Valor", "[ValorOriginal] > 0");
                    table.ForeignKey(
                        name: "FK_Contratos_Devedores",
                        column: x => x.DevedorId,
                        principalTable: "Devedores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Acionamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RealizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acionamentos", x => x.Id);
                    table.CheckConstraint("CK_Acionamentos_Tipo", "[Tipo] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Acionamentos_Contratos",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Acionamentos_Usuarios",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Negociacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataAcordo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValorAcordado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DescontoPercentual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    QuantidadeParcelas = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Negociacoes", x => x.Id);
                    table.CheckConstraint("CK_Negociacoes_Desconto", "[DescontoPercentual] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Negociacoes_Parcelas", "[QuantidadeParcelas] BETWEEN 1 AND 36");
                    table.CheckConstraint("CK_Negociacoes_Status", "[Status] IN (1, 2, 3)");
                    table.CheckConstraint("CK_Negociacoes_Valor", "[ValorAcordado] > 0");
                    table.ForeignKey(
                        name: "FK_Negociacoes_Contratos",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ParcelasPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NegociacaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Vencimento = table.Column<DateTime>(type: "date", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PagoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelasPagamento", x => x.Id);
                    table.CheckConstraint("CK_Parcelas_Numero", "[Numero] > 0");
                    table.CheckConstraint("CK_Parcelas_Valor", "[Valor] > 0");
                    table.ForeignKey(
                        name: "FK_Parcelas_Negociacoes",
                        column: x => x.NegociacaoId,
                        principalTable: "Negociacoes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acionamentos_Contrato_Data",
                table: "Acionamentos",
                columns: new[] { "ContratoId", "RealizadoEm" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Acionamentos_UsuarioId",
                table: "Acionamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Devedor_Status",
                table: "Contratos",
                columns: new[] { "DevedorId", "Status" })
                .Annotation("SqlServer:Include", new[] { "Vencimento", "ValorOriginal" });

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Status_Vencimento",
                table: "Contratos",
                columns: new[] { "Status", "Vencimento" })
                .Annotation("SqlServer:Include", new[] { "ValorOriginal" });

            migrationBuilder.CreateIndex(
                name: "UQ_Contratos_Numero",
                table: "Contratos",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Devedores_Documento",
                table: "Devedores",
                column: "Documento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Negociacoes_Contrato_Status",
                table: "Negociacoes",
                columns: new[] { "ContratoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_Negociacoes_Contrato_Ativa",
                table: "Negociacoes",
                column: "ContratoId",
                unique: true,
                filter: "[Status] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Parcelas_Vencimento",
                table: "ParcelasPagamento",
                columns: new[] { "Vencimento", "PagoEm" })
                .Annotation("SqlServer:Include", new[] { "Valor" });

            migrationBuilder.CreateIndex(
                name: "UQ_Parcelas_NegociacaoNumero",
                table: "ParcelasPagamento",
                columns: new[] { "NegociacaoId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.Sql("""
                CREATE VIEW dbo.vw_ContratosInadimplentesResumo AS
                SELECT c.Id AS ContratoId, c.Numero, d.Id AS DevedorId, d.Nome AS DevedorNome,
                       d.Documento, c.ValorOriginal, c.Vencimento,
                       DATEDIFF(DAY, c.Vencimento, CONVERT(date, SYSUTCDATETIME())) AS DiasAtraso
                FROM dbo.Contratos c
                JOIN dbo.Devedores d ON d.Id = c.DevedorId
                WHERE c.Status = 1 AND c.Vencimento < CONVERT(date, SYSUTCDATETIME());
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE dbo.sp_DashboardResumo AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT COUNT_BIG(*) AS ContratosInadimplentes,
                           COALESCE(SUM(ValorOriginal), 0) AS ValorInadimplente
                    FROM dbo.vw_ContratosInadimplentesResumo;
                    SELECT COALESCE(SUM(p.Valor), 0) AS ValorRecuperado
                    FROM dbo.ParcelasPagamento p WHERE p.PagoEm IS NOT NULL;
                    SELECT COUNT_BIG(*) AS AcordosAtivos FROM dbo.Negociacoes WHERE Status = 1;
                END;
                """);

            migrationBuilder.Sql("""
                CREATE PROCEDURE dbo.sp_RelatorioCobrancas
                    @Inicio date,
                    @Fim date
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF @Inicio IS NULL OR @Fim IS NULL OR @Fim < @Inicio
                        THROW 50001, 'Periodo invalido: informe Inicio e Fim em ordem.', 1;
                    SELECT a.Id, a.RealizadoEm, a.Tipo, a.Descricao, u.Nome AS Operador,
                           d.Nome AS Devedor, c.Numero AS Contrato
                    FROM dbo.Acionamentos a
                    JOIN dbo.Usuarios u ON u.Id = a.UsuarioId
                    JOIN dbo.Contratos c ON c.Id = a.ContratoId
                    JOIN dbo.Devedores d ON d.Id = c.DevedorId
                    WHERE a.RealizadoEm >= @Inicio AND a.RealizadoEm < DATEADD(DAY, 1, @Fim)
                    ORDER BY a.RealizadoEm DESC;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_RelatorioCobrancas;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_DashboardResumo;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.vw_ContratosInadimplentesResumo;");

            migrationBuilder.DropTable(
                name: "Acionamentos");

            migrationBuilder.DropTable(
                name: "FaixasJuros");

            migrationBuilder.DropTable(
                name: "ParcelasPagamento");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Negociacoes");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Devedores");
        }
    }
}
