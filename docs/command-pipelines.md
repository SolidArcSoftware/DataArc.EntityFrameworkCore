# Command Pipelines

Command pipelines provide a structured way to build and execute database write operations through registered DataArc execution contexts.

A command pipeline follows four steps:

1. Create a command builder through `ICommandFactory`.
2. Select an execution context with `UseDbExecutionContext<TExecutionContext>()`.
3. Add one or more database operations to the builder.
4. Build and execute the resulting command.

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

The execution context identifies where the operation should run.

The operation added after the execution-context selection identifies what work should be performed.

```csharp
.UseDbExecutionContext<IGoogleDbContext>()
.AddBulk(employees, batchSize);
```

## Why command pipelines exist

Entity Framework Core allows application code to perform write operations directly through a `DbContext`.

For a simple application with one database boundary, direct `DbContext` usage may be sufficient.

More complex applications may need to:

- target several registered `DbContext` instances;
- keep concrete persistence implementations outside the application layer;
- route writes through public execution-context contracts;
- build several related database operations before execution;
- execute operations sequentially or in parallel;
- return one structured result to the calling workflow;
- keep database-routing decisions visible in application code.

DataArc provides a command-building model for these scenarios.

```text
Application workflow
    -> create command builder
    -> select execution context
    -> add database operation
    -> build command
    -> execute command
    -> inspect structured result
```

The pipeline makes the execution boundary explicit rather than allowing the selected `DbContext` to remain an incidental implementation detail.

## Injecting the command factory

Inject `ICommandFactory` into the application service or workflow that needs to build database commands:

```csharp
private readonly ICommandFactory _commandFactory;

public SalaryAdjustmentService(
    ICommandFactory commandFactory)
{
    _commandFactory = commandFactory;
}
```

The application service depends on the DataArc command contract rather than directly resolving a concrete `DbContext`.

A service can combine `ICommandFactory` with `IQueryFactory` when a workflow needs to read data, apply application logic, and then persist the resulting changes:

```csharp
private readonly IQueryFactory _queryFactory;
private readonly ICommandFactory _commandFactory;

public SalaryAdjustmentService(
    IQueryFactory queryFactory,
    ICommandFactory commandFactory)
{
    _queryFactory = queryFactory;
    _commandFactory = commandFactory;
}
```

This keeps query and command responsibilities distinct while allowing them to participate in the same application workflow.

## Creating a command builder

Create an asynchronous command builder with:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();
```

The builder collects the operations that will form the final command.

At this point, no command has been executed.

```text
CreateCommandBuilderAsync()
    -> command builder created
    -> execution contexts selected
    -> operations added
    -> command built
    -> command executed
```

This separation allows the application to describe the required work before deciding how the completed command should execute.

## Selecting an execution context

Every database operation is associated with an explicitly selected execution context:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

The execution-context type answers:

> Which registered database boundary should perform this operation?

A public execution-context contract can be selected:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

A concrete `DbContext` that implements `IExecutionContext` directly can also be selected:

```csharp
commandBuilder
    .UseDbExecutionContext<ApplicationDbContext>()
    .AddBulk(employees, batchSize);
```

The command pipeline remains the same in both cases.

The difference lies in how the execution context was registered.

For a public contract and hidden concrete implementation:

```csharp
ef.AddDbExecutionContext<
    IGoogleDbContext,
    GoogleDbContext>(
        options =>
            options.UseSqlServer(connectionString));
```

For a directly exposed concrete context:

```csharp
ef.AddDbExecutionContext<ApplicationDbContext>(
    options =>
        options.UseSqlServer(connectionString));
```

For additional registration guidance, see [Execution Contexts](execution-contexts.md).

## Adding an operation

After selecting an execution context, add the required database operation.

The current demo uses `AddBulk(...)` to add high-volume entity operations:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

The supplied values identify:

- the entities to process;
- the batch size used by the operation;
- the execution context that should perform the work.

```text
IGoogleDbContext
    -> execution boundary

employees
    -> entities supplied to the operation

batchSize
    -> requested operation batch size
```

Bulk behavior and multi-target bulk workflows are covered in more detail in [Bulk and Parallel Operations](bulk-and-parallel-operations.md).

## Building a command

After all required operations have been added, build the executable command:

```csharp
var command = await commandBuilder.BuildAsync();
```

Building and executing are separate stages.

```text
Command builder
    -> describes the work

BuildAsync()
    -> produces the executable command

ExecuteAsync()
    -> runs the built command
```

The separation is intentional.

It allows the application to finish defining the command before selecting its execution mode.

## Executing a command sequentially

Execute the built command with:

```csharp
var commandResult = await command.ExecuteAsync();
```

A complete single-context pipeline is:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

`ExecuteAsync()` is the standard execution path when the operations should be processed through the command’s normal ordered execution flow.

The application should inspect the returned result rather than assuming that the command succeeded.

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        $"Command execution failed. " +
        $"{commandResult.Exception?.Message}");
}
```

## Targeting several execution contexts

One command builder can contain operations for several registered execution contexts.

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

Each call selects its own execution boundary.

```text
Command builder
    |
    +-- IGoogleDbContext
    |       -> AddBulk(...)
    |
    +-- IMicrosoftDbContext
    |       -> AddBulk(...)
    |
    +-- IOpenAiDbContext
            -> AddBulk(...)
```

The repeated context selection is intentional.

It keeps each target visible in the application workflow and prevents the database destination from being hidden inside generic application plumbing.

The command can then be built normally:

```csharp
var command = await commandBuilder.BuildAsync();
```

## Sequential multi-context execution

A command containing several context operations can use the standard execution path:

```csharp
var commandResult = await command.ExecuteAsync();
```

The complete shape is:

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

Use this approach when the workflow requires the command’s normal ordered execution behavior rather than concurrent processing of independent targets.

## Parallel multi-context execution

When the command contains independent operations targeting separate execution contexts, it can be executed through:

```csharp
var commandResult = await command
    .ExecuteParallelAsync();
```

For example:

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

The intended shape is:

```text
Built command
    |
    +-- Google operation --------+
    |                            |
    +-- Microsoft operation -----+--> parallel execution
    |                            |
    +-- OpenAI operation --------+
                                 |
                                 v
                      structured command result
```

Parallel execution is appropriate only when the operations are independent.

Do not use parallel execution merely because several operations exist.

If one operation depends on the successful output or committed state of another operation, the workflow requires ordered or transactional coordination instead.

For more detail, see:

- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Transactional Workflows](transactions.md)

## Sequential versus parallel execution

The execution method expresses how the built command should process its operations.

### Sequential execution

```csharp
var commandResult = await command.ExecuteAsync();
```

Use sequential execution when:

- operations must follow their defined order;
- a later operation conceptually depends on earlier work;
- concurrent execution would make the workflow harder to reason about;
- the application does not require parallel processing.

### Parallel execution

```csharp
var commandResult = await command.ExecuteParallelAsync();
```

Use parallel execution when:

- operations target independent execution contexts;
- the targets do not depend on each other;
- the same prepared data can be written independently;
- concurrency is appropriate for the database and provider configuration;
- the calling workflow can correctly handle a combined result.

The execution mode should be selected from the workflow’s consistency requirements, not purely from expected performance.

## Reading before writing

A common application workflow reads data through a query pipeline and then writes the transformed result through a command pipeline.

The following example reads employees from a source execution context:

```csharp
var employeesQuery = await _queryFactory
    .CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee =>
            employee.Salary > salaryThreshold);
```

The application applies its business operation to the materialized results:

```csharp
foreach (var employee in employees)
{
    employee.Salary +=
        employee.Salary * salaryAdjustmentBaseRate;
}
```

A command builder is then created:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();
```

The transformed employees are added to one or more target execution contexts:

```csharp
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

The command is built and executed:

```csharp
var command = await commandBuilder.BuildAsync();

var commandResult = await command
    .ExecuteParallelAsync();
```

The responsibilities remain visible:

```text
Query pipeline
    -> select source execution context
    -> read and materialize entities

Application workflow
    -> apply business rules or transformations

Command pipeline
    -> select target execution contexts
    -> add database operations
    -> build command
    -> select execution mode
    -> return structured result
```

## Complete salary-adjustment example

The following example demonstrates the complete pattern used by the demo.

```csharp
public async Task<int> ProcessEmployeeSalaryAdjustmentsAsync(
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
            "Failed to process salary adjustments. " +
            commandResult.Exception?.Message);
    }

    return commandResult.TotalAffected;
}
```

This workflow:

1. creates an asynchronous query;
2. reads employees from one source execution context;
3. applies the salary adjustment in application code;
4. creates a command builder;
5. adds operations for three independent target contexts;
6. builds the command;
7. executes the target operations in parallel;
8. checks the structured result;
9. returns the total affected count.

## Structured command results

Command execution returns a structured result.

The current workflow uses the following members:

```csharp
commandResult.Success
commandResult.Exception
commandResult.TotalAffected
```

### Checking success

```csharp
if (!commandResult.Success)
{
    // Handle failed command execution.
}
```

### Inspecting the exception

```csharp
if (commandResult.Exception is not null)
{
    Console.WriteLine(
        commandResult.Exception.Message);
}
```

### Reading the affected count

```csharp
var affectedRecords =
    commandResult.TotalAffected;
```

A typical result-handling block is:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        $"Command execution failed. " +
        $"{commandResult.Exception?.Message}");
}

return commandResult.TotalAffected;
```

The calling application remains responsible for deciding what a failed result means to the business workflow.

For example, it may:

- throw an application exception;
- return an error response;
- log the failure;
- retry through a higher-level process;
- stop a background workflow;
- compensate through application-specific logic.

Command-result behavior is covered in more detail in [Structured Execution Results](structured-results.md).

## Commands and public execution-context contracts

Command pipelines can target a public execution-context contract while keeping the concrete `DbContext` internal to the persistence layer.

For example:

```csharp
public interface IGoogleDbContext : IExecutionContext
{
    DbSet<Employer>? Employer { get; set; }

    DbSet<Employee>? Employee { get; set; }
}
```

The concrete context remains inside persistence:

```csharp
internal sealed class GoogleDbContext :
    DbContext,
    IGoogleDbContext
{
    public GoogleDbContext(
        DbContextOptions<GoogleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employer>? Employer { get; set; }

    public DbSet<Employee>? Employee { get; set; }
}
```

Application code targets the public contract:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

The application does not need to reference `GoogleDbContext` directly.

DataArc resolves the registered implementation associated with the selected execution-context contract.

This provides a clear distinction:

```text
Application layer
    -> IGoogleDbContext

Persistence layer
    -> GoogleDbContext

DataArc registration
    -> maps the contract to the implementation
```

## Commands across separate databases

A multi-context command does not imply that all selected contexts share one database.

Each execution context can represent:

- a separate database;
- a separate schema;
- a separate application module;
- a bounded persistence context;
- a different EF Core model;
- a different connection string.

For example:

```text
IGoogleDbContext
    -> SAS_GoogleDb

IMicrosoftDbContext
    -> SAS_MicrosoftDb

IOpenAiDbContext
    -> SAS_OpenAiDb

ISolidArcDbContext
    -> SAS_Db
```

The application can build one command containing explicit operations for these boundaries.

That does not turn the databases into one joined EF Core model.

Each operation continues to run through its own registered execution context.

## Command pipelines are not cross-database joins

A command pipeline coordinates operations.

It does not create a cross-database entity relationship or a shared `DbContext`.

```text
One command pipeline
    |
    +-- operation routed to database A
    +-- operation routed to database B
    +-- operation routed to database C
```

The execution contexts remain separate.

The command pipeline provides an application-level execution model above those contexts.

Any required data transformation, consolidation, or mapping should be made explicit in the application workflow before the command operations are added.

## Command pipelines and repositories

DataArc command pipelines do not require a repository for every table.

An application may use a command pipeline directly inside a focused application service:

```csharp
public async Task<int> ReplicateEmployeesAsync(
    IReadOnlyList<Employee> employees,
    int batchSize)
{
    var commandBuilder = await _commandFactory
        .CreateCommandBuilderAsync();

    commandBuilder
        .UseDbExecutionContext<IGoogleDbContext>()
        .AddBulk(employees, batchSize);

    var command = await commandBuilder.BuildAsync();

    var result = await command.ExecuteAsync();

    if (!result.Success)
    {
        throw new InvalidOperationException(
            result.Exception?.Message);
    }

    return result.TotalAffected;
}
```

A repository may also encapsulate a meaningful persistence operation when that improves the application design.

The choice should follow the application’s boundaries rather than a requirement to wrap every entity in generic repository infrastructure.

DataArc handles execution routing.

The application remains responsible for naming and organising meaningful business operations.

## Keeping command workflows explicit

Prefer a command pipeline in which each execution boundary can be identified from the application code:

```csharp
commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(googleEmployees, batchSize);

commandBuilder
    .UseDbExecutionContext<IMicrosoftDbContext>()
    .AddBulk(microsoftEmployees, batchSize);
```

Avoid hiding all context selection behind an unrelated generic helper unless that abstraction expresses a genuine application concept.

For example, this may make the execution boundary difficult to see:

```csharp
await _genericPersistenceProcessor
    .ProcessEverythingAsync(items);
```

The DataArc model is strongest when the workflow clearly communicates:

- which contexts participate;
- which operations are added;
- when the command is built;
- how the command is executed;
- how the result is handled.

## Empty input collections

Application workflows should determine how empty input collections should be handled.

For example:

```csharp
if (employees.Count == 0)
{
    return 0;
}
```

This can prevent the application from building a command when no database work is required.

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

Whether an empty operation is meaningful depends on the application workflow and the selected operation.

Making the decision before command construction keeps the workflow’s intention clear.

## Failure handling

A failed command result should be handled at the appropriate application boundary.

```csharp
var commandResult = await command.ExecuteAsync();

if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "The database command failed.",
        commandResult.Exception);
}
```

For a background worker:

```csharp
if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Employee replication failed.");

    return;
}
```

For an application service returning its own result model:

```csharp
if (!commandResult.Success)
{
    return ApplicationResult.Failure(
        commandResult.Exception?.Message ??
        "Command execution failed.");
}
```

DataArc provides the execution outcome.

The application decides how that outcome should be represented to:

- an API caller;
- a user interface;
- a background worker;
- a scheduled process;
- an event consumer;
- another application service.

## Transaction boundaries

Building several operations into one command does not by itself prove that all participating execution contexts share one atomic database transaction.

This distinction is especially important when the command targets separate databases.

```text
One application command
    !=
one distributed database transaction
```

Transaction behavior depends on:

- the selected execution contexts;
- the underlying database connections;
- the execution mode;
- the configured transaction workflow;
- the capabilities of the EF Core provider and database platform.

Do not assume that parallel operations across separate databases are atomically committed as one unit.

For transaction-specific guidance, see [Transactional Workflows](transactions.md).

## Choosing an execution mode

Select the execution mode after considering:

- whether operations depend on one another;
- whether each target is independent;
- whether ordering is significant;
- whether concurrent database work is appropriate;
- whether the workflow requires transactional coordination;
- how partial failures should be handled.

Use:

```csharp
await command.ExecuteAsync();
```

for the command’s standard sequential execution path.

Use:

```csharp
await command.ExecuteParallelAsync();
```

for independent operations that are intentionally allowed to execute concurrently.

Do not select parallel execution solely because it appears faster.

Correct workflow semantics take priority over throughput.

## Recommended application shape

A focused application workflow can use command pipelines without becoming a command-class-per-operation architecture.

```text
SalaryAdjustmentService
    |
    +-- IQueryFactory
    |       -> read source employees
    |
    +-- application logic
    |       -> adjust salaries
    |
    +-- ICommandFactory
            -> build target operations
            -> execute command
            -> inspect result
```

The workflow remains named after its application purpose.

The DataArc command pipeline provides the persistence execution model inside that workflow.

## Summary

A DataArc command pipeline consists of:

```text
ICommandFactory
    -> CreateCommandBuilderAsync()
    -> UseDbExecutionContext<TExecutionContext>()
    -> add operation
    -> BuildAsync()
    -> ExecuteAsync()
       or ExecuteParallelAsync()
    -> inspect structured result
```

The core responsibilities are:

- `ICommandFactory` creates the command builder;
- the command builder collects database operations;
- `UseDbExecutionContext<TExecutionContext>()` selects each database boundary;
- `BuildAsync()` produces the executable command;
- `ExecuteAsync()` executes through the standard ordered path;
- `ExecuteParallelAsync()` executes independent operations concurrently;
- the structured result reports success, failure information, and affected records.

Command pipelines are most useful when application workflows need explicit control over writes across one or more registered EF Core execution contexts.

## Next steps

Continue with:

- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Transactional Workflows](transactions.md)
- [Structured Execution Results](structured-results.md)