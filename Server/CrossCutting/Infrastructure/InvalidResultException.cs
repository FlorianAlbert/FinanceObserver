namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;

public class InvalidResultException : Exception
{
    public InvalidResultException() : base("A failed result must not provide a result value.")
    {
    }
}