using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST;

/// <summary>
///     Internal base class implementation for all controllers that adds some functionality
/// </summary>
public class BaseController : ControllerBase
{
    /// <summary>
    ///     Returns whether the current user is authenticated
    /// </summary>
    protected bool _IsAuthenticated => (User.Identity?.IsAuthenticated ?? false) && User.Identity.Name is not null;

    /// <summary>
    ///     Returns the user id of the current user if authenticated, otherwise null
    /// </summary>
    protected Guid? _UserId => _IsAuthenticated ? Guid.Parse(User.Identity!.Name!) : null;

    /// <summary>
    ///     Method that creates a ProblemDetails response from an array of errors
    /// </summary>
    /// <param name="errors">All errors that should be contained in the ProblemsDetails response</param>
    /// <returns>The ProblemDetails response</returns>
    protected IActionResult ProblemDetailsFromErrors(params Error[] errors)
    {
        if (errors.Length is 0)
        {
            return Problem();
        }

        var problemDetails = new ProblemDetails();

        if (errors.Length is 1)
        {
            problemDetails.Type = errors[0].Type;
            problemDetails.Title = errors[0].Title;
            problemDetails.Detail = errors[0].Detail;
        }
        else
        {
            problemDetails.Type = "MultipleErrorsError";
            problemDetails.Title = "Multiple errors occured";
            problemDetails.Detail = "Multiple errors occurred while processing your request.";

            problemDetails.Extensions["errors"] = errors;
        }

        int status = errors.All(e => e.Status == errors[0].Status) ? errors[0].Status : 500;

        problemDetails.Status = status;
        return StatusCode(status, problemDetails);
    }

    /// <summary>
    ///     Method that creates a ProblemDetails response from an IEnumerable of errors
    /// </summary>
    /// <param name="errors">All errors that should be contained in the ProblemsDetails response</param>
    /// <returns>The ProblemDetails response</returns>
    protected IActionResult ProblemDetailsFromErrors(IEnumerable<Error> errors)
    {
        return ProblemDetailsFromErrors(errors as Error[] ?? errors.ToArray());
    }
}