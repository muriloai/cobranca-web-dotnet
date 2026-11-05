using CobrancaWeb.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CobrancaWeb.Infrastructure.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261104140000_ProceduresAndApplicationRole")]
public partial class ProceduresAndApplicationRole : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF DATABASE_PRINCIPAL_ID(N'CobrancaApplicationRole') IS NULL
                CREATE ROLE [CobrancaApplicationRole] AUTHORIZATION [dbo];
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER PROCEDURE dbo.sp_EfetivarNegociacao
                @NegociacaoId uniqueidentifier,
                @ContratoId uniqueidentifier,
                @DataAcordo datetime2,
                @ValorAcordado decimal(18,2),
                @DescontoPercentual decimal(5,2),
                @QuantidadeParcelas int,
                @PrimeiroVencimento date
            WITH EXECUTE AS OWNER
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                IF @ValorAcordado <= 0 OR @DescontoPercentual NOT BETWEEN 0 AND 100
                   OR @QuantidadeParcelas NOT BETWEEN 1 AND 36
                    THROW 51001, N'Os dados da negociação são inválidos.', 1;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    DECLARE @StatusContrato int;
                    SELECT @StatusContrato = Status
                    FROM dbo.Contratos WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id = @ContratoId;

                    IF @StatusContrato IS NULL
                        THROW 51002, N'Contrato não encontrado.', 1;
                    IF @StatusContrato <> 1
                        THROW 51003, N'Somente contratos em aberto podem ser negociados.', 1;

                    INSERT INTO dbo.Negociacoes
                        (Id, ContratoId, DataAcordo, ValorAcordado, DescontoPercentual, QuantidadeParcelas, Status)
                    VALUES
                        (@NegociacaoId, @ContratoId, @DataAcordo, @ValorAcordado,
                         @DescontoPercentual, @QuantidadeParcelas, 1);

                    DECLARE @ValorBase decimal(18,2) = ROUND(@ValorAcordado / @QuantidadeParcelas, 2);
                    DECLARE @Residuo decimal(18,2) = @ValorAcordado - (@ValorBase * @QuantidadeParcelas);
                    DECLARE @Numero int = 1;

                    WHILE @Numero <= @QuantidadeParcelas
                    BEGIN
                        INSERT INTO dbo.ParcelasPagamento (Id, NegociacaoId, Numero, Vencimento, Valor)
                        VALUES
                        (
                            NEWID(),
                            @NegociacaoId,
                            @Numero,
                            DATEADD(MONTH, @Numero - 1, @PrimeiroVencimento),
                            CASE WHEN @Numero = 1 THEN @ValorBase + @Residuo ELSE @ValorBase END
                        );
                        SET @Numero += 1;
                    END;

                    UPDATE dbo.Contratos
                    SET Status = 2
                    WHERE Id = @ContratoId AND Status = 1;

                    IF @@ROWCOUNT <> 1
                        THROW 51004, N'O contrato mudou durante a efetivação do acordo.', 1;

                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    THROW;
                END CATCH;
            END;
            """);

        migrationBuilder.Sql("""
            CREATE OR ALTER PROCEDURE dbo.sp_RegistrarPagamentoParcela
                @NegociacaoId uniqueidentifier,
                @ParcelaId uniqueidentifier,
                @PagoEm datetime2
            WITH EXECUTE AS OWNER
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                IF @PagoEm IS NULL
                    THROW 51005, N'A data do pagamento é obrigatória.', 1;

                BEGIN TRY
                    BEGIN TRANSACTION;

                    DECLARE @ContratoId uniqueidentifier;
                    DECLARE @StatusNegociacao int;
                    SELECT @ContratoId = ContratoId, @StatusNegociacao = Status
                    FROM dbo.Negociacoes WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id = @NegociacaoId;

                    IF @StatusNegociacao IS NULL
                        THROW 51006, N'Negociação não encontrada.', 1;
                    IF @StatusNegociacao <> 1
                        THROW 51007, N'A negociação não está ativa.', 1;

                    UPDATE dbo.ParcelasPagamento
                    SET PagoEm = @PagoEm
                    WHERE Id = @ParcelaId AND NegociacaoId = @NegociacaoId AND PagoEm IS NULL;

                    IF @@ROWCOUNT <> 1
                        THROW 51008, N'Parcela não encontrada ou já paga.', 1;

                    IF NOT EXISTS
                    (
                        SELECT 1 FROM dbo.ParcelasPagamento WITH (UPDLOCK, HOLDLOCK)
                        WHERE NegociacaoId = @NegociacaoId AND PagoEm IS NULL
                    )
                    BEGIN
                        UPDATE dbo.Negociacoes SET Status = 2 WHERE Id = @NegociacaoId AND Status = 1;
                        UPDATE dbo.Contratos SET Status = 3 WHERE Id = @ContratoId AND Status = 2;
                        IF @@ROWCOUNT <> 1
                            THROW 51009, N'O contrato não está em negociação.', 1;
                    END;

                    COMMIT TRANSACTION;
                END TRY
                BEGIN CATCH
                    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                    THROW;
                END CATCH;
            END;
            """);

        migrationBuilder.Sql("""
            GRANT SELECT ON SCHEMA::dbo TO [CobrancaApplicationRole];
            GRANT INSERT ON OBJECT::dbo.Devedores TO [CobrancaApplicationRole];
            GRANT UPDATE ON OBJECT::dbo.Devedores TO [CobrancaApplicationRole];
            GRANT INSERT ON OBJECT::dbo.Contratos TO [CobrancaApplicationRole];
            GRANT INSERT ON OBJECT::dbo.Acionamentos TO [CobrancaApplicationRole];
            GRANT INSERT ON OBJECT::dbo.Usuarios TO [CobrancaApplicationRole];

            DENY UPDATE, DELETE ON OBJECT::dbo.Contratos TO [CobrancaApplicationRole];
            DENY INSERT, UPDATE, DELETE ON OBJECT::dbo.Negociacoes TO [CobrancaApplicationRole];
            DENY INSERT, UPDATE, DELETE ON OBJECT::dbo.ParcelasPagamento TO [CobrancaApplicationRole];

            GRANT EXECUTE ON OBJECT::dbo.sp_DashboardResumo TO [CobrancaApplicationRole];
            GRANT EXECUTE ON OBJECT::dbo.sp_RelatorioCobrancas TO [CobrancaApplicationRole];
            GRANT EXECUTE ON OBJECT::dbo.sp_EfetivarNegociacao TO [CobrancaApplicationRole];
            GRANT EXECUTE ON OBJECT::dbo.sp_RegistrarPagamentoParcela TO [CobrancaApplicationRole];
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            REVOKE INSERT ON OBJECT::dbo.Devedores FROM [CobrancaApplicationRole];
            REVOKE UPDATE ON OBJECT::dbo.Devedores FROM [CobrancaApplicationRole];
            REVOKE INSERT ON OBJECT::dbo.Contratos FROM [CobrancaApplicationRole];
            REVOKE INSERT ON OBJECT::dbo.Acionamentos FROM [CobrancaApplicationRole];
            REVOKE INSERT ON OBJECT::dbo.Usuarios FROM [CobrancaApplicationRole];
            REVOKE SELECT ON SCHEMA::dbo FROM [CobrancaApplicationRole];
            REVOKE UPDATE, DELETE ON OBJECT::dbo.Contratos FROM [CobrancaApplicationRole];
            REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.Negociacoes FROM [CobrancaApplicationRole];
            REVOKE INSERT, UPDATE, DELETE ON OBJECT::dbo.ParcelasPagamento FROM [CobrancaApplicationRole];
            REVOKE EXECUTE ON OBJECT::dbo.sp_DashboardResumo FROM [CobrancaApplicationRole];
            REVOKE EXECUTE ON OBJECT::dbo.sp_RelatorioCobrancas FROM [CobrancaApplicationRole];
            REVOKE EXECUTE ON OBJECT::dbo.sp_EfetivarNegociacao FROM [CobrancaApplicationRole];
            REVOKE EXECUTE ON OBJECT::dbo.sp_RegistrarPagamentoParcela FROM [CobrancaApplicationRole];
            """);
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_RegistrarPagamentoParcela;");
        migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.sp_EfetivarNegociacao;");
        migrationBuilder.Sql("""
            IF NOT EXISTS
            (
                SELECT 1
                FROM sys.database_role_members
                WHERE role_principal_id = DATABASE_PRINCIPAL_ID(N'CobrancaApplicationRole')
            )
                DROP ROLE [CobrancaApplicationRole];
            """);
    }
}
