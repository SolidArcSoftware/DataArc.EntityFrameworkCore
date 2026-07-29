# Transactional Workflows

Transactional workflows protect a defined set of database changes from being committed in an inconsistent state.

DataArc command pipelines can coordinate application operations across registered execution contexts, but the application must still understand the transaction boundary of each workflow.

The most important distinction is:

```text
One application workflow
    !=
one database transaction

One DataArc command
    !=
one distributed transaction across every execution context
```

A transaction is only as broad as the database connection and provider capabilities that participate in it.

## Why transaction boundaries matter

Consider an employee-onboarding operation:

```text
Create employee
    -> create payroll profile
    -> provision IT account
    -> assign equipment
```

The application may describe this as one business operation.

The underlying technical work may span:

- one `DbContext`;
- several `DbContext` instances connected to one database;
- several schemas;
- several databases;
- external services that do not participate in database transactions.

Those scenarios do not have the same consistency guarantees.

Before implementing a transactional workflow, identify:

1. which operations must succeed together;
2. which database connection each operation uses;
3. whether all operations can share one transaction;
4. what happens when an external or separate-database operation fails;
5. whether compensation or retry is required.

## Transaction scope and application scope

A business operation can be wider than a database transaction.

```text
Business operation
    |
    +-- local database transaction
    +-- external API call
    +-- message publication
    +-- second database operation
```

The application workflow coordinates all four steps.

The database transaction may protect only the local database changes.

DataArc helps make the participating execution contexts and command operations explicit, but it does not remove the physical limits of the underlying databases.

## EF Core's default transaction behavior

For relational providers, a single `SaveChanges` call is normally executed within a transaction when the provider supports transactions.

Conceptually:

```text
tracked changes
    -> SaveChanges
    -> transaction begins
    -> SQL operations execute
    -> commit on success
    -> rollback on failure
```

This is sufficient for many ordinary single-context writes.

An explicit transaction is useful when the workflow needs to group several persistence steps into one local transaction boundary.

The exact behavior remains provider-dependent and should be tested with the database used by the application.

## Single-context transactional workflow

The strongest transaction boundary is normally one execution context using one database connection.

```text
Application service
    -> one execution context
    -> one database connection
    -> one transaction
```

Within that boundary, the application can:

- read the required state;
- apply business rules;
- add or update several entities;
- save the changes;
- commit only when every required operation succeeds.

A representative EF Core shape is:

```csharp
await using var transaction = await dbContext
    .Database
    .BeginTransactionAsync();

try
{
    // Perform the required database operations.

    await dbContext.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

This example shows the underlying EF Core transaction principle.

The concrete transaction implementation should remain inside the persistence boundary when the application layer depends only on an execution-context contract.

## Keep provider-specific transaction code in persistence

When the application uses a public execution-context contract such as:

```csharp
public interface IHrDbContext : IExecutionContext
{
    DbSet<Employee>? Employee { get; set; }

    DbSet<EmploymentRecord>? EmploymentRecord { get; set; }
}
```

the concrete `DbContext` can remain internal:

```csharp
internal sealed class HrDbContext :
    DbContext,
    IHrDbContext
{
    public HrDbContext(
        DbContextOptions<HrDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee>? Employee { get; set; }

    public DbSet<EmploymentRecord>? EmploymentRecord { get; set; }
}
```

Transaction-specific EF Core code should not be forced into the application layer merely to gain access to `Database.BeginTransactionAsync()`.

A focused persistence operation can own the local transaction:

```csharp
public interface IEmployeePersistence
{
    Task<int> CreateEmployeeRecordAsync(
        Employee employee,
        EmploymentRecord employmentRecord);
}
```

The implementation can use the concrete context:

```csharp
internal sealed class EmployeePersistence :
    IEmployeePersistence
{
    private readonly HrDbContext _dbContext;

    public EmployeePersistence(
        HrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateEmployeeRecordAsync(
        Employee employee,
        EmploymentRecord employmentRecord)
    {
        await using var transaction = await _dbContext
            .Database
            .BeginTransactionAsync();

        try
        {
            _dbContext.Set<Employee>().Add(employee);
            _dbContext.Set<EmploymentRecord>()
                .Add(employmentRecord);

            var affected = await _dbContext
                .SaveChangesAsync();

            await transaction.CommitAsync();

            return affected;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

The application service coordinates the business use case while the persistence implementation owns the provider-specific transaction.

## Several operations in one context

Several related changes can be committed together when they use the same context and transaction.

```text
IHrDbContext
    |
    +-- add Employee
    +-- add EmploymentRecord
    +-- update Vacancy
    |
    -> one local transaction
```

This is the preferred shape when the records must be atomically consistent and belong to the same persistence boundary.

Avoid splitting tightly coupled records across separate database boundaries without a clear consistency strategy.

## Multiple `DbContext` instances against one database

A modular application may use several `DbContext` types while still connecting to one physical database.

```text
HrDbContext --------+
                    |
FinanceDbContext ---+--> same database
                    |
OperationsDbContext-+
```

This does not automatically mean that the contexts share one transaction.

To participate in one local transaction, they generally need compatible provider behavior and coordinated use of the same underlying connection and transaction.

That arrangement is more complex than a normal single-context transaction.

Use it only when the modular boundaries justify the added coordination.

Do not infer atomicity merely because the connection strings point to the same database.

## Separate databases

When execution contexts connect to separate databases:

```text
IHrDbContext
    -> HR database

IFinanceDbContext
    -> Finance database

IInformationTechnologyDbContext
    -> IT database
```

a command can coordinate operations for all three contexts, but those operations do not automatically share one local transaction.

```text
One command builder
    |
    +-- HR operation
    +-- Finance operation
    +-- IT operation

Each target
    -> its own connection
    -> its own transaction boundary
```

If the HR operation commits and the IT operation fails, a normal local database transaction cannot roll back the already committed HR database operation.

That is a distributed consistency problem.

## DataArc command coordination is not a distributed transaction

A command can contain several operations:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IHrDbContext>()
    .AddBulk(hrEmployees, batchSize);

commandBuilder
    .UseDbExecutionContext<IFinanceDbContext>()
    .AddBulk(financeEmployees, batchSize);

var command = await commandBuilder.BuildAsync();
```

Executing the command:

```csharp
var commandResult = await command.ExecuteAsync();
```

coordinates the application operations.

It does not prove that both databases participate in one atomic commit.

The application must not present a multi-database command as transactionally atomic unless that guarantee is explicitly implemented and verified.

## Parallel execution and transactions

Parallel execution is intended for independent operations:

```csharp
var commandResult = await command
    .ExecuteParallelAsync();
```

It should not be used as a substitute for a transaction.

```text
ExecuteParallelAsync()
    -> concurrent independent work

Transaction
    -> commit or roll back a defined unit of work
```

If operations must succeed together, concurrency does not solve the consistency requirement.

Parallel multi-database operations may also increase the possibility of partial completion because several targets can progress independently.

## Ordered workflows

When operations depend on one another, execute them in the required application order.

```text
Step 1: create employee
    |
    v
Step 2: use employee identifier
    |
    v
Step 3: create payroll profile
```

Do not run these steps in parallel.

If all steps belong to one local persistence boundary, place them in one local transaction.

If they cross databases or external systems, use a broader workflow strategy.

## Local transaction plus external operation

External services do not normally participate in an EF Core database transaction.

```text
Local database transaction
    -> commit employee

External identity provider
    -> create account
```

If the database commits and the external call fails, the workflow is partially complete.

Possible strategies include:

- call the external system before committing, when safe;
- commit locally and retry the external operation;
- record a pending provisioning state;
- publish an outbox message within the local transaction;
- execute compensation when the external step fails;
- require manual intervention for exceptional cases.

The correct strategy depends on the business process.

## Transactional outbox pattern

When a local database update must result in a message or external action, an outbox can preserve the local atomic boundary.

```text
One local transaction
    |
    +-- save business data
    +-- save outbox message
    |
    -> commit
```

A separate worker later publishes the outbox message:

```text
Outbox worker
    -> read pending message
    -> publish
    -> mark as completed
```

This prevents the application from committing business data and then losing the intent to publish when the process crashes between those steps.

DataArc can coordinate the application workflow around this pattern, while the local database transaction protects the business data and outbox record.

## Compensation

When a workflow crosses transaction boundaries, compensation may be more appropriate than attempting one distributed transaction.

Example:

```text
1. Create HR employee
2. Create finance profile
3. Provision IT account
```

If step 3 fails after steps 1 and 2 commit, compensation could:

```text
1. mark employee onboarding as incomplete
2. disable or remove the finance profile
3. queue IT provisioning for retry
```

Compensation is business logic.

It should be explicit, observable, and safe to repeat where possible.

Do not describe compensation as a rollback unless it truly restores the previous business state.

## Idempotency

Retries are common in cross-boundary workflows.

An operation is idempotent when repeating it does not create an incorrect additional effect.

Examples of idempotency controls include:

- unique workflow identifiers;
- unique database constraints;
- operation-status tables;
- idempotency keys;
- checking for an existing target record;
- recording completed workflow stages.

```text
Retry request
    -> check workflow identifier
    -> already completed?
       -> return existing outcome
       -> do not create a duplicate
```

Idempotency should be designed before enabling automatic retries.

## Savepoints and partial recovery

Some relational providers support savepoints inside an existing transaction.

A savepoint can allow a workflow to roll back part of a local transaction without abandoning the entire transaction.

Conceptually:

```text
begin transaction
    -> operation A
    -> create savepoint
    -> operation B
    -> operation B fails
    -> roll back to savepoint
    -> continue or abort
```

Savepoint support and behavior are provider-specific.

Use savepoints only when the added complexity is justified and tested with the target provider.

## Isolation levels

A transaction isolation level controls how concurrent database operations can observe one another.

Higher isolation may improve consistency but can increase:

- locking;
- blocking;
- deadlocks;
- transaction duration;
- reduced concurrency.

Lower isolation may improve concurrency but allow more concurrent-state effects.

Do not select an isolation level from a generic recommendation.

Choose it from:

- the business consistency requirement;
- the database provider;
- expected concurrent load;
- the exact read and write pattern.

Keep transactions as short as practical.

## Keep transactions short

Do not hold a database transaction open while performing slow unrelated work.

Avoid placing these operations inside a local database transaction unless required:

- remote HTTP calls;
- file uploads;
- long calculations;
- user interaction;
- artificial delays;
- waiting for another process.

A better shape is:

```text
Prepare and validate
    -> begin transaction
    -> perform required database work
    -> commit
    -> continue with external work
```

When the external work must be reliably triggered, use an outbox or another durable workflow mechanism.

## Error handling

A local transaction should roll back when the protected operation fails.

```csharp
await using var transaction = await dbContext
    .Database
    .BeginTransactionAsync();

try
{
    // Database work.

    await dbContext.SaveChangesAsync();

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

The application boundary can then convert the failure into its own result:

```csharp
try
{
    var affected = await _employeePersistence
        .CreateEmployeeRecordAsync(
            employee,
            employmentRecord);

    return ApplicationResult.Success(affected);
}
catch (Exception exception)
{
    logger.LogError(
        exception,
        "Employee creation failed.");

    return ApplicationResult.Failure(
        "Employee creation could not be completed.");
}
```

Do not silently catch a transaction failure and return success.

## Structured command results

DataArc command execution returns a structured outcome:

```csharp
commandResult.Success
commandResult.Exception
commandResult.TotalAffected
```

A failed result should stop the calling workflow unless the application has an explicit recovery strategy.

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        "The command failed.",
        commandResult.Exception);
}
```

`TotalAffected` is an execution metric.

It is not proof that every business invariant was satisfied.

The business workflow should still validate its expected outcome.

See [Structured Execution Results](structured-results.md).

## A transactional onboarding shape

A realistic onboarding workflow may combine local transactions with broader orchestration:

```text
Onboard employee
    |
    +-- HR local transaction
    |       -> create employee
    |       -> create employment record
    |       -> update vacancy
    |
    +-- save outbox event
    |
    -> commit HR transaction
    |
    +-- finance provisioning worker
    +-- IT provisioning worker
    +-- operations allocation worker
```

This preserves one strong local transaction for HR ownership while allowing the other bounded contexts to process durable follow-up work.

The complete business process is eventually consistent rather than one distributed atomic transaction.

## Choosing the correct consistency model

Use one local transaction when:

- all changes belong to one database boundary;
- the records must commit together;
- the provider supports the required behavior;
- the transaction can remain short.

Use ordered application orchestration when:

- steps depend on one another;
- each step has a clear outcome;
- a later step should not begin before an earlier step succeeds.

Use parallel execution when:

- operations are independent;
- order does not matter;
- partial failure can be handled;
- no shared atomic transaction is required.

Use an outbox when:

- a local database commit must reliably trigger messaging or external work.

Use compensation when:

- the workflow crosses boundaries;
- already committed work cannot be rolled back technically;
- the business can define a corrective action.

Use idempotent retries when:

- transient failures are expected;
- repeating the operation can be made safe.

## Common mistakes

### Assuming one command means one transaction

```text
One command
    !=
one atomic transaction across all contexts
```

### Using parallel execution for dependent steps

Parallelism is not appropriate when later work requires earlier output.

### Holding transactions open during external calls

This increases lock duration and failure risk.

### Retrying non-idempotent operations blindly

A retry may create duplicates or repeat already committed work.

### Treating compensation as a technical rollback

Compensation is a new business action and may not restore the exact previous state.

### Hiding the transaction boundary

The code and documentation should make clear which operations are protected together.

## Recommended design questions

Before implementing a transactional workflow, answer:

1. What is the business operation?
2. Which records must be atomically consistent?
3. Which execution context owns those records?
4. Do all writes use the same database connection?
5. Can the transaction remain short?
6. Are external calls involved?
7. What happens after partial completion?
8. Can failed operations be retried safely?
9. Is an outbox required?
10. What operational evidence is recorded?

If these questions do not have clear answers, adding a transaction API alone will not make the workflow reliable.

## Summary

Transactional workflows require an explicit understanding of boundaries.

```text
Single execution context
    -> strongest local transaction boundary

Several contexts on one database
    -> transaction sharing must be explicitly coordinated

Separate databases
    -> no automatic local atomic transaction

External services
    -> require retries, outbox, compensation,
       or another durable workflow strategy
```

DataArc can coordinate the application workflow and keep the participating execution contexts visible.

The underlying database transaction still belongs to the relevant provider, connection, and persistence boundary.

The central rules are:

- do not equate one application command with one distributed transaction;
- keep local transactions inside the persistence boundary;
- use parallel execution only for independent operations;
- use ordered execution for dependent steps;
- design for partial failure across separate databases and external systems;
- use idempotency, outbox processing, and compensation where appropriate;
- keep transaction duration short and observable.

## Next steps

Continue with:

- [Command Pipelines](command-pipelines.md)
- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Structured Execution Results](structured-results.md)
