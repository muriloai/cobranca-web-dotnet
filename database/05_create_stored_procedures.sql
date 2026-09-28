USE CobrancaWeb;
GO
CREATE OR ALTER PROCEDURE dbo.sp_DashboardResumo AS
BEGIN
    SET NOCOUNT ON;
    SELECT COUNT_BIG(*) AS ContratosInadimplentes,
           COALESCE(SUM(ValorOriginal), 0) AS ValorInadimplente
    FROM dbo.vw_ContratosInadimplentesResumo;
    SELECT COALESCE(SUM(p.Valor), 0) AS ValorRecuperado
    FROM dbo.ParcelasPagamento p WHERE p.PagoEm IS NOT NULL;
    SELECT COUNT_BIG(*) AS AcordosAtivos FROM dbo.Negociacoes WHERE Status = 1;
END;
GO
CREATE OR ALTER PROCEDURE dbo.sp_RelatorioCobrancas
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
GO
