using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class ContratosController(IContratoService contratoService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContratoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var contrato = await contratoService.ObterPorIdAsync(id, cancellationToken);
        if (contrato is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Contrato não encontrado",
                Detail = $"Nenhum contrato com o identificador '{id}' foi localizado."
            });
        }

        return Ok(contrato);
    }

    [HttpGet("devedor/{devedorId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ContratoDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorDevedor(Guid devedorId, CancellationToken cancellationToken)
    {
        var contratos = await contratoService.ObterPorDevedorAsync(devedorId, cancellationToken);
        return Ok(contratos);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContratoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Criar([FromBody] CriarContratoDto dto, CancellationToken cancellationToken)
    {
        var contrato = await contratoService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = contrato.Id }, contrato);
    }

    [HttpGet("{id:guid}/atualizacao")]
    [ProducesResponseType(typeof(AtualizacaoFinanceiraDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalcularAtualizacao(
        Guid id,
        [FromQuery] DateTime? dataReferencia,
        [FromQuery] bool jurosCompostos = false,
        CancellationToken cancellationToken = default)
    {
        var atualizacao = await contratoService.CalcularAtualizacaoAsync(id, dataReferencia, jurosCompostos, cancellationToken);
        return Ok(atualizacao);
    }
}
