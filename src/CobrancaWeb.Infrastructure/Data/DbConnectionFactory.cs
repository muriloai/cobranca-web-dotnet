using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CobrancaWeb.Infrastructure.Data;

public sealed class DbConnectionFactory(IConfiguration configuration)
{
    public SqlConnection Criar()
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não foi configurada.");

        return new SqlConnection(connectionString);
    }
}
