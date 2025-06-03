# DafornoMail - Modern Web Mail Client

DafornoMail is a modern, secure, and feature-rich web-based email client built with Blazor WebAssembly and .NET 8. It supports multiple email providers including Gmail and Microsoft, with features like secure credential storage using Azure Key Vault and SQL Server for data persistence.

## Features

- 📧 Support for multiple email providers (Gmail, Microsoft, etc.)
- 🔒 Secure credential storage with Azure Key Vault
- 🔄 Real-time email synchronization
- 🏷️ Gmail labels and nested labels support
- 🔍 Advanced search capabilities
- 📱 Responsive design for all devices
- 🔐 Secure authentication with JWT
- 🚀 Blazor WebAssembly for a fast, interactive UI

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (for frontend tooling)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Azure SQL Database)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for Azure deployments)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/) (for secure credential storage)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/yourusername/DafornoMail.git
cd DafornoMail
```

### 2. Configure the application

1. Update the connection strings in `src/DafornoMail.Server/appsettings.json`
2. Configure your Azure Key Vault settings
3. Update JWT settings for authentication

### 3. Set up the database

Run the following commands to apply database migrations:

```bash
cd src/DafornoMail.Server
dotnet ef database update
```

### 4. Run the application

```bash
dotnet run --project src/DafornoMail.Server
```

The application will be available at `https://localhost:5001`

## Project Structure

- `src/DafornoMail.Client` - Blazor WebAssembly frontend
- `src/DafornoMail.Server` - ASP.NET Core Web API backend
- `src/DafornoMail.Core` - Shared models and interfaces
- `src/DafornoMail.Infrastructure` - Data access and external service implementations
- `tests/DafornoMail.Tests` - Unit and integration tests

## Configuration

### Azure Key Vault

1. Create a Key Vault in Azure Portal
2. Add your email account passwords as secrets
3. Update the `KeyVault:VaultUri` in `appsettings.json`
4. Configure access policies for your application

### Email Providers

#### Gmail

To use Gmail with this application, you'll need to:

1. Enable "Less secure app access" in your Google Account settings
   - Or create an App Password if you have 2FA enabled
   - Or set up OAuth 2.0 credentials in Google Cloud Console

2. Update the provider configuration in `appsettings.json`

## Deployment

### Azure App Service

1. Create a new Web App in Azure Portal
2. Configure deployment from GitHub or Azure DevOps
3. Set up the following Application Settings:
   - `ConnectionStrings__DefaultConnection`
   - `KeyVault__VaultUri`
   - `Jwt:Key`
   - `Jwt:Issuer`
   - `Jwt:Audience`

### Docker

```bash
docker build -t dafornoma.il .
docker run -d -p 8080:80 --name dafornoma.il dafornoma.il
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a new Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- [MailKit](https://github.com/jstedfast/MailKit)
- [MudBlazor](https://mudblazor.com/)
- [Azure Key Vault](https://azure.microsoft.com/en-us/services/key-vault/)
