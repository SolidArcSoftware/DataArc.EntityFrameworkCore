---
title: Getting Started
description: Install and use DataArc.EntityFrameworkCore directly from a standard Entity Framework Core DbContext.
---

# Getting Started

DataArc.EntityFrameworkCore is designed to fit into an existing Entity Framework Core application without replacing the normal EF Core programming model.

## Install the package

```bash
dotnet add package DataArc.EntityFrameworkCore
```

No DataArc-specific dependency injection, execution context, runtime registration, license key, or activation is required for the free bulk APIs.

## Use your existing DbContext

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

The operation uses the database connection and entity metadata associated with the originating `DbContext`.

## Use IDbContextFactory<TContext>

Applications can continue to use the standard Entity Framework Core `IDbContextFactory<TContext>` pattern.

```csharp
public sealed class EmployeeImportService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public EmployeeImportService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task ImportAsync(
        IEnumerable<Employee> employees,
        int batchSize)
    {
        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync();

        await dbContext.AddBulkAsync(
            employees,
            batchSize);
    }
}
```

No DataArc-specific factory is required.

## Optional schema selection

Bulk APIs default to the `dbo` schema.

Supply a schema when the target table belongs to another schema.

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize,
    schema: "hr");
```

## Next

See [Bulk Operations](bulk-operations.md) for the complete supported bulk API surface.
