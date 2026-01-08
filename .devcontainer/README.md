# FinanceObserver Dev Container

This dev container provides a complete development environment for the FinanceObserver project, built on .NET 10.0 with Aspire orchestration support.

## Features

- **.NET SDK 10.0**: Full .NET development toolchain
- **Aspire**: Application orchestration for managing dependencies and resources
- **Docker-in-Docker**: Run and manage containers within the dev container
- **Node.js & npm**: JavaScript/Node.js development support
- **PowerShell**: Additional shell environment
- **Git**: Source control with built-in support
- **VS Code Extensions**: Pre-configured with C#, Aspire, Docker, and GitHub Copilot extensions

## Getting Started

1. Make sure Docker or some [other compliant container runtime](https://code.visualstudio.com/remote/advancedcontainers/docker-options) is installed
2. Open the project in VS Code with the Dev Container extension
3. The container will build automatically and restore .NET dependencies
4. HTTPS certificates will be trusted via `dotnet dev-certs https --trust`

## Launching with VS Code

The project is configured for debugging with VS Code's Aspire launcher. Use the following methods to start development:

1. **Via VS Code Debug UI**: Press `F5` or click the Run and Debug icon in the activity bar, then select "Aspire: Launch default apphost"
2. **Via Command Line**: Run `aspire run` in the terminal

For detailed Aspire workflow guidelines, see the [official Aspire documentation](https://aspire.dev/docs/).

## Working with Aspire

The application is orchestrated using Aspire. For detailed Aspire workflow guidelines, see the [official Aspire documentation](https://aspire.dev/docs/), including:
- Running the application with `aspire run`
- Checking resource status
- Debugging with structured logs and traces

## Git Commit Signing with Gpg4win

If you're using a Windows host with **Gpg4win ≥ 4.2**, be aware that it uses `keyboxd` to store certificates in a SQLite database in memory. This can cause issues with VS Code's remote functionality.

**If you experience signing problems:**

Disable keyboxd on your host machine:
```bash
gpg-disable-keyboxd
```

This is typically resolved in newer VS Code versions, but if problems persist, this command should resolve them.
