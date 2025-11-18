# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade Server\CrossCutting\DataClasses\DataClasses.csproj
4. Upgrade Server\DataAccess\DbAccess.Contract\DbAccess.Contract.csproj
5. Upgrade Server\Logic\Domain\RegistrationConfirmationManagement.Contract\RegistrationConfirmationManagement.Contract.csproj
6. Upgrade Server\Logic\Business\RegistrationWorkflow.Contract\RegistrationWorkflow.Contract.csproj
7. Upgrade Server\Logic\Domain\HashHandling.Contract\HashHandling.Contract.csproj
8. Upgrade Server\Logic\Domain\DataTransactionHandling.Contract\DataTransactionHandling.Contract.csproj
9. Upgrade Server\Logic\Domain\EmailManagement.Contract\EmailManagement.Contract.csproj
10. Upgrade Server\Logic\Domain\UserManagement.Contract\UserManagement.Contract.csproj
11. Upgrade Server\Logic\Domain\RegistrationConfirmationManagement\RegistrationConfirmationManagement.csproj
12. Upgrade AspireInfrastructure\Components\FluentEmail.MailKit\Aspire.FluentEmail.MailKit.csproj
13. Upgrade Server\Logic\Business\RegistrationWorkflow\RegistrationWorkflow.csproj
14. Upgrade Server\Logic\Domain\EmailManagement\EmailManagement.csproj
15. Upgrade Server\Logic\Domain\HashHandling.SHA512\HashHandling.SHA512.csproj
16. Upgrade Server\Logic\Domain\DataTransactionHandling\DataTransactionHandling.csproj
17. Upgrade Server\Presentation\REST\REST.csproj
18. Upgrade Server\Logic\Domain\UserManagement\UserManagement.csproj
19. Upgrade Server\DataAccess\DbAccess.EntityFrameworkCore\DbAccess.EntityFrameworkCore.csproj
20. Upgrade AspireInfrastructure\ServiceDefaults\ServiceDefaults.csproj
21. Upgrade AspireInfrastructure\Hosting\MailDev\Aspire.Hosting.MailDev.csproj
22. Upgrade Server\Presentation\Startup\Startup.csproj
23. Upgrade AspireInfrastructure\AppHost\AppHost.csproj
24. Upgrade Server\DataAccess\DbAccess.EntityFrameworkCore.Tests\DbAccess.EntityFrameworkCore.Tests.csproj
25. Upgrade Server\Logic\Business\RegistrationWorkflow.Tests\RegistrationWorkflow.Tests.csproj
26. Upgrade Server\Logic\Domain\UserManagement.Tests\UserManagement.Tests.csproj
27. Upgrade Server\Logic\Domain\RegistrationConfirmationManagement.Tests\RegistrationConfirmationManagement.Tests.csproj
28. Upgrade Server\Logic\Domain\HashHandling.SHA512.Tests\HashHandling.SHA512.Tests.csproj
29. Upgrade Server\Logic\Domain\EmailManagement.Tests\EmailManagement.Tests.csproj
30. Upgrade Server\Logic\Domain\DataTransactionHandling.Tests\DataTransactionHandling.Tests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                         | Current Version | New Version | Description                                   |
|:-------------------------------------|:---------------:|:-----------:|:----------------------------------------------|
| Aspire.Hosting                       | 8.0.2          | 13.0.0      | Recommended for .NET 10.0 (package deprecated) |
| Aspire.Hosting.AppHost               | 8.0.2          | 13.0.0      | Recommended for .NET 10.0 (package deprecated) |
| Aspire.Hosting.PostgreSQL            | 8.0.2          | 13.0.0      | Recommended for .NET 10.0 (package deprecated) |
| Aspire.Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.2 | 9.5.2 | Recommended for .NET 10.0 (package deprecated) |
| Microsoft.AspNetCore.OpenApi         | 8.0.7          | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.EntityFrameworkCore        | 8.0.7          | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.EntityFrameworkCore.Relational | 8.0.7      | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.EntityFrameworkCore.Tools  | 8.0.7          | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.Extensions.Hosting.Abstractions | 8.0.0    | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.Extensions.Http.Resilience | 8.7.0          | 10.0.0      | Recommended for .NET 10.0                      |
| Microsoft.Extensions.ServiceDiscovery | 8.0.2         | 10.0.0      | Recommended for .NET 10.0 (package deprecated) |
| OpenTelemetry.Instrumentation.AspNetCore | 1.9.0     | 1.14.0      | Recommended for .NET 10.0                      |
| OpenTelemetry.Instrumentation.Http   | 1.9.0          | 1.14.0      | Recommended for .NET 10.0                      |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### Server\CrossCutting\DataClasses\DataClasses.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\DataAccess\DbAccess.Contract\DbAccess.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\RegistrationConfirmationManagement.Contract\RegistrationConfirmationManagement.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Business\RegistrationWorkflow.Contract\RegistrationWorkflow.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\HashHandling.Contract\HashHandling.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\DataTransactionHandling.Contract\DataTransactionHandling.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\EmailManagement.Contract\EmailManagement.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\UserManagement.Contract\UserManagement.Contract.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\RegistrationConfirmationManagement\RegistrationConfirmationManagement.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### AspireInfrastructure\Components\FluentEmail.MailKit\Aspire.FluentEmail.MailKit.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting.Abstractions should be updated from `8.0.0` to `10.0.0` (recommended for .NET 10.0)

#### Server\Logic\Business\RegistrationWorkflow\RegistrationWorkflow.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Hosting.Abstractions should be updated from `8.0.0` to `10.0.0` (recommended for .NET 10.0)

#### Server\Logic\Domain\EmailManagement\EmailManagement.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\HashHandling.SHA512\HashHandling.SHA512.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\DataTransactionHandling\DataTransactionHandling.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Presentation\REST\REST.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.AspNetCore.OpenApi should be updated from `8.0.7` to `10.0.0` (recommended for .NET 10.0)

#### Server\Logic\Domain\UserManagement\UserManagement.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\DataAccess\DbAccess.EntityFrameworkCore\DbAccess.EntityFrameworkCore.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.EntityFrameworkCore should be updated from `8.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.EntityFrameworkCore.Relational should be updated from `8.0.7` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.EntityFrameworkCore.Tools should be updated from `8.0.7` to `10.0.0` (recommended for .NET 10.0)

#### AspireInfrastructure\ServiceDefaults\ServiceDefaults.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Microsoft.Extensions.Http.Resilience should be updated from `8.7.0` to `10.0.0` (recommended for .NET 10.0)
  - Microsoft.Extensions.ServiceDiscovery should be updated from `8.0.2` to `10.0.0` (deprecated version; upgrade recommended for .NET 10.0)
  - OpenTelemetry.Instrumentation.AspNetCore should be updated from `1.9.0` to `1.14.0` (recommended for .NET 10.0)
  - OpenTelemetry.Instrumentation.Http should be updated from `1.9.0` to `1.14.0` (recommended for .NET 10.0)

#### AspireInfrastructure\Hosting\MailDev\Aspire.Hosting.MailDev.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Aspire.Hosting should be updated from `8.0.2` to `13.0.0` (deprecated version; upgrade recommended for .NET 10.0)

#### Server\Presentation\Startup\Startup.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Aspire.Npgsql.EntityFrameworkCore.PostgreSQL should be updated from `8.0.2` to `9.5.2` (deprecated version; upgrade recommended for .NET 10.0)
  - Microsoft.AspNetCore.OpenApi should be updated from `8.0.7` to `10.0.0` (recommended for .NET 10.0)

#### AspireInfrastructure\AppHost\AppHost.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

NuGet packages changes:
  - Aspire.Hosting.AppHost should be updated from `8.0.2` to `13.0.0` (deprecated version; upgrade recommended for .NET 10.0)
  - Aspire.Hosting.PostgreSQL should be updated from `8.0.2` to `13.0.0` (deprecated version; upgrade recommended for .NET 10.0)

#### Server\DataAccess\DbAccess.EntityFrameworkCore.Tests\DbAccess.EntityFrameworkCore.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Business\RegistrationWorkflow.Tests\RegistrationWorkflow.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\UserManagement.Tests\UserManagement.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\RegistrationConfirmationManagement.Tests\RegistrationConfirmationManagement.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\HashHandling.SHA512.Tests\HashHandling.SHA512.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\EmailManagement.Tests\EmailManagement.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`

#### Server\Logic\Domain\DataTransactionHandling.Tests\DataTransactionHandling.Tests.csproj

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`
