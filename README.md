# Azure Visitor Registration System

A cloud-based visitor registration system built with Azure services that allows users to register their visits through a web form.

## Description

This project is an educational assignment with the goal of demonstrating how different Azure cloud services work together to create a complete application. The idea is that Users can fill in their name and contact information on a webpage, which gets processed by a backend API (Azure Function) and stored in a database (Azure SQL Database).

## Getting Started

### Dependencies

- .NET 8 SDK
- Azure subscription (This project was developed on an Azure for Students subscription)
- Visual Studio Code with Azure Functions extension (I used this to deploy to Azure)

### Architecture

The system consists of five main components:

- **Frontend**: GitHub Pages (HTML/CSS/JavaScript)
     - I was not able to create an Azure Static Web App on my student subscription so GitHub Pages was used.
- **Backend**: Azure Functions of type HTTP trigger for processing requests
- **Database**: Azure SQL Database for storing visitor data
     -  Also an Azure SQL Server (logical) for managing the database
- **Logging**: Application Insights for monitoring and logging

### Installing

1. Clone the repository

   ```
   git clone https://github.com/ludwigstenberg/azure-visitor-registration.git
   ```

2. Navigate to the API directory

   ```
   cd api
   ```

3. Install dependencies

   ```
   dotnet restore
   ```

4. The local.settings.json file is included in the .gitignore so:

Create a `local.settings.json` file in the api folder:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "SqlConnectionString": "your-connection-string-here"
  },
  "Host": {
    "CORS": "*"
  }
}
```

### Executing program

#### Running locally:

1. Start the Azure Function locally

   ```
   cd api
   func start
   ```

2. Open `docs/index.html` in a browser or with a tool such as the 'Live Server' extension.

3. Update the `localUrl` in `app.js` if your function runs on a different port

#### Deploying to Azure:

1. Deploy the Azure Function using VS Code Azure Functions extension (here you will be able select or create a new resource group and the additional resources such as Application Insights and Storage Account will be created.)
2. Deploy the frontend to GitHub Pages by pushing to your repository
3. Update CORS settings in your Function App to allow your GitHub Pages domain
4. Add your connection string to the Function App's Application Settings

## Help

### Common issues:

- **CORS errors**: Ensure your Function App has the correct CORS settings configured.
  - If you struggle to get CORS working one solution is to allow all CORS hosts with: "\*" but keep in mind that this is considered a security risk.
- **Database connection**: Verify your connection string and firewall rules.
- **Authentication**: If using Managed Identity, ensure proper roles are assigned

### Testing the API:

You can test the RegisterVisitor endpoint using the Bruno files in `/api/Bruno/`.
To install Bruno: https://www.usebruno.com/downloads

## Authors

Ludwig Stenberg
lludwigstenberg@gmail.com

## Acknowledgments

- Microsoft Azure documentation
- Azure Functions tutorials
- Tim Corey: https://www.youtube.com/@IAmTimCorey
- Adam Marczak - Azure for Everyone: https://www.youtube.com/@AdamMarczakYT

This README was formatted by AI
