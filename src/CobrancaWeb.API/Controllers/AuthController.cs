using System.Security.Claims;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using CobrancaWeb.Domain.Interfaces;
using CobrancaWeb.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CobrancaWeb.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController(
    IUsuarioRepository usuarioRepository,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    public sealed record LoginRequest(string Email, string Senha);

    public sealed record LoginResponse(
        string Token,
        Guid Id,
        string Nome,
        string Email,
        string Perfil
    );

    public sealed record RegistrarUsuarioRequest(
        string Nome,
        string Email,
        string Senha,
        PerfilUsuario Perfil
    );

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos",
                Detail = "E-mail e senha são obrigatórios."
            });
        }

        var usuario = await usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (usuario is null || !usuario.Ativo || !PasswordHasher.Verificar(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Falha de autenticação",
                Detail = "E-mail ou senha inválidos, ou usuário inativo."
            });
        }

        var token = jwtTokenService.GerarToken(usuario);

        return Ok(new LoginResponse(
            token,
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Perfil.ToString()));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var nome = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Id = id,
            Nome = nome,
            Email = email,
            Perfil = role
        });
    }

    [HttpPost("registrar")]
    [Authorize(Roles = "Supervisor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var existente = await usuarioRepository.ObterPorEmailAsync(request.Email, cancellationToken);
        if (existente is not null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "E-mail já cadastrado",
                Detail = $"Já existe um usuário registrado com o e-mail '{request.Email}'."
            });
        }

        var novoUsuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            SenhaHash = PasswordHasher.Hash(request.Senha),
            Perfil = request.Perfil,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        await usuarioRepository.AdicionarAsync(novoUsuario, cancellationToken);
        await usuarioRepository.SalvarAsync(cancellationToken);

        return CreatedAtAction(nameof(Me), new { id = novoUsuario.Id }, new
        {
            novoUsuario.Id,
            novoUsuario.Nome,
            novoUsuario.Email,
            Perfil = novoUsuario.Perfil.ToString()
        });
    }
}
