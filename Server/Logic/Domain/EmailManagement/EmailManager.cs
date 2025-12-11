using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Email = FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models.Email;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;

public class EmailManager : IEmailManager
{
    private readonly IFluentEmailFactory _fluentEmailFactory;
    private readonly IRepository<Guid, Email> _repository;

    public EmailManager(IRepositoryFactory repositoryFactory, IFluentEmailFactory fluentEmailFactory)
    {
        _repository = repositoryFactory.CreateRepository<Guid, Email>();
        _fluentEmailFactory = fluentEmailFactory;
    }

    public async Task<Result> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        await _repository.InsertAsync(email, cancellationToken);

        SendResponse sendResponse = await _fluentEmailFactory
            .Create()
            .To(email.Receivers.Select(user => new Address
            {
                Name = user.FullName,
                EmailAddress = user.Email
            }))
            .Subject(email.Subject)
            .Body(email.Message)
            .SendAsync(cancellationToken);

        if (!sendResponse.Successful)
        {
            return Result.Failure(Errors.EmailSendUnsuccessful(
                sendResponse.ErrorMessages.Aggregate(
                        "The following errors occured: ",
                        (current, next) => current + $"\n\t- {next},")
                    .TrimEnd(',')
            ));
        }

        return Result.Success();
    }
}