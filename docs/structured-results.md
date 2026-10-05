# Execution Results

DataArc.EntityFrameworkCore 2.0 keeps execution results deliberately simple.

The main terminal persistence APIs return an affected-row count and propagate failures through exceptions.

## Affected rows

```csharp
var affected = await dbContext
    .AsParallel()
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

`affected` represents the records reported as affected by the completed execution.

For several independent contexts:

```csharp
var results = await Task.WhenAll(
    googleDbContext
        .AsParallel()
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync(),

    microsoftDbContext
        .AsParallel()
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync());

var totalAffected = results.Sum();
```

## Transactional result

The commercial SQL Server transaction path also returns an affected count:

```csharp
var affected = await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

## Failure behavior

Do not use an affected count as a substitute for error handling.

If execution fails, handle the exception at the application boundary that owns the workflow:

```csharp
try
{
    var affected = await dbContext
        .AsParallel()
        .AddBulk(employees, batchSize)
        .SaveChangesParallelAsync();

    return affected;
}
catch (Exception exception)
{
    logger.LogError(
        exception,
        "Employee import failed.");

    throw;
}
```

For a public API, translate internal failures into an application-safe response rather than returning raw internal exception detail.

## Zero affected rows

A successful operation can legitimately affect zero rows.

Examples include:

- an empty prepared collection;
- a filter that matches no rows;
- an idempotent operation that has nothing to change.

Treat zero as a result, not automatically as a failure.

## Transaction rollback

For DataArc-owned SQL Server transaction execution, a failed operation causes the local transaction to be rolled back.

The integration tests verify that earlier staged work is not persisted after a later operation fails.

## Application results remain application-owned

DataArc reports execution results.

The application decides what those results mean.

An API, worker, or orchestrator may convert the affected count or exception into its own response contract:

```text
DataArc execution result
        |
        v
application decision
        |
        +-- API response
        +-- workflow output
        +-- log entry
        +-- retry decision
```

DataArc execution results are not intended to replace application-domain result models.
