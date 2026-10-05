# Multi-Context DDL Builder

The relational DDL Builder is provided by `DataArc.EntityFrameworkCore.SqlServer`.

It is a commercial SQL Server-specific capability.

## Why it exists

A modular application may use several `DbContext` models while still targeting one physical relational database.

```text
HrDbContext
FinanceDbContext
ItDbContext
OperationsDbContext
        |
        +----> one SQL Server database
```

A `DbContext` boundary does not have to become a relational boundary.

The DDL Builder composes participating EF Core models into a database definition that can be scripted or applied.

## Install the SQL Server package

```xml
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

The SQL Server package carries the free `DataArc.EntityFrameworkCore` package as a dependency.

## Configure DataArc

Commercial SQL Server capabilities require the applicable license configuration:

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

## Inject the database factory

```csharp
private readonly IDatabaseFactory _databaseFactory;

public DatabaseSetup(
    IDatabaseFactory databaseFactory)
{
    _databaseFactory = databaseFactory;
}
```

## Compose several DbContexts

The Orchestration Framework demo integration test composes four contexts:

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();

var demoDatabase = databaseBuilder
    .IncludeDbContext<HrDbContext>()
    .IncludeDbContext<ItDbContext>()
    .IncludeDbContext<OperationsDbContext>()
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);
```

The participating contexts remain independently usable EF Core models.

The builder composes their relational definitions for the physical database.

## Drop and recreate

The demo integration test can recreate the composed database without EF migrations:

```csharp
demoDatabase.ExecuteDrop();
demoDatabase.ExecuteCreate();
```

This is useful for:

- integration tests;
- disposable development databases;
- database bootstrap scenarios;
- generated-script review.

Do not point destructive database-creation tests at production data.

## Schemas and relational integrity

Schemas can express module ownership and organization inside one physical database.

They are not automatically security boundaries.

Where the relational model requires it, the composed database can still preserve database-level concepts such as:

- tables;
- primary keys;
- foreign keys;
- constraints;
- cross-context relational relationships.

The application can therefore keep modular `DbContext` boundaries without giving up a normalized relational database design.

## Multiple physical databases

DataArc can also support applications with several relational databases and separate connection strings.

Each physical database remains its own transaction and relational boundary.

Do not interpret multi-context composition as a distributed database transaction.

## Migrations

The DDL Builder is an alternative database-definition path for scenarios where generated relational DDL is useful.

It does not require every application to abandon EF Core migrations.

Choose the database lifecycle model that fits the application.

## Demo coverage

The `DataArc.EntityFrameworkCore` demo intentionally uses four independent databases and therefore does not force the DDL Builder into an artificial scenario.

The DataArc Orchestration Framework demo is the reference multi-context DDL example because HR, Finance, IT, and Operations naturally share one physical relational database while keeping separate `DbContext` models.
