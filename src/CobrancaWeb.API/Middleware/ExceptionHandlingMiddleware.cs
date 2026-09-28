using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocorreu uma exceção não tratada na requisição HTTP: {Method} {Path}", context.Request.Method, context.Request.Path);
            await TratarExcecaoAsync(context, ex);
        }
    }

    private static async Task TratarExcecaoAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            ValidationException valEx => CriarValidationProblemDetails(context, valEx),
            KeyNotFoundException notFoundEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Recurso não encontrado",
                Detail = notFoundEx.Message,
                Instance = context.Request.Path
            },
            InvalidOperationException opEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Regra de negócio violada",
                Detail = opEx.Message,
                Instance = context.Request.Path
            },
            ArgumentOutOfRangeException outOfRangeEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Valor fora dos limites permitidos",
                Detail = outOfRangeEx.Message,
                Instance = context.Request.Path
            },
            ArgumentException argEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Argumento inválido",
                Detail = argEx.Message,
                Instance = context.Request.Path
            },
            UnauthorizedAccessException authEx => new ProblemDetails
            {
                Status = (int)HttpStatusCode.Unauthorized,
                Title = "Acesso não autorizado",
                Detail = authEx.Message,
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Erro interno do servidor",
                Detail = "Ocorreu um erro inesperado ao processar a requisição. Contate o suporte técnico.",
                Instance = context.Request.Path
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        var json = JsonSerializer.Serialize(problemDetails, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CriarValidationProblemDetails(HttpContext context, ValidationException valEx)
    {
        var erros = valEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(erros)
        {
            Status = (int)HttpStatusCode.BadRequest,
            Title = "Erro de validação dos dados de entrada",
            Detail = "Um ou mais campos contêm valores inválidos. Verifique a lista de erros para mais detalhes.",
            Instance = context.Request.Path
        };

        return problemDetails;
    }
}
