# AGENTS.md instructions for C:\Users\ricar\Desafio-Tecnico-CIEE

# Project: C# .NET 8 + Angular 22 + SQL Server 2022

Fullstack application with ASP.NET Core Web API (.NET 8), Angular 22 (Standalone Components), and relational database with Entity Framework Core (SQL Server 2022 with EF Core 8).

## Role

You are a senior fullstack .NET and Angular developer. Always apply idiomatic .NET 8 and modern Angular patterns, clean architecture, and strict dependency injection. Avoid legacy patterns (no old NgModules, no manual DbContext lifecycles, no ad-hoc HTTP calls without services).

## Code standards

### Backend (.NET 8 Web API)

- Never instantiate services or DbContext directly (no `new ApplicationDbContext()`, no `new CandidateService()`) — always use constructor injection via native DI (`IServiceCollection`).
- Use proper DI lifetimes:
  - Repositories and DbContext: `AddScoped`
  - Stateless processing and calculation engines: `AddTransient` or `AddScoped`
  - Caches and single-instance cross-cutting clients: `AddSingleton`
- Layered layout:
  - `Ciee.Curriculos.Api/Controllers/` — thin controllers with `[ApiController]` and route attributes
  - `Ciee.Curriculos.Api/Services/` — business logic interfaces and implementations
  - `Ciee.Curriculos.Api/Data/` — DbContext, entity configurations (`IEntityTypeConfiguration<T>`), and migrations
  - `Ciee.Curriculos.Api/DTOs/` — request and response data contracts, validated using FluentValidation or Data Annotations
  - `Ciee.Curriculos.Api/Common/` — cross-cutting middleware, filters, and global error handling
- Enforce async/await from controller to database (`async Task<IActionResult>`, `SaveChangesAsync`, `ToListAsync`).
- Prevent query memory bloat: use `.AsNoTracking()` for read-only queries.

### Frontend (Angular 22)

- Always use **Standalone Components** (`standalone: true`), eliminating legacy `NgModule`.
- Prefer Angular Signals (`signal`, `computed`, `effect`) and reactive state management over redundant subscriptions.
- Manage forms strictly with **Reactive Forms** (`FormGroup`, `FormControl`, `Validators`).
- All backend communication must be encapsulated in injectable services (`@Injectable({ providedIn: 'root' })`) using `HttpClient`. Never call `fetch()` directly in components.
- Layered layout:
  - `ciee-curriculos-web/src/app/pages/` — smart components and view logic (Cadastro, Listagem, Detalhes)
  - `ciee-curriculos-web/src/app/services/` — injectable services communicating with the Web API
  - `ciee-curriculos-web/src/app/models/` — TypeScript interfaces and request/response contracts

### Database & Migrations

- Define entity mappings using Fluent API in `OnModelCreating` or separate configuration classes rather than polluting domain classes with data-layer attributes.
- Database target is exclusively Microsoft SQL Server 2022 (via Docker Compose container).
- Use EF Core CLI for migrations and idempotent SQL generation:
  - `dotnet ef migrations add <MigrationName> --project Ciee.Curriculos.Api`
  - `dotnet ef database update --project Ciee.Curriculos.Api`
  - `dotnet ef migrations script --idempotent --project Ciee.Curriculos.Api`

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
