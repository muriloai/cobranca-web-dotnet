# Cobrança Web

> Sistema de cobrança em .NET 8 + SQL Server

## Estrutura

- A solution está dividida em Domain, Application, Infrastructure, API e Tests.
- A migration EF Core cria tabelas, restrições, índices, view e procedures, então o script `database/seed_data.sql` insere dados de exemplo.
- O domínio modela devedores, contratos, negociações, parcelas, acionamentos e usuários, e também calcula atraso e juros e registra pagamentos.
- A infraestrutura usa EF Core 8 para mapear as entidades, implementar repositórios e versionar o esquema com uma migration inicial.
- A API tem apenas a estrutura básica e o Swagger em desenvolvimento, e os serviços de aplicação.

## Arquitetura

O backend está organizado em camadas seguindo a **Clean Architecture**: a API e a infraestrutura dependem das camadas internas. O domínio contém entidades com algumas regras de negócio, como cálculo de juros e transição de status. E parte das regras também está nos serviços e validadores da Application.

### Diagrama de Dependência entre Camadas

A regra de dependência é estrita: as camadas externas dependem das internas, e o **Domain** é o núcleo puro independente.

```mermaid
graph LR
    API["CobrancaWeb.API<br/>(Apresentação / REST / Swagger)"]
    INFRA["CobrancaWeb.Infrastructure<br/>(EF Core 8 / Dapper / JWT / PDF)"]
    APP["CobrancaWeb.Application<br/>(Casos de Uso / FluentValidation / DTOs)"]
    DOM["CobrancaWeb.Domain<br/>(Entidades / Regras Financeiras / Interfaces)"]

    API --> APP
    API --> INFRA
    INFRA --> APP
    INFRA --> DOM
    APP --> DOM

    classDef core fill:#dbeafe,stroke:#1d4ed8,stroke-width:2px;
    classDef app fill:#e0e7ff,stroke:#4338ca,stroke-width:2px;
    classDef ext fill:#f3f4f6,stroke:#374151,stroke-width:2px;

    class DOM core;
    class APP app;
    class API,INFRA ext;
```

## Decisões e padrões

- Repository separa o acesso a dados das regras de negócio. Os repositórios EF Core compartilham o `AppDbContext` para salvar alterações.
- EF Core 8 usa Fluent API para mapear chaves, relações, índices e restrições. Dapper atende consultas de relatórios e stored procedures.
- A migration é a fonte do esquema todo do banco e depois de aplicá-la, execute o seed para carregar dados de exemplo.

---

## MER

O conceitual modelo entidade-relacionamento representa o seguinte fluxo de cobrança: um devedor possui contratos, então cada contrato pode ter negociações e acionamentos e cada negociação possui parcelas, portanto, cada acionamento é registrado por um usuário. As faixas de juros definem parâmetros de cálculo e, no modelo atual, não têm relacionamento direto com contrato.

```mermaid
erDiagram
    DEVEDOR ||--o{ CONTRATO : possui
    CONTRATO ||--o{ NEGOCIACAO : recebe
    NEGOCIACAO ||--o{ PARCELA_PAGAMENTO : divide_em
    CONTRATO ||--o{ ACIONAMENTO : registra
    USUARIO ||--o{ ACIONAMENTO : realiza
    FAIXA_JUROS {
        int DiasMinimos
        decimal TaxaMensal
    }
```

## DER

O diagrama de entidade e relacionamento resume as tabelas criadas pela migration, sendo que `Id` é `uniqueidentifier` nas entidades que herdam de `BaseEntity`.

```mermaid
erDiagram
    Devedores {
        uniqueidentifier Id PK
        nvarchar Nome
        varchar Documento UK
        nvarchar Email
        varchar Telefone
        nvarchar Endereco
        datetime2 CriadoEm
    }
    Contratos {
        uniqueidentifier Id PK
        uniqueidentifier DevedorId FK
        nvarchar Numero UK
        decimal ValorOriginal
        date Vencimento
        decimal TaxaJurosMensal
        int Status
        datetime2 CriadoEm
    }
    Negociacoes {
        uniqueidentifier Id PK
        uniqueidentifier ContratoId FK
        datetime2 DataAcordo
        decimal ValorAcordado
        decimal DescontoPercentual
        int QuantidadeParcelas
        int Status
    }
    ParcelasPagamento {
        uniqueidentifier Id PK
        uniqueidentifier NegociacaoId FK
        int Numero
        date Vencimento
        decimal Valor
        datetime2 PagoEm
    }
    Acionamentos {
        uniqueidentifier Id PK
        uniqueidentifier ContratoId FK
        uniqueidentifier UsuarioId FK
        int Tipo
        nvarchar Descricao
        datetime2 RealizadoEm
    }
    Usuarios {
        uniqueidentifier Id PK
        nvarchar Nome
        nvarchar Email UK
        nvarchar SenhaHash
        int Perfil
        bit Ativo
        datetime2 CriadoEm
    }
    FaixasJuros {
        int DiasMinimos PK
        decimal TaxaMensal
    }
    Devedores ||--o{ Contratos : FK_DevedorId
    Contratos ||--o{ Negociacoes : FK_ContratoId
    Negociacoes ||--o{ ParcelasPagamento : FK_NegociacaoId
    Contratos ||--o{ Acionamentos : FK_ContratoId
    Usuarios ||--o{ Acionamentos : FK_UsuarioId
```

## Carregar Dados

Para criar a base local e carregar os dados de exemplo:

```powershell
dotnet ef database update --project src/CobrancaWeb.Infrastructure --startup-project src/CobrancaWeb.Infrastructure
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database/seed_data.sql
```
