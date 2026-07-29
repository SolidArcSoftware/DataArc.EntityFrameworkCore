# Execution Contexts

Execution contexts define the database boundaries used by DataArc.EntityFrameworkCore.

They allow application workflows to select a specific persistence boundary explicitly before executing a command or query.

IExecutionContext is not intended to be another repository-style abstraction over Entity Framework Core. It provides DataArc with a stable execution boundary that can be used to build commands, route queries, coordinate parallel operations across multiple registered contexts, and support transactional workflows.

The execution-context contract also separates application workflows from the concrete provider registered behind that boundary. In the current release, these contexts are backed by Entity Framework Core. The same execution model can support additional persistence providers in future without requiring workflows to abandon the execution-context patterns they already use.

DataArc supports two execution-context styles:

1. A public execution-context contract backed by a concrete `DbContext`.
2. A concrete `DbContext` that directly implements `IExecutionContext`.

Hiding the concrete `DbContext` is optional. DataArc supports both approaches so that applications can choose the boundary that fits their architecture.

## Why execution contexts exist

Entity Framework Core applications can access a `DbContext` directly and implement reads and writes in many different ways.

That flexibility is useful, but it can also result in different developers or modules creating their own persistence patterns:

- direct `DbSet` access;
- repository abstractions;
- custom query services;
- handler-specific persistence logic;
- locally defined transaction patterns;
- different approaches to projections, paging, and bulk operations.

DataArc introduces a consistent command and query contract over registered Entity Framework Core contexts.

Application workflows:

1. obtain a command or query from a DataArc factory;
2. select an execution context explicitly;
3. execute an operation through the DataArc command or query API.

For example:

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

The execution context identifies the database boundary, while `ReadWhere<Employee>` identifies the model and operation being executed.

This keeps developers using the same query and command patterns across an application instead of each feature introducing its own Entity Framework Core read and write implementation.

## Public execution-context contracts

A public execution-context contract can be used when application workflows should depend on an abstraction rather than a concrete Entity Framework Core context.

The contract implements `IExecutionContext`:

```csharp
using DataArc.Core.Abstractions;

public interface IHrDbContext : IExecutionContext
{
   DbSet<Employee>? Employee { get; set; }
}
```

The concrete Entity Framework Core context implements that contract:

```csharp
using Microsoft.EntityFrameworkCore;

internal sealed class HrDbContext : DbContext, IHrDbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(employee => employee.Id);

            entity.Property(employee => employee.Name)
                .HasMaxLength(200)
                .IsRequired();
        });
    }
}
```

Register the public contract and its concrete implementation with:

```csharp
AddDbExecutionContext<IContract, TDbContext>(...)
```

For example:

```csharp
services.ConfigureDataArc(provider =>
{
    provider.UseEntityFrameworkCore(entityFrameworkCore =>
    {
        entityFrameworkCore.AddDbExecutionContext<IHrDbContext, HrDbContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("HrDatabase"));
            });
    });
});
```

Application workflows can then select the public execution boundary without referencing `HrDbContext`:

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhere<Employee>(
        employee => employee.IsActive);
```

This style is useful when:

- the application layer should not reference the concrete `DbContext`;
- the concrete context should remain internal to an infrastructure project;
- the execution boundary represents an application module or domain;
- multiple workflows should share a stable persistence contract;
- Entity Framework Core implementation details should remain behind an abstraction.

The execution-context contract does not need to expose `DbSet<TEntity>` properties. Its purpose is to identify the registered execution boundary used by DataArc.

## Direct concrete DbContext usage

A concrete `DbContext` can also act as the execution context directly.

The context implements `IExecutionContext`:

```csharp
using DataArc.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext
    : DbContext, IExecutionContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
}
```

Register it with:

```csharp
AddDbExecutionContext<TDbContext>(...)
```

For example:

```csharp
services.ConfigureDataArc(provider =>
{
    provider.UseEntityFrameworkCore(entityFrameworkCore =>
    {
        entityFrameworkCore.AddDbExecutionContext<ApplicationDbContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("ApplicationDatabase"));
            });
    });
});
```

The concrete context is then selected directly:

```csharp
var customers = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<ApplicationDbContext>()
    .ReadWhere<Customer>(
        customer => customer.IsActive);
```

This style is useful when:

- the application already references its concrete `DbContext`;
- hiding the context would not create a meaningful architectural boundary;
- the solution is intentionally lightweight;
- a separate execution-context interface would add unnecessary ceremony.

Both registration styles use the same DataArc command and query contracts.

## Explicit execution routing

DataArc does not infer which context should execute a command or query.

The workflow selects the required boundary explicitly with:

```csharp
UseDbExecutionContext<TExecutionContext>()
```

For example:

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAll<Employee>();
```

The execution context and entity type are specified separately:

```csharp
UseDbExecutionContext<IHrDbContext>()
ReadAll<Employee>()
```

The execution-context type answers:

> Which registered database boundary should execute this operation?

The entity type answers:

> Which model should the operation read or modify?

Keeping these concerns separate makes the selected persistence boundary visible while retaining a consistent operation contract.

## Synchronous queries

Create a synchronous query with `CreateQuery()`:

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhere<Employee>(
        employee => employee.Surname == "Doe");
```

The entity type is supplied to the read operation:

```csharp
ReadWhere<Employee>(...)
```

Other synchronous query operations follow the same pattern, such as:

```csharp
var employee = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadOne<Employee>(employeeId);
```

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAll<Employee>();
```

```csharp
var employees = _queryFactory
    .CreateQuery()
    .UseDbExecutionContext<IHrDbContext>()
    .ReadPaged<Employee>(
        startIndex: 0,
        recordCount: 50);
```

## Asynchronous queries

Create an asynchronous query with `CreateQueryAsync()`:

```csharp
var employeesQuery = await _queryFactory.CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<ISolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

The asynchronous factory and operation are both awaited:

```csharp
var query = await _queryFactory.CreateQueryAsync();

var results = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadAllAsync<Employee>();
```

The asynchronous query contract returns materialized results such as:

```csharp
Task<TEntity?>
```

or:

```csharp
Task<IReadOnlyList<TEntity>>
```

depending on the selected operation.

For example:

```csharp
var employee = await employeesQuery
    .UseDbExecutionContext<IHrDbContext>()
    .ReadOneAsync<Employee>(employeeId);
```

```csharp
var employees = await employeesQuery
    .UseDbExecutionContext<IHrDbContext>()
    .ReadPagedAsync<Employee>(
        startIndex: 0,
        recordCount: 50);
```

```csharp
var employees = await employeesQuery
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.IsActive);
```

## Query projections

DataArc query operations support projections without exposing the underlying `IQueryable<TEntity>`.

For example:

```csharp
var employeeSummaries = await employeesQuery
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee, EmployeeSummaryDto>(
        employee => employee.IsActive,
        employee => new EmployeeSummaryDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Salary = employee.Salary
        });
```

The first generic type identifies the persisted entity:

```csharp
Employee
```

The second identifies the projected result:

```csharp
EmployeeSummaryDto
```

The projection remains part of the DataArc query contract and is translated through Entity Framework Core.

This allows workflows to select only the data they require while keeping the query shape consistent across the application.

## Why DataArc does not expose IQueryable directly

The current DataArc query contract executes reads through operations such as:

```csharp
ReadOne<TEntity>(...)
ReadAll<TEntity>()
ReadPaged<TEntity>(...)
ReadWhere<TEntity>(...)
ReadWhere<TEntity, TResult>(...)
```

and their asynchronous equivalents.

DataArc does not currently return an `IQueryable<TEntity>` for application code to compose with arbitrary Entity Framework Core operations.

A direct `IQueryable<TEntity>` API could feel more familiar to Entity Framework Core developers:

```csharp
var customers = await queryFactory
    .CreateQuery()
    .UseDbExecutionContext<ApplicationDbContext, Customer>()
    .Where(customer => customer.IsActive)
    .ToListAsync();
```

However, that is not the current DataArc contract.

The current API deliberately keeps filtering, projection, paging, and execution behind a shared set of command and query operations.

This provides a consistent pattern across teams and modules:

```csharp
UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee>(...)
```

rather than allowing each workflow to define its own persistence composition and terminal execution pattern.

A future version could introduce a more directly composable Entity Framework Core query surface, but applications should use the documented DataArc contracts for the current release.

## Multiple execution contexts

An application can register multiple execution contexts:

```csharp
services.ConfigureDataArc(provider =>
{
    provider.UseEntityFrameworkCore(entityFrameworkCore =>
    {
        entityFrameworkCore
            .AddDbExecutionContext<IHrDbContext, HrDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString("HrDatabase"));
                })
            .AddDbExecutionContext<FinanceDbContext>(
                options =>
                {
                    options.UseSqlServer(
                        configuration.GetConnectionString("FinanceDatabase"));
                });
    });
});
```

Each workflow selects the context required for its operation:

```csharp
var employees = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.IsActive);
```

```csharp
var payrollRecords = await query
    .UseDbExecutionContext<FinanceDbContext>()
    .ReadWhereAsync<PayrollRecord>(
        record => record.EmployeeId == employeeId);
```

The execution-context type acts as:

- the registration key used by DataArc;
- the selected Entity Framework Core boundary;
- an explicit architectural marker in the workflow.

## One database with multiple contexts

Several execution contexts may target the same physical database.

For example, an application may contain separate HR, finance, IT, and operations contexts while storing their tables in one SQL Server database:

```csharp
entityFrameworkCore
    .AddDbExecutionContext<IHrDbContext, HrDbContext>(
        options =>
        {
            options.UseSqlServer(sharedConnectionString);
        })
    .AddDbExecutionContext<IFinanceDbContext, FinanceDbContext>(
        options =>
        {
            options.UseSqlServer(sharedConnectionString);
        })
    .AddDbExecutionContext<IItDbContext, ItDbContext>(
        options =>
        {
            options.UseSqlServer(sharedConnectionString);
        });
```

The physical connection is shared, but workflows continue to select the relevant logical boundary explicitly:

```csharp
var employee = await query
    .UseDbExecutionContext<IHrDbContext>()
    .ReadOneAsync<Employee>(employeeId);
```

```csharp
var payroll = await query
    .UseDbExecutionContext<IFinanceDbContext>()
    .ReadWhereAsync<PayrollRecord>(
        record => record.EmployeeId == employeeId);
```

The shared database does not remove the architectural distinction between the registered contexts.

## Separate databases

Execution contexts may also target separate physical databases:

```csharp
entityFrameworkCore
    .AddDbExecutionContext<GoogleDbContext>(
        options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("GoogleDatabase"));
        })
    .AddDbExecutionContext<MicrosoftDbContext>(
        options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("MicrosoftDatabase"));
        })
    .AddDbExecutionContext<OpenAiDbContext>(
        options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("OpenAiDatabase"));
        });
```

Each operation routes to its intended database:

```csharp
var googleEmployees = await query
    .UseDbExecutionContext<GoogleDbContext>()
    .ReadAllAsync<Employee>();
```

```csharp
var microsoftEmployees = await query
    .UseDbExecutionContext<MicrosoftDbContext>()
    .ReadAllAsync<Employee>();
```

```csharp
var openAiEmployees = await query
    .UseDbExecutionContext<OpenAiDbContext>()
    .ReadAllAsync<Employee>();
```

No global or ambient context selector is required.

The selected execution context is visible directly in the workflow.

## Choosing a registration style

Use a public execution-context contract when the abstraction represents a useful application or module boundary:

```csharp
AddDbExecutionContext<IHrDbContext, HrDbContext>(...)
```

Use the concrete `DbContext` directly when an additional interface would provide little value:

```csharp
AddDbExecutionContext<ApplicationDbContext>(...)
```

Both approaches follow the same execution model:

- the selected type implements `IExecutionContext`;
- the context is registered with DataArc;
- workflows select it explicitly;
- entity types are supplied to command and query operations;
- reads and writes use DataArc’s shared operation contracts.

The choice between an interface and a concrete context is architectural rather than technical.

DataArc does not require applications to hide their concrete `DbContext`. It provides that option while preserving the same command and query patterns for both styles.