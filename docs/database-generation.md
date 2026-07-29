# Database and Script Generation

DataArc.EntityFrameworkCore includes database-building support for generating database creation and deletion scripts from one or more EF Core `DbContext` models.

The database builder is created through `IDatabaseFactory`:

```csharp
private readonly IDatabaseFactory _databaseFactory;
```

A database definition is then composed by:

1. creating a database builder;
2. including one or more concrete `DbContext` types;
3. building the database operation;
4. choosing whether scripts should be generated;
5. choosing whether the generated changes should be applied;
6. executing the create or drop operation.

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();

var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);

database.ExecuteCreate();
```

## Why database generation exists

EF Core applications commonly use migrations or `EnsureCreated()` to create database structures.

DataArc's database builder provides another option for applications that need:

- database creation from the current EF Core model;
- database deletion through the same builder model;
- reviewable SQL script output;
- several database contexts handled through a consistent API;
- repeatable database setup for demos, development, testing, or onboarding;
- model-based database generation without requiring migration files.

```text
EF Core DbContext model
    -> DataArc database builder
    -> generated SQL script
    -> optionally apply database changes
```

The generated scripts make the resulting database operations visible and reviewable before or after execution.

## Injecting the database factory

Inject `IDatabaseFactory` into the database creator or persistence component responsible for database generation:

```csharp
private readonly IDatabaseFactory _databaseFactory;

public FinanceDbCreator(
    IDatabaseFactory databaseFactory)
{
    _databaseFactory = databaseFactory;
}
```

The factory provides the entry point for creating a database builder:

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();
```

## Including a DbContext

Use `IncludeDbContext<TDbContext>()` to add a concrete EF Core context model to the database definition:

```csharp
var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);
```

The included context provides the EF Core model used to generate the database structure.

```text
FinanceDbContext
    -> schemas
    -> tables
    -> columns
    -> keys
    -> constraints
```

The database builder uses the concrete `DbContext` type because database generation requires access to its complete EF Core model and provider configuration.

## Including several DbContext models

A database builder can include more than one context when several EF Core models contribute to the same database-generation workflow.

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();

var database = databaseBuilder
    .IncludeDbContext<HrDbContext>()
    .IncludeDbContext<FinanceDbContext>()
    .IncludeDbContext<OperationsDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);
```

Use this only when the included models are intended to participate in the same generated database operation.

Do not combine unrelated contexts merely because they are registered in the same application.

Before including several contexts, confirm:

- whether they target the same physical database;
- whether their schemas and object names are compatible;
- whether duplicate table definitions exist;
- whether the provider configuration is consistent;
- whether the generated script reflects the intended deployment boundary.

For separate databases, build and execute separate database definitions.

## Building the database operation

Call `Build(...)` after all required contexts have been included:

```csharp
var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);
```

The current builder accepts:

```csharp
generateScripts
applyChanges
```

These flags control script output and execution behavior.

## Generating scripts

Set:

```csharp
generateScripts: true
```

to generate SQL scripts for the database operation:

```csharp
var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: false);
```

This is useful when the SQL should be reviewed or deployed separately.

```text
DbContext model
    -> build database operation
    -> generate SQL files
    -> review scripts
    -> apply through deployment process
```

Generated scripts can be inspected for:

- database creation;
- schema creation;
- table creation;
- primary keys;
- foreign keys;
- indexes;
- constraints;
- database deletion statements.

## Applying changes

Set:

```csharp
applyChanges: true
```

when the database operation should execute the generated changes:

```csharp
var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);

database.ExecuteCreate();
```

This configuration both generates the scripts and applies the database creation operation.

For script-only generation:

```csharp
var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: false);

database.ExecuteCreate();
```

The exact runtime behavior should be validated in the target environment before using database generation in an automated production deployment.

## Creating a database

Use `ExecuteCreate()` to execute the create operation:

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();

var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);

database.ExecuteCreate();
```

The operation generates the database structure from the included EF Core model.

Conceptually:

```text
ExecuteCreate()
    |
    +-- create database
    +-- create schemas
    +-- create tables
    +-- create keys and constraints
```

The exact SQL depends on:

- the EF Core provider;
- the configured connection string;
- the included context model;
- entity mappings;
- schema mappings;
- column types;
- key and relationship configuration.

## Deleting a database

Use `ExecuteDrop()` to execute the database deletion operation:

```csharp
var databaseBuilder =
    _databaseFactory.CreateDatabaseBuilder();

var database = databaseBuilder
    .IncludeDbContext<FinanceDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);

database.ExecuteDrop();
```

Database deletion is destructive.

Use it only in an environment where the target database is known and deletion is intentional.

Typical uses include:

- local development;
- integration testing;
- repeatable demo setup;
- disposable test environments.

Do not run automated database deletion against a production connection string.

## Database creator pattern

A project can place database creation behind a focused creator interface.

```csharp
public interface IFinanceDbCreator
{
    bool EnsureCreated();

    bool EnsureDeleted();
}
```

The implementation uses `IDatabaseFactory`:

```csharp
public sealed class FinanceDbCreator :
    IFinanceDbCreator
{
    private readonly IDatabaseFactory _databaseFactory;

    public FinanceDbCreator(
        IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public bool EnsureCreated()
    {
        var databaseBuilder =
            _databaseFactory.CreateDatabaseBuilder();

        var database = databaseBuilder
            .IncludeDbContext<FinanceDbContext>()
            .Build(
                generateScripts: true,
                applyChanges: true);

        database.ExecuteCreate();

        return true;
    }

    public bool EnsureDeleted()
    {
        var databaseBuilder =
            _databaseFactory.CreateDatabaseBuilder();

        var database = databaseBuilder
            .IncludeDbContext<FinanceDbContext>()
            .Build(
                generateScripts: true,
                applyChanges: true);

        database.ExecuteDrop();

        return true;
    }
}
```

This keeps database lifecycle logic inside the persistence layer.

The application or demo host can then depend on the creator contract:

```csharp
_financeDbCreator.EnsureDeleted();
_financeDbCreator.EnsureCreated();
```

## Separate database creators

For applications with several separate databases, use one creator per database boundary.

```text
FinanceDbCreator
HrDbCreator
ItDbCreator
OperationsDbCreator
```

Each creator includes the context associated with its own database:

```csharp
var database = databaseBuilder
    .IncludeDbContext<HrDbContext>()
    .Build(
        generateScripts: true,
        applyChanges: true);
```

This keeps the target database explicit and prevents unrelated database models from being combined accidentally.

## Complete multi-database setup example

```csharp
public sealed class DemoDatabaseInitializer
{
    private readonly IFinanceDbCreator _finance;
    private readonly IHrDbCreator _hr;
    private readonly IItDbCreator _it;
    private readonly IOperationsDbCreator _operations;

    public DemoDatabaseInitializer(
        IFinanceDbCreator finance,
        IHrDbCreator hr,
        IItDbCreator it,
        IOperationsDbCreator operations)
    {
        _finance = finance;
        _hr = hr;
        _it = it;
        _operations = operations;
    }

    public void RecreateDatabases()
    {
        _finance.EnsureDeleted();
        _hr.EnsureDeleted();
        _it.EnsureDeleted();
        _operations.EnsureDeleted();

        _finance.EnsureCreated();
        _hr.EnsureCreated();
        _it.EnsureCreated();
        _operations.EnsureCreated();
    }
}
```

The workflow is explicit:

```text
Delete Finance database
Delete HR database
Delete IT database
Delete Operations database

Create Finance database
Create HR database
Create IT database
Create Operations database
```

This pattern is suitable for disposable demo or integration-test environments.

## Generated script location

Generated SQL scripts are written beneath the application output directory in a `Scripts` folder.

Example:

```text
bin/Debug/net8.0/Scripts
```

The target framework segment depends on the application being built.

For a .NET 10 application, the path may resemble:

```text
bin/Debug/net10.0/Scripts
```

The precise output location also depends on the selected build configuration and host project.

Typical configurations include:

```text
bin/Debug/<target-framework>/Scripts
bin/Release/<target-framework>/Scripts
```

## Example generated SQL

A generated creation script can contain SQL such as:

```sql
CREATE DATABASE [FinanceDb];
GO

USE [FinanceDb];
GO

IF SCHEMA_ID('employees') IS NULL
    EXEC('CREATE SCHEMA [employees]');
GO

CREATE TABLE [employees].[Employees] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [EmployeeName] varchar(50) NULL,
    [EmployeeSalary] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Employees]
        PRIMARY KEY ([Id])
);
GO
```

The actual generated SQL is determined by the EF Core model and provider.

Do not copy this example as a replacement for the generated output of the application's own context model.

## Script review

Generated scripts should be reviewed before production use.

Check:

- the target database name;
- schema names;
- table names;
- column types and lengths;
- decimal precision and scale;
- nullable columns;
- primary keys;
- foreign keys;
- cascade-delete behavior;
- indexes;
- unique constraints;
- destructive statements;
- provider-specific SQL.

A script can be technically valid while still representing an incorrect model configuration.

Database generation reflects the model it receives.

It does not decide whether that model is correct for the business.

## Database generation and migrations

DataArc database generation does not require EF Core migration files for the demonstrated create-and-drop workflow.

```text
Current DbContext model
    -> generate complete database script
```

EF Core migrations solve a different problem:

```text
Previous schema version
    -> ordered migration changes
    -> new schema version
```

Use model-based database generation when:

- creating a new database;
- recreating a disposable environment;
- producing a complete baseline script;
- running demos or integration tests;
- onboarding a new empty database from the current model.

Use migrations when:

- upgrading an existing database that contains data;
- applying incremental schema changes;
- preserving a schema-change history;
- supporting controlled version-to-version deployment.

Do not drop and recreate a production database merely to avoid migrations.

## Baseline generation

A generated create script can be used as a reviewable baseline for a new environment.

```text
Current model
    -> generate create script
    -> review
    -> approve
    -> deploy to empty database
```

This can be useful when a database administrator requires SQL scripts rather than direct application-driven database creation.

In that case, use:

```csharp
.Build(
    generateScripts: true,
    applyChanges: false);
```

and deploy the reviewed script through the organisation's database-release process.

## Development and test environments

Automatic create-and-drop workflows are useful in disposable environments:

```csharp
database.ExecuteDrop();
database.ExecuteCreate();
```

Suitable environments include:

- developer workstations;
- local demos;
- automated integration tests;
- short-lived test databases;
- ephemeral CI environments.

Use environment checks before allowing destructive operations:

```csharp
if (!hostEnvironment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Database recreation is allowed only in Development.");
}
```

A stronger implementation can also validate the target database name before deletion.

## Production safety

Database generation should be handled conservatively in production.

Recommended controls include:

- generate scripts without applying them;
- review scripts through source control or release tooling;
- require database-owner approval;
- back up existing databases;
- verify the target connection string;
- run under a least-privileged account;
- separate create permissions from normal application permissions;
- prohibit automated `ExecuteDrop()` calls;
- record script hashes and deployment results.

A production application account usually should not have permission to create or delete databases.

## Provider-specific behavior

The generated SQL depends on the configured EF Core provider.

For example:

```csharp
options.UseSqlServer(connectionString);
```

produces SQL Server-specific output.

Other providers may differ in:

- database creation support;
- schema support;
- identifier quoting;
- data types;
- identity columns;
- computed columns;
- default constraints;
- index syntax;
- deletion behavior.

Validate generated scripts against the exact provider and database version used by the target environment.

## Model configuration affects generated scripts

The builder generates scripts from the EF Core model.

For example:

```csharp
modelBuilder
    .Entity<Employee>()
    .Property(employee => employee.EmployeeSalary)
    .HasPrecision(18, 2);
```

affects the generated column definition.

Likewise:

```csharp
modelBuilder
    .Entity<Employee>()
    .ToTable(
        "Employees",
        "employees");
```

affects the generated schema and table names.

Review EF Core model warnings before trusting generated output.

Warnings about unspecified decimal precision, required relationships, or provider compatibility can indicate that the generated schema will not match the intended design.

## Registration requirements

The concrete contexts used by the database builder must have valid EF Core configuration.

For example:

```csharp
services.ConfigureDataArc(dataArc =>
{
    dataArc.UseEntityFrameworkCore(ef =>
    {
        ef.AddDbExecutionContext<
            IFinanceDbContext,
            FinanceDbContext>(
                options =>
                    options.UseSqlServer(
                        financeConnectionString));
    });
});
```

The database builder uses the configured context model and provider.

Verify that:

- the connection string targets the intended database;
- the provider package is installed;
- the context constructor accepts the correct options;
- the context model builds successfully;
- required services are registered.

## Recommended creator implementation

A focused creator keeps the operation clear:

```csharp
public sealed class FinanceDbCreator :
    IFinanceDbCreator
{
    private readonly IDatabaseFactory _databaseFactory;

    public FinanceDbCreator(
        IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public bool EnsureCreated()
    {
        var database = _databaseFactory
            .CreateDatabaseBuilder()
            .IncludeDbContext<FinanceDbContext>()
            .Build(
                generateScripts: true,
                applyChanges: true);

        database.ExecuteCreate();

        return true;
    }

    public bool EnsureDeleted()
    {
        var database = _databaseFactory
            .CreateDatabaseBuilder()
            .IncludeDbContext<FinanceDbContext>()
            .Build(
                generateScripts: true,
                applyChanges: true);

        database.ExecuteDrop();

        return true;
    }
}
```

The application can create one implementation for each separate database boundary.

## Choosing the correct mode

Generate and apply changes:

```csharp
.Build(
    generateScripts: true,
    applyChanges: true);
```

Use this for controlled disposable environments where automatic creation is intended.

Generate scripts without applying changes:

```csharp
.Build(
    generateScripts: true,
    applyChanges: false);
```

Use this when scripts must be reviewed or deployed separately.

Apply changes without retaining scripts:

```csharp
.Build(
    generateScripts: false,
    applyChanges: true);
```

Use this only when direct application is intentional and reviewable script output is not required.

The preferred production posture is normally script generation and review rather than granting the running application broad database-creation permissions.

## Summary

The DataArc database-generation flow is:

```text
IDatabaseFactory
    -> CreateDatabaseBuilder()
    -> IncludeDbContext<TDbContext>()
    -> Build(
           generateScripts,
           applyChanges)
    -> ExecuteCreate()
       or ExecuteDrop()
```

The central principles are:

- database structure is generated from the current EF Core model;
- one or more concrete contexts can be included;
- scripts can be generated without immediately applying changes;
- `ExecuteCreate()` creates the generated database structure;
- `ExecuteDrop()` performs destructive database deletion;
- separate databases should normally have separate creator implementations;
- generated scripts are written beneath the application's `Scripts` output folder;
- complete model generation is not a replacement for incremental production migrations;
- destructive operations should be restricted to disposable environments;
- generated SQL must be reviewed against the intended provider and schema.

## Next steps

Continue with:

- [Execution Contexts](execution-contexts.md)
- [Transactional Workflows](transactions.md)
- [Trial and Licensing](licensing-and-trial.md)
