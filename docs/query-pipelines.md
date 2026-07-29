# Query Pipelines

Query pipelines provide a consistent way to execute read operations through a registered DataArc execution context.

A query pipeline follows three steps:

1. Create a query through `IQueryFactory`.
2. Select an execution context with `UseDbExecutionContext<TExecutionContext>()`.
3. Execute one of DataArc’s defined read operations.

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

The execution context identifies where the query runs, while the generic type supplied to the read operation identifies the model being queried.

```csharp
UseDbExecutionContext<IGoogleDbContext>()
ReadWhere<Employee>(...)
```

## Why query pipelines exist

Entity Framework Core allows developers to read data directly through a `DbContext` or `DbSet<TEntity>`.

That flexibility can lead to different features or developers implementing reads in different ways:

- direct `DbSet<TEntity>` access;
- custom repository methods;
- feature-specific query services;
- arbitrary `IQueryable<TEntity>` composition;
- inconsistent projection and paging patterns;
- different approaches to synchronous and asynchronous execution.

DataArc provides a defined query contract so that reads follow the same recognizable pattern throughout an application.

```csharp
_queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IApplicationDbContext>()
    .ReadWhere<Employee>(
        employee => employee.IsActive);
```

This keeps the following decisions visible in the workflow:

- which execution context is selected;
- which model is being queried;
- which read operation is being performed;
- whether execution is synchronous or asynchronous;
- whether the result contains full entities or a selected projection.

The current DataArc contract returns materialized results rather than exposing the underlying `IQueryable<TEntity>` to application workflows.

## Synchronous query pipelines

Create a synchronous query with `CreateQuery()`:

```csharp
var query = _queryFactory.CreateQuery();
```

Select the required execution context and execute a read operation:

```csharp
var employees = query
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

The query can also be written as one fluent pipeline:

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

Synchronous query operations are appropriate when the application intentionally uses synchronous database access.

For web applications, services, background workers, and other scalable workloads, asynchronous query operations are generally the preferred path.

## Asynchronous query pipelines

Create an asynchronous query with `CreateQueryAsync()`:

```csharp
var employeesQuery = await _queryFactory.CreateQueryAsync();
```

Select the execution context and execute an asynchronous read operation:

```csharp
var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

The factory operation and the database operation are both asynchronous:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<IApplicationDbContext>()
    .ReadAllAsync<Employee>();
```

Depending on the selected operation, asynchronous queries return materialized results such as:

```csharp
Task<TEntity?>
```

or:

```csharp
Task<IReadOnlyList<TEntity>>
```

## Selecting an execution context

Every query pipeline explicitly selects an execution context:

```csharp
UseDbExecutionContext<TExecutionContext>()
```

For a public execution-context contract:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAllAsync<Employee>();
```

For a concrete `DbContext` that implements `IExecutionContext` directly:

```csharp
var customers = await query
    .UseDbExecutionContext<ApplicationDbContext>()
    .ReadAllAsync<Customer>();
```

The query contract remains the same in both cases.

The execution-context type answers:

> Which registered database boundary should execute this query?

The generic type supplied to the read operation answers:

> Which model should be queried?

```csharp
.UseDbExecutionContext<IHrDbContext>()
.ReadWhereAsync<Employee>(...)
```

The execution context and entity type are therefore selected independently.

## Reading one entity

Use `ReadOne<TEntity>` or `ReadOneAsync<TEntity>` to retrieve an entity by its key values.

```csharp
var employee = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadOne<Employee>(employeeId);
```

The asynchronous equivalent is:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employee = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadOneAsync<Employee>(employeeId);
```

The asynchronous operation returns:

```csharp
Task<TEntity?>
```

A null result indicates that no matching entity was found.

Composite keys can be supplied as multiple key values:

```csharp
var record = await query
    .UseDbExecutionContext<IApplicationDbContext>()
    .ReadOneAsync<ApplicationRecord>(
        tenantId,
        recordId);
```

The key values must be supplied in the order defined by the Entity Framework Core model.

## Reading all entities

Use `ReadAll<TEntity>` or `ReadAllAsync<TEntity>` to retrieve all rows for a model.

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAll<Employee>();
```

The asynchronous equivalent is:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAllAsync<Employee>();
```

The asynchronous operation returns:

```csharp
Task<IReadOnlyList<TEntity>>
```

`ReadAll` should be used carefully for tables that may contain a large number of records.

For large datasets, use filtering or paging instead.

## Filtering entities

Use `ReadWhere<TEntity>` or `ReadWhereAsync<TEntity>` to filter entities with an expression.

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

The asynchronous equivalent is:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

The predicate is supplied as:

```csharp
Expression<Func<TEntity, bool>>
```

Entity Framework Core translates supported expressions through the database provider configured for the selected execution context.

For example:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee>(
        employee =>
            employee.IsActive &&
            employee.Salary >= minimumSalary);
```

## Entity-shaped projections

DataArc supports projections that return the same entity type.

```csharp
var employeesQuery = await _queryFactory.CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold,
        employee => new Employee
        {
            Name = employee.Name,
            Surname = employee.Surname
        });
```

The second expression is a projection selector:

```csharp
employee => new Employee
{
    Name = employee.Name,
    Surname = employee.Surname
}
```

Because the selector is supplied as an expression tree, Entity Framework Core can translate it as part of the database query.

Only the referenced columns are selected from the database.

Conceptually, the generated query is equivalent to:

```sql
SELECT Name, Surname
FROM Employees
WHERE Salary > @salaryThreshold;
```

The returned objects are therefore entity-shaped query results containing only the selected values.

They are not fully populated entity records.

The results can then be mapped to an application DTO after materialization:

```csharp
var employeeDtos = employees
    .Select(employee => new EmployeeDto
    {
        Name = employee.Name,
        Surname = employee.Surname
    })
    .ToList();
```

The complete flow is:

```text
Database
    -> filter by Salary
    -> select Name and Surname
    -> materialize entity-shaped results
    -> map results to EmployeeDto in memory
```

This approach is useful when:

- the workflow requires only a subset of entity columns;
- unnecessary data transfer should be reduced;
- the current query contract should continue returning `TEntity`;
- DTO mapping is intentionally performed after materialization.

Because the result may contain only selected columns, consumers should treat these objects as read results rather than complete entities intended for later updates.

## DTO and read-model projections

DataArc also supports projecting an entity directly into a different result type.

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employeeSummaries = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee, EmployeeSummaryDto>(
        employee => employee.IsActive,
        employee => new EmployeeSummaryDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Surname = employee.Surname,
            Salary = employee.Salary
        });
```

The first generic type identifies the persisted entity:

```csharp
Employee
```

The second generic type identifies the projected result:

```csharp
EmployeeSummaryDto
```

The selector is supplied as:

```csharp
Expression<Func<TEntity, TResult>>
```

The operation returns:

```csharp
Task<IReadOnlyList<TResult>>
```

A result model could be defined as:

```csharp
public sealed class EmployeeSummaryDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Surname { get; init; } = string.Empty;

    public decimal Salary { get; init; }
}
```

Direct result projections are useful when:

- returning API or application response models;
- selecting only the required database columns;
- creating workflow-specific read models;
- avoiding an additional in-memory mapping step;
- preventing persistence entities from crossing application boundaries.

Both projection styles are supported:

```csharp
ReadWhereAsync<TEntity>(
    predicate,
    entitySelector)
```

and:

```csharp
ReadWhereAsync<TEntity, TResult>(
    predicate,
    resultSelector)
```

Use an entity-shaped projection when the result should remain `TEntity`.

Use the two-type projection when the query should return a separate DTO or read model.

## Paging query results

Use `ReadPaged<TEntity>` or `ReadPagedAsync<TEntity>` to retrieve a fixed range of records.

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadPaged<Employee>(
        startIndex: 0,
        recordCount: 50);
```

The asynchronous equivalent is:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadPagedAsync<Employee>(
        startIndex: 0,
        recordCount: 50);
```

The `startIndex` specifies how many records should be skipped.

The `recordCount` specifies the maximum number of records returned.

For example, the second page of a fifty-record result set begins at index `50`:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadPagedAsync<Employee>(
        startIndex: 50,
        recordCount: 50);
```

## Filtering with paging

Use `ReadWherePaged<TEntity>` or `ReadWherePagedAsync<TEntity>` to combine filtering and paging.

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWherePagedAsync<Employee>(
        employee => employee.IsActive,
        startIndex: 0,
        recordCount: 50);
```

Filtering is applied before the requested range is materialized.

Projected results can also be returned from a filtered and paged query:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWherePagedAsync<Employee, EmployeeSummaryDto>(
        employee => employee.IsActive,
        employee => new EmployeeSummaryDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Salary = employee.Salary
        },
        startIndex: 0,
        recordCount: 50);
```

This combines:

- an execution context;
- a persisted entity type;
- a filter expression;
- a result projection;
- a paging range;
- asynchronous materialization.

The asynchronous operation returns:

```csharp
Task<IReadOnlyList<TResult>>
```

## Stored procedure queries

The asynchronous query contract supports stored procedure execution.

A typed stored procedure result can be returned with:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var results = await query
    .UseDbExecutionContext<IReportingDbContext>()
    .ExecuteStoredProcedureAsync<EmployeeReportRow>(
        "dbo.GetEmployeeReport",
        new
        {
            DepartmentId = departmentId,
            MinimumSalary = minimumSalary
        });
```

The result type must be a reference type with a parameterless constructor:

```csharp
public sealed class EmployeeReportRow
{
    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public decimal Salary { get; set; }
}
```

Stored procedure results can also be returned dynamically:

```csharp
var results = await query
    .UseDbExecutionContext<IReportingDbContext>()
    .ExecuteStoredProcedureAsync(
        "dbo.GetEmployeeReport",
        new
        {
            DepartmentId = departmentId
        });
```

A cancellation token can be supplied:

```csharp
var results = await query
    .UseDbExecutionContext<IReportingDbContext>()
    .ExecuteStoredProcedureAsync<EmployeeReportRow>(
        "dbo.GetEmployeeReport",
        parameters,
        cancellationToken);
```

Stored procedure names, parameter handling, result mapping, and supported behavior remain dependent on the configured Entity Framework Core database provider.

## Using query results in a workflow

Query pipelines can be combined with command pipelines in an application workflow.

The following example reads employees from one execution context:

```csharp
var employeesQuery = await _queryFactory.CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

The application then applies its business operation to the materialized results:

```csharp
foreach (var employee in employees)
{
    employee.Salary +=
        employee.Salary * salaryAdjustmentBaseRate;
}
```

The resulting collection can then be supplied to a command pipeline:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<IGoogleDbContext>()
    .AddBulk(employees, batchSize);
```

This keeps the responsibilities visible:

```text
Query pipeline
    -> select execution context
    -> execute read operation
    -> materialize results

Application workflow
    -> apply business changes

Command pipeline
    -> select target execution contexts
    -> build database operations
    -> execute commands
```

A complete workflow may therefore read from one execution context and write to several others while using the same DataArc execution model throughout.

## Materialized results

The current DataArc query contract returns materialized results.

Collection operations return:

```csharp
IReadOnlyList<TEntity>
```

or:

```csharp
IReadOnlyList<TResult>
```

DataArc does not currently expose an `IQueryable<TEntity>` for application code to continue composing after selecting an execution context.

The supported pattern is:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.IsActive);
```

rather than:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext, Employee>()
    .Where(employee => employee.IsActive)
    .ToListAsync();
```

The second form may be familiar to Entity Framework Core users, but it is not the current DataArc query contract.

The defined read operations ensure that developers use consistent patterns for:

- execution-context selection;
- filtering;
- projection;
- paging;
- stored procedure execution;
- result materialization.

## Query pipeline summary

A DataArc query pipeline consists of:

```text
IQueryFactory
    -> CreateQuery() or CreateQueryAsync()
    -> UseDbExecutionContext<TExecutionContext>()
    -> Read operation
    -> Materialized result
```

For example:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var employees = await query
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

The factory creates the query pipeline.

The execution context selects the registered database boundary.

The read operation identifies the entity, filter, projection, paging, or stored procedure behavior.

Entity Framework Core executes the resulting database query through the provider registered for that execution context.