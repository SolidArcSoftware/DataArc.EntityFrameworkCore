# Bulk And Parallel Operations

Bulk and parallel execution are the primary free execution capabilities in `DataArc.EntityFrameworkCore`.

## Direct bulk insertion

Use `AddBulkAsync` for an immediate bulk insert:

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

The `DbContext` remains a normal EF Core context.

## Batch size

The batch size controls how many rows are sent through each bulk batch:

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize: 100_000);
```

The best value depends on:

- entity shape;
- row size;
- SQL Server configuration;
- network latency;
- available memory;
- indexes and constraints;
- workload concurrency.

Benchmark the real workload rather than assuming one universal batch size.

## Parallel persistence plans

Use `AsParallel()` to stage DataArc persistence work:

```csharp
var affected = await dbContext
    .AsParallel()
    .AddBulk(employers, 100_000)
    .AddBulk(employees, 100_000)
    .SaveChangesParallelAsync();
```

The terminal operation returns the total affected count.

## Mixed operations

Normal entity operations can be combined with bulk work:

```csharp
var affected = await dbContext
    .AsParallel()
    .Add(employee)
    .AddRange(otherEmployees)
    .Update(manager)
    .AddBulk(importRows, batchSize)
    .SaveChangesParallelAsync();
```

## DbContext thread safety

EF Core `DbContext` is not thread-safe.

Do not start unrelated EF Core operations concurrently against the same context instance.

DataArc's parallel execution machinery coordinates work without requiring the consumer to run concurrent EF operations against one shared EF context.

## Parallel work across independent DbContexts

Independent databases are naturally coordinated at application level:

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
```

This is application concurrency across independent persistence boundaries.

## Parallel does not mean transactional

The free `AsParallel()` path does not imply one atomic transaction across several databases.

Each destination remains independent.

For a local caller-owned EF transaction, use the free transactional bulk API.

For DataArc-owned SQL Server transactions, use `DataArc.EntityFrameworkCore.SqlServer`.

See [Transactions](transactions.md).

## Relationships and ordering

Bulk operations should be planned with database relationships in mind.

For related data such as:

```text
Employer
    |
    +-- Employee
```

the database's foreign-key requirements still matter.

A DataArc fluent plan describes execution work; application-level business sequencing should remain explicit rather than relying on the fluent chain as a general workflow engine.

## Change tracking

Bulk operations are intended for high-volume persistence and do not behave like ordinary EF Core tracked `Add` operations.

If the application requires EF Core-generated state or tracked entity behavior, use the normal EF Core path where appropriate.

## Failure handling

Treat a failed terminal operation as a failed execution.

Do not infer success from a partial affected count.

For retryable workloads:

- use deterministic input;
- understand whether the operation is idempotent;
- avoid blindly retrying side effects;
- account for database constraints and duplicate keys.

## Demo workload

The current demo distributes:

```text
100,000 Employers
100,000 Employees
```

to three independent destination databases.

That produces:

```text
200,000 rows per destination
x 3 destinations
= 600,000 affected rows
```

The benchmark project expands the same two-table bulk shape across four independent SQL Server databases.
