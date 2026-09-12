# Nesto Clean .NET 10

API-only apartment booking system built with .NET 10, Clean Architecture, DDD and CQRS. It preserves the functionality of the Gabriel variant while replacing its licensed or unnecessary dependencies with small interfaces and free open-source components.

## Main decisions

- ASP.NET Core Identity stores local accounts and hashes passwords.
- JWT access tokens and rotating refresh tokens authenticate API clients.
- EF Core handles commands and simple `AsNoTracking` reads; Dapper handles only complex read models behind Application ports.
- Commands and queries use local handler interfaces plus Scrutor decorators, without MediatR.
- Mapping is explicit, without AutoMapper.
- Tests use Shouldly, without FluentAssertions.
- HybridCache provides local L1 caching and Redis-backed L2 caching. Apartment searches are cached and invalidated by tag after writes.
- A transactional domain Outbox dispatches idempotent domain handlers. Those handlers stage versioned integration events with stable identities in a second Outbox before a replaceable transport publishes them.
- PostgreSQL provides persistence and optimistic concurrency.
- Serilog, Seq, OpenTelemetry, health checks, rate limiting and Swagger cover operations.

All third-party runtime and test packages are free and open source.

## Architecture

```text
src/
  SharedKernel/     Nesto.SharedKernel: Result, Error, Entity and domain event primitives
  Contracts/        Nesto.Contracts: versioned integration events shared with external consumers
  Domain/           Nesto.Domain: aggregates, value objects, business rules and events
  Application/      Nesto.Application: use cases, ports, validation and CQRS handlers
  Infrastructure/   Nesto.Infrastructure: EF Core, Dapper, Identity, JWT, HybridCache and Outbox
  Web.Api/          Nesto.Api: versioned Minimal API endpoints and middleware
tests/
  Domain.UnitTests/       Nesto.Domain.UnitTests
  Application.UnitTests/  Nesto.Application.UnitTests
  ArchitectureTests/      Nesto.ArchitectureTests
  IntegrationTests/       Nesto.IntegrationTests
```

Dependencies point inward: Domain only references SharedKernel; Application references Domain, Contracts and SharedKernel; Infrastructure implements the ports; Web.Api is the composition root. Repositories are aggregate-specific instead of generic.

Domain events never leave the service boundary. Application handlers translate relevant business facts into `Contracts.IntegrationEvents.V1` messages. Infrastructure stores those messages transactionally in `integration_outbox_messages`; the included logging transport is a working example that can be replaced by RabbitMQ, Kafka, Azure Service Bus or another free or managed transport without changing Domain or Application.

Query handlers depend on read-service interfaces declared in Application. Infrastructure implements simple user, booking and review reads with EF Core `AsNoTracking`; apartment availability remains an optimized Dapper query because it contains overlap exclusion logic. Application contains no SQL and references neither Dapper, EF Core nor Npgsql.

## Design patterns and boundaries

### Rich domain model

Aggregates expose behavior instead of public setters. Static factory methods such as `User.Create`, `Apartment.Create`, `Booking.Reserve` and `Review.Create` ensure valid construction and raise domain events. Value objects including `Money`, `Currency`, `DateRange`, `Email` and `Rating` prevent invalid primitive values from moving through the application.

`PricingService` is a domain service because pricing combines apartment amenities, duration and currency rules without naturally belonging to one value object. It is passed explicitly to the aggregate, keeping Domain independent from the dependency-injection container.

### Result and Problem Details

Expected validation and business failures use `Result` and typed `Error` values instead of exceptions. Web.Api translates them centrally into RFC Problem Details responses. Exceptions are reserved for unexpected technical failures and are handled by the global exception handler.

### Repository and Unit of Work

Repository interfaces live beside their aggregates in Domain and expose only operations required by the use cases. Infrastructure provides the EF Core implementations. Command handlers change aggregates through those repositories and commit once through `IUnitOfWork`, defining a clear transaction boundary that also captures domain events in the Outbox.

### Decorator pipeline

Scrutor composes cross-cutting behavior around command and query handlers:

```text
Endpoint -> Logging decorator -> Validation decorator -> Handler
```

Handlers therefore contain use-case orchestration without duplicated logging or validation code.

### Vertical slices and REPR

Projects enforce Clean Architecture between layers, while code inside each layer is grouped by feature and use case, such as `Bookings/Reserve` or `Reviews/Create`. Minimal API endpoints follow Request-Endpoint-Response: each endpoint owns its HTTP request contract and delegates to one application handler. Endpoints and handlers are discovered by convention, avoiding a central registration list.

### Identity boundary

`IdentityAccount` owns password hashes and ASP.NET Core Identity concerns. `Domain.User` owns profile and business behavior. Both records share the same identifier and are persisted in one EF Core unit of work, so Domain never references Identity while account creation remains atomic.

### Consistency and delivery semantics

PostgreSQL `xmin` provides optimistic concurrency for write conflicts. HybridCache follows cache-aside with tag invalidation. Domain and integration Outbox processors retry failed messages and provide at-least-once delivery, so external consumers must be idempotent.

### Domain-event idempotency

The domain Outbox message identifier represents one specific occurrence of a business fact. It is passed to handlers through `DomainEventContext` together with the original timestamp, leaving domain-event payloads free of persistence concerns. Two legitimate occurrences receive different identifiers; a retry of the same occurrence keeps the original identifier.

Each successful handler writes a checkpoint to `processed_domain_event_handlers`. Its composite primary key `(event_id, handler_name)` is the deduplication constraint, so a replay skips only handlers that already completed and still allows other handlers for the same event to run. Handler changes, the checkpoint and any integration Outbox record are persisted in the same PostgreSQL transaction. A savepoint prevents partial database changes from a failed handler from being committed.

Both Outbox processors claim batches with `FOR UPDATE SKIP LOCKED`. Multiple application instances can therefore process different messages concurrently without selecting the same row. An integration-event identifier is derived deterministically from the domain occurrence and the integration-event type. Replays keep the same identifier, while one domain occurrence can still produce several different integration events without key collisions.

This produces at-least-once message delivery with an idempotent transactional effect inside Nesto. It does not claim distributed exactly-once delivery: an external broker can redeliver after a publish/acknowledgement failure, and consumers must deduplicate the stable integration-event identifier through an Inbox or an equivalent unique constraint. Domain-event handlers must not call external systems directly; they stage integration events in the transactional Outbox instead.

### Executable architecture rules

Architecture tests act as fitness functions. They prevent forbidden layer references, keep persistence libraries out of Application, require immutable domain-event records and protect aggregate encapsulation. Integration tests run against a real PostgreSQL container so EF mappings, value converters and Dapper SQL are verified against the production database engine.

### Current authorization scope

The project includes dynamic permission-policy infrastructure, but it is not a complete RBAC implementation yet. `PermissionProvider` has no persistent permission source, so protected endpoints currently rely primarily on authentication and resource-ownership checks. This capability should not be treated as production role management until a permission store and corresponding tests are added.

## Functional scope

The versioned API is exposed below `/api/v1`:

- Users: register, login, refresh token, current profile, update profile and get by id.
- Apartments: search availability, create and update owned apartments.
- Bookings: reserve, list, get, confirm, reject, complete and cancel.
- Reviews: list, create after a completed stay, update and delete.

Resource-level authorization prevents users from reading or changing records they do not own.

## Run locally

Requirements: .NET 10 SDK and Docker.

```bash
docker compose up -d nesto-db nesto-cache nesto-seq
dotnet run --project src/Web.Api/Nesto.Api.csproj --launch-profile http
```

- Swagger: `http://localhost:5000/swagger`
- Health: `http://localhost:5000/health`
- Seq: `http://localhost:8082`
- PostgreSQL: `localhost:5440`
- Redis: `localhost:6380`

Development startup applies migrations and seeds sample apartments. Secrets and production connection strings must be supplied through environment variables or a secret store.

To run the complete stack in containers:

```bash
docker compose up --build
```

## Verify

```bash
dotnet build Nesto.sln
dotnet test Nesto.sln
```

Integration tests use Testcontainers and require Docker. They create an isolated PostgreSQL instance and do not use the developer database.
