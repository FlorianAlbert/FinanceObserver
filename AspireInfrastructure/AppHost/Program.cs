using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.MailDev;
using Aspire.Hosting.PostgreSQL;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgresResource = 
    builder.AddPostgres("postgres")
           .WithPgAdmin();

IResourceBuilder<MailDevResource> mailDevResource = builder.AddMailDev("maildev")
    .ExcludeFromManifest();

builder.AddProject<Projects.Startup>("startup")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_ADDRESS", "no-reply@finance-observer.com")
    .WithEnvironment("FINANCE_OBSERVER_FROM_EMAIL_NAME", "Finance Observer")
    .WithEnvironment("FINANCE_OBSERVER_DB_PROVIDER", "Npgsql")
    .WithEnvironment("FINANCE_OBSERVER_DB_CONNECTIONSTRING", "postgres")
    .WithEnvironment("FINANCE_OBSERVER_HASHING_ITERATIONS", "123456")
    .WithEnvironment("FINANCE_OBSERVER_HASHING_HASH_SIZE", "64")
    .WithEnvironment("FINANCE_OBSERVER_HASHING_SALT_SIZE", "16")
    .WithEnvironment("FINANCE_OBSERVER_EXPIRED_REGISTRATION_DELETION_EXECUTION_PERIOD", "60")
    .WithReference(postgresResource)
    .WithReference(mailDevResource);

builder.Build().Run();
