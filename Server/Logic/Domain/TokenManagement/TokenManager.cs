using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Data;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement;

public class TokenManager : ITokenManager
{
    private readonly TokenGenerationOptions _tokenGenerationOptions;

    private readonly byte[] _encodedSignatureSecret;

    public TokenManager(TokenGenerationOptions tokenGenerationOptions)
    {
        _tokenGenerationOptions = tokenGenerationOptions;
        _encodedSignatureSecret = Encoding.UTF8.GetBytes(_tokenGenerationOptions.SignatureSecret);
    }

    public Task<Result<TokenGenerationPayload>> GenerateTokens(TokenGenerationRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHandler = new JsonWebTokenHandler();

        DateTime utcNow = DateTime.UtcNow;
        DateTime expiration = utcNow.AddMinutes(_tokenGenerationOptions.AccessTokenExpirationInMinutes);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = expiration,
            NotBefore = utcNow,
            IssuedAt = utcNow,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_encodedSignatureSecret), SecurityAlgorithms.HmacSha512Signature),
            Claims = new Dictionary<string, object>()
            {
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid(),
                [JwtRegisteredClaimNames.Sub] = request.User.Id,
                [JwtRegisteredClaimNames.Email] = request.User.EmailAddress,
                [JwtRegisteredClaimNames.UniqueName] = request.User.UserName,
                [JwtRegisteredClaimNames.Name] = request.User.FullName,
                [JwtRegisteredClaimNames.GivenName] = request.User.FirstName,
                [JwtRegisteredClaimNames.FamilyName] = request.User.LastName,
                [JwtRegisteredClaimNames.Birthdate] = request.User.BirthDate
            }
        };

        string accessToken = tokenHandler.CreateToken(tokenDescriptor);

        byte[] binaryRefreshToken = RandomNumberGenerator.GetBytes(64);

        return Task.FromResult(Result.Success(new TokenGenerationPayload
        {
            AccessToken = accessToken,
            AccessTokenExpiration = expiration,
            RefreshToken = Convert.ToBase64String(binaryRefreshToken)
        }));
    }
}
