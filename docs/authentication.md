# Authentication

Shipment uses **JSON Web Tokens (JWT)** for authentication.

## Authentication Flow

```
User
 │
 ▼
Login Request
 │
 ▼
Server Validates Credentials
 │
 ▼
Access Token Generated
 │
 ▼
Refresh Token Generated
 │
 ▼
Tokens Returned to Client
```

## Access Token

Used for accessing protected endpoints.

Typical lifetime:

15 minutes.

## Refresh Token

Used to obtain a new access token.

Typical lifetime:

7–30 days.

## Security Practices

- Cryptographically secure token generation
- Short access token lifetime
- Refresh token rotation