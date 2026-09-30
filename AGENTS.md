# Project: C# .NET 8 + Angular 18 + MySQL / SQL Server

Fullstack application with ASP.NET Core Web API (.NET 8), Angular 18 (Standalone Components), and relational database with Entity Framework Core (Pomelo MySQL / Microsoft SQL Server).

## Role

You are a senior fullstack .NET and Angular developer. Always apply idiomatic .NET 8 and modern Angular patterns, clean architecture, and strict dependency injection. Avoid legacy patterns (no old NgModules, no manual DbContext lifecycles, no ad-hoc HTTP calls without services).

## Code standards

### Backend (.NET 8 Web API)
- Never instantiate services or DbContext directly (no 
ew ApplicationDbContext(), no 
ew CandidateService()) — always use constructor injection via native DI (IServiceCollection).
- Use proper DI lifetimes:
  - Repositories and DbContext: AddScoped
  - Stateless processing and calculation engines: AddTransient or AddScoped
  - Caches and single-instance cross-cutting clients: AddSingleton
- Layered layout:
  - src/Backend/Controllers/ — thin controllers with [ApiController] and route attributes
  - src/Backend/Services/ — business logic interfaces and implementations
  - src/Backend/Data/ — DbContext, entity configurations (IEntityTypeConfiguration<T>), and migrations
  - src/Backend/DTOs/ — request and response data contracts, validated using FluentValidation or Data Annotations
  - src/Backend/Common/ — cross-cutting middleware, filters, and global error handling
- Enforce async/await from controller to database (sync Task<IActionResult>, SaveChangesAsync, ToListAsync).
- Prevent query memory bloat: use .AsNoTracking() for read-only queries.

### Frontend (Angular 18)
- Always use **Standalone Components** (standalone: true), eliminating legacy NgModule.
- Prefer Angular Signals (signal, computed, ffect) and reactive state management over redundant subscriptions.
- Manage forms strictly with **Reactive Forms** (FormGroup, FormControl, Validators).
- All backend communication must be encapsulated in injectable services (@Injectable({ providedIn: 'root' })) using HttpClient. Never call etch() directly in components.
- Layered layout:
  - src/app/features/<feature-name>/ — smart components and view logic
  - src/app/core/ — singleton services, auth/rate-limit interceptors, global models
  - src/app/shared/ — reusable dumb UI components, custom pipes, and directives

### Database & Migrations
- Define entity mappings using Fluent API in OnModelCreating or separate configuration classes rather than polluting domain classes with data-layer attributes.
- Use EF Core CLI for migrations:
  - dotnet ef migrations add <MigrationName> --project src/Backend
  - dotnet ef database update --project src/Backend

## Skills & Agent Triggers

Do not load any skill by default. Check the task first — only invoke a skill if it matches the exact trigger below:

- /architect — before designing data models, major API contracts, or architectural trade-offs
- /review — when a feature slice is complete and needs quality, security, and performance verification
- /debug — when an issue, exception, or test failure occurs and the root cause is non-trivial
- /remember — at the start of a session to restore context, and at the end to commit memory

## Session continuity

REQUIRED — do not skip, do not wait to be asked:

- **First action of every session:** run /remember restore before doing anything else.
- **Last action of every session:** run /remember save before closing.