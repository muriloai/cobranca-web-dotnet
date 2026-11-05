USE CobrancaWeb;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @ContratoId uniqueidentifier;
SELECT @ContratoId = Id FROM dbo.Contratos WHERE Numero = N'CT-TEST-100';

IF @ContratoId IS NULL
    THROW 51010, N'O contrato de demonstração CT-TEST-100 não existe.', 1;

DELETE p
FROM dbo.ParcelasPagamento p
JOIN dbo.Negociacoes n ON n.Id = p.NegociacaoId
WHERE n.ContratoId = @ContratoId;

DELETE FROM dbo.Negociacoes WHERE ContratoId = @ContratoId;
UPDATE dbo.Contratos SET Status = 1 WHERE Id = @ContratoId;

COMMIT TRANSACTION;
GO
