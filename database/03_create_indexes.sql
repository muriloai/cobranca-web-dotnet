USE CobrancaWeb;
GO
-- Opções exigidas pelo índice.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET NUMERIC_ROUNDABORT OFF;
GO
CREATE INDEX IX_Contratos_Devedor_Status ON dbo.Contratos(DevedorId, Status) INCLUDE (Vencimento, ValorOriginal);
CREATE INDEX IX_Contratos_Status_Vencimento ON dbo.Contratos(Status, Vencimento) INCLUDE (ValorOriginal);
CREATE INDEX IX_Negociacoes_Contrato_Status ON dbo.Negociacoes(ContratoId, Status);
CREATE UNIQUE INDEX UX_Negociacoes_Contrato_Ativa ON dbo.Negociacoes(ContratoId) WHERE Status = 1;
CREATE INDEX IX_Parcelas_Vencimento ON dbo.ParcelasPagamento(Vencimento, PagoEm) INCLUDE (Valor);
CREATE INDEX IX_Acionamentos_Contrato_Data ON dbo.Acionamentos(ContratoId, RealizadoEm DESC);
GO
