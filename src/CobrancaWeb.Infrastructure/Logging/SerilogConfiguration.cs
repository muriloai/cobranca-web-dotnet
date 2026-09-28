using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace CobrancaWeb.Infrastructure.Logging;

public static class SerilogConfiguration
{
    private const string FormatoConsole = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    private const string FormatoArquivo = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration CriarConfiguracaoPadrao(IConfiguration? configuration = null)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "CobrancaWeb")
            .WriteTo.Console(outputTemplate: FormatoConsole)
            .WriteTo.File(
                path: "logs/cobrancaweb-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: FormatoArquivo);

        if (configuration is not null)
        {
            config.ReadFrom.Configuration(configuration);
        }

        return config;
    }

    public static IHostBuilder UsarSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "CobrancaWeb")
                .WriteTo.Console(outputTemplate: FormatoConsole)
                .WriteTo.File(
                    path: "logs/cobrancaweb-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: FormatoArquivo);
        });
    }
}
