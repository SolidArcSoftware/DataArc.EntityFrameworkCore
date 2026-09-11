---
title: Transactions
description: Use DataArc.EntityFrameworkCore bulk operations inside standard Entity Framework Core transactions.
---

# Transactions

DataArc.EntityFrameworkCore bulk operations are designed to participate in normal Entity Framework Core transaction handling.

This allows standard EF Core operations and DataArc bulk operations to execute within the same transaction when required.

## Active Entity Framework Core transaction

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.AddAsync(employee);

await dbContext.SaveChangesAsync();

await dbContext.AddBulkTransactionalAsync(
    additionalEmployees,
    batchSize);

await transaction.CommitAsync();
```

`AddBulkTransactionalAsync` can resolve the active transaction from `DbContext.Database.CurrentTransaction`.

`RemoveBulkTransactionalAsync` also requires an active transaction on the current `DbContext`.

## Explicit DbTransaction for bulk inserts

`AddBulkTransactionalAsync` also provides an overload that accepts an existing `DbTransaction`.

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.AddBulkTransactionalAsync(
    employees,
    batchSize,
    transaction.GetDbTransaction());

await transaction.CommitAsync();
```

## Transaction ownership

DataArc does not take ownership of a caller-supplied or active Entity Framework Core transaction.

The application remains responsible for:

- committing the transaction;
- rolling the transaction back when required; and
- disposing the transaction.

This keeps transaction control within the existing application persistence model.
