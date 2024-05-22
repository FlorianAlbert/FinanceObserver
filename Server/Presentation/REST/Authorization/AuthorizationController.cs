using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract.Data;
using FlorianAlbert.FinanceObserver.Server.Presentation.REST.Authorization.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Authorization;

/// <summary>
///     Controller grouping all endpoints handling authorization concerns
/// </summary>
[ApiController]
[Route("authorization")]
public class AuthorizationController : BaseController
{
    private readonly IAuthorizationWorkflow _authorizationWorkflow;

    /// <summary>
    ///     Constructor for class AuthorizationController
    /// </summary>
    /// <param name="authorizationWorkflow">The authorization workflow implementation for handling authorizations</param>
    public AuthorizationController(IAuthorizationWorkflow authorizationWorkflow)
    {
        _authorizationWorkflow = authorizationWorkflow;
    }

    /// <summary>
    ///     The endpoint that handles the authorization of a user
    /// </summary>
    /// <param name="request">The details of the authorization request</param>
    /// <param name="cancellationToken">The cancellation token to cancel the authorization request</param>
    /// <returns></returns>
    public async Task<IActionResult> Authorize(AuthorizationRequest request, CancellationToken cancellationToken)
    {
        if (_IsAuthenticated)
        {
            return ProblemDetailsFromErrors(new Error(
                "AlreadyAuthenticatedError",
                "Already authenticated",
                "You are already authenticated, please log out first before authorizing again."));
        }

        Result<AuthorizationPayload> authorizationResult = await _authorizationWorkflow.ExecuteAuthorizationAsync(
            new AuthorizationWorkflowRequest
            {
                EmailAddress = request.EmailAddress,
                Password = request.Password
            }, cancellationToken);

        if (authorizationResult.Failed)
        {
            return ProblemDetailsFromErrors(authorizationResult.Errors);
        }

        return Ok(new AuthorizationResponse 
        { 
            AccessToken = authorizationResult.Value.AccessToken,
            RefreshToken = authorizationResult.Value.RefreshToken,
            AccessTokenExpiration = authorizationResult.Value.AccessTokenExpiration
        });
    }
}