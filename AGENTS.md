# AGENTS.md

## Project Purpose

This repository contains a portfolio and learning-focused e-commerce backend built with:

* .NET 9
* ASP.NET Core Web API
* Domain-Driven Design
* Clean Architecture
* Modular Monolith architecture
* SQLite with Entity Framework Core
* MongoDB for the Catalog module
* Redis for caching
* MediatR for in-process messaging
* Stripe for payment processing
* xUnit for testing
* Docker and Docker Compose

The project is implemented through small, reviewable tasks.

Do not implement features outside the active task.

---

## Required Reading

Before implementing a task, read:

1. `AGENTS.md`
2. `ARCHITECTURE.md`
3. `docs/glossary.md`
4. The assigned task under `docs/tasks/`

The task document has the most specific implementation requirements.

---

## Core Architecture Rules

* The application is a modular monolith.
* Each module owns its business logic and persistence.
* Business logic belongs in Domain.
* Use-case coordination belongs in Application.
* Database and external service implementations belong in Infrastructure.
* HTTP endpoints and request handling belong in Presentation.
* Public integration events belong in Contracts.
* Modules must not access another module's database directly.
* Modules must not reference another module's Domain, Application, Infrastructure, or Presentation project.
* A module may reference another module's Contracts project when required.
* Common projects must not contain module-specific business concepts.

---

## Module Structure

Each module normally contains:

```text
ECommerce.Modules.<Module>.Domain
ECommerce.Modules.<Module>.Application
ECommerce.Modules.<Module>.Infrastructure
ECommerce.Modules.<Module>.Presentation
ECommerce.Modules.<Module>.Contracts
```

Initial modules:

```text
Identity
Catalog
Inventory
Cart
Ordering
Payments
```

---

## Layer Rules

### Domain

May contain:

* Aggregates
* Entities
* Value objects
* Domain events
* Domain services
* Domain exceptions
* Repository abstractions

Must not depend on:

* ASP.NET Core
* Entity Framework Core
* MongoDB
* Redis
* Stripe
* MediatR
* Infrastructure
* Presentation
* Another module

### Application

May contain:

* Commands
* Queries
* Handlers
* Validators
* DTOs
* Application services
* Domain event handlers
* Integration event handlers
* Application abstractions

May depend on:

* Its own Domain
* Its own Contracts
* Common Application
* Another module's Contracts when explicitly required

Must not depend on:

* Infrastructure
* Presentation
* Another module's internal projects

### Infrastructure

May contain:

* EF Core
* MongoDB
* Redis
* Stripe
* Repositories
* DbContexts
* Migrations
* External clients
* Outbox processing
* Dependency injection registration

Infrastructure implements abstractions defined by Domain or Application.

### Presentation

May contain:

* Controllers
* Minimal API endpoints
* HTTP requests
* HTTP responses
* Authorization configuration
* Endpoint registration

Presentation must not:

* Access databases directly
* Contain business rules
* Use another module's infrastructure
* Return domain entities directly

### Contracts

May contain:

* Integration events
* Public module responses
* Explicit inter-module interfaces

Contracts must not expose:

* Domain entities
* Aggregates
* Repositories
* DbContexts
* MongoDB documents
* Internal commands
* Internal queries

---

## Module Communication

Modules communicate primarily using integration events.

Example:

```text
Catalog publishes ProductCreatedIntegrationEvent
Inventory handles ProductCreatedIntegrationEvent
```

The publishing module owns the event.

Example:

```text
Catalog.Contracts
└── ProductCreatedIntegrationEvent
```

The consuming handler belongs to the consumer module.

Example:

```text
Inventory.Application
└── ProductCreatedIntegrationEventHandler
```

Domain events remain internal to the module.

Integration events are public contracts.

---

## Coding Rules

* Use modern C#.
* Enable nullable reference types.
* Prefer sealed classes when inheritance is not required.
* Prefer immutable records for commands, queries, and events.
* Pass `CancellationToken` through asynchronous calls.
* Use clear names.
* Avoid vague names such as `Helper`, `Manager`, or `Utils`.
* Keep methods focused.
* Use feature folders.
* Do not create abstractions without a real use case.
* Do not create repositories for every entity.
* Create repositories only for aggregate roots.
* Protect domain invariants inside aggregates.
* Use expected failure results for normal business errors.
* Reserve exceptions for exceptional or invalid internal states.

---

## Feature Folder Structure

Prefer:

```text
Application/
└── Products/
    ├── CreateProduct/
    │   ├── CreateProductCommand.cs
    │   ├── CreateProductCommandHandler.cs
    │   └── CreateProductValidator.cs
    └── GetProductById/
        ├── GetProductByIdQuery.cs
        └── GetProductByIdQueryHandler.cs
```

Avoid grouping the whole module into global folders such as:

```text
Commands/
Queries/
Handlers/
Validators/
```

---

## Data Ownership

Initial persistence ownership:

| Module    | Persistence         |
| --------- | ------------------- |
| Identity  | SQLite with EF Core |
| Catalog   | MongoDB             |
| Inventory | SQLite with EF Core |
| Cart      | SQLite with EF Core |
| Ordering  | SQLite with EF Core |
| Payments  | SQLite with EF Core |

Redis is used only for caching.

A module must not:

* Use another module's DbContext
* Query another module's tables
* Query another module's MongoDB collections
* Use another module's repositories

---

## Task Workflow

For every task:

1. Read the task completely.
2. Inspect the existing repository.
3. Identify affected modules and projects.
4. Implement only the requested scope.
5. Add appropriate tests.
6. Run validation commands.
7. Review architecture boundaries.
8. Provide a final implementation report.

Do not perform unrelated refactoring.

Do not implement future tasks early.

---

## Validation

Run:

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

Run any additional commands required by the active task.

Do not claim a command passed unless it was actually executed successfully.

---

## Completion Report

After completing a task, report:

### Summary

What was implemented.

### Files Changed

Important files created or modified.

### Tests

Tests added or updated.

### Validation

Commands executed and their result.

### Architectural Decisions

Any important implementation choices.

### Scope Confirmation

Confirm that no unrelated functionality was added.

### Issues

Any unresolved problem, assumption, or follow-up item.
