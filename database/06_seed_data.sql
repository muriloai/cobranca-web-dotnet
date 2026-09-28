USE CobrancaWeb;
GO
INSERT INTO dbo.FaixasJuros(DiasMinimos, TaxaMensal) VALUES (0, 0.010000), (90, 0.015000), (180, 0.020000);
INSERT INTO dbo.Devedores(Id, Nome, Documento, Email, Telefone, Endereco) VALUES
('11111111-1111-1111-1111-111111111111', N'Maria Silva', '52998224725', 'maria@example.test', '11999990000', N'Rua Exemplo, 10'),
('22222222-2222-2222-2222-222222222222', N'Empresa Exemplo Ltda', '11222333000181', 'financeiro@example.test', '1133334444', N'Av. Modelo, 20');
INSERT INTO dbo.Contratos(Id, DevedorId, Numero, ValorOriginal, Vencimento, TaxaJurosMensal, Status) VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'CT-001', 1200.00, DATEADD(DAY, -120, CONVERT(date, SYSUTCDATETIME())), 0.01, 1),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '22222222-2222-2222-2222-222222222222', 'CT-002', 4500.00, DATEADD(DAY, -35, CONVERT(date, SYSUTCDATETIME())), 0.01, 1);

-- Usuário de exemplo inativo.
INSERT INTO dbo.Usuarios(Id, Nome, Email, SenhaHash, Perfil, Ativo) VALUES
('33333333-3333-3333-3333-333333333333', N'Operador Exemplo', 'operador@example.test', '!sem-credencial!', 1, 0);
INSERT INTO dbo.Acionamentos(Id, ContratoId, UsuarioId, Tipo, Descricao, RealizadoEm) VALUES
('cccccccc-cccc-cccc-cccc-cccccccccccc', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '33333333-3333-3333-3333-333333333333', 1, N'Contato de exemplo para relatório.', DATEADD(DAY, -1, SYSUTCDATETIME()));
GO
