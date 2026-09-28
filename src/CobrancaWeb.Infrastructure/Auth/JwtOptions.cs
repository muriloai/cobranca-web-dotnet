namespace CobrancaWeb.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string SecretKey { get; set; } = "CobrancaWeb_ChaveSeguraJWT_SuperSecreta_2026_@RecuperacaoCredito#";
    public string Issuer { get; set; } = "CobrancaWeb.API";
    public string Audience { get; set; } = "CobrancaWeb.Client";
    public int ExpiracaoEmMinutos { get; set; } = 480; // 8 horas padrão para turno operacional
}
