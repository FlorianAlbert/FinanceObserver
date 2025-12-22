using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting.MailDev;

public static class MailDevResourceBuilderExtensions
{
    private const string _userEnvVarName = "MAILDEV_INCOMING_USER";
    private const string _passwordEnvVarName = "MAILDEV_INCOMING_PASS";

    /// <summary>
    /// Adds the <see cref="MailDevResource"/> to the given
    /// <paramref name="builder"/> instance. Uses the "2.2.1" tag.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="httpPort">The HTTP port.</param>
    /// <param name="smtpPort">The SMTP port.</param>
    /// <returns>
    /// An <see cref="IResourceBuilder{MailDevResource}"/> instance that
    /// represents the added MailDev resource.
    /// </returns>
    public static IResourceBuilder<MailDevResource> AddMailDev(
        this IDistributedApplicationBuilder builder,
        string name,
        IResourceBuilder<ParameterResource>? userName = null,
        IResourceBuilder<ParameterResource>? password = null,
        int? httpPort = null,
        int? smtpPort = null)
    {
        ParameterResource passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password", special: false);

        var mailDevServer = new MailDevResource(name, userName?.Resource, passwordParameter);
        return builder.AddResource(mailDevServer)
                      .WithImage(MailDevContainerImageTags.Image)
                      .WithImageRegistry(MailDevContainerImageTags.Registry)
                      .WithImageTag(MailDevContainerImageTags.Tag)
                      .WithHttpEndpoint(
                          targetPort: 1080,
                          port: httpPort,
                          name: MailDevResource.HttpEndpointName)
                      .WithEndpoint(
                          targetPort: 1025,
                          port: smtpPort,
                          name: MailDevResource.SmtpEndpointName)
                      .WithEnvironment(context =>
                      {
                          context.EnvironmentVariables[_userEnvVarName] = mailDevServer.UserNameReference;
                          context.EnvironmentVariables[_passwordEnvVarName] = mailDevServer.PasswordParameter;
                      })
                      .WithHttpHealthCheck("/healthz");
    }
}

// This class just contains constant strings that can be updated periodically
// when new versions of the underlying container are released.
internal static class MailDevContainerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "maildev/maildev";

    internal const string Tag = "2.2.1";
}