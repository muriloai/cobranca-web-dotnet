using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Services;
using CobrancaWeb.Application.Validators;
using CobrancaWeb.Domain.Entities;
using CobrancaWeb.Domain.Enums;
using CobrancaWeb.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CobrancaWeb.Tests.Services;

public class NegociacaoServiceTests
{
    private readonly Mock<INegociacaoRepository> _negociacaoRepositoryMock = new();
    private readonly Mock<IContratoRepository> _contratoRepositoryMock = new();
    private readonly NegociacaoValidator _validator = new();
    private readonly NegociacaoService _service;

    public NegociacaoServiceTests()
    {
        _service = new NegociacaoService(
            _negociacaoRepositoryMock.Object,
            _contratoRepositoryMock.Object,
            _validator);
    }

    [Fact]
    public async Task SimularAsync_ComMenosDe90DiasEAtrasoEDescontoMaiorQue30Porcento_DeveLancarExcecao()
    {
        // Arrange: 45 dias de atraso (< 90 dias) e tentativa de 35% de desconto (> 30%)
        var hoje = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var vencimento = hoje.AddDays(-45);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-001",
            ValorOriginal = 2000m,
            Vencimento = vencimento,
            TaxaJurosMensal = 0.01m
        };

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        // Act & Assert: Regra de corte obrigatória da recuperação de crédito
        var act = async () => await _service.SimularAsync(
            contrato.Id,
            descontoPercentual: 35.0m,
            parcelas: 3,
            dataReferencia: hoje);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*menos de 90 dias*desconto máximo permitido é de 30*");
    }

    [Fact]
    public async Task SimularAsync_ComMaisDe90DiasEAtrasoEDescontoMaiorQue30Porcento_DevePermitir()
    {
        // Arrange: 120 dias de atraso (>= 90 dias) e desconto de 40%
        var hoje = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var vencimento = hoje.AddDays(-120);

        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-002",
            ValorOriginal = 3000m,
            Vencimento = vencimento,
            TaxaJurosMensal = 0.01m
        };

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        // Act
        var resultado = await _service.SimularAsync(
            contrato.Id,
            descontoPercentual: 40.0m,
            parcelas: 4,
            dataReferencia: hoje);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DiasAtraso.Should().Be(120);
        resultado.DescontoPercentual.Should().Be(40.0m);
        resultado.ValorFinal.Should().BeLessThan(resultado.ValorCorrigido);
        resultado.ParcelasEstimadas.Should().HaveCount(4);
    }

    [Fact]
    public async Task SimularAsync_ComContratoJaNegociado_DeveLancarExcecao()
    {
        // Arrange
        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-003",
            ValorOriginal = 1500m,
            Vencimento = DateTime.UtcNow.AddDays(-30)
        };
        contrato.MarcarComoNegociado(); // Status = Negociado

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        // Act & Assert
        var act = async () => await _service.SimularAsync(contrato.Id, 10m, 2);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Apenas contratos em aberto podem ser negociados.");
    }

    [Fact]
    public async Task EfetivarAsync_DeveRatearParcelasCorretamenteEAjustarDizimaDeCentavos()
    {
        // Arrange: R$ 1000 dividido em 3 parcelas (dízima 333,33...)
        // Primeira parcela deve receber o centavo de ajuste (333.34) e a soma de todas deve ser rigorosamente 1000.00
        var vencimento = DateTime.UtcNow.AddDays(-100);
        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-004",
            ValorOriginal = 1000m,
            Vencimento = vencimento,
            TaxaJurosMensal = 0.0m // Sem juros para validar divisão exata
        };

        _contratoRepositoryMock
            .Setup(r => r.ObterAsync(contrato.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contrato);

        var dto = new NegociacaoInputDto(
            ContratoId: contrato.Id,
            DescontoPercentual: 0m,
            QuantidadeParcelas: 3
        );

        // Act
        var resultado = await _service.EfetivarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.ValorAcordado.Should().Be(1000.00m);
        resultado.Parcelas.Should().HaveCount(3);

        // A soma das parcelas tem que bater exatamente com o valor total acordado
        var somaParcelas = resultado.Parcelas.Sum(p => p.Valor);
        somaParcelas.Should().Be(1000.00m);

        // A primeira parcela absorve o ajuste de centavos
        resultado.Parcelas[0].Valor.Should().Be(333.34m);
        resultado.Parcelas[1].Valor.Should().Be(333.33m);
        resultado.Parcelas[2].Valor.Should().Be(333.33m);

        // O contrato deve ter transitado para o status 'Negociado'
        contrato.Status.Should().Be(StatusContrato.Negociado);

        _negociacaoRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Negociacao>(), It.IsAny<CancellationToken>()), Times.Once);
        _negociacaoRepositoryMock.Verify(r => r.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarPagamentoParcelaAsync_AoPagarUltimaParcela_DeveLiquidarContratoEConcluirNegociacao()
    {
        // Arrange
        var contrato = new Contrato
        {
            Id = Guid.NewGuid(),
            Numero = "CT-005",
            ValorOriginal = 1000m,
            Vencimento = DateTime.UtcNow.AddDays(-50)
        };
        contrato.MarcarComoNegociado();

        var negociacao = new Negociacao
        {
            Id = Guid.NewGuid(),
            ContratoId = contrato.Id,
            Contrato = contrato,
            ValorAcordado = 1000m,
            QuantidadeParcelas = 2,
            DescontoPercentual = 0m
        };

        var parcela1 = new ParcelaPagamento
        {
            Id = Guid.NewGuid(),
            NegociacaoId = negociacao.Id,
            Negociacao = negociacao,
            Numero = 1,
            Vencimento = DateTime.UtcNow.AddDays(30),
            Valor = 500m
        };

        var parcela2 = new ParcelaPagamento
        {
            Id = Guid.NewGuid(),
            NegociacaoId = negociacao.Id,
            Negociacao = negociacao,
            Numero = 2,
            Vencimento = DateTime.UtcNow.AddDays(60),
            Valor = 500m
        };

        negociacao.Parcelas.Add(parcela1);
        negociacao.Parcelas.Add(parcela2);

        // Parcela 1 já está paga
        parcela1.RegistrarPagamento(DateTime.UtcNow.AddDays(-5));

        _negociacaoRepositoryMock
            .Setup(r => r.ObterAsync(negociacao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(negociacao);

        // Act: Paga a parcela 2 (última restante)
        var resultado = await _service.RegistrarPagamentoParcelaAsync(
            negociacao.Id,
            parcela2.Id,
            DateTime.UtcNow);

        // Assert
        resultado.Should().NotBeNull();
        parcela2.PagoEm.Should().NotBeNull();

        // Como todas as parcelas foram quitadas, a negociação deve ser Concluída e o contrato Liquidado
        negociacao.Status.Should().Be(StatusNegociacao.Concluida);
        contrato.Status.Should().Be(StatusContrato.Liquidado);

        _negociacaoRepositoryMock.Verify(r => r.SalvarAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
