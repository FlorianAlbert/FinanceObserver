using System.Security.Cryptography;
using System.Text;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512;

public class HashGenerator : IHashGenerator
{
    private readonly int _hashSize;
    private readonly int _iterations;
    private readonly int _saltSize;

    public HashGenerator(HashingOptions options)
    {
        _hashSize = options.HashSize;
        _saltSize = options.SaltSize;
        _iterations = options.Iterations;
    }

    private int _CombinedHashSize => _hashSize + _saltSize;

    public Task<string> GenerateAsync(string input, CancellationToken cancellationToken = default)
    {
        var salt = RandomNumberGenerator.GetBytes(_saltSize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(input),
            salt,
            _iterations,
            HashAlgorithmName.SHA512,
            _hashSize);

        var combinedHash = new byte[_CombinedHashSize];
        Buffer.BlockCopy(hash, 0, combinedHash, 0, _hashSize);
        Buffer.BlockCopy(salt, 0, combinedHash, _hashSize, _saltSize);

        var resultingHash = Convert.ToBase64String(combinedHash);
        return Task.FromResult(resultingHash);
    }
}