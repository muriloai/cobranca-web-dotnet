# Cobrança Web

> Sistema de cobrança em .NET 8 + SQL Server

## Estrutura

- A solution está dividida em Domain, Application, Infrastructure, API e Tests.
- Os scripts SQL criam tabelas, restrições, índices, view, procedures e dados de exemplo.
- O domínio modela devedores, contratos, negociações, parcelas, acionamentos e usuários, e também calcula atraso e juros e registra pagamentos.
- A infraestrutura usa EF Core 8 para mapear as entidades, implementar repositórios e versionar o esquema com uma migration inicial.
- A API tem apenas a estrutura básica e o Swagger em desenvolvimento, e os serviços de aplicação e os testes de negócio ainda não os tenho.

## Decisões de arquitetura

- O fluxo das dependências seguem para o domínio, que não usa EF Core nem ASP.NET.
- O padrão Repository separa o acesso a dados das regras de negócio, então os repositórios compartilham o `AppDbContext` para salvar alterações.
- Os mapeamentos usam Fluent API para definir chaves, relações, índices e restrições.
- Os scripts SQL foram criados antes da migration, para serem usados no desenvolvimento da base do sistema, portanto a implementação da migration atual com estes scripts SQL formam caminhos alternativos para criar uma base nova e não devem ser aplicados juntos na mesma base.
