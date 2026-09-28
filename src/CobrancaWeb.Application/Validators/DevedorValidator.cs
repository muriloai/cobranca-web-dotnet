using CobrancaWeb.Application.DTOs;
using FluentValidation;

namespace CobrancaWeb.Application.Validators;

public sealed class DevedorValidator : AbstractValidator<CriarDevedorDto>
{
    public DevedorValidator()
    {
        RuleFor(d => d.Nome)
            .NotEmpty().WithMessage("O nome do devedor é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve conter pelo menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome deve conter no máximo 150 caracteres.");

        RuleFor(d => d.Documento)
            .NotEmpty().WithMessage("O CPF ou CNPJ é obrigatório.")
            .Must(CpfCnpjValidator.Validar).WithMessage("O CPF ou CNPJ informado é inválido.");

        When(d => !string.IsNullOrWhiteSpace(d.Email), () =>
        {
            RuleFor(d => d.Email)
                .EmailAddress().WithMessage("O e-mail informado não possui formato válido.")
                .MaximumLength(150).WithMessage("O e-mail deve conter no máximo 150 caracteres.");
        });

        When(d => !string.IsNullOrWhiteSpace(d.Telefone), () =>
        {
            RuleFor(d => d.Telefone)
                .Must(tel =>
                {
                    var limpo = CpfCnpjValidator.Limpar(tel);
                    return limpo.Length >= 8 && limpo.Length <= 15;
                })
                .WithMessage("O telefone deve conter entre 8 e 15 dígitos numéricos.")
                .MaximumLength(20).WithMessage("O telefone deve conter no máximo 20 caracteres.");
        });

        When(d => !string.IsNullOrWhiteSpace(d.Endereco), () =>
        {
            RuleFor(d => d.Endereco)
                .MaximumLength(250).WithMessage("O endereço deve conter no máximo 250 caracteres.");
        });
    }
}

public sealed class AtualizarDevedorValidator : AbstractValidator<AtualizarDevedorDto>
{
    public AtualizarDevedorValidator()
    {
        RuleFor(d => d.Nome)
            .NotEmpty().WithMessage("O nome do devedor é obrigatório.")
            .MinimumLength(3).WithMessage("O nome deve conter pelo menos 3 caracteres.")
            .MaximumLength(150).WithMessage("O nome deve conter no máximo 150 caracteres.");

        When(d => !string.IsNullOrWhiteSpace(d.Email), () =>
        {
            RuleFor(d => d.Email)
                .EmailAddress().WithMessage("O e-mail informado não possui formato válido.")
                .MaximumLength(150).WithMessage("O e-mail deve conter no máximo 150 caracteres.");
        });

        When(d => !string.IsNullOrWhiteSpace(d.Telefone), () =>
        {
            RuleFor(d => d.Telefone)
                .Must(tel =>
                {
                    var limpo = CpfCnpjValidator.Limpar(tel);
                    return limpo.Length >= 8 && limpo.Length <= 15;
                })
                .WithMessage("O telefone deve conter entre 8 e 15 dígitos numéricos.")
                .MaximumLength(20).WithMessage("O telefone deve conter no máximo 20 caracteres.");
        });
    }
}
