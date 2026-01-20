#nullable enable
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using DanceAcademy.Application.Interfaces;

namespace DanceAcademy.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña es obligatoria.", nameof(password));

        byte[] salt = RandomNumberGenerator.GetBytes(16);

        var hash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32));

        return $"{Convert.ToBase64String(salt)}.{hash}";
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        var parts = hash.Split('.');
        if (parts.Length != 2) return false;

        if (!TryFromBase64(parts[0], out var salt)) return false;

        var computed = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32));

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computed),
            Convert.FromBase64String(parts[1]));
    }

    private static bool TryFromBase64(string input, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromBase64String(input);
            return true;
        }
        catch
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }
}
