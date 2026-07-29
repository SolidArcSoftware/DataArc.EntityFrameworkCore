# DataArc.EntityFrameworkCore

> **Explicit EF Core execution across multiple `DbContext` and database boundaries.**

[![Documentation](https://img.shields.io/badge/docs-DataArc.EntityFrameworkCore-2F81F7)](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)
[![NuGet](https://img.shields.io/badge/NuGet-DataArc.EntityFrameworkCore-004880)](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
[![Website](https://img.shields.io/badge/website-dataarc.dev-222222)](https://www.dataarc.dev)
[![.NET](https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

**EF Core owns data access. DataArc controls execution.**

---

## The problem

EF Core handles one `DbContext` well.

The difficulty starts when an application must coordinate work across:

- multiple `DbContext` implementations;
- bounded contexts or modular persistence boundaries;
- separate databases;
- high-volume imports and synchronisation jobs;
- ordered, parallel, or transaction-aware workflows;
- database and script generation;
- structured execution outcomes.

Without a consistent execution model, this logic often spreads across repositories, handlers, services, factories, and orchestration code.

```text
Application workflow
    ├── DbContext A
    ├── DbContext B
    ├── DbContext C
    └── custom coordination everywhere
```

## The DataArc approach

DataArc.EntityFrameworkCore adds an explicit execution layer above EF Core.

```text
Application workflow
    ↓
DataArc query and command pipelines
    ↓
Explicit execution-context selection
    ↓
Registered EF Core DbContext
```

Concrete `DbContext` implementations stay inside persistence.

Application code targets public execution-context contracts and chooses exactly where each query or command must run.

```csharp
var employees = await query
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Rating > 4.5);
```

The execution boundary is visible, deliberate, and testable.

---

## What it gives you

| Capability | What it solves |
|---|---|
| **Execution contexts** | Route work to the correct `DbContext` without exposing concrete persistence classes |
| **Query pipelines** | Run explicit reads through registered database boundaries |
| **Command pipelines** | Build coordinated write workflows before execution |
| **Bulk operations** | Process large prepared collections through the same execution model |
| **Parallel execution** | Run independent context operations concurrently |
| **Structured results** | Inspect success, exceptions, and affected-record counts consistently |
| **Database generation** | Create databases and generate reviewable SQL scripts from EF Core models |
| **Architectural flexibility** | Use DataArc inside services, repositories, modular monoliths, bounded contexts, CQRS, or Clean Architecture |

DataArc does not require a repository per table or a command/query class for every database permutation.

It provides the execution infrastructure. Your application keeps the business intent.

---

## Command pipeline

Create one command, target multiple execution contexts, and choose the execution mode.

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IOpenAiDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var result = await command.ExecuteParallelAsync();

if (!result.Success)
{
    throw new InvalidOperationException(
        "Employee distribution failed.",
        result.Exception);
}

return result.TotalAffected;
```

```text
One application command
    ├── GoogleDbContext
    ├── MicrosoftDbContext
    └── OpenAiDbContext
            ↓
    structured execution result
```

---

## Public contracts, internal DbContexts

Application code depends on an execution boundary:

```csharp
public interface IGoogleDbContext : IExecutionContext
{
    DbSet<Employer>? Employer { get; set; }

    DbSet<Employee>? Employee { get; set; }
}
```

The concrete EF Core implementation remains internal:

```csharp
internal sealed class GoogleDbContext :
    DbContext,
    IGoogleDbContext
{
    public GoogleDbContext(
        DbContextOptions<GoogleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employer>? Employer { get; set; }

    public DbSet<Employee>? Employee { get; set; }
}
```

Registration maps the public contract to the implementation:

```csharp
services.AddDataArcCore();

services.ConfigureDataArc(dataArc =>
{
    dataArc.UseEntityFrameworkCore(ef =>
    {
        ef.AddDbExecutionContext<
            IGoogleDbContext,
            GoogleDbContext>(
                options =>
                    options.UseSqlServer(
                        googleConnectionString));
    });
});
```

---

## Database and script generation

Generate a database from the current EF Core model and retain reviewable SQL scripts.

```csharp
var database = _databaseFactory
    .CreateDatabaseBuilder()
    .IncludeDbContext<GoogleDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);

database.ExecuteCreate();
```

The same builder model supports database deletion:

```csharp
database.ExecuteDrop();
```

This is useful for:

- repeatable development environments;
- integration tests;
- demos;
- empty-database onboarding;
- reviewed baseline SQL generation.

For existing production databases, use an appropriate incremental schema-migration strategy.

---

## Demonstration repository

This repository demonstrates DataArc.EntityFrameworkCore across four isolated SQL Server database boundaries:

```text
SAS_GoogleDb
SAS_MicrosoftDb
SAS_OpenAiDb
SAS_Db
```

The demo:

1. creates and seeds the databases;
2. reads employees through an explicit source context;
3. applies salary-adjustment rules;
4. bulk-writes prepared data to independent targets;
5. executes target operations in parallel;
6. queries each database independently;
7. consolidates the read model in application code;
8. returns structured execution results.

No cross-database EF Core join is used.

Each database remains its own persistence boundary.

---

## Benchmark snapshot

The included BenchmarkDotNet project exercises parallel bulk insertion across four EF Core contexts.

| Total inserted records | Mean |
|---:|---:|
| 250,000 | 560.5 ms |
| 500,000 | 1,077.9 ms |
| 1,000,000 | 1,964.6 ms |

These results are workload-specific, not a universal performance guarantee.

Hardware, SQL Server configuration, schema design, indexes, batch size, runtime version, and database state all affect performance.

The important execution shape is:

```text
one explicit command pipeline
    + four EF Core contexts
    + parallel execution
    + structured result handling
```

---

## Run the demo

### 1. Configure SQL Server

Update the connection strings in the host `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "GoogleDb": "Server=YOUR_SERVER;Database=SAS_GoogleDb;Integrated Security=true;TrustServerCertificate=True;",
    "MicrosoftDb": "Server=YOUR_SERVER;Database=SAS_MicrosoftDb;Integrated Security=true;TrustServerCertificate=True;",
    "OpenAiDb": "Server=YOUR_SERVER;Database=SAS_OpenAiDb;Integrated Security=true;TrustServerCertificate=True;",
    "SolidArcDb": "Server=YOUR_SERVER;Database=SAS_Db;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

### 2. Run

```bash
dotnet run --framework net8.0
```

The demo multi-targets .NET 6, 7, 8, 9, and 10.

> The demo recreates its configured databases. Use disposable development databases only.

---

## Documentation

The full documentation covers:

- getting started;
- compatibility;
- execution contexts;
- query pipelines;
- command pipelines;
- bulk and parallel operations;
- transactional workflows;
- structured execution results;
- database and script generation;
- trial and licensing.

### [Read the DataArc.EntityFrameworkCore documentation →](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)

---

## Where DataArc fits

DataArc.EntityFrameworkCore is intended for applications where EF Core execution must be coordinated across controlled persistence boundaries.

It fits naturally inside:

- modular monoliths;
- bounded-context systems;
- Clean Architecture;
- CQRS-style applications;
- background workers;
- batch-processing systems;
- data synchronisation workflows;
- architecture-remediation projects.

It is not intended to bypass service ownership or encourage unrelated services to access each other's private databases.

The goal is explicit coordination—not hidden coupling.

---

## Evaluate DataArc

Use the trial against a real workload and measure whether DataArc reduces:

- custom EF Core coordination code;
- repository and handler sprawl;
- duplicated execution logic;
- ambiguity around context selection;
- bulk-processing complexity;
- result-handling inconsistency.

### [Start a trial or purchase a license →](https://www.dataarc.dev)

---

## The core idea

```text
EF Core
    owns database access

DataArc.EntityFrameworkCore
    owns explicit execution
```

**Keep your contexts separate. Make execution visible. Coordinate the workflow.**
