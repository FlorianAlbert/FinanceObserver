using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;

public interface IEmailManager
{
    Task<Result> SendEmailAsync(Email email, CancellationToken cancellationToken = default);
}