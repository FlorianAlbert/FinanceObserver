using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Aspire.FluentEmail.MailKit;

internal class FluentEmailSender : IEmailSender
{
    private readonly IFluentEmailFactory _fluentEmailFactory;

    public FluentEmailSender(IFluentEmailFactory fluentEmailFactory)
    {
        _fluentEmailFactory = fluentEmailFactory;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return _fluentEmailFactory.Create()
            .To(email)
            .Subject(subject)
            .Body(htmlMessage, isHtml: true)
            .SendAsync();
    }
}