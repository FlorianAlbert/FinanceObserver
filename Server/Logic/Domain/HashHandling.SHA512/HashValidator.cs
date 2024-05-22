using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;
using System.Security.Cryptography;
using System.Text;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512;

public class HashValidator : IHashValidator
{
    private readonly int _hashSize;
    private readonly int _iterations;
    private readonly int _saltSize;

    public HashValidator(HashingOptions options)
    {
        _hashSize = options.HashSize;
        _saltSize = options.SaltSize;
        _iterations = options.Iterations;
    }

    public Task<bool> ValidateAsync(string validated, string savedHash, CancellationToken cancellationToken = default)
    {
        byte[] combinedHash = Convert.FromBase64String(savedHash);

        Span<byte> hash = combinedHash.AsSpan(0, _hashSize);
        Span<byte> salt = combinedHash.AsSpan(_hashSize, _saltSize);

        byte[] hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(validated),
            salt,
            _iterations,
            HashAlgorithmName.SHA512,
            _hashSize);

        bool isEqual = CryptographicOperations.FixedTimeEquals(hashToCompare, hash);

        return Task.FromResult(isEqual);
    }
}