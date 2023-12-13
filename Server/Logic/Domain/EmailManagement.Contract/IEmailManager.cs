using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;

public interface IEmailManager
{
    Task<Result> SendEmailAsync(Email email, CancellationToken cancellationToken = default);
}