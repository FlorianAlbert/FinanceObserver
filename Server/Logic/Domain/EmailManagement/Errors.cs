using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;

public static class Errors
{
    public static Error EmailSendUnsuccessful(string errorMessage)
    {
        return new Error("EmailSendUnsuccessful",
            "The email sending failed",
            errorMessage);
    }
}