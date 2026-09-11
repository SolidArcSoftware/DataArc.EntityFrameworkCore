---
title: DbContext Integration
description: Keep standard Entity Framework Core DbContext and IDbContextFactory usage while adding DataArc bulk operations.
---

# DbContext Integration

DataArc.EntityFrameworkCore does not replace `DbContext`.

A context created or injected through normal Entity Framework Core mechanisms can continue to use tracked operations, LINQ queries, `SaveChangesAsync`, transactions, and DataArc bulk operations in the same application.

## Standard DbContext usage

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();

var existingEmployees = await dbContext
    .Employees
    .AsNoTracking()
    .ToListAsync();

await dbContext.AddBulkAsync(
    newEmployees,
    batchSize);
```

This allows DataArc to be introduced into individual high-volume workflows without changing the rest of the application's persistence architecture.

## Existing persistence patterns remain valid

Applications can continue to use:

- injected scoped `DbContext` instances;
- `IDbContextFactory<TContext>`;
- repositories;
- application services;
- standard Entity Framework Core transactions;
- ordinary EF Core querying and change tracking.

No DataArc-specific execution context is required for the free bulk API.

## Suitable workloads

Bulk operations are useful for workloads such as:

- imports;
- synchronization;
- batch processing;
- integration workloads;
- data migration paths;
- background processing;
- other high-volume persistence operations.
