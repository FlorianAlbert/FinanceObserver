using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Tests;

public class HashValidatorTests
{
    private readonly Fixture _fixture;
    private readonly HashingOptions _hashingOptions;

    private readonly HashGenerator _hashGenerator;

    private readonly HashValidator _sut;

    public HashValidatorTests()
    {
        _fixture = new Fixture();
        _hashingOptions = _fixture.Create<HashingOptions>();

        _hashGenerator = new HashGenerator(_hashingOptions);

        _sut = new HashValidator(_hashingOptions);
    }

    [Fact]
    public async Task GenerateAsync_CallWithMatchingHash_ReturnsTrue()
    {
        // Arrange
        var clearString = _fixture.Create<string>();

        var hash = await _hashGenerator.GenerateAsync(clearString);

        // Act
        var validationResult = await _sut.ValidateAsync(clearString, hash);

        // Assert
        validationResult.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateAsync_CallWithNonMatchingHash_ReturnsFalse()
    {
        // Arrange
        var clearString = _fixture.Create<string>();

        var hash = _fixture.CreateMany<byte>(_hashingOptions.HashSize + _hashingOptions.SaltSize).ToArray();
        var hashString = Convert.ToBase64String(hash);

        // Act
        var validationResult = await _sut.ValidateAsync(clearString, hashString);

        // Assert
        validationResult.Should().BeFalse();
    }
}