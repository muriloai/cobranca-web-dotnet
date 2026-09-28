using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class NegociacoesController(INegociacaoService negociacaoService) : ControllerBase
{
    public sealed record SimularAcordoRequest(
        Guid ContratoId,
        decimal DescontoPercentual,
        int QuantidadeParcelas,
        bool UsarJurosCompostos = false,
        DateTime? DataReferencia = null
    );

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NegociacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var negociacao = await negociacaoService.ObterPorIdAsync(id, cancellationToken);
        if (negociacao is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Negociação não encontrada",
                Detail = $"Nenhuma negociação com o identificador '{id}' foi localizada."
            });
        }

        return Ok(negociacao);
    }

    [HttpPost("simular")]
    [ProducesResponseType(typeof(SimulacaoNegociacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Simular([FromBody] SimularAcordoRequest request, CancellationToken cancellationToken)
    {
        var simulacao = await negociacaoService.SimularAsync(
            request.ContratoId,
            request.DescontoPercentual,
            request.QuantidadeParcelas,
            request.UsarJurosCompostos,
            request.DataReferencia,
            cancellationToken);

        return Ok(simulacao);
    }

    [HttpPost("efetivar")]
    [ProducesResponseType(typeof(NegociacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Efetivar([FromBody] NegociacaoInputDto dto, CancellationToken cancellationToken)
    {
        var negociacao = await negociacaoService.EfetivarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = negociacao.Id }, negociacao);
    }

    [HttpPost("{id:guid}/parcelas/{parcelaId:guid}/pagamento")]
    [ProducesResponseType(typeof(NegociacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarPagamento(
        Guid id,
        Guid parcelaId,
        [FromBody] RegistrarPagamentoDto? dto,
        CancellationToken cancellationToken)
    {
        var pagoEm = dto?.DataPagamento ?? DateTime.UtcNow;
        var atualizada = await negociacaoService.RegistrarPagamentoParcelaAsync(id, parcelaId, pagoEm, cancellationToken);
        return Ok(atualizada);
    }
}
