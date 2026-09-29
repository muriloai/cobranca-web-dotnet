using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CobrancaWeb.Tests.Validators;

public class NegociacaoValidatorTests
{
    private readonly NegociacaoValidator _validator = new();

    [Fact]
    public void Validar_ComDadosValidos_DevePassar()
    {
        // Arrange
        var dto = new NegociacaoInputDto(
            ContratoId: Guid.NewGuid(),
            DescontoPercentual: 20m,
            QuantidadeParcelas: 6
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_ComContratoIdVazio_DeveFalhar()
    {
        // Arrange
        var dto = new NegociacaoInputDto(
            ContratoId: Guid.Empty,
            DescontoPercentual: 10m,
            QuantidadeParcelas: 3
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NegociacaoInputDto.ContratoId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(37)]
    [InlineData(100)]
    public void Validar_ComQuantidadeParcelasInvalida_DeveFalhar(int parcelasInvalidas)
    {
        // Arrange
        var dto = new NegociacaoInputDto(
            ContratoId: Guid.NewGuid(),
            DescontoPercentual: 15m,
            QuantidadeParcelas: parcelasInvalidas
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NegociacaoInputDto.QuantidadeParcelas));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validar_ComDescontoForaDosLimites_DeveFalhar(decimal descontoInvalido)
    {
        // Arrange
        var dto = new NegociacaoInputDto(
            ContratoId: Guid.NewGuid(),
            DescontoPercentual: descontoInvalido,
            QuantidadeParcelas: 4
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NegociacaoInputDto.DescontoPercentual));
    }

    [Theory]
    [InlineData(89, 30.0, true)]
    [InlineData(89, 30.01, false)] // > 30% em < 90 dias deve falhar
    [InlineData(30, 35.0, false)] // > 30% em < 90 dias deve falhar
    [InlineData(90, 35.0, true)]  // >= 90 dias pode ultrapassar 30%
    [InlineData(180, 50.0, true)] // >= 90 dias pode ultrapassar 30%
    public void ValidarRegraDesconto_ConformeDiasAtraso_DeveValidarRegraDeNegocio(int diasAtraso, decimal desconto, bool esperadoValido)
    {
        // Act
        var valido = NegociacaoValidator.ValidarRegraDesconto(diasAtraso, desconto);

        // Assert
        valido.Should().Be(esperadoValido);
    }
}
