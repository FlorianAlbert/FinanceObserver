using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Data;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.SHA512.Tests;

public class HashGeneratorTests
{
    private readonly Fixture _fixture;
    private readonly HashingOptions _hashingOptions;

    private readonly HashGenerator _sut;

    public HashGeneratorTests()
    {
        _fixture = new Fixture();
        _hashingOptions = _fixture.Create<HashingOptions>();

        _sut = new HashGenerator(_hashingOptions);
    }

    [Fact]
    public async Task GenerateAsync_Call_ReturnedHashIsNotEmpty()
    {
        // Arrange
        var clearString = _fixture.Create<string>();

        // Act
        var hashedString = await _sut.GenerateAsync(clearString);

        // Assert
        hashedString.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateAsync_Call_ReturnedHashStringSizeIsHashSizePlusSaltSizeInBase64()
    {
        // Arrange
        var clearString = _fixture.Create<string>();

        // Act
        var hashedString = await _sut.GenerateAsync(clearString);

        // Assert
        var expectedLength =
            (int)Math.Ceiling((double)(_hashingOptions.HashSize + _hashingOptions.SaltSize) / 3) *
            4; // Divide by 3 and times 4 because of the Base64 String we get back
        hashedString.Should().HaveLength(expectedLength);
    }

    [Fact]
    public async Task GenerateAsync_Call_ReturnedHashStringIsDifferentFromClearString()
    {
        // Arrange
        var clearString = _fixture.Create<string>();

        // Act
        var hashedString = await _sut.GenerateAsync(clearString);

        // Assert
        hashedString.Should().NotBe(clearString);
    }
}