using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Services;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CobrancaWeb.Tests.Services;

public class ContratoServiceTests
{
    private readonly Mock<IContratoRepository> _contratoRepositoryMock = new();
    private readonly Mock<IDevedorRepository> _devedorRepositoryMock = new();
    private readonly ContratoService _service;

    public ContratoServiceTests()
    {
        _service = new ContratoService(_contratoRepositoryMock.Object, _devedorRepositoryMock.Object);
    }

    [Fact]
    public async Task CalcularAtualizacaoAsync_ComJurosSimples_DeveCalcularCorretamente()
    {
        // Arrange: 60 dias de atraso = 2 meses financeiros. Taxa = 2% ao mês (0.02)
        var hoje = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var vencimento = hoje.AddDays(-60);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-TESTE-01",
            ValorOriginal = 1000.00m,
            Vencimento = vencimento,
            TaxaJurosMensal = 0.02m
        };

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        // Act: juros simples -> 1000 * (1 + 0.02 * 2) = 1040.00
        var resultado = await _service.CalcularAtualizacaoAsync(
            contrato.Id,
            dataReferencia: hoje,
            jurosCompostos: false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DiasAtraso.Should().Be(60);
        resultado.ValorOriginal.Should().Be(1000.00m);
        resultado.ValorJuros.Should().Be(40.00m);
        resultado.ValorTotalAtualizado.Should().Be(1040.00m);
    }

    [Fact]
    public async Task CalcularAtualizacaoAsync_ComJurosCompostos_DeveCalcularCorretamente()
    {
        // Arrange: 60 dias de atraso = 2 meses financeiros. Taxa = 2% ao mês (0.02)
        var hoje = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var vencimento = hoje.AddDays(-60);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-TESTE-02",
            ValorOriginal = 1000.00m,
            Vencimento = vencimento,
            TaxaJurosMensal = 0.02m
        };

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        // Act: juros compostos -> 1000 * (1 + 0.02)^2 = 1040.40
        var resultado = await _service.CalcularAtualizacaoAsync(
            contrato.Id,
            dataReferencia: hoje,
            jurosCompostos: true);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DiasAtraso.Should().Be(60);
        resultado.ValorTotalAtualizado.Should().Be(1040.40m);
        resultado.ValorJuros.Should().Be(40.40m);
    }

    [Fact]
    public async Task CalcularAtualizacaoAsync_ComContratoInexistente_DeveLancarKeyNotFoundException()
    {
        // Arrange
        var contratoIdInexistente = Guid.NewGuid();
        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contratoIdInexistente, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contrato?)null);

        // Act & Assert
        await _service.Invoking(s => s.CalcularAtualizacaoAsync(contratoIdInexistente))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CriarAsync_ComDevedorExistente_DeveAdicionarEPersistirContrato()
    {
        // Arrange
        var devedor = new Devedor
        {
            Id = Guid.NewGuid(),
            Nome = "Devedor Modelo",
            Documento = "52998224725"
        };

        _devedorRepositoryMock
            .Setup(r => r.ObterAsync(devedor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(devedor);

        var dto = new CriarContratoDto(
            DevedorId: devedor.Id,
            Numero: "CT-NEW-100",
            ValorOriginal: 2500m,
            Vencimento: DateTime.UtcNow.AddDays(30),
            TaxaJurosMensal: 0.015m
        );

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Numero.Should().Be("CT-NEW-100");
        resultado.ValorOriginal.Should().Be(2500m);

        _contratoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Contrato>(), It.IsAny<CancellationToken>()), Times.Once);
        _contratoRepositoryMock.Verify(r => r.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 0.01)]
    [InlineData(-100, 0.01)]
    [InlineData(1000, -0.01)]
    [InlineData(1000, 1.5)]
    public async Task CriarAsync_ComValoresInvalidos_DeveLancarArgumentException(decimal valor, decimal taxa)
    {
        // Arrange
        var devedor = new Devedor { Id = Guid.NewGuid(), Nome = "Teste" };
        _devedorRepositoryMock
            .Setup(r => r.ObterAsync(devedor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(devedor);

        var dto = new CriarContratoDto(devedor.Id, "CT-INV", valor, DateTime.UtcNow, taxa);

        // Act & Assert
        await _service.Invoking(s => s.CriarAsync(dto))
            .Should().ThrowAsync<ArgumentException>();
    }
}
