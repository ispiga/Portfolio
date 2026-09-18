# Portfolio

Personal portfolio developed with .NET 10 and Blazor Web App.

## About the project

This project is my personal professional portfolio...
...

## Technologies

- .NET 10
- Blazor Web App
- C#
- SQL Server
- Entity Framework Core
- ASP.NET Core Identity
- Tailwind CSS
- MudBlazor
- TinyMCE
- Docker

## Features

- Professional profile
- Experience
- Projects
- Certifications
- Blog
- Contact form
- Administration panel

## Architecture

Portfolio.Web
Portfolio.Application
Portfolio.Domain
Portfolio.Infrastructure
Portfolio.Tests

## SQL Server development configuration

The application reads the connection from the `ConnectionStrings:Portfolio` configuration key. The repository does not contain a machine-specific SQL Server name or credentials.

`Portfolio.Web/appsettings.json` and `Portfolio.Web/appsettings.Development.json` are neutral. The connection is configured locally through User Secrets and is not stored in the repository.

### Configure User Secrets

Run these commands from the repository root after cloning the project:

```powershell
dotnet user-secrets init --project Portfolio.Web/Portfolio.Web.csproj

dotnet user-secrets set `
  "ConnectionStrings:Portfolio" `
  "Server=YOUR-SERVER;Database=PortfolioDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;" `
  --project Portfolio.Web/Portfolio.Web.csproj
```

Replace `YOUR-SERVER` with the SQL Server instance available in the current development environment. For example, it could be `YOUR-PC-NAME` on one computer or another server name on a different computer.

User Secrets are local to the developer and are not committed to Git. To inspect the configured values:

```powershell
dotnet user-secrets list --project Portfolio.Web/Portfolio.Web.csproj
```

To replace the connection, run `dotnet user-secrets set` again with the same key. To remove it:

```powershell
dotnet user-secrets remove "ConnectionStrings:Portfolio" --project Portfolio.Web/Portfolio.Web.csproj
```

### Apply EF Core migrations

After configuring a SQL Server instance, apply the migrations with:

```powershell
dotnet ef database update `
  --project Portfolio.Infrastructure/Portfolio.Infrastructure.csproj `
  --startup-project Portfolio.Web/Portfolio.Web.csproj
```

The database name used by the initial migration is `PortfolioDb`. Development and production must use different connection strings and databases. Production values must be provided through the deployment environment, Docker secrets or another secure configuration mechanism.

## Deployment

Docker + QNAP

## Status

🚧 In development
