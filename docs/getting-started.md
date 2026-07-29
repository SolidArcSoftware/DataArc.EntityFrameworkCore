# Getting Started

This guide introduces the basic DataArc.EntityFrameworkCore execution model.

The setup is:

```text
Install DataArc.EntityFrameworkCore
    ↓
Create or expose an EF Core execution context
    ↓
Register the execution context
    ↓
Inject a command or query factory
    ↓
Select the execution context
    ↓
Execute the operation
```

## Prerequisites

You need:

- a supported .NET SDK
- a supported Entity Framework Core version
- an EF Core database provider
- a database connection string
- the DataArc.EntityFrameworkCore NuGet package

The public demo uses SQL Server.

See [Compatibility](compatibility.md) for supported versions.

## Install the package

Install DataArc.EntityFrameworkCore in the project that configures persistence:

```bash
dotnet add package DataArc.EntityFrameworkCore
```

Install the EF Core provider required by the application.

For SQL Server:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

## Choose an execution-context style

DataArc supports two valid styles:

1. a public execution-context contract implemented by an internal concrete `DbContext`
2. a concrete `DbContext` that implements `IExecutionContext` directly

Use a contract when application code should not depend on the concrete persistence implementation.

Use the concrete context directly when hiding it adds no value.

## Option 1: Execution-context contract

Create a public contract that extends `IExecutionContext`:

```csharp
using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Contracts
{
    public interface IGoogleDbContext : IExecutionContext
    {
        DbSet<Employer>? Employer { get; set; }

        DbSet<Employee>? Employee { get; set; }
    }
}
```

Implement the contract with the concrete EF Core `DbContext`:

```csharp
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts
{
    internal class GoogleDbContext
        : DbContext,
          IGoogleDbContext
    {
        public GoogleDbContext(
            DbContextOptions<GoogleDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<Employer>? Employer { get; set; }

        public DbSet<Employee>? Employee { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Employer>()
                .Property(employer => employer.Description)
                .HasColumnType("text");
        }
    }
}
```

Application workflows target the contract:

```csharp
.UseDbExecutionContext<IGoogleDbContext>()
```

## Option 2: Direct concrete DbContext

A concrete context can implement `IExecutionContext` directly:

```csharp
using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;

using Microsoft.EntityFrameworkCore;

namespace DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts
{
    public class GoogleDbContext
        : DbContext,
          IExecutionContext
    {
        public GoogleDbContext(
            DbContextOptions<GoogleDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<Employer>? Employer { get; set; }

        public DbSet<Employee>? Employee { get; set; }

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Employer>()
                .Property(employer => employer.Description)
                .HasColumnType("text");
        }
    }
}
```

Application workflows target the concrete context:

```csharp
.UseDbExecutionContext<GoogleDbContext>()
```

## Register DataArc

Call `AddDataArcCore` once at the application root.

Register EF Core execution contexts through `ConfigureDataArc`.

### Contract-based registration

```csharp
services.AddDataArcCore();

services.ConfigureDataArc(dataArc =>
{
    dataArc.UseEntityFrameworkCore(ef =>
    {
        ef.AddDbExecutionContext<
            IGoogleDbContext,
            GoogleDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "GoogleDb"));
        });
    });
});
```

### Direct DbContext registration

```csharp
services.AddDataArcCore();

services.ConfigureDataArc(dataArc =>
{
    dataArc.UseEntityFrameworkCore(ef =>
    {
        ef.AddDbExecutionContext<GoogleDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString(
                    "GoogleDb"));
        });
    });
});
```

## Execute a query

Inject `IQueryFactory` into an application service or repository:

```csharp
private readonly IQueryFactory _queryFactory;

public SalaryAdjustmentService(
    IQueryFactory queryFactory)
{
    _queryFactory = queryFactory;
}
```

Create a query and select the execution boundary explicitly:

```csharp
var employeesQuery = await _queryFactory.CreateQueryAsync();

var employees = await employeesQuery
    .UseDbExecutionContext<SolidArcDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Salary > salaryThreshold);
```

A contract-based context can be selected in the same way:

```csharp
var employees = await employeesQuery
    .UseDbExecutionContext<IGoogleDbContext>()
    .ReadWhereAsync<Employee>(
        employee => employee.Rating > 4.5);
```

## Execute a command

Inject `ICommandFactory` into the application workflow:

```csharp
private readonly ICommandFactory _commandFactory;

public SalaryAdjustmentService(
    ICommandFactory commandFactory)
{
    _commandFactory = commandFactory;
}
```

Create a command builder and add work to an execution context:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<GoogleDbContext>()
    .AddBulk(employees, batchSize);
```

Build and execute the command:

```csharp
var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

## Execute work across several contexts

A single command pipeline can target several registered execution contexts:

```csharp
var commandBuilder = await _commandFactory
    .CreateCommandBuilderAsync();

commandBuilder
    .UseDbExecutionContext<GoogleDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<MicrosoftDbContext>()
    .AddBulk(employees, batchSize);

commandBuilder
    .UseDbExecutionContext<OpenAIDbContext>()
    .AddBulk(employees, batchSize);

var command = await commandBuilder.BuildAsync();

var commandResult = await command.ExecuteAsync();
```

Each target remains explicit and independently routed.

## Check the result

```csharp
if (!commandResult.Success)
{
    throw new InvalidOperationException(
        $"Execution failed. {commandResult.Exception?.Message}");
}

return commandResult.TotalAffected;
```

## Next steps

Continue with:

- [Execution Contexts](execution-contexts.md)
- [Query Pipelines](query-pipelines.md)
- [Command Pipelines](command-pipelines.md)
- [Bulk and Parallel Operations](bulk-and-parallel-operations.md)
- [Structured Execution Results](structured-results.md)