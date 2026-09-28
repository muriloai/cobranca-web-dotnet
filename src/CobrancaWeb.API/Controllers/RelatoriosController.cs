using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Domain.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class RelatoriosController(
    IRelatorioRepository relatorioRepository,
    INegociacaoRepository negociacaoRepository,
    ITermoAcordoPdfService pdfService) : ControllerBase
{
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterDashboard(CancellationToken cancellationToken)
    {
        var resumo = await relatorioRepository.DashboardAsync(cancellationToken);

        var totalGeral = resumo.ValorRecuperado + resumo.ValorInadimplente;
        var taxa = totalGeral > 0
            ? Math.Round((resumo.ValorRecuperado / totalGeral) * 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        var dto = new DashboardDto(
            resumo.ContratosInadimplentes,
            resumo.ValorInadimplente,
            resumo.ValorRecuperado,
            resumo.AcordosAtivos,
            taxa);

        return Ok(dto);
    }

    [HttpGet("cobrancas")]
    [ProducesResponseType(typeof(IReadOnlyList<CobrancaRelatorio>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RelatorioCobrancas(
        [FromQuery] DateTime? inicio,
        [FromQuery] DateTime? fim,
        CancellationToken cancellationToken)
    {
        var dataFim = (fim ?? DateTime.UtcNow).Date;
        var dataInicio = (inicio ?? dataFim.AddDays(-30)).Date;

        var relatorio = await relatorioRepository.CobrancasAsync(dataInicio, dataFim, cancellationToken);
        return Ok(relatorio);
    }

    [HttpGet("devedores-resumo")]
    [ProducesResponseType(typeof(IReadOnlyList<DevedorResumo>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumoPorDevedor(CancellationToken cancellationToken)
    {
        var resumo = await relatorioRepository.ResumoPorDevedorAsync(cancellationToken);
        return Ok(resumo);
    }

    [HttpGet("negociacoes/{id:guid}/termo-pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BaixarTermoAcordoPdf(Guid id, CancellationToken cancellationToken)
    {
        var negociacao = await negociacaoRepository.ObterAsync(id, cancellationToken);
        if (negociacao is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Negociação não encontrada",
                Detail = $"Nenhuma negociação com o identificador '{id}' foi localizada para gerar o termo."
            });
        }

        var pdfBytes = pdfService.GerarTermoAcordoPdf(negociacao);
        var nomeArquivo = $"Termo_Acordo_{negociacao.Contrato?.Numero ?? id.ToString()[..8]}.pdf";

        return File(pdfBytes, "application/pdf", nomeArquivo);
    }
}
