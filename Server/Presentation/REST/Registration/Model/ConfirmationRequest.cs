namespace FlorianAlbert.FinanceObserver.Server.Presentation.REST.Registration.Model;

/// <summary>
///     A request to confirm a pending registration
/// </summary>
public class ConfirmationRequest
{
    /// <summary>
    ///     The confirmation id to confirm
    /// </summary>
    /// <example>4fa65b7d-99b8-471d-aacc-b801ed308b6a</example>
    public required Guid ConfirmationId { get; init; }
}