# Architecture

Shipment follows a **layered architecture** optimized for simplicity and maintainability.

## High-Level Architecture

```
Client
 │
 ▼
HTTP API Layer (Minimal APIs)
 │
 ▼
Application Layer (Handlers)
 │
 ▼
Data Layer (EF Core)
 │
 ▼
Database
```

## Layer Responsibilities

### API Layer

Responsible for:

- Request routing
- Request binding
- Response formatting

### Application Layer

Contains business logic.

Examples:

- User creation
- User deletion
- Token generation

### Data Layer

Responsible for database access using **Entity Framework Core**.

### Infrastructure

Handles cross-cutting concerns:

- Authentication
- Configuration
- Security

---

## Feature-Based Architecture

Instead of separating by technical layers (controllers/services), Shipment separates by **features**.

Example:

```
Features
 ├── User
 │   ├── CreateUser
 │   ├── DeleteUser
 │   ├── UpdateUser
 │   └── GetUsers
```

Advantages:

- Better modularity
- Easier scaling
- Reduced coupling