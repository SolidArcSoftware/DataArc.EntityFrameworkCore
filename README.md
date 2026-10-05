# DataArc.EntityFrameworkCore

> **High-performance EF Core execution without replacing EF Core.**

[![Documentation](https://img.shields.io/badge/docs-DataArc.EntityFrameworkCore-2F81F7)](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)
[![NuGet](https://img.shields.io/badge/NuGet-DataArc.EntityFrameworkCore-004880)](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
[![Website](https://img.shields.io/badge/website-dataarc.dev-222222)](https://www.dataarc.dev)
[![.NET](https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

**EF Core remains EF Core. DataArc adds execution capabilities where they are useful.**

---

## Overview

`DataArc.EntityFrameworkCore` extends normal Entity Framework Core usage with bulk and parallel persistence execution.

It does not replace:

- `DbContext`
- `IDbContextFactory<TContext>`
- LINQ
- EF Core change tracking
- EF Core transactions
- normal application-level coordination

Use EF Core normally where normal EF Core is sufficient.

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();

var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Rating > 4.5)
    .ToListAsync();
```

Use DataArc when persistence execution benefits from bulk or parallel work.

```csharp
await dbContext
    .AsParallel()
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

The goal is additive execution capability rather than another persistence abstraction.

---

## Installation

```xml
<PackageReference Include="DataArc.EntityFrameworkCore" Version="2.0.0" />
```

Register DataArc once at the application composition root:

```csharp
services.AddDataArcCore();
```

Register your contexts using normal EF Core configuration:

```csharp
services.AddDbContextFactory<GoogleDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("GoogleDb")));

services.AddDbContextFactory<MicrosoftDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("MicrosoftDb")));

services.AddDbContextFactory<OpenAIDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("OpenAiDb")));

services.AddDbContextFactory<SolidArcDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("SolidArcDb")));
```

DataArc does not replace `DbContext` registration or ordinary EF Core data access.

---

## Bulk Operations

Prepared collections can be written directly through the `DbContext`.

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

The free package also supports bulk work inside an EF Core transaction owned by the caller.

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.AddBulkTransactionalAsync(
    employees,
    batchSize);

await transaction.CommitAsync();
```

In this model:

```text
EF Core owns the transaction.
DataArc participates in the current transaction.
```

---

## Parallel Persistence

DataArc provides a fluent execution model for persistence operations staged against a `DbContext`.

```csharp
await dbContext
    .AsParallel()
    .Add(employee)
    .AddRange(employees)
    .AddBulk(importEmployees, batchSize)
    .SaveChangesParallelAsync();
```

Supported operations include:

- `Add`
- `AddRange`
- `Update`
- `Remove`
- `AddBulk`

The fluent chain describes persistence work to be executed by DataArc.

It should not be treated as an ordered business-workflow script.

Business workflow ordering belongs in the application or orchestration layer.

---

## Multiple Bulk Operations

A single execution plan can contain multiple bulk operations.

The demo uses this to write employers and employees together:

```csharp
await dbContext
    .AsParallel()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

This keeps the application-facing API close to normal EF Core usage while allowing DataArc to own the execution machinery.

---

## Multiple DbContexts And Databases

Independent `DbContext`s remain ordinary EF Core contexts.

Applications can coordinate them using normal .NET concurrency:

```csharp
await using var googleDbContext =
    await googleDbContextFactory.CreateDbContextAsync();

await using var microsoftDbContext =
    await microsoftDbContextFactory.CreateDbContextAsync();

await using var openAiDbContext =
    await openAiDbContextFactory.CreateDbContextAsync();

var results = await Task.WhenAll(
    googleDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    microsoftDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    openAiDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync());

var totalAffected = results.Sum();
```

Each `DbContext` remains independent:

```text
Application
    |
    ├── GoogleDbContext
    │       └── DataArc parallel execution
    |
    ├── MicrosoftDbContext
    │       └── DataArc parallel execution
    |
    └── OpenAIDbContext
            └── DataArc parallel execution
```

There is no shared `DbContext` and no distributed transaction implied by `Task.WhenAll`.

The application owns coordination across independent database boundaries.

---

## Demonstration Repository

This repository demonstrates `DataArc.EntityFrameworkCore` against four independent SQL Server databases:

```text
SAS_GoogleDb
SAS_MicrosoftDb
SAS_OpenAiDb
SAS_Db
```

The persistence layer uses ordinary EF Core:

```text
Persistence
    ├── IDbContextFactory<TContext>
    ├── normal LINQ queries
    ├── normal EF Core database creation
    └── no DataArc persistence abstraction
```

DataArc is used only where it adds execution capability.

The main demo workflow:

1. reads employers and employees from `SolidArcDbContext` using normal EF Core;
2. applies salary adjustments in application code;
3. creates three independent destination `DbContext`s;
4. stages employer and employee bulk operations for each destination;
5. executes the three destination pipelines concurrently with `Task.WhenAll`;
6. returns the total number of affected rows.

With 100,000 employers and 100,000 employees distributed to three destination databases, the workflow processes:

```text
200,000 rows per destination
x 3 destinations
= 600,000 affected rows
```

The resulting application code remains deliberately uncomplicated:

```text
EF Core
    owns DbContext and data access

DataArc.EntityFrameworkCore
    adds bulk and parallel persistence execution

.NET
    coordinates independent DbContexts
```

---

## Benchmark Project

The repository includes a BenchmarkDotNet project that exercises parallel bulk insertion across four independent SQL Server databases.

Test environment:

```text
.NET 10
BenchmarkDotNet 0.15.8
SQL Server
13th Gen Intel Core i7-13700H
```

The current benchmark workload inserts both:

```text
Employers
Employees
```

into each of four independent databases.

Conceptually:

```csharp
await Task.WhenAll(
    googleDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    microsoftDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    openAiDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    solidArcDbContext
        .AsParallel()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync());
```

The benchmark records both execution time and allocation behavior for this workload.

Published benchmark numbers should be treated as workload-specific measurements rather than universal performance guarantees.

Performance varies according to hardware, SQL Server configuration, network topology, schema design, indexes, entity shape, batch size, database state, and workload.

---

## Run The Demo

The demonstration projects target .NET 10.

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
dotnet run --project DataArc.EntityFrameworkCore.Demo.Host
```

> The demo recreates its configured databases. Use disposable development databases only.

---

## Advanced SQL Server Transactions

The solution also contains integration tests for the SQL Server-specific transactional execution capabilities provided by `DataArc.EntityFrameworkCore.SqlServer`.

These tests use the same demo persistence model rather than introducing a separate test domain.

Install the commercial SQL Server package where these capabilities are required:

```xml
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

### DataArc-Owned Transactions

The advanced transactional API stages normal and bulk persistence operations and commits them through one DataArc-owned SQL transaction.

```csharp
await dbContext
    .AsParallelTransaction()
    .Add(employer)
    .Add(employee)
    .CommitTransactionParallelAsync();
```

Bulk operations can participate in the same execution model:

```csharp
await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

For one source `DbContext`, transactional execution uses:

```text
1 execution DbContext
1 SQL connection
1 SQL transaction
N staged persistence operations
1 commit
```

If any operation fails, the transaction is rolled back and the failure is propagated.

---

## Transactional Integration Tests

The SQL Server integration-test project verifies two important behaviors.

### Real workflow execution

The transactional salary-adjustment workflow uses the same source data and destination databases as the normal demo flow:

```text
SolidArcDbContext
    |
    ├── employers
    └── employees
            |
            v
    DataArc transactional execution
            |
    ├── GoogleDbContext
    ├── MicrosoftDbContext
    └── OpenAIDbContext
```

Each destination executes its own DataArc-owned SQL transaction.

The three destination transactions are coordinated concurrently by the application with `Task.WhenAll`.

With 100,000 employers and 100,000 employees written to three destinations, the integration test verifies:

```text
600,000 affected rows
```

### Rollback behavior

The rollback test deliberately causes a foreign-key failure after earlier work has been staged:

```csharp
await dbContext
    .AsParallelTransaction()
    .Add(employer)
    .Add(invalidEmployee)
    .CommitTransactionParallelAsync();
```

After the failure, the test verifies that the employer was not persisted.

This confirms that earlier successful work was rolled back with the failed operation.

---

## Independent Transactional Execution

Independent `DbContext`s can execute their own DataArc-owned transactions concurrently:

```csharp
await Task.WhenAll(
    googleDbContext
        .AsParallelTransaction()
        .AddBulk(googleEmployers, batchSize)
        .CommitTransactionParallelAsync(),

    microsoftDbContext
        .AsParallelTransaction()
        .AddBulk(microsoftEmployers, batchSize)
        .CommitTransactionParallelAsync());
```

Each `DbContext` owns an independent SQL transaction.

`Task.WhenAll` coordinates those independent executions at the application level.

This is not a distributed transaction and does not represent one atomic transaction spanning multiple databases.

---

## License Configuration For SQL Server Integration Tests

The free package requires no DataArc runtime license:

```csharp
services.AddDataArcCore();
```

Commercial SQL Server integration tests configure DataArc with either an explicit license key or the server-key path:

```csharp
services
    .AddDataArcCore(options =>
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            options.UseServerKey();
        else
            options.UseKey(licenseKey);
    })
    .ConfigureDataArc();
```

This keeps the normal demo path free while allowing the integration-test project to exercise commercial SQL Server capabilities.

---

## Package Boundaries

### DataArc.EntityFrameworkCore

`DataArc.EntityFrameworkCore` contains the general EF Core execution capabilities demonstrated by the main demo.

These include:

- bulk insertion
- caller-owned transactional bulk execution
- fluent persistence composition
- parallel persistence execution

No DataArc runtime license is required to use `DataArc.EntityFrameworkCore`.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore" Version="2.0.0" />
```

### DataArc.EntityFrameworkCore.SqlServer

`DataArc.EntityFrameworkCore.SqlServer` adds SQL Server-specific commercial capabilities.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

This repository demonstrates its DataArc-owned SQL transaction execution through integration tests.

Other SQL Server-specific capabilities include multi-`DbContext` coordination and relational database-definition tooling.

The multi-context DDL Builder is demonstrated in the **DataArc Orchestration Framework** demo, where multiple modular `DbContext` models naturally participate in one physical relational database.

---

## Where DataArc Fits

`DataArc.EntityFrameworkCore` is useful when an EF Core application needs more efficient or more explicit persistence execution without replacing EF Core itself.

Typical workloads include:

- high-volume imports
- batch processing
- synchronisation jobs
- background workers
- multiple `DbContext` applications
- multiple database applications
- data distribution workflows
- modular monoliths
- application modernisation

It does not require applications to introduce repository abstractions, CQRS pipelines, execution-context interfaces, or DataArc-specific `DbContext` wrappers.

Use the architecture that fits the application.

Add DataArc where its execution capabilities are useful.

---

## Documentation

Full documentation is available at:

### [Read the DataArc.EntityFrameworkCore documentation →](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)

Documentation covers:

- installation and registration
- compatibility
- bulk operations
- transactional bulk operations
- parallel persistence execution
- fluent persistence composition
- SQL Server transactional execution
- advanced SQL Server capabilities

---

## Support

For product and licensing information:

https://www.dataarc.dev

For support:

**[support@dataarc.dev](mailto:support@dataarc.dev)**

---

## The Core Idea

```text
Use EF Core normally.

When persistence execution becomes the problem:

    add DataArc.
```

**EF Core remains the data-access model. DataArc adds execution capabilities where they matter.**
