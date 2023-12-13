namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.RegistrationWorkflow.Contract.Data;

public class RegistrationRequest
{
    public required string Username { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string EmailAddress { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required string Password { get; init; }

    public required string ConfirmationLinkTemplate { get; init; }
}