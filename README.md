# DataArc.EntityFrameworkCore Demo

> **High-performance bulk operations for normal Entity Framework Core applications.**

[![Documentation](https://img.shields.io/badge/docs-DataArc.EntityFrameworkCore-2F81F7)](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)
[![NuGet](https://img.shields.io/badge/NuGet-DataArc.EntityFrameworkCore-004880)](https://www.nuget.org/packages/DataArc.EntityFrameworkCore)
[![Website](https://img.shields.io/badge/website-dataarc.dev-222222)](https://www.dataarc.dev)
[![.NET](https://img.shields.io/badge/.NET-6%20%7C%207%20%7C%208%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

**Use Entity Framework Core normally. Add DataArc where bulk execution is useful.**

This repository is a runnable reference application for the free `DataArc.EntityFrameworkCore` bulk APIs.

The demo deliberately keeps ordinary Entity Framework Core at the center of the application:

- standard `DbContext` implementations;
- standard `IDbContextFactory<TContext>`;
- standard EF Core LINQ queries;
- standard EF Core dependency injection;
- DataArc bulk operations only where high-volume persistence is required.

No DataArc-specific execution context, runtime registration, license key, or activation is required for the bulk APIs demonstrated here.

> The bulk implementation demonstrated by this repository currently targets SQL Server.

---

## Install

```bash
dotnet add package DataArc.EntityFrameworkCore
```

`DataArc.EntityFrameworkCore` supports **.NET 6 through .NET 10** with the corresponding Entity Framework Core versions.

The demo projects target **.NET 10**.

---

## What the demo shows

The demo uses four independent SQL Server databases:

```text
GoogleDbContext      → SAS_GoogleDb
MicrosoftDbContext   → SAS_MicrosoftDb
OpenAIDbContext      → SAS_OpenAiDb
SolidArcDbContext    → SAS_Db
```

The Solid Arc database is the source workload.

At startup the demo:

1. deletes and recreates all four demo databases;
2. creates the schemas and tables using normal Entity Framework Core database creation;
3. seeds one employer into each database;
4. seeds **100,000 employees** into the Solid Arc database using `AddBulkAsync`;
5. reads the source employees using a normal EF Core LINQ query;
6. applies a salary adjustment in application code;
7. maps the application entities back to persistence models;
8. bulk-inserts the adjusted employees into the Google, Microsoft, and OpenAI databases;
9. reads all four databases independently using normal EF Core;
10. consolidates the results in application memory.

There are **no cross-database EF Core joins**.

---

## Repository structure

```text
DataArc.EntityFrameworkCore
│
├── src
│   ├── App
│   │   └── DataArc.EntityFrameworkCore.Demo
│   │       └── Application
│   │           ├── Dtos
│   │           ├── Entities
│   │           ├── Features
│   │           └── Repositories
│   │
│   ├── Host
│   │   └── DataArc.EntityFrameworkCore.Demo.Host
│   │       ├── Repositories
│   │       ├── Services
│   │       └── Workers
│   │
│   └── Infrastructure
│       └── DataArc.EntityFrameworkCore.Demo.Persistence
│           ├── Database
│           │   ├── Creator
│           │   ├── DBContexts
│           │   ├── DBModels
│           │   └── Seeders
│           └── Utils
│
├── docs
├── .github
└── DataArc.EntityFrameworkCore.Demos.sln
```

The application project owns the application-facing entities, DTOs, feature contracts, and repository contracts.

The persistence project owns the EF Core contexts, database models, database creation, and seed data.

The host composes the application and contains the concrete repository and workflow implementations used by the demo.

---

## Normal EF Core registration

Each database context is registered using Microsoft's standard `IDbContextFactory<TContext>` support.

```csharp
services.AddDbContextFactory<GoogleDbContext>(options =>
    options.UseSqlServer(
        configurationManager.GetConnectionString("GoogleDb")));
```

Contexts are created normally:

```csharp
await using var dbContext =
    await _googleDbContextFactory.CreateDbContextAsync();
```

Queries remain ordinary EF Core:

```csharp
var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Rating > rating)
    .Select(employee => new EmployeeEntity
    {
        Id = employee.Id,
        Name = employee.Name,
        Surname = employee.Surname,
        Salary = employee.Salary,
        EmployerId = employee.EmployerId,
        Order = employee.Order,
        IsArchived = employee.IsArchived,
        CreatedUtc = employee.CreatedUtc,
        LastUpdatedUtc = employee.LastUpdatedUtc,
        Notes = employee.Notes,
        Status = employee.Status,
        Rating = employee.Rating
    })
    .ToListAsync();
```

DataArc does not replace the normal EF Core query path.

---

## Bulk insert

Once a collection of persistence entities has been prepared, call `AddBulkAsync` directly on the existing `DbContext`.

```csharp
await using var dbContext =
    await _googleDbContextFactory.CreateDbContextAsync();

await dbContext.AddBulkAsync(
    employees,
    batchSize);
```

The execution path stays small:

```text
IDbContextFactory<TContext>
        ↓
DbContext
        ↓
AddBulkAsync(...)
        ↓
SQL Server
```

No command builder or separate DataArc execution runtime is required.

---

## Keeping persistence models at the persistence boundary

The demo does not expose EF Core database models through its application repository contracts.

Repository implementations project persistence models into application-owned entities:

```text
SQL Server
    ↓
DbContext
    ↓
Persistence model
    ↓ repository projection
EmployeeEntity
    ↓
Application
```

For writes, the repository maps in the opposite direction:

```text
Application
    ↓
EmployeeEntity
    ↓ repository mapping
Persistence model
    ↓
DbContext
    ↓
AddBulkAsync(...)
    ↓
SQL Server
```

This keeps the application contracts independent of the EF Core persistence model while still using ordinary EF Core and DataArc directly inside the concrete repository implementations.

---

## Demo workflow

The salary-adjustment workflow begins with the Solid Arc database:

```text
SolidArcDbContext
        ↓
normal EF Core query
        ↓
EmployeeEntity
        ↓
salary adjustment
        ↓
destination repositories
        ├── GoogleDbContext
        ├── MicrosoftDbContext
        └── OpenAIDbContext
                ↓
          AddBulkAsync(...)
```

The consolidated read path is also ordinary EF Core:

```text
GoogleDbContext ──────┐
MicrosoftDbContext ───┤
OpenAIDbContext ──────┼──> repository projection ──> application consolidation
SolidArcDbContext ────┘
```

Each database is queried independently and the results are combined in application memory.

---

## Example full-volume run

With the demo configured to process all 100,000 source employees, one development run produced:

```text
Top Rated Employee: Name62433 Surname62433, Salary: 157,499.16,
Number of top rated employees: 400,000

Processed 300,000 salary adjustment records in 3084ms
Demo workflow completed in 4415 ms.
```

The result reflects:

```text
100,000 source employees
        ↓
100,000 bulk inserts → Google
100,000 bulk inserts → Microsoft
100,000 bulk inserts → OpenAI
        ↓
300,000 destination records processed
        ↓
100,000 records × 4 databases
        ↓
400,000 consolidated records
```

This is an **example development run, not a formal benchmark**.

Execution time varies with hardware, SQL Server configuration, database state, runtime version, entity shape, batch size, and other environmental factors.

For published performance measurements and methodology, see the [DataArc.EntityFrameworkCore documentation](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/performance.html).

---

## Run the demo

### Requirements

You need:

- .NET 10 SDK;
- SQL Server accessible from the machine running the demo.

### Configure SQL Server

Update the connection strings in:

```text
src/Host/DataArc.EntityFrameworkCore.Demo.Host/appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "SASDb": "Server=YOUR_SERVER;Database=SAS_Db;Integrated Security=true;TrustServerCertificate=True;",
    "GoogleDb": "Server=YOUR_SERVER;Database=SAS_GoogleDb;Integrated Security=true;TrustServerCertificate=True;",
    "MicrosoftDb": "Server=YOUR_SERVER;Database=SAS_MicrosoftDb;Integrated Security=true;TrustServerCertificate=True;",
    "OpenAiDb": "Server=YOUR_SERVER;Database=SAS_OpenAiDb;Integrated Security=true;TrustServerCertificate=True;"
  }
}
```

### Important

> **The demo deletes and recreates all four configured databases every time it starts. Use disposable development databases only.**

### Run

From the repository root:

```bash
dotnet run --project src/Host/DataArc.EntityFrameworkCore.Demo.Host/DataArc.EntityFrameworkCore.Demo.Host.csproj
```

---

## Documentation

Full documentation is available at:

### [Read the DataArc.EntityFrameworkCore documentation →](https://solidarcsoftware.github.io/DataArc.EntityFrameworkCore/)

The documentation covers:

- getting started;
- bulk inserts and deletes;
- transaction participation;
- standard `DbContext` and `IDbContextFactory<TContext>` integration;
- memory-efficient bulk execution;
- licensing;
- the relationship to the commercial DataArc Orchestration Framework.

---

## Microsoft Learn

DataArc.EntityFrameworkCore is listed in the Microsoft Learn Entity Framework Core extensions documentation.

This repository provides the runnable reference application for developers evaluating the package.

---

## Licensing

`DataArc.EntityFrameworkCore` is available free of charge for permitted use under its package license.

The free package does not require a DataArc license key, runtime activation, per-developer fees, runtime fees, or a time-limited trial.

See the NuGet package license for the complete terms.

---

## The core idea

```text
Entity Framework Core
    owns normal application data access

DataArc.EntityFrameworkCore
    adds high-performance bulk persistence where required
```

**Keep normal EF Core. Use DataArc where it earns its place.**
