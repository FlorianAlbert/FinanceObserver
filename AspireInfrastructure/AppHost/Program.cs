using Aspire.Hosting.MailDev;
using Scalar.Aspire;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgresResource = builder.AddPostgres("postgres");

if (builder.ExecutionContext.IsRunMode)
{
    postgresResource.WithPgAdmin(pgAdminBuilder =>
    {
        pgAdminBuilder.WithLifetime(ContainerLifetime.Persistent);
    });
}

string databaseResourceName = "finance-observer-db";
IResourceBuilder<PostgresDatabaseResource> database = postgresResource.AddDatabase(databaseResourceName);

string smtpServerResourceName;
IResourceBuilder<IResourceWithConnectionString> smtpServerResource;
if (builder.ExecutionContext.IsRunMode)
{
    smtpServerResourceName = "maildev";
    smtpServerResource = builder.AddMailDev(smtpServerResourceName);
}
else
{
    // When we support publish mode we should add a real, production ready SMTP server here.
    throw new DistributedApplicationException("Currently we only support run mode of the AppHost.");
}

IResourceBuilder<ProjectResource> migrationService = builder.AddProject<Projects.MigrationService>("migration-service")
    .WithEnvironment("FINANCE_OBSERVER_DB_CONNECTIONNAME", databaseResourceName)
    .WithReference(database)
    .WaitFor(database);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Startup>("startup")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_ADDRESS", "no-reply@finance-observer.com")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_NAME", "Finance Observer")
    .WithEnvironment("FINANCE_OBSERVER_SMTP_CONNECTIONNAME", smtpServerResourceName)
    .WithEnvironment("FINANCE_OBSERVER_DB_CONNECTIONNAME", databaseResourceName)
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(smtpServerResource)
    .WaitFor(smtpServerResource)
    .WaitForCompletion(migrationService);

if (builder.ExecutionContext.IsRunMode)
{
    IResourceBuilder<ScalarResource> scalar = builder.AddScalarApiReference(options =>
    {
        options.PreferHttpsEndpoint()
            .AllowSelfSignedCertificates();

        // Disable proxying to get correct confirmation links in emails
        options.DisableDefaultProxy();
    });

    // Every reference added in the future should have CORS configured to allow calls from Scalar
    scalar.WithApiReference(api).WaitFor(api);
}

builder.Build().Run();
