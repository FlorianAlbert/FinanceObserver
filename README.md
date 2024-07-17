# FinanceObserver

This is a learning project aiming to develop a platform to observe your finances and everything around it. 

[![Continous Integration .NET](https://github.com/FlorianAlbert/FinanceObserver/actions/workflows/dotnet-integration.yml/badge.svg?event=push)](https://github.com/FlorianAlbert/FinanceObserver/actions/workflows/dotnet-integration.yml)
[![codecov](https://codecov.io/gh/FlorianAlbert/FinanceObserver/graph/badge.svg?token=LW7XCTFPW3)](https://codecov.io/gh/FlorianAlbert/FinanceObserver)

## Description

The main purpose of this project is to inspect, test and learn different technologies in a practical context.

The application domain centers around the management of private financials and other financial topics with several possibilities to extend it in related areas.

For now, the project only consists of an [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet) backend, backed by a [PosgreSQL](https://www.postgresql.org/) database and a ([MailDev](https://maildev.github.io/maildev/)) development SMTP Server. Plans are to extend it by a release profile with a production ready SMTP Server and by different UI clients based on different technology stacks.

Contributions are always welcome! 🤝

## Getting Started

### Dependencies

* [.NET SDK](https://dotnet.microsoft.com/en-us/download)
* [Docker](https://www.docker.com/)

### Installing

There is nothing to set up.

### Executing program

#### Development version:

* The project uses .NET Aspire.

* To start the whole application just start the `AppHost` project.

* This starts:
    * A development SMTP Server ([MailDev](https://maildev.github.io/maildev/)) on the [localhost](localhost:1080) with a web UI.
    * A [PosgreSQL](https://www.postgresql.org/) database.
    * A [pgAdmin](https://www.pgadmin.org/) web UI on the [localhost](localhost:9080).
    * The FinanceObserver backend on the [localhost](localhost:5000).

* If you want to customize some settings of the FinanceObserver backend you can edit them by adjusting the environment variables in the `Program.cs` of the `AppHost` project.

## Authors

#### Florian Albert

* [Github](https://github.com/FlorianAlbert)
* [LinkedIn](https://www.linkedin.com/in/florian-albert-2b9a89213)

## Contributors

## Version History

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details