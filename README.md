# Shipment API

Shipment is a **high-performance backend API** built with **ASP.NET Core Minimal APIs** for managing users and shipment operations.

The project focuses on **modular feature-based architecture**, modern API design practices, and secure authentication mechanisms.

This repository can serve as:

* A **production-ready backend API**
* A **reference implementation** for building modular ASP.NET APIs

---

# Features

* JWT Authentication
* Refresh Token Support
* User Management
* Shipment Management
* Pagination
* FluentValidation Request Validation
* Structured Result Pattern
* Feature-based architecture
* SignalR real-time notifications

---

# Technology Stack

| Technology               | Purpose                 |
| ------------------------ | ----------------------- |
| ASP.NET Core Minimal API | Web API framework       |
| C#                       | Programming language    |
| Entity Framework Core    | ORM                     |
| FluentValidation         | Request validation      |
| JWT                      | Authentication          |
| SQL Database             | Data persistence        |
| SignalR                  | Real-time communication |

---

# Architecture

Shipment follows a **feature-based architecture** with clear separation of responsibilities.

```
Client
   │
   ▼
Minimal API Endpoints
   │
   ▼
Application Handlers
   │
   ▼
Domain / Entities
   │
   ▼
Entity Framework Core
   │
   ▼
Database
```

Additional architectural details can be found in:

```
/docs/architecture.md
```

---

# Project Structure

```
├── docs
└── src
    ├── Abstract
    │   ├── Messaging
    │   └── Results
    │       └── Errors
    ├── Auth
    ├── Configurations
    ├── Database
    ├── Entities
    │   └── Shared
    ├── Extensions
    ├── Features
    │   ├── Auth
    │   │   ├── Login
    │   │   ├── RefreshToken
    │   │   ├── Register
    │   │   └── RevokeRefreshToken
    │   ├── Shipments
    │   │   ├── CreateShipments
    │   │   ├── DeleteShipments
    │   │   ├── GetAllShipments
    │   │   ├── GetShipmentById
    │   │   ├── Shared
    │   │   └── UpdateShipments
    │   └── User
    │       ├── CreateUsers
    │       ├── DeleteUsers
    │       ├── GetAllUsers
    │       ├── GetUsersById
    │       └── UpdateUsers
    ├── Hubs
    ├── Migrations
    ├── Options
    └── Properties
```

## Feature-Based Design

Each feature module typically contains:

* Request / Response DTOs
* Handler (application logic)
* Validator
* Endpoint registration

This structure keeps features **isolated, maintainable, and scalable**.

---

# Getting Started

## Prerequisites

* .NET SDK
* SQL Server or PostgreSQL
* Git

---

# Installation

Clone the repository:

```
git clone https://github.com/CLeovison/Shipment.git
```

Navigate into the project directory:

```
cd Shipment
```

Restore dependencies:

```
dotnet restore
```

Run the application:

```
dotnet run
```

---

# Authentication

Shipment uses **JWT-based authentication** combined with **refresh tokens**.

Authentication flow:

1. User submits credentials
2. Server validates the request
3. Access token is issued
4. Refresh token is stored
5. Client can request a new access token using the refresh token

More details are available in:

```
/docs/authentication.md
```

---

# Development

Development conventions, architectural guidelines, and coding standards are documented in:

```
/docs/development.md
```

---

# Roadmap

Planned improvements include:

* Role-based authorization
* Shipment tracking
* Bulk operations
* API rate limiting
* Distributed caching
* Observability (logging and metrics)

---

# License

This project is licensed under the **MIT License**.

See the full license in the `LICENSE` file.

---

# Author

Clark Leovison Rey
