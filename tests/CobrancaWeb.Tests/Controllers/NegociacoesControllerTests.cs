using CobrancaWeb.API.Controllers;
using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Application.Interfaces;
using CobrancaWeb.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CobrancaWeb.Tests.Controllers;

public class NegociacoesControllerTests
{
    [Fact]
    public async Task ObterPorId_MantemContratoV1ComTotalSemDetalhesDeParcelas()
    {
        var negotiation = AcordoDeCemReais();
        var service = new Mock<INegociacaoService>();
        service.Setup(item => item.ObterPorIdAsync(negotiation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(negotiation);
        var controller = new NegociacoesController(service.Object);

        var result = await controller.ObterPorId(negotiation.Id, CancellationToken.None);

        var response = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = response.Value.Should().BeOfType<NegociacaoResumoDto>().Subject;
        body.ValorAcordado.Should().Be(100m);
        typeof(NegociacaoResumoDto).GetProperty(nameof(NegociacaoDto.Parcelas)).Should().BeNull();
    }

    [Fact]
    public async Task ObterPorIdV2_ExponeParcelasQueReconciliamComOTotal()
    {
        var negotiation = AcordoDeCemReais();
        var service = new Mock<INegociacaoService>();
        service.Setup(item => item.ObterPorIdAsync(negotiation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(negotiation);
        var controller = new NegociacoesController(service.Object);

        var result = await controller.ObterPorIdV2(negotiation.Id, CancellationToken.None);

        var response = result.Should().BeOfType<OkObjectResult>().Subject;
        var body = response.Value.Should().BeOfType<NegociacaoDto>().Subject;
        body.Parcelas.Select(item => item.Valor).Should().Equal(33.34m, 33.33m, 33.33m);
        body.Parcelas.Sum(item => item.Valor).Should().Be(body.ValorAcordado);
    }

    private static NegociacaoDto AcordoDeCemReais()
    {
        var acordoId = Guid.NewGuid();
        return new NegociacaoDto(
            acordoId,
            Guid.NewGuid(),
            "CT-100",
            "Maria Silva",
            new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            100m,
            0m,
            3,
            StatusNegociacao.Ativa,
            [
                new ParcelaDto(Guid.NewGuid(), 1, new DateTime(2026, 11, 1), 33.34m, null),
                new ParcelaDto(Guid.NewGuid(), 2, new DateTime(2026, 12, 1), 33.33m, null),
                new ParcelaDto(Guid.NewGuid(), 3, new DateTime(2027, 1, 1), 33.33m, null)
            ]);
    }
}
