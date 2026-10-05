# DataArc.EntityFrameworkCore

> **High-performance EF Core execution without replacing EF Core.**

[![NuGet](https://img.shields.io/badge/NuGet-DataArc.EntityFrameworkCore-004880)](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
[![Website](https://img.shields.io/badge/website-dataarc.dev-222222)](https://www.dataarc.dev)
[![.NET](https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

`DataArc.EntityFrameworkCore` extends ordinary Entity Framework Core with bulk and parallel persistence execution.

It does not replace `DbContext`, `IDbContextFactory<TContext>`, LINQ, EF Core change tracking, or normal application architecture.

```text
EF Core
    owns DbContext and data access

DataArc.EntityFrameworkCore
    adds bulk and parallel execution

.NET
    coordinates independent DbContexts
```

## Package model

DataArc 2.0 separates the free EF Core execution package from the commercial SQL Server package.

### DataArc.EntityFrameworkCore

The free package contains the general EF Core execution capabilities:

- direct bulk insertion;
- caller-owned transactional bulk execution;
- fluent `AsParallel()` persistence execution;
- `Add`, `AddRange`, `Update`, `Remove`, and `AddBulk`;
- `SaveChangesParallelAsync()`.

No DataArc runtime license is required for this package.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore" Version="2.0.0" />
```

### DataArc.EntityFrameworkCore.SqlServer

The SQL Server package adds commercial SQL Server-specific capabilities, including:

- DataArc-owned transaction execution;
- `AsParallelTransaction()`;
- `CommitTransactionParallelAsync()`;
- multi-`DbContext` coordination;
- relational database-definition tooling;
- multi-context DDL generation.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

The SQL Server package carries `DataArc.EntityFrameworkCore` as a dependency.

## Core execution model

Normal reads remain normal EF Core:

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();

var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Rating > 4.5)
    .ToListAsync();
```

Use DataArc where execution benefits from bulk or parallel work:

```csharp
await dbContext
    .AsParallel()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

For commercial SQL Server transaction execution:

```csharp
await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

## Demonstration repositories

The `DataArc.EntityFrameworkCore` demo uses four independent SQL Server databases and demonstrates normal EF Core reads with DataArc bulk and parallel writes.

The DataArc Orchestration Framework demo demonstrates the multi-context DDL Builder with HR, Finance, IT, and Operations `DbContext` models composed into one physical relational database.

## Documentation

- [Getting Started](getting-started.md)
- [Compatibility](compatibility.md)
- [DbContext Boundaries](execution-contexts.md)
- [Queries with EF Core](query-pipelines.md)
- [Persistence Execution](command-pipelines.md)
- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Transactions](transactions.md)
- [Execution Results](structured-results.md)
- [Multi-Context DDL Builder](database-generation.md)
- [Licensing](licensing-and-trial.md)

## Public resources

- [DataArc website](https://www.dataarc.dev)
- [DataArc.EntityFrameworkCore on NuGet](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
