using CobrancaWeb.Application.DTOs;
using CobrancaWeb.Domain.Entities;

namespace CobrancaWeb.Application.Interfaces;

public interface ITermoAcordoPdfService
{
    byte[] GerarTermoAcordoPdf(Negociacao negociacao);
    byte[] GerarTermoAcordoPdf(
        NegociacaoDto negociacao,
        string devedorNome,
        string devedorDocumento,
        string? devedorEmail,
        string? devedorTelefone,
        decimal valorOriginal,
        int diasAtraso);
}
