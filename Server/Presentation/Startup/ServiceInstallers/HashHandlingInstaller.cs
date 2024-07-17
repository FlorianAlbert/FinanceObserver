using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;

namespace FlorianAlbert.FinanceObserver.Server.Startup.ServiceInstallers;

internal class HashHandlingInstaller : IServiceInstaller
{
    private const string _iterationsEnvKey = "FINANCE_OBSERVER_HASHING_ITERATIONS";
    private const string _iterationsFileEnvKey = "FINANCE_OBSERVER_HASHING_ITERATIONS_FILE";
    private const string _iterationsKey = "HashingOptions:Iterations";
    private const string _hashSizeEnvKey = "FINANCE_OBSERVER_HASHING_HASH_SIZE";
    private const string _hashSizeFileEnvKey = "FINANCE_OBSERVER_HASHING_HASH_SIZE_FILE";
    private const string _hashSizeKey = "HashingOptions:HashSize";
    private const string _saltSizeEnvKey = "FINANCE_OBSERVER_HASHING_SALT_SIZE";
    private const string _saltSizeFileEnvKey = "FINANCE_OBSERVER_HASHING_SALT_SIZE_FILE";
    private const string _saltSizeKey = "HashingOptions:SaltSize";

    public void Install(IHostApplicationBuilder builder, ILogger logger)
    {
        logger.LogInformation("Adding hash handling");
        
        string? iterationsString = null;
        if (Environment.GetEnvironmentVariable(_iterationsFileEnvKey) is { } iterationsFileLocation
            && File.Exists(iterationsFileLocation))
        {
            iterationsString = File.ReadAllText(iterationsFileLocation);
        }

        iterationsString ??= Environment.GetEnvironmentVariable(_iterationsEnvKey)
                             ?? builder.Configuration[_iterationsKey];

        ArgumentException.ThrowIfNullOrEmpty(iterationsString);
        if (!int.TryParse(iterationsString, out var iterations))
        {
            throw new StartupValidationException("There was no valid iteration count found in the configuration.");
        }

        string? hashSizeString = null;
        if (Environment.GetEnvironmentVariable(_hashSizeFileEnvKey) is { } hashSizeFileLocation
            && File.Exists(hashSizeFileLocation))
        {
            hashSizeString = File.ReadAllText(hashSizeFileLocation);
        }

        hashSizeString ??= Environment.GetEnvironmentVariable(_hashSizeEnvKey)
                           ?? builder.Configuration[_hashSizeKey];

        ArgumentException.ThrowIfNullOrEmpty(hashSizeString);
        if (!int.TryParse(hashSizeString, out var hashSize))
        {
            throw new StartupValidationException("There was no valid hash size found in the configuration.");
        }

        string? saltSizeString = null;
        if (Environment.GetEnvironmentVariable(_saltSizeFileEnvKey) is { } saltSizeFileLocation
            && File.Exists(saltSizeFileLocation))
        {
            saltSizeString = File.ReadAllText(saltSizeFileLocation);
        }

        saltSizeString ??= Environment.GetEnvironmentVariable(_saltSizeEnvKey)
                           ?? builder.Configuration[_saltSizeKey];

        ArgumentException.ThrowIfNullOrEmpty(saltSizeString);
        if (!int.TryParse(saltSizeString, out var saltSize))
        {
            throw new StartupValidationException("There was no valid salt size count found in the configuration.");
        }

        builder.Services.AddTransient(_ => new HashingOptions
        {
            Iterations = iterations,
            HashSize = hashSize,
            SaltSize = saltSize
        });

        builder.Services.AddTransient<IHashGenerator, HashGenerator>();
        builder.Services.AddTransient<IHashValidator, HashValidator>();
    }
}