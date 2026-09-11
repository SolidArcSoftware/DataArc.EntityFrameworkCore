---
title: Bulk Operations
description: Use DataArc.EntityFrameworkCore bulk insert and bulk delete APIs directly from DbContext.
---

# Bulk Operations

DataArc.EntityFrameworkCore adds high-performance SQL Server bulk operations directly to the standard Entity Framework Core `DbContext`.

Existing applications can introduce bulk execution only where needed without redesigning repositories, replacing `DbContext`, or moving persistence into a separate DataArc execution model.

## AddBulkAsync

Insert a collection of entities into the database in batches.

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

The optional `schema` argument defaults to `dbo`.

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize,
    schema: "hr");
```

The originating `DbContext` is returned, allowing additional operations to be chained when appropriate.

## AddBulkTransactionalAsync

Bulk inserts can participate in an active Entity Framework Core transaction.

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.AddBulkTransactionalAsync(
    employees,
    batchSize);

await transaction.CommitAsync();
```

An existing `DbTransaction` can also be supplied explicitly.

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.AddBulkTransactionalAsync(
    employees,
    batchSize,
    transaction.GetDbTransaction());

await transaction.CommitAsync();
```

The transaction remains owned by the caller. DataArc does not commit or roll back the transaction.

The optional `schema` argument defaults to `dbo`.

## RemoveBulkAsync

Delete a collection of entities from the database in batches.

```csharp
await dbContext.RemoveBulkAsync(
    employees,
    batchSize);
```

The optional `schema` argument defaults to `dbo`.

```csharp
await dbContext.RemoveBulkAsync(
    employees,
    batchSize,
    schema: "hr");
```

The originating `DbContext` is returned.

## RemoveBulkTransactionalAsync

Bulk deletes can participate in the active Entity Framework Core transaction associated with the current `DbContext`.

```csharp
await using var transaction =
    await dbContext.Database.BeginTransactionAsync();

await dbContext.RemoveBulkTransactionalAsync(
    employees,
    batchSize);

await transaction.CommitAsync();
```

The `DbContext` must have an active transaction before the operation is executed.

The transaction remains owned by the caller. DataArc does not commit or roll back the transaction.

The optional `schema` argument defaults to `dbo`.

## SQL Server

The current bulk implementation targets SQL Server and uses the connection infrastructure already associated with the Entity Framework Core `DbContext`.
