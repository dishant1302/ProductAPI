# ProductAPI — CRN Technical Assessment

A RESTful API for Product Management built with Clean Architecture.

## Tech Stack
- .NET 8 / ASP.NET Core Web API
- SQL Server + Entity Framework Core
- JWT Authentication with Refresh Tokens
- FluentValidation
- AutoMapper
- Swagger/OpenAPI
- Serilog Logging
- Docker + Docker Compose
- xUnit + Moq Tests

## Project Structure  
Solution/
├── src/
│   ├── API/           # Controllers, Middleware, Filters
│   ├── Application/   # Services, DTOs, Validators, Mapping
│   ├── Domain/        # Entities, Exceptions
│   └── Infrastructure/# EF Core, Repositories, Identity, JWT
└── tests/             # Unit & Integration Tests

## Getting Started

### Run Locally
```bash
# 1. Update connection string in src/API/appsettings.json

# 2. Run migrations
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj

# 3. Run API
dotnet run --project src/API/API.csproj

# 4. Open Swagger
http://localhost:5000
```

### Run with Docker
```bash
docker-compose up --build
```

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | /api/v1/auth/register | ❌ | Register user |
| POST | /api/v1/auth/login | ❌ | Login |
| POST | /api/v1/auth/refresh-token | ❌ | Refresh token |
| POST | /api/v1/auth/revoke-token | ✅ | Logout |
| GET | /api/v1/products | ✅ | Get all products |
| POST | /api/v1/products | ✅ | Create product |
| GET | /api/v1/products/{id} | ✅ | Get product |
| PUT | /api/v1/products/{id} | ✅ | Update product |
| DELETE | /api/v1/products/{id} | ✅ | Delete product |
| GET | /api/v1/products/{id}/items | ✅ | Get items |

## Testing
```bash
dotnet test
```
