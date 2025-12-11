using Aspire.Hosting.MailDev;

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

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.Startup>("startup")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_ADDRESS", "no-reply@finance-observer.com")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_NAME", "Finance Observer")
    .WithEnvironment("FINANCE_OBSERVER_SMTP_CONNECTIONNAME", smtpServerResourceName)
    .WithEnvironment("FINANCE_OBSERVER_DB_CONNECTIONNAME", databaseResourceName)
    .WithEnvironment("FINANCE_OBSERVER_HASHING_ITERATIONS", "123456")
    .WithEnvironment("FINANCE_OBSERVER_HASHING_HASH_SIZE", "64")
    .WithEnvironment("FINANCE_OBSERVER_HASHING_SALT_SIZE", "16")
    .WithEnvironment("FINANCE_OBSERVER_EXPIRED_REGISTRATION_DELETION_EXECUTION_PERIOD", "60")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database)
    .WithReference(smtpServerResource)
    .WaitFor(smtpServerResource);

builder.Build().Run();
