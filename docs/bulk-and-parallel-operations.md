# Bulk and Parallel Operations

Bulk and parallel operations are intended for application workflows that need to process large entity collections or execute independent database operations across several registered execution contexts.

DataArc keeps these concerns explicit:

- `AddBulk(...)` describes a bulk operation for a selected execution context.
- `BuildAsync()` produces the executable command.
- `ExecuteAsync()` uses the command's normal execution path.
- `ExecuteParallelAsync()` allows independent command operations to execute concurrently.
- the returned command result reports the combined execution outcome.

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

## Why bulk operations exist

Ordinary Entity Framework Core change tracking is appropriate for many application writes.

Bulk operations become useful when a workflow needs to process large collections and the cost of handling each record individually becomes significant.

Typical examples include:

- importing employees from an external source;
- replicating prepared data to another database;
- applying a calculated update to a large entity set;
- seeding or refreshing reference data;
- distributing one prepared collection to several independent targets.

A bulk operation should still represent a meaningful application task. It should not be used automatically for every write.

```text
Small or ordinary write
    -> normal tracked EF Core operation may be sufficient

Large prepared collection
    -> bulk operation may reduce execution overhead
```

The correct choice depends on the number of records, provider capabilities, transaction requirements, generated values, relationships, and the amount of application logic involved.

## Creating a bulk command

Inject `ICommandFactory` into the service or workflow that performs the operation:

```csharp
private readonly ICommandFactory _commandFactory;

public EmployeeReplicationService(
    ICommandFactory commandFactory)
{
    _commandFactory = commandFactory;
}
```

Create the command builder:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();
```

Select the execution context and add the bulk operation:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

Build and execute the command:

```csharp
var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

The complete shape is:

```text
ICommandFactory
    -> CreateCommandBuilderAsync()
    -> UseDbExecutionContext<TExecutionContext>()
    -> AddBulk(entities, batchSize)
    -> BuildAsync()
    -> ExecuteAsync()
    -> inspect result
```

## The role of the batch size

`AddBulk(...)` accepts the entity collection and a batch size:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

The batch size controls how the supplied collection is divided for processing by the bulk operation.

Conceptually:

```text
100,000 entities
    |
    +-- batch 1
    +-- batch 2
    +-- batch 3
    +-- ...
```

A larger batch size may reduce the number of database round trips, but it can also increase:

- memory usage;
- transaction duration;
- command size;
- database locking pressure;
- the cost of retrying a failed batch.

A smaller batch size may reduce the impact of each individual batch, but it can increase total database round trips.

There is no universal batch size that is correct for every database and workload.

Choose a starting value, benchmark it with realistic data, and confirm that the database remains stable under the expected production load.

## Handling empty collections

Do not build a database command when the workflow has no work to perform.

```csharp
if (employees.Count == 0)
{
    return 0;
}
```

Then build the command only when records exist:

```csharp
if (employees.Count == 0)
{
    return 0;
}

var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

This keeps the application intent explicit and avoids unnecessary command construction.

## Single-context bulk execution

A bulk operation can target one registered execution context:

```csharp
public async Task<int> ImportEmployeesAsync(
    IReadOnlyList<Employee> employees,
    int batchSize)
{
    if (employees.Count == 0)
    {
        return 0;
    }

    var commandBuilder = await _commandFactory
        .CreateCommandBuilderAsync();

    commandBuilder
        .UseDbExecutionContext<IGoogleDbContext>()
        .AddBulk(employees, batchSize);

    var command = await commandBuilder.BuildAsync();

    var commandResult = await command.ExecuteAsync();

    if (!commandResult.Success)
    {
        throw new InvalidOperationException(
            "The employee import failed.",
            commandResult.Exception);
    }

    return commandResult.TotalAffected;
}
```

The selected execution context remains visible:

```text
Employee import
    -> IGoogleDbContext
    -> AddBulk(...)
    -> execute
```

## Multi-context bulk commands

One command builder can contain bulk operations for several execution contexts.

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IOpenAiDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();
```

Each operation remains associated with its own persistence boundary.

```text
Built command
    |
    +-- IGoogleDbContext
    |       -> AddBulk(employees, batchSize)
    |
    +-- IMicrosoftDbContext
    |       -> AddBulk(employees, batchSize)
    |
    +-- IOpenAiDbContext
            -> AddBulk(employees, batchSize)
```

This does not combine the contexts into one EF Core model.

It creates one application command containing several explicitly routed operations.

## Executing through the normal command path

Use:

```csharp
var commandResult = await command.ExecuteAsync();
```

when the command should use its normal execution path.

A complete multi-context example is:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IOpenAiDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

Use this path when concurrent execution is not required or when the workflow should not treat the operations as independent.

## Parallel execution

Use:

```csharp
var commandResult = await command
    .ExecuteParallelAsync();
```

when the built command contains independent operations that are intentionally allowed to run concurrently.

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IOpenAiDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command
    .ExecuteParallelAsync();
```

Conceptually:

```text
Built command
    |
    +-- Google bulk operation --------+
    |                                 |
    +-- Microsoft bulk operation -----+--> concurrent execution
    |                                 |
    +-- OpenAI bulk operation --------+
                                      |
                                      v
                           structured command result
```

## When parallel execution is appropriate

Parallel execution is appropriate when:

- each operation targets an independent execution context;
- no operation depends on the result of another;
- the same prepared data can be processed independently;
- the database servers and connection pools can support the added concurrency;
- the workflow can correctly handle combined success or failure information.

A replication workflow is a typical example:

```text
Prepared employee collection
    |
    +-- write to Google database
    +-- write to Microsoft database
    +-- write to OpenAI database
```

If each target is independent, concurrent execution may reduce the total elapsed time.

## When parallel execution is not appropriate

Do not use parallel execution when:

- one operation requires an identifier produced by another;
- one operation must observe committed data from another;
- business ordering is significant;
- the contexts share a resource that cannot be used concurrently;
- the workflow requires one atomic transaction;
- database load is already constrained;
- partial completion would be unacceptable.

For example:

```text
Create employee
    -> use generated EmployeeId
    -> create payroll profile
```

The second step depends on the first.

That is an ordered workflow, not an independent parallel operation.

## Parallel does not mean transactional

Parallel execution and transactional execution solve different problems.

```text
Parallel execution
    -> reduce elapsed time for independent operations

Transaction
    -> preserve consistency within a defined transaction boundary
```

A command containing operations for separate databases does not automatically become one distributed transaction.

```text
One DataArc command
    !=
one atomic transaction across every database
```

If one target succeeds and another fails, the application must understand what that means for the workflow.

Possible responses include:

- return a failed structured result;
- log the failed target;
- retry the failed operation;
- initiate application-specific compensation;
- mark the operation for manual review.

See [Transactional Workflows](transactions.md) for the transaction-boundary discussion.

## Reading once and writing to several targets

A common pattern is:

1. read source records through a query pipeline;
2. apply application logic;
3. add bulk operations for several target contexts;
4. execute the independent target operations in parallel.

```csharp
var employeesQuery = await _queryFactory
    .CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee =>
            employee.Salary > salaryThreshold);
```

Apply the required transformation:

```csharp
foreach (var employee in employees)
{
    employee.Salary +=
        employee.Salary * salaryAdjustmentBaseRate;
}
```

Build the target operations:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<IOpenAiDbContext>()
    .AddBulk(employees, batchSize);
```

Execute them:

```csharp
var command = await commandBuilder.BuildAsync();

var commandResult = await command
    .ExecuteParallelAsync();
```

Inspect the result:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "The parallel bulk operation failed.",
        commandResult.Exception);
}

return commandResult.TotalAffected;
```

## Complete parallel bulk example

```csharp
public async Task<int> ReplicateEmployeeAdjustmentsAsync(
    decimal salaryAdjustmentBaseRate,
    decimal salaryThreshold,
    int batchSize)
{
    var employeesQuery = await _queryFactory
        .CreateQueryAsync();

    var employees = await employeesQuery
        .UseDbExecutionContext<ISolidArcDbContext>()
        .ReadWhereAsync<Employee>(
            employee =>
                employee.Salary > salaryThreshold);

    if (employees.Count == 0)
    {
        return 0;
    }

    foreach (var employee in employees)
    {
        employee.Salary +=
            employee.Salary *
            salaryAdjustmentBaseRate;
    }

    var commandBuilder = await _commandFactory
        .CreateCommandBuilderAsync();

    commandBuilder
        .UseDbExecutionContext<IGoogleDbContext>()
        .AddBulk(employees, batchSize);

    commandBuilder
        .UseDbExecutionContext<IMicrosoftDbContext>()
        .AddBulk(employees, batchSize);

    commandBuilder
        .UseDbExecutionContext<IOpenAiDbContext>()
        .AddBulk(employees, batchSize);

    var command = await commandBuilder.BuildAsync();

    var commandResult = await command
        .ExecuteParallelAsync();

    if (!commandResult.Success)
    {
        throw new InvalidOperationException(
            "Employee replication failed.",
            commandResult.Exception);
    }

    return commandResult.TotalAffected;
}
```

## Interpreting the combined result

The current command workflow uses:

```csharp
commandResult.Success
commandResult.Exception
commandResult.TotalAffected
```

Check the result before returning the affected count:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "Bulk execution failed.",
        commandResult.Exception);
}

return commandResult.TotalAffected;
```

Do not assume that a non-zero affected count means that every operation completed successfully.

Use `Success` as the primary outcome indicator and inspect `Exception` when the operation fails.

Structured result behavior is covered in [Structured Execution Results](structured-results.md).

## DbContext concurrency

An EF Core `DbContext` instance is not intended to execute several database operations concurrently.

Parallel DataArc operations should therefore remain isolated behind their independently registered execution contexts.

Do not:

- share one manually created `DbContext` instance across concurrent tasks;
- start several operations on the same active context;
- reuse tracked entities in ways that create conflicting state;
- assume that every database provider has identical concurrency behavior.

The parallelism should exist between independent execution operations, not inside one active `DbContext` instance.

## Connection-pool and database pressure

Parallel execution can increase pressure on:

- database connection pools;
- database CPU;
- transaction logs;
- network bandwidth;
- locks and latches;
- application memory.

A workflow that is faster during a small local test may become less stable under production load.

Benchmark with:

- realistic record counts;
- realistic entity sizes;
- the production database provider;
- expected concurrent users or workers;
- representative network latency;
- the actual batch size.

Measure both elapsed time and operational impact.

## Avoid accidental nested parallelism

Be careful when the application already processes several jobs concurrently.

For example:

```text
10 background jobs
    x
3 parallel execution contexts
    =
up to 30 concurrent database operations
```

Adding parallelism at several layers can overwhelm the database even when each individual command appears reasonable.

Choose one deliberate concurrency boundary and control the total number of concurrent workflows at the application level.

## Failure and retry considerations

A retry should be based on the semantics of the operation.

Before retrying, determine whether the operation is:

- idempotent;
- safe to repeat;
- able to identify already processed records;
- protected by unique constraints;
- using generated values that may change;
- capable of creating duplicates.

A parallel command may partially complete before a failure is returned.

Retrying the entire workflow blindly can repeat operations that already succeeded.

The application may need to:

- retry only failed targets;
- record a workflow identifier;
- use idempotency keys;
- query the target state before retrying;
- run compensation logic.

These are application-level decisions and should not be hidden behind a generic retry loop.

## Bulk operations and relationships

Bulk operations are best suited to collections whose required key values and relationships are already prepared.

Before execution, confirm that:

- required foreign keys are present;
- entity order does not hide a dependency;
- generated keys are not required by another parallel operation;
- navigation properties are not being relied on implicitly;
- the target model matches the supplied entity data.

When dependent records require generated identifiers, use an ordered workflow.

## Bulk operations and change tracking

Bulk operations should not be treated as an automatic replacement for normal tracked EF Core behavior.

A bulk operation may not provide the same workflow semantics as:

- loading tracked entities;
- changing individual properties;
- relying on change detection;
- processing entity relationships;
- using application hooks around each entity.

Use bulk processing when the collection is prepared and the application understands the operation as a set.

Use normal tracked operations when entity-by-entity state and domain behavior are significant.

## Recommended decision guide

Use an ordinary command when:

- the record count is small;
- change tracking is useful;
- relationships and generated values matter;
- the workflow contains substantial entity-level behavior.

Use `AddBulk(...)` when:

- a large collection is already prepared;
- the target context is explicit;
- set-oriented processing is appropriate;
- the provider and database have been tested with the workload.

Use `ExecuteParallelAsync()` when:

- several operations are independent;
- each operation has its own execution context;
- order does not matter;
- partial failure can be handled correctly;
- the database environment can support the concurrency.

Use an ordered or transactional workflow when:

- operations depend on one another;
- consistency is more important than elapsed time;
- one operation must observe another operation's committed result.

## Summary

Bulk and parallel execution follow this shape:

```text
ICommandFactory
    -> CreateCommandBuilderAsync()
    -> UseDbExecutionContext<TExecutionContext>()
    -> AddBulk(entities, batchSize)
    -> BuildAsync()
    -> ExecuteAsync()
       or ExecuteParallelAsync()
    -> inspect structured result
```

The main principles are:

- use bulk processing for large, prepared collections;
- select each execution context explicitly;
- benchmark the batch size with realistic data;
- use parallel execution only for independent operations;
- do not treat parallel execution as a distributed transaction;
- account for partial failure, retries, connection pressure, and idempotency;
- keep workflow consistency more important than raw throughput.

## Next steps

Continue with:

- [Command Pipelines](command-pipelines.md)
- [Transactional Workflows](transactions.md)
- [Structured Execution Results](structured-results.md)
