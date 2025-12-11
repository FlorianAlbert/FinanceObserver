namespace FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;

public class Error
{
    public Error(string type, string title, string detail, int? status = null)
    {
        Type = type;
        Title = title;
        Detail = detail;
        Status = status ?? 500;
    }

    public string Type { get; }

    public string Title { get; }

    public int Status { get; }

    public string Detail { get; }

    public override string ToString()
    {
        return $"{Title} (Type: {Type}, Status: {Status}) - {Detail}";
    }
}