# Host MVC 5 no IIS

Este recorte legado é **ASP.NET MVC 5 sobre .NET Framework 4.8**, com ADO.NET (`SqlConnection` / `SqlCommand`). Não é ASP clássico `.asp`.

Requisitos no Windows: .NET Framework 4.8 Developer Pack, IIS 10 com ASP.NET 4.8, .NET Extensibility, ISAPI Extensions e ISAPI Filters, .NET 8 SDK e SQL Server 2019+. A conta Windows do application pool precisa pertencer ao papel SQL `CobrancaApplicationRole`.

Publique o site:

```powershell
dotnet publish legacy/CobrancaWeb.LegacyMvc/CobrancaWeb.LegacyMvc.csproj -c Release -o legacy/publish
```

No IIS, use um application pool com CLR v4.0 e pipeline Integrated, aponte o site para `legacy/publish` e configure `CobrancaLegadoDb` em `Web.config` com Integrated Security para a conta do pool. A publicação coloca a aplicação e as dependências .NET Framework em `bin`.

Rotas mantidas pelo cliente MVC:

- `GET /ConsultaCobranca/Index?termoBusca=...`: consulta contratos em aberto.
- `POST /ConsultaCobranca/SimularAcordo`: conserva o JSON `Sucesso`, `ValorAcordo`, `Parcelas` e `ValorParcela` esperado por `legacy-cobranca.js`.
- `POST /ConsultaCobranca/EfetivarAcordo`: conserva os campos JSON `Sucesso`, `NegociacaoId` e `ValorAcordado`; grava com `dbo.sp_EfetivarNegociacao`.
- `GET /ConsultaCobranca/Negociacao/{id}`: consulta o total de um acordo antigo.

O guia de grants, migrations, contratos v1/v2, verificação do rateio e comandos HTTP/SQL está em [README.md](../README.md#negocia%C3%A7%C3%A3o-v1v2-e-consulta-mvc-5).
