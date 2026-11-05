param(
    [Parameter(Mandatory = $true)] [string] $ApiBaseUrl,
    [Parameter(Mandatory = $true)] [string] $BearerToken,
    [Parameter(Mandatory = $true)] [guid] $NegociacaoId,
    [string] $LegacyBaseUrl
)

$base = $ApiBaseUrl.TrimEnd('/')
$headers = @{ Authorization = "Bearer $BearerToken" }
$v1Url = "$base/api/v1/negociacoes/$NegociacaoId"
$v2Url = "$base/api/v2/negociacoes/$NegociacaoId"
$v1 = Invoke-RestMethod -Method Get -Uri $v1Url -Headers $headers
$v2 = Invoke-RestMethod -Method Get -Uri $v2Url -Headers $headers

if ($v1.id -ne $NegociacaoId -or $v2.id -ne $NegociacaoId) {
    throw 'As duas versões não retornaram a mesma negociação.'
}
if ($v1.valorAcordado -ne $v2.valorAcordado) {
    throw "O total divergiu entre v1 ($($v1.valorAcordado)) e v2 ($($v2.valorAcordado))."
}
if ($v1.PSObject.Properties.Name -contains 'parcelas') {
    throw 'O contrato v1 mudou: o cliente anterior passou a receber o detalhamento de parcelas.'
}
if (-not $v2.parcelas -or $v2.parcelas.Count -ne $v2.quantidadeParcelas) {
    throw 'O contrato v2 não trouxe todas as parcelas.'
}

$somaParcelas = [decimal]::Round((($v2.parcelas | Measure-Object -Property valor -Sum).Sum), 2)
if ($somaParcelas -ne [decimal]$v2.valorAcordado) {
    throw "As parcelas somam $somaParcelas e o acordo registra $($v2.valorAcordado)."
}

if ($v2.valorAcordado -eq 100 -and $v2.quantidadeParcelas -eq 3) {
    $invariantCulture = [System.Globalization.CultureInfo]::InvariantCulture
    $rateioExato = @($v2.parcelas | Sort-Object numero | ForEach-Object { ([decimal]$_.valor).ToString('0.00', $invariantCulture) })
    if (($rateioExato -join ',') -ne '33.34,33.33,33.33') {
        throw "Rateio inesperado para R$ 100 em 3 parcelas: $($rateioExato -join ', ')."
    }
}

$legacyStatus = 'não consultado'
if ($LegacyBaseUrl) {
    $legacyUrl = "$($LegacyBaseUrl.TrimEnd('/'))/ConsultaCobranca/Negociacao/$NegociacaoId"
    $legacy = Invoke-WebRequest -Method Get -Uri $legacyUrl
    if ($legacy.StatusCode -ne 200 -or $legacy.Content -notmatch [regex]::Escape($NegociacaoId.ToString())) {
        throw 'A tela MVC 5 não apresentou a negociação informada.'
    }
    $legacyStatus = 'HTTP 200'
}

[pscustomobject]@{
    NegociacaoId = $NegociacaoId
    V1Url = $v1Url
    V1ValorAcordado = $v1.valorAcordado
    V1SemParcelas = $true
    V2Url = $v2Url
    V2Parcelas = $v2.parcelas.Count
    SomaParcelas = $somaParcelas
    Rateio = (($v2.parcelas | Sort-Object numero | ForEach-Object { '{0:N2}' -f [decimal]$_.valor }) -join ', ')
    LeituraMvc5 = $legacyStatus
    RollbackLeituraUrl = $v1Url
}
