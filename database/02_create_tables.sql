USE CobrancaWeb;
GO
CREATE TABLE dbo.Devedores (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Devedores PRIMARY KEY,
    Nome nvarchar(180) NOT NULL,
    Documento varchar(14) NOT NULL,
    Email nvarchar(254) NULL,
    Telefone varchar(20) NULL,
    Endereco nvarchar(300) NULL,
    CriadoEm datetime2 NOT NULL CONSTRAINT DF_Devedores_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Devedores_Documento UNIQUE (Documento),
    CONSTRAINT CK_Devedores_Documento CHECK (LEN(Documento) IN (11, 14) AND Documento NOT LIKE '%[^0-9]%')
);

CREATE TABLE dbo.Usuarios (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Usuarios PRIMARY KEY,
    Nome nvarchar(180) NOT NULL,
    Email nvarchar(254) NOT NULL,
    SenhaHash nvarchar(255) NOT NULL,
    Perfil int NOT NULL,
    Ativo bit NOT NULL CONSTRAINT DF_Usuarios_Ativo DEFAULT (1),
    CriadoEm datetime2 NOT NULL CONSTRAINT DF_Usuarios_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email),
    CONSTRAINT CK_Usuarios_Perfil CHECK (Perfil IN (1, 2))
);

CREATE TABLE dbo.Contratos (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Contratos PRIMARY KEY,
    DevedorId uniqueidentifier NOT NULL,
    Numero nvarchar(50) NOT NULL,
    ValorOriginal decimal(18,2) NOT NULL,
    Vencimento date NOT NULL,
    TaxaJurosMensal decimal(9,6) NOT NULL CONSTRAINT DF_Contratos_Taxa DEFAULT (0.01),
    Status int NOT NULL CONSTRAINT DF_Contratos_Status DEFAULT (1),
    CriadoEm datetime2 NOT NULL CONSTRAINT DF_Contratos_CriadoEm DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Contratos_Devedores FOREIGN KEY (DevedorId) REFERENCES dbo.Devedores(Id),
    CONSTRAINT UQ_Contratos_Numero UNIQUE (Numero),
    CONSTRAINT CK_Contratos_Valor CHECK (ValorOriginal > 0),
    CONSTRAINT CK_Contratos_Taxa CHECK (TaxaJurosMensal >= 0 AND TaxaJurosMensal <= 1),
    CONSTRAINT CK_Contratos_Status CHECK (Status IN (1, 2, 3))
);

CREATE TABLE dbo.Negociacoes (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Negociacoes PRIMARY KEY,
    ContratoId uniqueidentifier NOT NULL,
    DataAcordo datetime2 NOT NULL,
    ValorAcordado decimal(18,2) NOT NULL,
    DescontoPercentual decimal(5,2) NOT NULL,
    QuantidadeParcelas int NOT NULL,
    Status int NOT NULL,
    CONSTRAINT FK_Negociacoes_Contratos FOREIGN KEY (ContratoId) REFERENCES dbo.Contratos(Id),
    CONSTRAINT CK_Negociacoes_Valor CHECK (ValorAcordado > 0),
    CONSTRAINT CK_Negociacoes_Desconto CHECK (DescontoPercentual BETWEEN 0 AND 100),
    CONSTRAINT CK_Negociacoes_Parcelas CHECK (QuantidadeParcelas BETWEEN 1 AND 36),
    CONSTRAINT CK_Negociacoes_Status CHECK (Status IN (1, 2, 3))
);

CREATE TABLE dbo.ParcelasPagamento (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_ParcelasPagamento PRIMARY KEY,
    NegociacaoId uniqueidentifier NOT NULL,
    Numero int NOT NULL,
    Vencimento date NOT NULL,
    Valor decimal(18,2) NOT NULL,
    PagoEm datetime2 NULL,
    CONSTRAINT FK_Parcelas_Negociacoes FOREIGN KEY (NegociacaoId) REFERENCES dbo.Negociacoes(Id),
    CONSTRAINT UQ_Parcelas_NegociacaoNumero UNIQUE (NegociacaoId, Numero),
    CONSTRAINT CK_Parcelas_Numero CHECK (Numero > 0),
    CONSTRAINT CK_Parcelas_Valor CHECK (Valor > 0)
);

CREATE TABLE dbo.Acionamentos (
    Id uniqueidentifier NOT NULL CONSTRAINT PK_Acionamentos PRIMARY KEY,
    ContratoId uniqueidentifier NOT NULL,
    UsuarioId uniqueidentifier NOT NULL,
    Tipo int NOT NULL,
    Descricao nvarchar(1000) NOT NULL,
    RealizadoEm datetime2 NOT NULL,
    CONSTRAINT FK_Acionamentos_Contratos FOREIGN KEY (ContratoId) REFERENCES dbo.Contratos(Id),
    CONSTRAINT FK_Acionamentos_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(Id),
    CONSTRAINT CK_Acionamentos_Tipo CHECK (Tipo IN (1, 2, 3))
);

CREATE TABLE dbo.FaixasJuros (
    DiasMinimos int NOT NULL CONSTRAINT PK_FaixasJuros PRIMARY KEY,
    TaxaMensal decimal(9,6) NOT NULL,
    CONSTRAINT CK_FaixasJuros_Dias CHECK (DiasMinimos >= 0),
    CONSTRAINT CK_FaixasJuros_Taxa CHECK (TaxaMensal >= 0 AND TaxaMensal <= 1)
);
GO
