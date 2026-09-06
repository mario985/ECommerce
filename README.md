# ECommerce Backend

A modular monolith e-commerce backend built with .NET 9 and ASP.NET Core. The project is designed as a portfolio-quality example of domain-focused backend development, explicit module boundaries, and production-minded infrastructure.

## Features

- Authentication and authorization with JWT
- Product catalog and categories
- Product search and filtering
- Shopping cart
- Orders and checkout
- Stripe payment integration
- Order fulfillment and shipment tracking
- Product reviews and ratings
- Wishlist and favorites
- Swagger/OpenAPI documentation

## Architecture

The solution uses a modular monolith with Clean Architecture and Domain-Driven Design principles. Each module is separated into Domain, Application, Infrastructure, Presentation, and Contracts projects where applicable. CQRS-style commands and queries, repositories, domain events, and explicit inter-module contracts are used throughout the application.

## Technology stack

- .NET 9 and ASP.NET Core Web API
- Entity Framework Core with SQLite for transactional modules
- MongoDB for Catalog and Reviews data
- Redis for caching
- MediatR and FluentValidation
- Stripe for payment processing
- Swagger/OpenAPI
- xUnit
- Docker and Docker Compose

## Project structure

```text
src/
├── Api/ECommerce.Api/
├── Common/
└── Modules/
    ├── Identity/
    ├── Catalog/
    ├── Inventory/
    ├── Cart/
    ├── Ordering/
    ├── Payments/
    ├── Reviews/
    └── Wishlist/
tests/
├── ECommerce.IntegrationTests/
├── ECommerce.ArchitectureTests/
└── Modules/
```

## Running with Docker

Copy the environment template and set a JWT signing key:

```bash
cp .env.example .env
docker compose up --build
```

The API is available at `http://localhost:8080`.

- Swagger UI: `http://localhost:8080/swagger`
- Readiness probe: `http://localhost:8080/health/ready`
- MongoDB: `localhost:27017`
- Redis: `localhost:6379`

The Compose stack persists SQLite files, MongoDB data, and Redis data in named volumes. `JWT_KEY` is required. Stripe variables are optional and payments are disabled by default.

## Running locally

Start MongoDB and Redis, then run:

```bash
dotnet restore
dotnet build --no-restore
dotnet run --project src/Api/ECommerce.Api
```

Swagger is available at `http://localhost:5080/swagger` and can be disabled through the `OpenApi:Enabled` setting.

## Tests

```bash
dotnet test --no-build
```

## Future improvements

Potential future additions include notifications, analytics, and product recommendations.
