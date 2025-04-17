# MCP_PostgreSQL

## Overview
MCP_PostgreSQL is a .NET solution that provides integration between Model Context Protocol (MCP) and PostgreSQL databases. It consists of two main components:

1. **PostgreSqlAPI**: An ASP.NET Core Web API that provides RESTful interfaces for interacting with PostgreSQL databases.
2. **ConsoleMcpPostgreSQL**: A command-line application that uses the Model Context Protocol to interact with PostgreSQL.

## Features
- PostgreSQL database schema exploration
- Query execution against PostgreSQL databases
- Integration with Model Context Protocol
- RESTful API for database operations
- Cross-origin resource sharing (CORS) support
- Interactive Swagger documentation

## Technology Stack
- .NET 8.0
- ASP.NET Core
- Npgsql (PostgreSQL .NET driver)
- Model Context Protocol (MCP)
- Swagger/OpenAPI

## Prerequisites
- .NET 8.0 SDK or later
- PostgreSQL database server
- Visual Studio 2022 or any compatible IDE

## Getting Started

### Installation
1. Clone the repository:
```bash
git clone https://github.com/yourusername/MCP_PostgreSQL.git
cd MCP_PostgreSQL
```

2. Build the solution:
```bash
dotnet build
```

### Configuration
Configure your PostgreSQL connection string in the appropriate configuration files:
- For PostgreSqlAPI: `appsettings.json`
- For ConsoleMcpPostgreSQL: Update the connection settings in the appropriate configuration

### Running the Projects

#### PostgreSqlAPI
```bash
cd PostgreSqlAPI
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

#### ConsoleMcpPostgreSQL
```bash
cd ConsoleMcpPostgreSQL
dotnet run
```

## API Documentation
The PostgreSqlAPI provides the following endpoints:
- GET /api/mcp/schema: Retrieve database schema information
- POST /api/mcp/query: Execute SQL queries against the PostgreSQL database

Detailed API documentation is available through Swagger when running the application.

## Project Structure
- **PostgreSqlAPI**
  - Controllers: Contains API controllers
  - Models: Data models for API requests and responses
  - Services: Database interaction services
  
- **ConsoleMcpPostgreSQL**
  - Uses Model Context Protocol for PostgreSQL interaction
  - Provides command-line interface for database operations

## License
[Specify your license information]

## Contact
[Your contact information]