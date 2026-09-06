# ECommerce Architecture

## Overview

The application is a modular monolith built with .NET 9 and ASP.NET Core.

It is deployed as one application, but its business capabilities are separated into independent modules.

The architecture combines:

* Modular Monolith
* Domain-Driven Design
* Clean Architecture
* CQRS-style commands and queries
* Event-based module communication

The goal is to keep the application easy to run while maintaining clear boundaries between business areas.

---

## High-Level Structure

```text
Client
  |
  v
ECommerce.Api
  |
  +-------------------------------------------+
  |          |          |         |          |
  v          v          v         v          v
Identity   Catalog   Inventory   Cart   Ordering
                                               |
                                               v
                                           Payments
```

The API project is the application host.

Business behavior belongs inside modules.

---

## Repository Structure

```text
.
├── AGENTS.md
├── ARCHITECTURE.md
├── README.md
├── Directory.Build.props
├── Directory.Packages.props
├── global.json
├── ECommerce.sln
│
├── docs/
│   ├── glossary.md
│   └── tasks/
│
├── src/
│   ├── Api/
│   │   └── ECommerce.Api/
│   │
│   ├── Common/
│   │   ├── ECommerce.Common.Domain/
│   │   ├── ECommerce.Common.Application/
│   │   ├── ECommerce.Common.Infrastructure/
│   │   └── ECommerce.Common.Presentation/
│   │
│   └── Modules/
│       ├── Identity/
│       ├── Catalog/
│       ├── Inventory/
│       ├── Cart/
│       ├── Ordering/
│       └── Payments/
│
└── tests/
    ├── ECommerce.ArchitectureTests/
    ├── ECommerce.IntegrationTests/
    └── Modules/
```

---

## Module Structure

Each module normally contains:

```text
Domain
Application
Infrastructure
Presentation
Contracts
```

Example:

```text
Catalog/
├── ECommerce.Modules.Catalog.Domain/
├── ECommerce.Modules.Catalog.Application/
├── ECommerce.Modules.Catalog.Infrastructure/
├── ECommerce.Modules.Catalog.Presentation/
└── ECommerce.Modules.Catalog.Contracts/
```

---

## Layer Responsibilities

### Domain

Contains business behavior and rules.

Examples:

```text
Product
Cart
Order
Payment
StockItem
Money
Sku
DomainEvents
```

The Domain project must not depend on frameworks or persistence libraries.

### Application

Contains use cases.

Examples:

```text
CreateProductCommand
GetProductByIdQuery
PlaceOrderCommand
ReserveInventoryCommand
```

Application handlers coordinate domain objects and abstractions.

### Infrastructure

Contains technical implementations.

Examples:

```text
EF Core DbContexts
MongoDB repositories
Redis caching
Stripe integration
Repository implementations
Migrations
```

### Presentation

Contains HTTP-facing code.

Examples:

```text
Controllers
Minimal API endpoints
Request models
Response models
Authorization configuration
```

Presentation converts HTTP requests into commands and queries.

### Contracts

Contains public messages used by other modules.

Examples:

```text
ProductCreatedIntegrationEvent
OrderPlacedIntegrationEvent
PaymentCompletedIntegrationEvent
```

Other modules may reference Contracts but not internal module projects.

---

## Dependency Direction

Within a module:

```text
Presentation
    |
    v
Application
    |
    v
Domain

Infrastructure
    |
    +----> Application
    |
    +----> Domain
```

Contracts remain independent from internal implementation details.

Allowed:

```text
Inventory.Application
    -> Catalog.Contracts
```

Forbidden:

```text
Inventory.Application
    -> Catalog.Domain
```

Forbidden:

```text
Ordering.Infrastructure
    -> Inventory.Infrastructure
```

---

## Initial Modules

## Identity

Owns:

* Users
* Roles
* Authentication
* JWT access tokens
* Refresh tokens
* External login identities

Persistence:

```text
SQLite with EF Core
```

Possible integration events:

```text
UserRegisteredIntegrationEvent
UserDisabledIntegrationEvent
```

---

## Catalog

Owns:

* Products
* Categories
* Product descriptions
* Product prices
* Product attributes
* Product visibility
* Product search data

Does not own stock.

Persistence:

```text
MongoDB
```

Possible integration events:

```text
ProductCreatedIntegrationEvent
ProductPriceChangedIntegrationEvent
ProductDiscontinuedIntegrationEvent
```

---

## Inventory

Owns:

* Stock
* Available quantity
* Reserved quantity
* Stock adjustments
* Stock reservations

Does not own product descriptions or prices.

Persistence:

```text
SQLite with EF Core
```

Possible integration events:

```text
InventoryReservedIntegrationEvent
InventoryReservationFailedIntegrationEvent
InventoryReleasedIntegrationEvent
```

---

## Cart

Owns:

* Active carts
* Cart items
* Item quantities
* Cart item snapshots

Does not own product or stock truth.

Persistence:

```text
SQLite with EF Core
```

---

## Ordering

Owns:

* Orders
* Order lines
* Order status
* Order totals
* Product snapshots at purchase time
* Order history

Persistence:

```text
SQLite with EF Core
```

Possible integration events:

```text
OrderPlacedIntegrationEvent
OrderCancelledIntegrationEvent
OrderCompletedIntegrationEvent
```

---

## Payments

Owns:

* Payment attempts
* Payment status
* Stripe references
* Payment idempotency
* Webhook processing

Does not update orders directly.

Persistence:

```text
SQLite with EF Core
```

Possible integration events:

```text
PaymentStartedIntegrationEvent
PaymentCompletedIntegrationEvent
PaymentFailedIntegrationEvent
PaymentRefundedIntegrationEvent
```

---

## Module Communication

Modules communicate primarily through integration events.

Example:

```text
Catalog
  |
  | ProductCreatedIntegrationEvent
  v
Inventory
```

The event belongs to:

```text
Catalog.Contracts
```

The handler belongs to:

```text
Inventory.Application
```

The consumer does not reference the publisher's Domain or Infrastructure.

---

## Domain Events and Integration Events

Domain events are internal to a module.

Example:

```text
Catalog.Domain
└── ProductCreatedDomainEvent
```

Integration events are public messages.

Example:

```text
Catalog.Contracts
└── ProductCreatedIntegrationEvent
```

Typical flow:

```text
Product aggregate
      |
      v
ProductCreatedDomainEvent
      |
      v
Catalog Application
      |
      v
ProductCreatedIntegrationEvent
      |
      v
Inventory Application
```

---

## Event Delivery

The first version uses in-process event delivery.

```text
Publisher
   |
   v
Internal event bus
   |
   v
Consumer handler
```

A later task will introduce:

* Outbox pattern
* Background dispatcher
* Retry handling
* Idempotent consumers

No external message broker is required for the initial modular monolith.

---

## Data Ownership

Each module owns its persistence.

Even when relational modules use the same SQLite database file, they should use separate DbContexts.

Example:

```text
IdentityDbContext
InventoryDbContext
CartDbContext
OrderingDbContext
PaymentsDbContext
```

Catalog owns its MongoDB database or collections.

Modules must not access another module's persistence directly.

---

## Data Duplication

Controlled duplication is allowed when it protects module independence.

Example:

An order line may store:

```text
ProductId
ProductName
Sku
UnitPrice
Quantity
```

The order keeps this snapshot even when the Catalog product later changes.

Ordering must not load the current product every time an old order is displayed.

---

## Request Flow

Command flow:

```text
HTTP Request
    |
    v
Presentation
    |
    v
Command
    |
    v
Application Handler
    |
    v
Domain Aggregate
    |
    v
Repository
    |
    v
Infrastructure
```

Query flow:

```text
HTTP Request
    |
    v
Presentation
    |
    v
Query
    |
    v
Query Handler
    |
    v
Read Model or Repository
    |
    v
Response DTO
```

---

## Common Projects

Common contains technical building blocks only.

Possible shared types:

```text
Entity
AggregateRoot
ValueObject
IDomainEvent
IIntegrationEvent
Result
Error
TimeProvider abstraction
Event bus abstraction
Pagination
```

Common must not contain:

```text
Product
Order
Cart
Payment
InventoryItem
Customer
```

Those concepts belong to modules.

---

## Testing

The solution will contain:

### Domain Tests

Test:

* Aggregate behavior
* Value objects
* Business invariants
* State transitions
* Domain events

### Application Tests

Test:

* Commands
* Queries
* Handler coordination
* Expected failures
* Event handling

### Integration Tests

Test:

* HTTP endpoints
* Persistence
* Authentication
* Cross-module workflows

### Architecture Tests

Enforce:

* Layer dependencies
* Module isolation
* Contracts-only cross-module references
* Domain framework independence

---

## Development Order

The project will be implemented in this order:

1. Solution structure
2. Module registration
3. Architecture tests
4. Common building blocks
5. Catalog
6. Inventory
7. Identity
8. Cart
9. Ordering
10. Payments
11. Reliable event delivery
12. Redis caching
13. Docker and observability

Each step will have its own task file under:

```text
docs/tasks/
```

---

## Core Constraints

1. The API contains composition, not business logic.
2. Domain contains business rules.
3. Application coordinates use cases.
4. Infrastructure implements technical details.
5. Presentation handles HTTP.
6. Contracts contain public module messages.
7. Modules do not access one another's databases.
8. Modules reference one another only through Contracts.
9. Domain events remain internal.
10. Integration events belong to their publisher.
11. Common does not contain business models.
12. Cross-module workflows use eventual consistency when necessary.
