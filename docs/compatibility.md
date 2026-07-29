# Compatibility

DataArc.EntityFrameworkCore is designed to support modern .NET and Entity Framework Core applications across multiple framework versions.

The current release multi-targets .NET and Entity Framework Core versions 6 through 10.

## Supported framework versions

| .NET target | Entity Framework Core |
|---|---|
| .NET 6 | EF Core 6 |
| .NET 7 | EF Core 7 |
| .NET 8 | EF Core 8 |
| .NET 9 | EF Core 9 |
| .NET 10 | EF Core 10 |

Applications should use the DataArc target that matches their own .NET and Entity Framework Core version.

For example:

```xml
<TargetFramework>net8.0</TargetFramework>
```

should use the .NET 8-compatible DataArc assemblies and EF Core 8 packages.

A .NET 10 application should similarly use EF Core 10:

```xml
<TargetFramework>net10.0</TargetFramework>
```

## Multi-targeted applications

DataArc.EntityFrameworkCore packages contain target-specific assemblies for supported .NET versions.

A project may target one framework:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

or multiple frameworks:

```xml
<PropertyGroup>
  <TargetFrameworks>net6.0;net7.0;net8.0;net9.0;net10.0</TargetFrameworks>
</PropertyGroup>
```

When a project is multi-targeted, the .NET SDK selects the appropriate DataArc assembly for each target framework during restore and build.

## Running a multi-targeted project

When a project targets several frameworks, specify the framework when running it:

```bash
dotnet run --framework net8.0
```

or:

```bash
dotnet run -f net10.0
```

This is especially important when the machine only has one of the targeted SDKs or runtimes installed.

For example, a project targeting .NET 6 through .NET 10 may not run without an explicit framework selection on a machine that only has the .NET 10 runtime available:

```bash
dotnet run -f net10.0
```

The same applies to other commands:

```bash
dotnet build -f net10.0
```

```bash
dotnet test -f net10.0
```

```bash
dotnet publish -c Release -f net10.0
```

## Entity Framework Core version alignment

The application's Entity Framework Core packages should align with its target framework and with the DataArc assembly selected for that target.

For example, a .NET 8 project would typically reference EF Core 8 packages:

```xml
<ItemGroup>
  <PackageReference
    Include="Microsoft.EntityFrameworkCore"
    Version="8.*" />

  <PackageReference
    Include="Microsoft.EntityFrameworkCore.SqlServer"
    Version="8.*" />
</ItemGroup>
```

A .NET 10 project would use the corresponding EF Core 10 packages:

```xml
<ItemGroup>
  <PackageReference
    Include="Microsoft.EntityFrameworkCore"
    Version="10.*" />

  <PackageReference
    Include="Microsoft.EntityFrameworkCore.SqlServer"
    Version="10.*" />
</ItemGroup>
```

Avoid mixing incompatible major versions of:

- the target framework;
- Entity Framework Core;
- the selected database provider;
- Entity Framework Core tooling.

For example, an EF Core 8 runtime package should normally be used with the EF Core 8 SQL Server provider and EF Core 8 design package.

## Database providers

The public DataArc.EntityFrameworkCore demo uses Microsoft SQL Server:

```csharp
services.ConfigureDataArc(provider =>
{
    provider.UseEntityFrameworkCore(entityFrameworkCore =>
    {
        entityFrameworkCore.AddDbExecutionContext<ApplicationDbContext>(
            options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString(
                        "ApplicationDatabase"));
            });
    });
});
```

DataArc registers Entity Framework Core execution contexts through standard `DbContextOptions` configuration.

The database provider is configured in the same registration callback used by a normal Entity Framework Core application:

```csharp
options.UseSqlServer(connectionString);
```

Provider-specific behavior remains the responsibility of Entity Framework Core and the selected provider.

Capabilities such as:

- SQL generation;
- transaction behavior;
- stored procedure support;
- batching;
- parameter handling;
- database type mapping;
- provider-specific limitations;

may differ between providers and versions.

The current public examples and compatibility guidance are based on SQL Server.

## SQL Server

The public demo uses:

```xml
<PackageReference
  Include="Microsoft.EntityFrameworkCore.SqlServer"
  Version="..." />
```

and configures contexts with:

```csharp
options.UseSqlServer(connectionString);
```

DataArc features demonstrated with SQL Server include:

- command pipelines;
- query pipelines;
- bulk and parallel operations;
- transactional workflows;
- multiple execution contexts;
- database generation;
- script generation;
- stored procedure execution.

Applications should test provider-specific behavior against the database and EF Core version they intend to use.

## EF Core tools

Entity Framework Core command-line tools should normally match the major version of the EF Core packages used by the project.

Check the installed tool version with:

```bash
dotnet ef --version
```

Install or update a version that matches the project:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

or:

```bash
dotnet tool update --global dotnet-ef --version 10.*
```

Projects that use design-time EF Core operations may also require:

```xml
<PackageReference
  Include="Microsoft.EntityFrameworkCore.Design"
  Version="10.*">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>
    runtime;
    build;
    native;
    contentfiles;
    analyzers;
    buildtransitive
  </IncludeAssets>
</PackageReference>
```

Use the version that matches the project's EF Core major version.

## C# language versions

The available C# language version is determined by the selected .NET SDK and project configuration.

DataArc does not require application code to use the newest available C# syntax.

Execution-context contracts can use a conventional declaration:

```csharp
public interface IApplicationDbContext : IExecutionContext
{
}
```

Concrete contexts can also use standard syntax supported by the target framework:

```csharp
public sealed class ApplicationDbContext
    : DbContext, IExecutionContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

Applications may use newer C# syntax where supported by their selected SDK.

## Operating systems

DataArc.EntityFrameworkCore runs within the supported .NET and Entity Framework Core runtime environments.

The application must also satisfy the requirements of:

- the selected .NET runtime;
- the selected Entity Framework Core provider;
- the target database;
- any native or operating-system dependencies introduced by the application.

The public demo can be built and run through the standard .NET CLI:

```bash
dotnet restore
dotnet build
dotnet run -f net10.0
```

Database connectivity remains dependent on the configured provider, connection string, network access, authentication method, and database server.

## Dependency versioning

Keep related DataArc packages on the same release version.

For example:

```xml
<ItemGroup>
  <PackageReference
    Include="DataArc.Core"
    Version="1.0.0" />

  <PackageReference
    Include="DataArc.EntityFrameworkCore"
    Version="1.0.0" />

  <PackageReference
    Include="DataArc.EntityFrameworkCore.Abstractions"
    Version="1.0.0" />
</ItemGroup>
```

Mixing different DataArc package versions may introduce incompatible contracts or runtime behavior.

When upgrading, update the related DataArc packages together and then rebuild the application.

## Current compatibility scope

The current release is intended for:

- .NET 6 through .NET 10;
- EF Core 6 through EF Core 10;
- single-targeted or multi-targeted applications;
- public execution-context contracts;
- direct concrete `DbContext` execution contexts;
- SQL Server-based applications and demos;
- applications with one or multiple registered execution contexts;
- contexts targeting one shared database or separate databases.

The public demo is the reference implementation for the currently documented setup.

Where provider or framework behavior differs, the behavior of the selected .NET runtime, Entity Framework Core version, and database provider should be treated as authoritative.