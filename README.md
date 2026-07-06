# Loggin API - Observability and Audit Orchestrator

> [!IMPORTANT]
> This repository contains the **main API developed in C#**.
> To access the log collector library developed in **Go**, please visit:
> 👉 **[Loggin Collector (Go)](https://github.com/Artses/Loggin_Collector)**

---

## 📌 About the Project

This project is part of an **end-to-end observability solution** designed for auditing, contextualizing, and centrally managing logs in distributed applications. The complete solution is split into two primary components:

1. **Collector and Contextualizer (Go Library)**: Integrated directly into applications via the Gin framework, responsible for collecting and contextualizing logs at runtime.
2. **Orchestrator API (C# API - This Repository)**: Built with **ASP.NET Core**, this is the brain of the solution. It orchestrates the collection process, manages collector agent configurations, persists structured log data, and defines the remaining lifecycle of the log data (processing, interpretation, storage, and disposition).

---

## 🚀 Features of the C# API

- **Collector Management (CRUD)**: Registry and management of active log collectors (`Url`, `Path`, `Name`).
- **Authentication & Authorization**: Endpoint protection using **JWT (JSON Web Tokens)** and secure password hashing using **BCrypt**.
- **Automatic Migrations**: Database schema updates applied automatically to the PostgreSQL database on startup (no need for manual migration commands in development).
- **Interactive Documentation**: Pre-configured Swagger UI with Bearer Token authentication support for easy API testing directly from your browser.

---

## 🛠️ Tech Stack

- **Language / Framework**: C# .NET 8.0 / ASP.NET Core
- **Database**: PostgreSQL (via `Npgsql.EntityFrameworkCore.PostgreSQL`)
- **ORM / Persistence**: Entity Framework Core (EF Core)
- **Security**: JWT Bearer Authentication & BCrypt.Net-Next
- **Documentation**: Swagger / OpenAPI (via Swashbuckle)

---

## 🗂️ Project Structure

The C# API is organized according to clean architecture principles and dependency injection:

- **[Controllers](file:///e:/Loggin/Api_Loggin/Controllers)**: Exposes the HTTP REST endpoints (`AuthController`, `CollectorController`).
- **[Services](file:///e:/Loggin/Api_Loggin/Services)**: Contains the core business logic (`AuthServices`, `CollectorService`).
- **[Repositories](file:///e:/Loggin/Api_Loggin/Repositories)**: Abstraction layer for PostgreSQL database operations (`AuthRepository`, `CollectorRepository`).
- **[Models](file:///e:/Loggin/Api_Loggin/Models)**: Database entities (`User`, `Collector`).
- **[DTOs](file:///e:/Loggin/Api_Loggin/DTOs)**: Data Transfer Objects used for validation of incoming and outgoing requests.
- **[Data](file:///e:/Loggin/Api_Loggin/Data)**: Entity Framework DB Context configuration (`AppDbContext`).

---

## 🔑 Setup and Execution

### Prerequisites
- **.NET 8.0 SDK** installed.
- Running **PostgreSQL** instance.

### 1. Configure Connection Strings and Secrets
Adjust the PostgreSQL connection string (`ConnectionStrings`) and JWT keys in [appsettings.json](file:///e:/Loggin/Api_Loggin/appsettings.json) or via environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=password"
  },
  "Jwt": {
    "Key": "your-at-least-32-character-secret-jwt-key",
    "Issuer": "ProductsApi",
    "Audience": "ProductsApiUsers",
    "ExpiresInMinutes": 60
  }
}
```

### 2. Run the Application
Run the following command from the project root directory:

```powershell
dotnet run
```

Any pending database migrations will be automatically applied at startup.

### 3. Access Swagger Documentation
Once the app is running in your development environment, open your browser and navigate to:
👉 `http://localhost:<port>/swagger/index.html` (check your console output for the generated port).

---

## 📌 Main Endpoints

### Authentication (`api/Auth`)
- `POST /api/Auth/register` - Create a new user.
- `POST /api/Auth/login` - Authenticate a user and receive a JWT token.
- `GET /api/Auth/hi` (Authorized) - Token verification test endpoint (requires the `User` role).

### Collector Management (`api/Collector` - All require JWT Token)
- `POST /api/Collector` - Register a new collector.
- `GET /api/Collector` - Retrieve all registered collectors.
- `GET /api/Collector/{id}` - Retrieve details of a specific collector.
- `PUT /api/Collector/{id}` - Update a collector's configuration.
- `DELETE /api/Collector/{id}` - Remove a collector from monitoring.
