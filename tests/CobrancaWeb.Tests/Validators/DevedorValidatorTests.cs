using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Validators;
using FluentAssertions;
using Xunit;

namespace CobrancaWeb.Tests.Validators;

public class DevedorValidatorTests
{
    private readonly DevedorValidator _validator = new();

    [Theory]
    [InlineData("52998224725")] // CPF válido sem pontuação
    [InlineData("529.982.247-25")] // CPF válido com pontuação
    [InlineData("11222333000181")] // CNPJ válido sem pontuação
    [InlineData("11.222.333/0001-81")] // CNPJ válido com pontuação
    public void Validar_ComDocumentoValido_DevePassar(string documento)
    {
        // Arrange
        var dto = new CriarDevedorDto(
            Nome: "João da Silva",
            Documento: documento,
            Email: "joao@exemplo.com",
            Telefone: "(11) 98765-4321",
            Endereco: "Rua Teste, 100"
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("11111111111")] // Sequência repetida
    [InlineData("111.111.111-11")] // Sequência repetida com pontuação
    [InlineData("12345678900")] // Dígito verificador incorreto
    [InlineData("123")] // Tamanho incompatível
    [InlineData("")] // Vazio
    public void Validar_ComCpfInvalido_DeveFalhar(string cpfInvalido)
    {
        // Arrange
        var dto = new CriarDevedorDto(
            Nome: "Carlos Alberto",
            Documento: cpfInvalido,
            Email: "carlos@exemplo.com",
            Telefone: "11988887777",
            Endereco: null
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarDevedorDto.Documento));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("AB")] // Menos de 3 caracteres
    public void Validar_ComNomeInvalido_DeveFalhar(string nomeInvalido)
    {
        // Arrange
        var dto = new CriarDevedorDto(
            Nome: nomeInvalido,
            Documento: "52998224725",
            Email: "teste@exemplo.com",
            Telefone: null,
            Endereco: null
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarDevedorDto.Nome));
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("teste@")]
    [InlineData("@dominio.com")]
    public void Validar_ComEmailInvalido_DeveFalhar(string emailInvalido)
    {
        // Arrange
        var dto = new CriarDevedorDto(
            Nome: "Maria Santos",
            Documento: "52998224725",
            Email: emailInvalido,
            Telefone: null,
            Endereco: null
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarDevedorDto.Email));
    }

    [Theory]
    [InlineData("123")] // Menos de 8 dígitos
    [InlineData("12345678901234567")] // Mais de 15 dígitos
    public void Validar_ComTelefoneInvalido_DeveFalhar(string telefoneInvalido)
    {
        // Arrange
        var dto = new CriarDevedorDto(
            Nome: "Maria Santos",
            Documento: "52998224725",
            Email: "maria@exemplo.com",
            Telefone: telefoneInvalido,
            Endereco: null
        );

        // Act
        var result = _validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CriarDevedorDto.Telefone));
    }
}
