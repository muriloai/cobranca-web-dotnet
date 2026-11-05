param(
    [Parameter(Mandatory = $true)] [guid] $ContratoId,
    [string] $ConnectionString = $env:ConnectionStrings__DefaultConnection
)

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw 'Defina ConnectionStrings__DefaultConnection para a identidade da aplicação.'
}

$connection = [System.Data.SqlClient.SqlConnection]::new($ConnectionString)
$transaction = $null
try {
    $connection.Open()
    $transaction = $connection.BeginTransaction()
    $who = $connection.CreateCommand()
    $who.Transaction = $transaction
    $who.CommandText = 'SELECT ORIGINAL_LOGIN()'
    $applicationLogin = [string]$who.ExecuteScalar()

    $negociacaoId = [guid]::NewGuid()
    $procedure = $connection.CreateCommand()
    $procedure.Transaction = $transaction
    $procedure.CommandType = [System.Data.CommandType]::StoredProcedure
    $procedure.CommandText = 'dbo.sp_EfetivarNegociacao'
    $procedure.Parameters.Add('@NegociacaoId', [System.Data.SqlDbType]::UniqueIdentifier).Value = $negociacaoId
    $procedure.Parameters.Add('@ContratoId', [System.Data.SqlDbType]::UniqueIdentifier).Value = $ContratoId
    $procedure.Parameters.Add('@DataAcordo', [System.Data.SqlDbType]::DateTime2).Value = [DateTime]::UtcNow
    $procedure.Parameters.Add('@QuantidadeParcelas', [System.Data.SqlDbType]::Int).Value = 3
    $procedure.Parameters.Add('@PrimeiroVencimento', [System.Data.SqlDbType]::Date).Value = [DateTime]::Today.AddDays(30)
    $total = $procedure.Parameters.Add('@ValorAcordado', [System.Data.SqlDbType]::Decimal)
    $total.Precision = 18
    $total.Scale = 2
    $total.Value = [decimal]100
    $desconto = $procedure.Parameters.Add('@DescontoPercentual', [System.Data.SqlDbType]::Decimal)
    $desconto.Precision = 5
    $desconto.Scale = 2
    $desconto.Value = [decimal]0
    $procedure.ExecuteNonQuery() | Out-Null

    $read = $connection.CreateCommand()
    $read.Transaction = $transaction
    $read.CommandText = @'
SELECT n.ValorAcordado, COUNT(p.Id) AS Quantidade, SUM(p.Valor) AS Soma,
       STRING_AGG(CONVERT(varchar(20), p.Valor), ',') WITHIN GROUP (ORDER BY p.Numero) AS Rateio
FROM dbo.Negociacoes n
JOIN dbo.ParcelasPagamento p ON p.NegociacaoId = n.Id
WHERE n.Id = @NegociacaoId
GROUP BY n.ValorAcordado;
'@
    $read.Parameters.Add('@NegociacaoId', [System.Data.SqlDbType]::UniqueIdentifier).Value = $negociacaoId
    $reader = $read.ExecuteReader()
    if (-not $reader.Read()) {
        throw 'A procedure executou, mas o acordo não ficou visível para a identidade da aplicação.'
    }
    $valorAcordado = [decimal]$reader['ValorAcordado']
    $quantidade = [int]$reader['Quantidade']
    $soma = [decimal]$reader['Soma']
    $rateio = [string]$reader['Rateio']
    $reader.Close()

    if ($valorAcordado -ne 100 -or $quantidade -ne 3 -or $soma -ne 100 -or $rateio -ne '33.34,33.33,33.33') {
        throw "A procedure não produziu o rateio esperado: total=$valorAcordado quantidade=$quantidade soma=$soma rateio=$rateio."
    }

    $directUpdateDenied = $false
    $directUpdate = $connection.CreateCommand()
    $directUpdate.Transaction = $transaction
    $directUpdate.CommandText = 'UPDATE dbo.Contratos SET Status = 2 WHERE Id = @ContratoId'
    $directUpdate.Parameters.Add('@ContratoId', [System.Data.SqlDbType]::UniqueIdentifier).Value = $ContratoId
    try {
        $directUpdate.ExecuteNonQuery() | Out-Null
    }
    catch [System.Data.SqlClient.SqlException] {
        if ($_.Exception.Number -eq 229) {
            $directUpdateDenied = $true
        }
        else {
            throw
        }
    }
    if (-not $directUpdateDenied) {
        throw 'UPDATE direto em dbo.Contratos não foi recusado para a identidade da aplicação.'
    }

    [pscustomobject]@{
        ApplicationLogin = $applicationLogin
        Procedure = 'dbo.sp_EfetivarNegociacao executada'
        NegotiationId = $negociacaoId
        AgreementTotal = $valorAcordado
        Installments = $quantidade
        InstallmentSum = $soma
        Split = $rateio
        DirectUpdate = 'recusado (SQL Server 229)'
        Transaction = 'rollback ao terminar o ensaio'
    }
}
finally {
    if ($transaction) {
        try { $transaction.Rollback() } catch { }
    }
    $connection.Dispose()
}
