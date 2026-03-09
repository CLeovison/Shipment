# Shipment API

Shipment is a high-performance backend API built with **ASP.NET Core Minimal APIs** designed for managing users and shipment-related operations.

The project focuses on clean architecture, modular feature-based design, and modern API development practices.

---

# Features

- JWT Authentication
- Refresh Token Support
- User Management
- Pagination
- FluentValidation Request Validation
- Structured Result Pattern
- Feature-based architecture

---

# Technology Stack

| Technology | Purpose |
|-----------|--------|
| ASP.NET Core Minimal API | Web API Framework |
| C# | Programming Language |
| Entity Framework Core | ORM |
| FluentValidation | Request Validation |
| JWT | Authentication |
| SQL Database | Data Persistence |

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
Entity Framework Core
   │
   ▼
Database
```

More details are available in:

`/docs/architecture.md`

---

# Project Structure

```
src
│
├── Features
│   ├── User
│   │   ├── CreateUsers
│   │   ├── DeleteUsers
│   │   ├── UpdateUsers
│   │   └── GetUsers
│
├── Entities
│
├── Database
│
├── Auth
│
└── Abstract
```

### Feature-Based Design

Each feature contains:

- Request DTO
- Handler
- Validator
- Endpoint registration

This approach improves maintainability and scalability.

---

# Getting Started

## Prerequisites

- .NET SDK
- SQL Server or PostgreSQL
- Git

---

# Installation

Clone the repository:

```
git clone https://github.com/CLeovison/Shipment.git
```

Navigate to the project:

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

Shipment uses **JWT-based authentication**.

Authentication flow:

1. User logs in
2. Server validates credentials
3. Access token is generated
4. Refresh token is issued

See:

`/docs/authentication.md`

---

# API Documentation

Full API documentation is available in:

`/docs/api.md`

---

# Development

Development guidelines and best practices are documented in:

`/docs/development.md`

---

# Roadmap

Planned improvements:

- Role-based authorization
- Shipment tracking
- Bulk operations
- API rate limiting
- Distributed caching
- Observability (logging + metrics)

---

# Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Open a Pull Request

---

# License

This project currently has no license specified.

---

# Author

Clark Leovison Rey