using System.Text.RegularExpressions;

namespace CobrancaWeb.Application.Validators;

public static partial class CpfCnpjValidator
{
    [GeneratedRegex(@"\D")]
    private static partial Regex ApenasDigitosRegex();

    public static string Limpar(string? documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
        {
            return string.Empty;
        }

        return ApenasDigitosRegex().Replace(documento, "");
    }

    public static bool Validar(string? documento)
    {
        var limpo = Limpar(documento);

        return limpo.Length switch
        {
            11 => ValidarCpf(limpo),
            14 => ValidarCnpj(limpo),
            _ => false
        };
    }

    public static bool ValidarCpf(string? cpf)
    {
        var limpo = Limpar(cpf);

        if (limpo.Length != 11)
        {
            return false;
        }

        // Rejeita sequências repetidas como 111.111.111-11
        if (new string(limpo[0], 11) == limpo)
        {
            return false;
        }

        var numeros = limpo.Select(c => c - '0').ToArray();

        // Primeiro dígito verificador
        var soma = 0;
        for (var i = 0; i < 9; i++)
        {
            soma += numeros[i] * (10 - i);
        }

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[9] != digito1)
        {
            return false;
        }

        // Segundo dígito verificador
        soma = 0;
        for (var i = 0; i < 10; i++)
        {
            soma += numeros[i] * (11 - i);
        }

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return numeros[10] == digito2;
    }

    public static bool ValidarCnpj(string? cnpj)
    {
        var limpo = Limpar(cnpj);

        if (limpo.Length != 14)
        {
            return false;
        }

        // Rejeita sequências repetidas
        if (new string(limpo[0], 14) == limpo)
        {
            return false;
        }

        var numeros = limpo.Select(c => c - '0').ToArray();

        int[] multiplicadores1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var soma = 0;
        for (var i = 0; i < 12; i++)
        {
            soma += numeros[i] * multiplicadores1[i];
        }

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (numeros[12] != digito1)
        {
            return false;
        }

        int[] multiplicadores2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        soma = 0;
        for (var i = 0; i < 13; i++)
        {
            soma += numeros[i] * multiplicadores2[i];
        }

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return numeros[13] == digito2;
    }
}
