# DataArc.EntityFrameworkCore

DataArc.EntityFrameworkCore provides an explicit execution layer for Entity Framework Core applications.

It is designed for workflows that need to coordinate queries, commands, bulk operations, transactions, or database tooling across one or more EF Core `DbContext` boundaries.

## DataArc portal

For product information, licensing, pricing, and trial access, visit the [DataArc portal](https://www.dataarc.dev).

EF Core remains responsible for:

- entity mapping
- change tracking
- LINQ translation
- database-provider integration
- database communication

DataArc controls how application workflows execute that work.

## What DataArc provides

DataArc.EntityFrameworkCore supports:

- explicit database execution boundaries
- command and query factories
- command and query pipelines
- contract-based or direct `DbContext` execution
- bulk operations
- parallel execution across multiple contexts
- transactional command workflows
- structured execution results
- database creation and deletion
- SQL script generation
- multi-`DbContext` and multi-database workflows

## Core execution model

```text
Application workflow
    ↓
Command or query factory
    ↓
Explicit execution-context selection
    ↓
EF Core DbContext
    ↓
Database
```

A workflow can select a contract-based execution boundary explicitly:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Rating > 4.5);
```

DataArc can also target a concrete `DbContext` directly:

```csharp
var employees = await query
    .UseDbExecutionContext<GoogleDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Rating > 4.5);
```

Execution-context contracts are optional architectural boundaries. They are not required when targeting the concrete `DbContext` directly is appropriate.

## Example scenarios

DataArc.EntityFrameworkCore can support:

- background workers
- scheduled jobs
- high-volume data imports
- batch processing
- salary or pricing adjustments
- multi-database workflows
- company or tenant database onboarding
- bulk data distribution
- controlled transactional execution
- database creation and reset tooling

## Documentation

Continue with:

- [Getting Started](getting-started.md)
- [Compatibility](compatibility.md)
- [Execution Contexts](execution-contexts.md)
- [Query Pipelines](query-pipelines.md)
- [Command Pipelines](command-pipelines.md)
- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Transactional Workflows](transactions.md)
- [Structured Execution Results](structured-results.md)
- [Database and Script Generation](database-generation.md)
- [Trial and Licensing](licensing-and-trial.md)

## Public resources

- [GitHub repository](https://github.com/pierriemostert/DataArc.EntityFrameworkCore)
- [NuGet package](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
- [DataArc website](https://www.dataarc.dev)

---

**EF Core owns data access.**

**DataArc controls execution.**