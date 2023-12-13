namespace FlorianAlbert.FinanceObserver.Server.Startup;

public class StartupValidationException : Exception
{
    public StartupValidationException(string message) : base(message)
    {
    }

    public StartupValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}