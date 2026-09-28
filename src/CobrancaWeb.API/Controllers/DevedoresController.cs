using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public sealed class DevedoresController(IDevedorService devedorService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DevedorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? termo, CancellationToken cancellationToken)
    {
        var devedores = await devedorService.BuscarAsync(termo, cancellationToken);
        return Ok(devedores);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DevedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var devedor = await devedorService.ObterPorIdAsync(id, cancellationToken);
        if (devedor is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Devedor não encontrado",
                Detail = $"Nenhum devedor com o identificador '{id}' foi localizado."
            });
        }

        return Ok(devedor);
    }

    [HttpGet("documento/{documento}")]
    [ProducesResponseType(typeof(DevedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorDocumento(string documento, CancellationToken cancellationToken)
    {
        var devedor = await devedorService.ObterPorDocumentoAsync(documento, cancellationToken);
        if (devedor is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Devedor não encontrado",
                Detail = $"Nenhum devedor com o documento '{documento}' foi localizado."
            });
        }

        return Ok(devedor);
    }

    [HttpPost]
    [ProducesResponseType(typeof(DevedorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarDevedorDto dto, CancellationToken cancellationToken)
    {
        var criado = await devedorService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DevedorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarDevedorDto dto, CancellationToken cancellationToken)
    {
        var atualizado = await devedorService.AtualizarAsync(id, dto, cancellationToken);
        return Ok(atualizado);
    }
}
