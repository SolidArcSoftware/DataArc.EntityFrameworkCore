# Persistence Execution

DataArc.EntityFrameworkCore 2.0 does not require command factories or command-builder abstractions for normal EF Core persistence.

Use the `DbContext` directly.

## Normal EF Core writes

```csharp
dbContext.Add(employee);

await dbContext.SaveChangesAsync();
```

Use normal EF Core whenever normal EF Core is sufficient.

## Direct bulk execution

For a prepared collection:

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

Bulk execution is immediate and is not the same thing as staging entities in EF Core's change tracker.

## Fluent parallel execution

Use `AsParallel()` when a set of persistence operations should be staged for DataArc execution:

```csharp
var affected = await dbContext
    .AsParallel()
    .Add(employee)
    .AddRange(otherEmployees)
    .AddBulk(importEmployees, batchSize)
    .SaveChangesParallelAsync();
```

Supported operations include:

- `Add`;
- `AddRange`;
- `Update`;
- `Remove`;
- `AddBulk`.

## Multiple bulk operations

A single plan can stage several bulk operations:

```csharp
var affected = await dbContext
    .AsParallel()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

The fluent plan should not be treated as an application workflow language.

Business ordering and policy decisions still belong in the application or orchestration layer.

## Several independent contexts

Use normal .NET coordination:

```csharp
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

`Task.WhenAll` coordinates independent executions. It does not create a distributed transaction.

## Advanced SQL Server transaction execution

The commercial SQL Server package adds a DataArc-owned transaction path:

```csharp
var affected = await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

See [Transactions](transactions.md).
