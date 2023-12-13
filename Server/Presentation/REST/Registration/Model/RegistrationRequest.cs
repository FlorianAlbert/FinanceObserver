namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Registration.Model;

/// <summary>
///     A request to start a registration process
/// </summary>
public class RegistrationRequest
{
    /// <summary>
    ///     The requested user name
    /// </summary>
    /// <example>MaxMustermann</example>
    public required string UserName { get; init; }

    /// <summary>
    ///     The first name of the user
    /// </summary>
    /// <example>Max</example>
    public required string FirstName { get; init; }

    /// <summary>
    ///     The last name of the user
    /// </summary>
    /// <example>Mustermann</example>
    public required string LastName { get; init; }

    /// <summary>
    ///     A valid email address of the user <br />
    ///     This mail gets used to verify the validity of the registration request
    /// </summary>
    /// <example>Max.Mustermann@muster.com</example>
    public required string EmailAddress { get; init; }

    /// <summary>
    ///     The birth date of the user
    /// </summary>
    /// <example>1980-01-01</example>
    public required DateOnly BirthDate { get; init; }

    /// <summary>
    ///     The requested password the user wants to associate with this account
    /// </summary>
    /// <example>Password123</example>
    public required string Password { get; init; }

    /// <summary>
    ///     The template for the link to put in the confirmation email <br />
    ///     The placeholder for the confirmation id is &lt;confirmationId&gt;
    /// </summary>
    /// <example>https://example.com/registration/confirm/&lt;confirmationId&gt;</example>
    public required string ConfirmationLinkTemplate { get; init; }
}