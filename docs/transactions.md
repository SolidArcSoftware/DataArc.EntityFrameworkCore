# Transactions

DataArc 2.0 exposes two distinct transaction models.

Keeping them separate is important.

## 1. Caller-Owned EF Core Transaction

The free `DataArc.EntityFrameworkCore` package can participate in a transaction created and owned by EF Core.

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
EF Core owns transaction creation.
The caller owns commit and rollback.
DataArc bulk execution participates in the current transaction.
```

Use this when the application already owns the local EF Core transaction boundary.

## 2. DataArc-Owned SQL Server Transaction

`DataArc.EntityFrameworkCore.SqlServer` adds a commercial transaction path:

```csharp
var affected = await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

The consumer does not create an outer EF Core transaction for this path.

For one source `DbContext`, DataArc transaction execution uses:

```text
1 execution DbContext
1 SQL connection
1 SQL transaction
N staged persistence operations
1 commit
```

If an operation fails before commit, DataArc rolls the transaction back and propagates the execution failure.

## Normal tracked operations

The SQL Server transaction path can include ordinary EF Core entity operations:

```csharp
await dbContext
    .AsParallelTransaction()
    .Add(employer)
    .Add(employee)
    .CommitTransactionParallelAsync();
```

The integration tests verify rollback by deliberately causing a foreign-key failure after earlier work has been staged and then confirming that the earlier insert was not persisted.

## Bulk operations

Bulk operations can participate in the same DataArc-owned transaction:

```csharp
await dbContext
    .AsParallelTransaction()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .CommitTransactionParallelAsync();
```

Transactional bulk execution requires an active DataArc-owned SQL transaction. It does not silently fall back to non-transactional bulk execution.

## Several independent contexts

Independent contexts can execute their own transactions concurrently:

```csharp
var results = await Task.WhenAll(
    googleDbContext
        .AsParallelTransaction()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .CommitTransactionParallelAsync(),

    microsoftDbContext
        .AsParallelTransaction()
        .AddBulk(employers, batchSize)
        .AddBulk(employees, batchSize)
        .CommitTransactionParallelAsync());
```

Each context owns an independent SQL transaction.

`Task.WhenAll` coordinates those executions at application level.

It does not create:

- one distributed transaction;
- one transaction spanning several databases;
- one atomic commit across independent SQL Server databases.

## Transaction boundaries

A business workflow may be wider than one database transaction:

```text
Application workflow
    |
    +-- local SQL transaction
    +-- second database
    +-- external API
    +-- event publication
```

The application or orchestration layer still owns the wider consistency model.

Use compensation, idempotency, retries, or an outbox where the workflow crosses boundaries that cannot share one physical transaction.

## Keep transactions short

Do not hold database transactions open while waiting for:

- user interaction;
- long-running external API calls;
- unrelated network operations;
- background work that does not require the transaction.

Prepare the data first, enter the transaction for the required database work, and commit or roll back promptly.

## Licensing

The caller-owned EF transaction path is part of the free `DataArc.EntityFrameworkCore` package.

The DataArc-owned SQL Server transaction path is provided by `DataArc.EntityFrameworkCore.SqlServer` and requires the applicable commercial entitlement.

See [Licensing](licensing-and-trial.md).
