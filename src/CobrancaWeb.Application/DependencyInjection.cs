using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Application.Services;
using CobrancaWeb.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CobrancaWeb.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<DevedorValidator>();

        services.AddScoped<IDevedorService, DevedorService>();
        services.AddScoped<IContratoService, ContratoService>();
        services.AddScoped<INegociacaoService, NegociacaoService>();

        return services;
    }
}
