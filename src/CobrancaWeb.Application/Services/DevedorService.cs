using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Application.Validators;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using FluentValidation;

namespace CobrancaWeb.Application.Services;

public sealed class DevedorService(
    IDevedorRepository devedorRepository,
    IValidator<CriarDevedorDto> criarValidator,
    IValidator<AtualizarDevedorDto> atualizarValidator) : IDevedorService
{
    public async Task<DevedorDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var devedor = await devedorRepository.ObterAsync(id, cancellationToken);
        return devedor is null ? null : MapearParaDto(devedor);
    }

    public async Task<DevedorDto?> ObterPorDocumentoAsync(string documento, CancellationToken cancellationToken = default)
    {
        var limpo = CpfCnpjValidator.Limpar(documento);
        var devedor = await devedorRepository.ObterPorDocumentoAsync(limpo, cancellationToken);
        return devedor is null ? null : MapearParaDto(devedor);
    }

    public async Task<IReadOnlyList<DevedorDto>> BuscarAsync(string? termo, CancellationToken cancellationToken = default)
    {
        var lista = await devedorRepository.BuscarAsync(termo, cancellationToken);
        return lista.Select(MapearParaDto).ToList();
    }

    public async Task<DevedorDto> CriarAsync(CriarDevedorDto dto, CancellationToken cancellationToken = default)
    {
        await criarValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var documentoLimpo = CpfCnpjValidator.Limpar(dto.Documento);
        var existente = await devedorRepository.ObterPorDocumentoAsync(documentoLimpo, cancellationToken);
        if (existente is not null)
        {
            throw new InvalidOperationException($"Já existe um devedor cadastrado com o documento '{dto.Documento}'.");
        }

        var devedor = new Devedor
        {
            Id = Guid.NewGuid(),
            Nome = dto.Nome.Trim(),
            Documento = documentoLimpo,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant(),
            Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : CpfCnpjValidator.Limpar(dto.Telefone),
            Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim(),
            CriadoEm = DateTime.UtcNow
        };

        await devedorRepository.AdicionarAsync(devedor, cancellationToken);
        await devedorRepository.SalvarAsync(cancellationToken);

        return MapearParaDto(devedor);
    }

    public async Task<DevedorDto> AtualizarAsync(Guid id, AtualizarDevedorDto dto, CancellationToken cancellationToken = default)
    {
        await atualizarValidator.ValidateAndThrowAsync(dto, cancellationToken);

        var devedor = await devedorRepository.ObterAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Devedor com ID '{id}' não foi encontrado.");

        devedor.Nome = dto.Nome.Trim();
        devedor.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        devedor.Telefone = string.IsNullOrWhiteSpace(dto.Telefone) ? null : CpfCnpjValidator.Limpar(dto.Telefone);
        devedor.Endereco = string.IsNullOrWhiteSpace(dto.Endereco) ? null : dto.Endereco.Trim();

        await devedorRepository.SalvarAsync(cancellationToken);

        return MapearParaDto(devedor);
    }

    private static DevedorDto MapearParaDto(Devedor devedor)
    {
        var contratos = devedor.Contratos?
            .Select(c => new ContratoResumoDto(
                c.Id,
                c.Numero,
                c.ValorOriginal,
                c.Vencimento,
                c.Status.ToString()))
            .ToList() ?? [];

        return new DevedorDto(
            devedor.Id,
            devedor.Nome,
            devedor.Documento,
            devedor.Email,
            devedor.Telefone,
            devedor.Endereco,
            devedor.CriadoEm,
            contratos);
    }
}
