using System.Diagnostics;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Contract;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Email = FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models.Email;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement;

public class EmailManager : IEmailManager
{
    private static readonly ActivitySource _activity = new(typeof(EmailManager).FullName ?? nameof(EmailManager));
    private readonly IFluentEmailFactory _fluentEmailFactory;
    private readonly IRepository<Guid, Email> _repository;

    public EmailManager(IRepositoryFactory repositoryFactory, IFluentEmailFactory fluentEmailFactory)
    {
        _repository = repositoryFactory.CreateRepository<Guid, Email>();
        _fluentEmailFactory = fluentEmailFactory;
    }

    public async Task<Result> SendEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        using Activity? activity = _activity.StartActivity("EmailManager.SendEmail", ActivityKind.Internal);
        activity?.SetTag("email.recipient_count", email.Receivers.Count);
        activity?.SetTag("email.subject", email.Subject);

        Email storedEmail = await _repository.InsertAsync(email, cancellationToken);

        activity?.SetTag("email.id", storedEmail.Id);

        using Activity? sendActivity = _activity.StartActivity("EmailManager.FluentEmailSend", ActivityKind.Internal);
        SendResponse sendResponse = await _fluentEmailFactory
            .Create()
            .To(storedEmail.Receivers.Select(user => new Address
            {
                Name = user.FullName,
                EmailAddress = user.Email
            }))
            .Subject(storedEmail.Subject)
            .Body(storedEmail.Message)
            .SendAsync(cancellationToken);
        sendActivity?.SetTag("email.send.success", sendResponse.Successful);

        if (!sendResponse.Successful)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Email send failed");
            return Result.Failure(Errors.EmailSendUnsuccessful(
                sendResponse.ErrorMessages.Aggregate(
                        "The following errors occured: ",
                        (current, next) => current + $"\n\t- {next},")
                    .TrimEnd(',')
            ));
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return Result.Success();
    }
}