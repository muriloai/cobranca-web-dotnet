using System.Security.Cryptography;

namespace CobrancaWeb.Infrastructure.Auth;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, Algorithm, KeySize);

        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string hashArmazenado)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(hashArmazenado))
        {
            return false;
        }

        var partes = hashArmazenado.Split('.');
        if (partes.Length != 2)
        {
            // Suporte para credenciais legadas/demo em ambiente de teste
            return hashArmazenado == senha;
        }

        try
        {
            var salt = Convert.FromBase64String(partes[0]);
            var hashEsperado = Convert.FromBase64String(partes[1]);

            var hashEntrada = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iterations, Algorithm, KeySize);

            return CryptographicOperations.FixedTimeEquals(hashEsperado, hashEntrada);
        }
        catch
        {
            return false;
        }
    }
}
