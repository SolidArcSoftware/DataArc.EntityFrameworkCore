# Getting Started

## Prerequisites

You need:

- a supported .NET SDK;
- the matching Entity Framework Core major version;
- an EF Core database provider;
- `DataArc.EntityFrameworkCore` 2.0.0.

The public demo targets .NET 10 and SQL Server.

See [Compatibility](compatibility.md) for the supported framework matrix.

## Install the free package

```bash
dotnet add package DataArc.EntityFrameworkCore --version 2.0.0
```

For SQL Server applications, install the matching EF Core provider:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

## Register DataArc once

Register DataArc at the application composition root:

```csharp
services.AddDataArcCore();
```

Do not repeat the application-level DataArc bootstrap inside feature or module registration methods.

## Register DbContext factories normally

DataArc uses normal EF Core contexts.

For a console or worker application:

```csharp
services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(
        configuration.GetConnectionString("ApplicationDb")));
```

For an ASP.NET Core application where factory-created contexts participate in DataArc execution that resolves scoped infrastructure, register the factory as scoped:

```csharp
services.AddDbContextFactory<ApplicationDbContext>(
    options =>
        options.UseSqlServer(
            configuration.GetConnectionString("ApplicationDb")),
    ServiceLifetime.Scoped);
```

`AddDbContextFactory<TContext>()` registers the factory as a singleton by default. A factory-created `DbContext` is caller-owned and is not the same thing as a request-scoped `DbContext` resolved directly from DI.

## Query with ordinary EF Core

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();

var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Salary > salaryThreshold)
    .ToListAsync();
```

No DataArc query abstraction is required.

## Execute a direct bulk operation

```csharp
await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

## Execute a parallel persistence plan

```csharp
await dbContext
    .AsParallel()
    .AddBulk(employers, batchSize)
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

The terminal operation returns the affected-row count:

```csharp
var affected = await dbContext
    .AsParallel()
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

## Coordinate several DbContexts

Independent contexts can be coordinated with normal .NET concurrency:

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

var totalAffected = results.Sum();
```

Each `DbContext` remains an independent EF Core boundary.

## Commercial SQL Server capabilities

Install the SQL Server package when the application needs DataArc-owned SQL transactions or database-definition tooling:

```bash
dotnet add package DataArc.EntityFrameworkCore.SqlServer --version 2.0.0
```

Then configure the commercial DataArc path according to the installed license:

```csharp
services
    .AddDataArcCore(options =>
    {
        if (string.IsNullOrWhiteSpace(licenseKey))
            options.UseServerKey();
        else
            options.UseKey(licenseKey);
    })
    .ConfigureDataArc();
```

See:

- [Transactions](transactions.md)
- [Multi-Context DDL Builder](database-generation.md)
- [Licensing](licensing-and-trial.md)
