# CosmosDBPartialUpdateTypeConverter

Requires Dotnet 8.0 SDK
https://dotnet.microsoft.com/en-us/download/dotnet/8.0

## Project Description

The `CosmosDBPartialUpdateTypeConverter` repository is a .NET project designed to facilitate partial updates in Azure Cosmos DB using JSON Patch operations. This project aims to provide a seamless way to perform partial updates on documents stored in Cosmos DB, reducing the need for full document replacements and improving performance.

## Purpose

The primary purpose of this project is to demonstrate how to implement partial updates in Cosmos DB using JSON Patch operations. By leveraging this project, developers can efficiently update specific fields within a document without the need to replace the entire document.

## Features

- Partial updates using JSON Patch operations
- Sample console application demonstrating usage
- Securely manage secrets using Azure Key Vault
- Comprehensive `.gitignore` file
- MIT License
- GitHub Actions workflows for building and testing the .NET project

## Setup and Run

### Prerequisites

- .NET 8.0 SDK: [Download .NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Azure Cosmos DB account
- Azure Key Vault for managing secrets

### Steps

1. Clone the repository:
   ```sh
   git clone https://github.com/ewdlop/CosmosDBPartialUpdateTypeConverter.git
   cd CosmosDBPartialUpdateTypeConverter
   ```

2. Build the project:
   ```sh
   dotnet build
   ```

3. Set up Azure Key Vault:
   - Create a Key Vault in Azure.
   - Add secrets for `MyCosmosDBConnectionString` and `MyCosmosDBDatabaseId`.

4. Run the sample console application:
   ```sh
   dotnet run --project ConsoleApp
   ```

## Contributing

We welcome contributions to the `CosmosDBPartialUpdateTypeConverter` project. To contribute, follow these steps:

1. Fork the repository.
2. Create a new branch for your feature or bugfix.
3. Make your changes and commit them with descriptive commit messages.
4. Push your changes to your forked repository.
5. Create a pull request to the main repository.

Please ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for more details.
