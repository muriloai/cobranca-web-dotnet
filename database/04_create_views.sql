USE CobrancaWeb;
GO
CREATE OR ALTER VIEW dbo.vw_ContratosInadimplentesResumo AS
SELECT c.Id AS ContratoId, c.Numero, d.Id AS DevedorId, d.Nome AS DevedorNome,
       d.Documento, c.ValorOriginal, c.Vencimento,
       DATEDIFF(DAY, c.Vencimento, CONVERT(date, SYSUTCDATETIME())) AS DiasAtraso
FROM dbo.Contratos c
JOIN dbo.Devedores d ON d.Id = c.DevedorId
WHERE c.Status = 1 AND c.Vencimento < CONVERT(date, SYSUTCDATETIME());
GO
