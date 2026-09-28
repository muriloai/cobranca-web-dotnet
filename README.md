# Cobrança Web

> Sistema de cobrança em .NET 8 + SQL Server

## Estrutura

- A solution está dividida em Domain, Application, Infrastructure, API e Tests.
- A migration EF Core cria tabelas, restrições, índices, view e procedures, então o script `database/seed_data.sql` insere dados de exemplo.
- O domínio modela devedores, contratos, negociações, parcelas, acionamentos e usuários, e também calcula atraso e juros e registra pagamentos.
- A infraestrutura usa EF Core 8 para mapear as entidades, implementar repositórios e versionar o esquema com uma migration inicial.
- A API tem apenas a estrutura básica e o Swagger em desenvolvimento, e os serviços de aplicação.

## Arquitetura

O backend segue Clean Architecture, com as dependências apontando para dentro:

`API → Infrastructure → Application → Domain`

O Domain concentra entidades e regras de negócio e não depende de EF Core nem de ASP.NET, já a Application depende do Domain, e a Infrastructure implementa persistência e relatórios, por fim, A API recebe as requisições e é responsável pela configuração e resolução das dependências da aplicação.

## Decisões e padrões

- Repository separa o acesso a dados das regras de negócio. Os repositórios EF Core compartilham o `AppDbContext` para salvar alterações.
- EF Core 8 usa Fluent API para mapear chaves, relações, índices e restrições. Dapper atende consultas de relatórios e stored procedures.
- A migration é a fonte do esquema todo do banco e depois de aplicá-la, execute o seed para carregar dados de exemplo.

---

## Rodar

Para criar a base local e carregar os dados de exemplo:

```powershell
dotnet ef database update --project src/CobrancaWeb.Infrastructure --startup-project src/CobrancaWeb.Infrastructure
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database/seed_data.sql
```
