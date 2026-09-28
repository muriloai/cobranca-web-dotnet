using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Auth;
using CobrancaWeb.Infrastructure.Data;
using CobrancaWeb.Infrastructure.Reports;
using CobrancaWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CobrancaWeb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=CobrancaWeb;Trusted_Connection=True;MultipleActiveResultSets=true";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton(new DbConnectionFactory(configuration));

        // Repositórios EF Core & Dapper
        services.AddScoped<IDevedorRepository, DevedorRepository>();
        services.AddScoped<IContratoRepository, ContratoRepository>();
        services.AddScoped<INegociacaoRepository, NegociacaoRepository>();
        services.AddScoped<IAcionamentoRepository, AcionamentoRepository>();
        services.AddScoped<IRelatorioRepository, RelatorioDapperRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Autenticação JWT
        var jwtOptions = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || jwtOptions.SecretKey.Length < 32)
        {
            jwtOptions.SecretKey = "CobrancaWeb_ChaveSeguraJWT_SuperSecreta_2026_@RecuperacaoCredito#";
        }
        services.AddSingleton(jwtOptions);
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Geração de Relatórios em PDF
        services.AddScoped<ITermoAcordoPdfService, TermoAcordoPdfService>();

        return services;
    }
}
