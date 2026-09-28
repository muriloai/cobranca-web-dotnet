USE CobrancaWeb;
GO
SET XACT_ABORT ON;
BEGIN TRANSACTION;

INSERT INTO dbo.FaixasJuros (DiasMinimos, TaxaMensal)
SELECT valores.DiasMinimos, valores.TaxaMensal
FROM (VALUES (0, 0.010000), (90, 0.015000), (180, 0.020000)) AS valores(DiasMinimos, TaxaMensal)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.FaixasJuros faixa
    WHERE faixa.DiasMinimos = valores.DiasMinimos
);

DECLARE @MariaId uniqueidentifier;
SELECT @MariaId = Id FROM dbo.Devedores WHERE Documento = '52998224725';
IF @MariaId IS NULL
BEGIN
    SET @MariaId = '11111111-1111-1111-1111-111111111111';
    INSERT INTO dbo.Devedores (Id, Nome, Documento, Email, Telefone, Endereco)
    VALUES (@MariaId, N'Maria Silva', '52998224725', 'maria@example.test', '11999990000', N'Rua Exemplo, 10');
END;

DECLARE @EmpresaId uniqueidentifier;
SELECT @EmpresaId = Id FROM dbo.Devedores WHERE Documento = '11222333000181';
IF @EmpresaId IS NULL
BEGIN
    SET @EmpresaId = '22222222-2222-2222-2222-222222222222';
    INSERT INTO dbo.Devedores (Id, Nome, Documento, Email, Telefone, Endereco)
    VALUES (@EmpresaId, N'Empresa Exemplo Ltda', '11222333000181', 'financeiro@example.test', '1133334444', N'Av. Modelo, 20');
END;

DECLARE @ContratoMariaId uniqueidentifier;
SELECT @ContratoMariaId = Id FROM dbo.Contratos WHERE Numero = N'CT-001';
IF @ContratoMariaId IS NULL
BEGIN
    SET @ContratoMariaId = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
    INSERT INTO dbo.Contratos (Id, DevedorId, Numero, ValorOriginal, Vencimento, TaxaJurosMensal, Status)
    VALUES (@ContratoMariaId, @MariaId, N'CT-001', 1200.00, DATEADD(DAY, -120, CONVERT(date, SYSUTCDATETIME())), 0.01, 1);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Contratos WHERE Numero = N'CT-002')
BEGIN
    INSERT INTO dbo.Contratos (Id, DevedorId, Numero, ValorOriginal, Vencimento, TaxaJurosMensal, Status)
    VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', @EmpresaId, N'CT-002', 4500.00, DATEADD(DAY, -35, CONVERT(date, SYSUTCDATETIME())), 0.01, 1);
END;

DECLARE @UsuarioId uniqueidentifier;
SELECT @UsuarioId = Id FROM dbo.Usuarios WHERE Email = N'operador@example.test';
IF @UsuarioId IS NULL
BEGIN
    SET @UsuarioId = '33333333-3333-3333-3333-333333333333';
    INSERT INTO dbo.Usuarios (Id, Nome, Email, SenhaHash, Perfil, Ativo)
    VALUES (@UsuarioId, N'Operador Exemplo', N'operador@example.test', N'!sem-credencial!', 1, 0);
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Acionamentos WHERE Id = 'cccccccc-cccc-cccc-cccc-cccccccccccc')
BEGIN
    INSERT INTO dbo.Acionamentos (Id, ContratoId, UsuarioId, Tipo, Descricao, RealizadoEm)
    VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', @ContratoMariaId, @UsuarioId, 1,
            N'Contato de exemplo para relatório.', DATEADD(DAY, -1, SYSUTCDATETIME()));
END;

COMMIT TRANSACTION;
GO
