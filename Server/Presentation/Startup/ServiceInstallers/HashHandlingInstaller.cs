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

    public void Install(IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Adding hash handling");
        
        string? iterationsString = null;
        if (Environment.GetEnvironmentVariable(_iterationsFileEnvKey) is { } iterationsFileLocation
            && File.Exists(iterationsFileLocation))
        {
            iterationsString = File.ReadAllText(iterationsFileLocation);
        }

        iterationsString ??= Environment.GetEnvironmentVariable(_iterationsEnvKey)
                             ?? configuration[_iterationsKey];

        ArgumentException.ThrowIfNullOrEmpty(iterationsString);
        if (!int.TryParse(iterationsString, out var iterations))
        {
            throw new StartupValidationException("There was no valid iteration count found in the configuration.");
        }
        
        logger.LogTrace("HashIterations: {HashIterations}", iterations);

        string? hashSizeString = null;
        if (Environment.GetEnvironmentVariable(_hashSizeFileEnvKey) is { } hashSizeFileLocation
            && File.Exists(hashSizeFileLocation))
        {
            hashSizeString = File.ReadAllText(hashSizeFileLocation);
        }

        hashSizeString ??= Environment.GetEnvironmentVariable(_hashSizeEnvKey)
                           ?? configuration[_hashSizeKey];

        ArgumentException.ThrowIfNullOrEmpty(hashSizeString);
        if (!int.TryParse(hashSizeString, out var hashSize))
        {
            throw new StartupValidationException("There was no valid hash size found in the configuration.");
        }
        
        logger.LogTrace("HashSize: {HashSize}", hashSize);

        string? saltSizeString = null;
        if (Environment.GetEnvironmentVariable(_saltSizeFileEnvKey) is { } saltSizeFileLocation
            && File.Exists(saltSizeFileLocation))
        {
            saltSizeString = File.ReadAllText(saltSizeFileLocation);
        }

        saltSizeString ??= Environment.GetEnvironmentVariable(_saltSizeEnvKey)
                           ?? configuration[_saltSizeKey];

        ArgumentException.ThrowIfNullOrEmpty(saltSizeString);
        if (!int.TryParse(saltSizeString, out var saltSize))
        {
            throw new StartupValidationException("There was no valid salt size count found in the configuration.");
        }
        
        logger.LogTrace("SaltSize: {SaltSize}", saltSize);

        services.AddTransient<HashingOptions>(serviceProvider => new HashingOptions
        {
            Iterations = iterations,
            HashSize = hashSize,
            SaltSize = saltSize
        });

        services.AddTransient<IHashGenerator, HashGenerator>();
        services.AddTransient<IHashValidator, HashValidator>();
    }
}