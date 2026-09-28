using CobrancaWeb.Application.DTOs;
using FluentValidation;

namespace CobrancaWeb.Application.Validators;

public sealed class NegociacaoValidator : AbstractValidator<NegociacaoInputDto>
{
    public const decimal DescontoMaximoMenos90Dias = 30.0m;
    public const int LimiteDiasAtrasoDescontoEspecial = 90;

    public NegociacaoValidator()
    {
        RuleFor(n => n.ContratoId)
            .NotEmpty().WithMessage("O contrato é obrigatório para negociação.");

        RuleFor(n => n.QuantidadeParcelas)
            .InclusiveBetween(1, 36).WithMessage("A quantidade de parcelas deve ser entre 1 e 36.");

        RuleFor(n => n.DescontoPercentual)
            .InclusiveBetween(0m, 100m).WithMessage("O desconto deve estar entre 0% e 100%.");
    }

    public static bool ValidarRegraDesconto(int diasAtraso, decimal descontoPercentual)
    {
        if (diasAtraso < LimiteDiasAtrasoDescontoEspecial)
        {
            return descontoPercentual <= DescontoMaximoMenos90Dias;
        }

        return descontoPercentual <= 100m;
    }
}
