using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.HashHandling.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.TokenManagement.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace AuthorizationWorkflow;

public class AuthorizationWorkflow : IAuthorizationWorkflow
{
    private readonly IUserManager _userManager;
    private readonly IHashValidator _hashValidator;
    private readonly ITokenManager _tokenManager;

    public AuthorizationWorkflow(IUserManager userManager, IHashValidator hashValidator, ITokenManager tokenManager)
    {
        _userManager = userManager;
        _hashValidator = hashValidator;
        _tokenManager = tokenManager;
    }

    public async Task<Result<AuthorizationPayload>> ExecuteAuthorizationAsync(AuthorizationWorkflowRequest authorizationWorkflowRequest,
        CancellationToken cancellationToken = default)
    {
        // Check if user with given email address exists
        Result<User> existsResult = await _userManager.GetUserByEmailAddressAsync(authorizationWorkflowRequest.EmailAddress, cancellationToken);

        if (existsResult.Failed)
        {
            return Result.Failure<AuthorizationPayload>(Errors.AuthorizationFailedError);
        }

        User user = existsResult.Value;

        // Check if password is correct
        bool passwordIsCorrect = await _hashValidator.ValidateAsync(authorizationWorkflowRequest.Password, user.PasswordHash, cancellationToken);

        if (!passwordIsCorrect)
        {
            return Result.Failure<AuthorizationPayload>(Errors.AuthorizationFailedError);
        }

        // Generate access and refresh token
        var tokenGenerationRequest = new TokenGenerationRequest
        {
            User = user
        };

        Result<TokenGenerationPayload> tokenGenerationResult = await _tokenManager.GenerateTokens(tokenGenerationRequest, cancellationToken);

        if (tokenGenerationResult.Failed)
        {
            return Result.Failure<AuthorizationPayload>(tokenGenerationResult.Errors);
        }

        var payload = new AuthorizationPayload
        {
            AccessToken = tokenGenerationResult.Value.AccessToken,
            RefreshToken = tokenGenerationResult.Value.RefreshToken,
            AccessTokenExpiration = tokenGenerationResult.Value.AccessTokenExpiration,
        };

        var result = Result.Success(payload);

        return result;
    }
}