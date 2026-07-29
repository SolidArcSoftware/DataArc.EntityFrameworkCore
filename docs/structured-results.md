# Structured Execution Results

DataArc command execution returns a structured result instead of requiring the calling workflow to infer success from control flow alone.

The current command result exposes:

```csharp
commandResult.Success
commandResult.Exception
commandResult.TotalAffected
```

These values allow the application to determine:

- whether execution completed successfully;
- whether an exception was captured;
- how many records were affected across the executed operations.

```csharp
var commandResult = await command.ExecuteAsync();

if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "Command execution failed.",
        commandResult.Exception);
}

return commandResult.TotalAffected;
```

## Why structured results exist

A database operation can complete in several ways:

```text
Execution
    |
    +-- succeeds
    |       -> Success = true
    |       -> TotalAffected contains the affected count
    |
    +-- fails
            -> Success = false
            -> Exception contains failure information
```

Returning a structured result keeps the execution outcome explicit and gives the application one place to inspect command completion.

This is particularly useful when a command contains:

- several operations;
- bulk operations;
- several execution contexts;
- parallel execution;
- a workflow that must report one combined result.

## The result does not replace application decisions

DataArc reports the execution outcome.

The application still decides what that outcome means.

For example, a failed command may cause an API to return an error response:

```csharp
if (!commandResult.Success)
{
    return Results.Problem(
        title: "Database operation failed",
        detail: commandResult.Exception?.Message);
}
```

A background worker may log the failure and stop processing:

```csharp
if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Employee replication failed.");

    return;
}
```

An application service may convert the result into its own contract:

```csharp
if (!commandResult.Success)
{
    return ApplicationResult.Failure(
        commandResult.Exception?.Message
        ?? "The command could not be completed.");
}

return ApplicationResult.Success(
    commandResult.TotalAffected);
```

The DataArc result belongs to the execution layer.

The public API, user interface, worker, or application service should translate it into the form required by that boundary.

## Success

`Success` indicates whether the command execution completed successfully.

```csharp
if (commandResult.Success)
{
    // Continue the workflow.
}
```

A typical failure check is:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "Command execution failed.",
        commandResult.Exception);
}
```

Use `Success` as the primary execution outcome.

Do not use `TotalAffected > 0` as a substitute for checking success.

A successful command may legitimately affect zero records.

Examples include:

- no rows matched the operation;
- the input collection was empty and handled upstream;
- the target state already matched the requested state;
- an idempotent operation had nothing new to apply.

Likewise, a command that affected some records before another operation failed should not automatically be treated as successful.

## Exception

`Exception` contains failure information when command execution fails.

```csharp
if (commandResult.Exception is not null)
{
    logger.LogError(
        commandResult.Exception,
        "Command execution failed.");
}
```

Preserve the original exception when raising a higher-level exception:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "The employee update could not be completed.",
        commandResult.Exception);
}
```

This retains the underlying cause for diagnostics.

Avoid returning raw internal exception details directly to end users.

For a public API, log the exception and return an appropriate application-safe message:

```csharp
if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Salary adjustment execution failed.");

    return Results.Problem(
        title: "Salary adjustment failed",
        detail: "The operation could not be completed.");
}
```

## TotalAffected

`TotalAffected` reports the total number of records affected by the command execution.

```csharp
var affectedRecords =
    commandResult.TotalAffected;
```

A typical successful workflow returns the count:

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "Command execution failed.",
        commandResult.Exception);
}

return commandResult.TotalAffected;
```

For a command containing several operations, `TotalAffected` represents the combined affected count reported by the command result.

Conceptually:

```text
Operation A -> 100 affected
Operation B -> 100 affected
Operation C -> 100 affected
                |
                v
TotalAffected -> combined command count
```

The affected count is an execution metric.

It is not, by itself, proof that the business workflow reached the expected state.

The application may still need to verify:

- that the expected targets participated;
- that the required entities were processed;
- that no business rule was violated;
- that external follow-up work completed;
- that a partial failure did not occur across separate boundaries.

## Complete command result example

```csharp
public async Task<int> ReplicateEmployeesAsync(
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

    commandBuilder
        .UseDbExecutionContext<IMicrosoftDbContext>()
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

This workflow:

1. avoids building a command when no work exists;
2. adds operations to explicit execution contexts;
3. builds the command;
4. executes the independent operations in parallel;
5. checks `Success`;
6. preserves `Exception` when raising a higher-level failure;
7. returns `TotalAffected` only after successful execution.

## Result handling in an application service

An application service should normally convert the DataArc result into a use-case-specific result.

```csharp
public async Task<ApplicationResult<int>>
    ProcessSalaryAdjustmentsAsync(
        IReadOnlyList<Employee> employees,
        int batchSize)
{
    var commandBuilder = await _commandFactory
        .CreateCommandBuilderAsync();

    commandBuilder
        .UseDbExecutionContext<IGoogleDbContext>()
        .AddBulk(employees, batchSize);

    var command = await commandBuilder.BuildAsync();

    var commandResult = await command.ExecuteAsync();

    if (!commandResult.Success)
    {
        return ApplicationResult<int>.Failure(
            commandResult.Exception?.Message
            ?? "Salary adjustment execution failed.");
    }

    return ApplicationResult<int>.Success(
        commandResult.TotalAffected);
}
```

This separates:

```text
DataArc execution result
    -> technical command outcome

Application result
    -> use-case outcome exposed to the caller
```

The application result may include additional information such as:

- a business error code;
- validation messages;
- a workflow identifier;
- a user-safe description;
- the affected record count;
- retry guidance.

## Result handling in an API endpoint

An endpoint can map the application outcome to HTTP responses:

```csharp
var result = await salaryAdjustmentService
    .ProcessSalaryAdjustmentsAsync(
        employees,
        batchSize);

if (!result.Success)
{
    return Results.Problem(
        title: "Salary adjustment failed",
        detail: result.ErrorMessage);
}

return Results.Ok(new
{
    Affected = result.Value
});
```

The endpoint should not need to understand the internal database exception model.

That translation belongs in the application boundary.

## Result handling in a background worker

A worker may choose to log and retry:

```csharp
var commandResult = await command.ExecuteAsync();

if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Import command failed.");

    throw new InvalidOperationException(
        "The import will be retried.",
        commandResult.Exception);
}
```

Automatic retry is only appropriate when repeating the operation is safe.

Before retrying, determine whether the command is:

- idempotent;
- protected against duplicate inserts;
- able to detect already completed work;
- safe after partial completion;
- using a stable workflow identifier.

## Results from multi-context execution

A command may contain operations for several execution contexts:

```text
Command
    |
    +-- Google operation
    +-- Microsoft operation
    +-- OpenAI operation
```

The returned result represents the command execution as one application-level outcome.

However, one application-level result does not imply one distributed transaction.

If the contexts use separate databases, the workflow must still account for partial completion.

```text
Google operation succeeds
Microsoft operation succeeds
OpenAI operation fails
```

The result may report failure, but the successful database operations may already have committed.

The application may need to:

- identify the failed target;
- retry only incomplete work;
- record a reconciliation task;
- run compensation logic;
- flag the workflow for manual review.

See [Transactional Workflows](transactions.md).

## Successful execution with zero affected rows

A successful result can report:

```csharp
commandResult.Success == true
commandResult.TotalAffected == 0
```

This can be valid.

The application should decide whether zero affected records is acceptable.

For example:

```csharp
if (!commandResult.Success)
{
    return ApplicationResult.Failure(
        "The command failed.");
}

if (commandResult.TotalAffected == 0)
{
    return ApplicationResult.Failure(
        "No employee records were updated.");
}
```

In another workflow, zero affected rows may simply mean that the data was already current.

The correct interpretation belongs to the use case.

## Failed execution with partial effects

When operations cross separate execution contexts or databases, a failed command can still leave partial effects.

Do not assume:

```text
Success = false
    -> every database change was rolled back
```

That is only guaranteed when all protected changes share a verified transaction boundary.

For multi-database workflows, record enough operational information to reconcile the outcome.

Useful information can include:

- workflow identifier;
- execution-context name;
- operation name;
- start and completion timestamps;
- affected count per target;
- captured exception;
- retry status;
- compensation status.

The current result members provide the overall command outcome. Application-level telemetry can add the business and operational context required by the system.

## Logging structured results

Log the result at the boundary where the workflow is understood.

```csharp
if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Employee replication failed.");
}
else
{
    logger.LogInformation(
        "Employee replication completed. " +
        "Affected records: {TotalAffected}",
        commandResult.TotalAffected);
}
```

Avoid logging sensitive entity data unnecessarily.

Prefer identifiers and execution metadata over full payloads.

## Do not hide failure

Avoid code that discards the result:

```csharp
await command.ExecuteAsync();
```

when the workflow requires confirmation that the operation succeeded.

Prefer:

```csharp
var commandResult = await command.ExecuteAsync();

if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "Command execution failed.",
        commandResult.Exception);
}
```

A structured result is only useful when the calling workflow inspects it.

## Do not expose internal exceptions directly

This is weak API handling:

```csharp
return Results.BadRequest(
    commandResult.Exception?.ToString());
```

It may expose:

- database details;
- connection information;
- SQL fragments;
- internal type names;
- implementation details.

Instead:

```csharp
logger.LogError(
    commandResult.Exception,
    "Command execution failed.");

return Results.Problem(
    title: "Operation failed",
    detail: "The requested operation could not be completed.");
```

## Recommended handling pattern

Use this general shape:

```csharp
var commandResult = await command.ExecuteAsync();

if (!commandResult.Success)
{
    logger.LogError(
        commandResult.Exception,
        "Command execution failed.");

    return ApplicationResult<int>.Failure(
        "The operation could not be completed.");
}

return ApplicationResult<int>.Success(
    commandResult.TotalAffected);
```

The responsibilities remain clear:

```text
DataArc
    -> executes command
    -> returns structured execution result

Application
    -> interprets result
    -> applies business meaning
    -> maps failure or success to its own contract

Presentation or worker
    -> exposes or reacts to the application result
```

## Summary

The current DataArc command result exposes:

```csharp
Success
Exception
TotalAffected
```

Use them as follows:

- check `Success` before treating the command as completed;
- inspect or log `Exception` when execution fails;
- preserve the original exception when raising a higher-level failure;
- use `TotalAffected` as an execution metric after success;
- do not equate affected rows with business success;
- do not assume a failed multi-context command rolled back every target;
- translate the DataArc result into an application-specific result;
- avoid exposing internal exception details to public callers.

## Next steps

Continue with:

- [Command Pipelines](command-pipelines.md)
- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Transactional Workflows](transactions.md)
