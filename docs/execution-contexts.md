# DbContext Boundaries

DataArc.EntityFrameworkCore 2.0 does not require a separate execution-context abstraction around EF Core.

A normal `DbContext` remains the persistence boundary.

```csharp
public sealed class HrDbContext : DbContext
{
    public HrDbContext(
        DbContextOptions<HrDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
}
```

## IDbContextFactory

For applications that need short-lived or independently created contexts, use Microsoft's `IDbContextFactory<TContext>`:

```csharp
private readonly IDbContextFactory<HrDbContext> _dbContextFactory;

public EmployeeService(
    IDbContextFactory<HrDbContext> dbContextFactory)
{
    _dbContextFactory = dbContextFactory;
}
```

Create and dispose contexts normally:

```csharp
await using var dbContext =
    await _dbContextFactory.CreateDbContextAsync();
```

## A DbContext boundary is not necessarily a database boundary

Applications may use several contexts against:

- separate databases;
- one shared physical database;
- different schemas inside one database.

The application boundary and the relational boundary are separate design decisions.

```text
HrDbContext
FinanceDbContext
ItDbContext
OperationsDbContext
        |
        +----> one physical relational database
```

The commercial SQL Server DDL Builder can compose several context models into one relational database definition.

See [Multi-Context DDL Builder](database-generation.md).

## Independent databases

The EntityFrameworkCore demo uses four independent databases:

```text
SolidArcDbContext  -> SAS_Db
GoogleDbContext    -> SAS_GoogleDb
MicrosoftDbContext -> SAS_MicrosoftDb
OpenAIDbContext    -> SAS_OpenAiDb
```

Each context is created and queried independently.

Application-level coordination is explicit:

```csharp
await Task.WhenAll(
    ExecuteGoogleAsync(),
    ExecuteMicrosoftAsync(),
    ExecuteOpenAiAsync());
```

## Thread safety

EF Core `DbContext` itself is not thread-safe and should not be used for concurrent EF operations against the same instance.

DataArc parallel execution does not change that EF Core rule.

Where DataArc performs parallel work, it coordinates execution through separate internal execution contexts rather than concurrently mutating the same EF Core context instance.

## Lifetime nuance

A `DbContext` registered directly with `AddDbContext<TContext>()` is scoped by default.

An `IDbContextFactory<TContext>` registered with `AddDbContextFactory<TContext>()` is singleton by default, and contexts created by that factory are caller-owned independent instances.

For ASP.NET Core applications using DataArc scoped infrastructure, a scoped factory registration is recommended:

```csharp
services.AddDbContextFactory<HrDbContext>(
    options => options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);
```

The correct lifetime therefore depends on how the context is created and how it participates in the application scope.
