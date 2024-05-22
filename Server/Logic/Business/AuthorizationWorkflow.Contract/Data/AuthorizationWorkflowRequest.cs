namespace FlorianAlbert.FinanceObserver.Server.Logic.Business.AuthorizationWorkflow.Contract.Data;

public class AuthorizationWorkflowRequest
{
    public required string EmailAddress { get; init; }

    public required string Password { get; init; }
}