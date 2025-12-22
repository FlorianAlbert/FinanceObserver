using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.MailDev;

public class MailDevResource : ContainerResource, IResourceWithConnectionString
{
    public MailDevResource(string name, ParameterResource? userName, ParameterResource password) : base(name)
    {
        ArgumentNullException.ThrowIfNull(password);

        SmtpEndpoint = new(this, SmtpEndpointName);
        UserNameParameter = userName;
        PasswordParameter = password;
    }

    // Constants used to refer to well known-endpoint names, this is specific
    // for each resource type. MailDev exposes an SMTP endpoint and a HTTP
    // endpoint.
    internal const string SmtpEndpointName = "smtp";
    internal const string HttpEndpointName = "http";

    private const string _defaultUserName = "maildev";

    /// <summary>
    /// Gets the primary endpoint for the MailDev server.
    /// </summary>
    public EndpointReference SmtpEndpoint { get; }

    /// <summary>
    /// Gets the parameter that contains the MailDev server user name.
    /// </summary>
    public ParameterResource? UserNameParameter { get; }

    internal ReferenceExpression UserNameReference =>
        UserNameParameter is not null ?
            ReferenceExpression.Create($"{UserNameParameter}") :
            ReferenceExpression.Create($"{_defaultUserName}");

    /// <summary>
    /// Gets the parameter that contains the MailDev server password.
    /// </summary>
    public ParameterResource PasswordParameter { get; }

    /// <summary>
    /// Gets the connection string expression for the MailDev server.
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
            ReferenceExpression.Create(
                $"smtp://{UserNameReference}:{PasswordParameter}@{SmtpEndpoint.Property(EndpointProperty.Host)}:{SmtpEndpoint.Property(EndpointProperty.Port)}"
            );
}
