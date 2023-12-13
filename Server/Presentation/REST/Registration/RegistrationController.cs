using FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract;
using FlorianAlbert.FinanceObserver.Server.Presentation.REST.Registration.Model;
using Microsoft.AspNetCore.Mvc;

namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Registration;

/// <summary>
///     Controller grouping all endpoints handling registration concerns
/// </summary>
[ApiController]
[Route("registration")]
public class RegistrationController : BaseController
{
    private readonly IRegistrationWorkflow _registrationWorkflow;

    /// <summary>
    ///     Constructor for class RegistrationController
    /// </summary>
    /// <param name="registrationWorkflow">The registration workflow implementation that starts registration processes</param>
    public RegistrationController(IRegistrationWorkflow registrationWorkflow)
    {
        _registrationWorkflow = registrationWorkflow;
    }

    /// <summary>
    ///     The endpoint that starts a registration process
    /// </summary>
    /// <param name="request">The details of the new registration</param>
    /// <param name="cancellationToken">The cancellation token to cancel the registration request</param>
    /// <returns>Code status code OK (200) if success, otherwise a ProblemDetails object</returns>
    [HttpPost("request", Name = nameof(Register))]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var registrationResult = await _registrationWorkflow.ExecuteRegistrationAsync(
            new Logic.Business.RegistrationWorkflow.Contract.Data.RegistrationRequest
            {
                Username = request.UserName,
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDate = request.BirthDate,
                EmailAddress = request.EmailAddress,
                Password = request.Password,
                ConfirmationLinkTemplate = request.ConfirmationLinkTemplate
            }, cancellationToken);

        if (registrationResult.Failed)
        {
            return ProblemDetailsFromErrors(registrationResult.Errors);
        }

        return Ok();
    }

    /// <summary>
    ///     The endpoint that confirms a pending registration
    /// </summary>
    /// <param name="request">The details of the new confirmation</param>
    /// <param name="cancellationToken">The cancellation token to cancel the registration request</param>
    /// <returns>Code status code OK (200) if success, otherwise a ProblemDetails object</returns>
    [HttpPost("confirmations", Name = nameof(Confirm))]
    public async Task<IActionResult> Confirm([FromBody] ConfirmationRequest request,
        CancellationToken cancellationToken)
    {
        var confirmationResult = await _registrationWorkflow.ExecuteConfirmationAsync(
            new Logic.Business.RegistrationWorkflow.Contract.Data.ConfirmationRequest
            {
                ConfirmationId = request.ConfirmationId
            }, cancellationToken);

        if (confirmationResult.Failed)
        {
            return ProblemDetailsFromErrors(confirmationResult.Errors);
        }

        return Ok();
    }
}