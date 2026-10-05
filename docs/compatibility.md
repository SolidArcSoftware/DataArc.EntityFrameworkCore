# Compatibility

DataArc.EntityFrameworkCore 2.0 supports .NET and Entity Framework Core versions 6 through 10.

## Supported framework versions

| .NET target | Entity Framework Core |
|---|---|
| .NET 6 | EF Core 6 |
| .NET 7 | EF Core 7 |
| .NET 8 | EF Core 8 |
| .NET 9 | EF Core 9 |
| .NET 10 | EF Core 10 |

Keep the application's EF Core major version aligned with its target framework and provider packages.

## Package targets

DataArc packages contain target-specific assemblies for supported frameworks.

A single-targeted project can use:

```xml
<TargetFramework>net10.0</TargetFramework>
```

A multi-targeted library can use:

```xml
<TargetFrameworks>net6.0;net7.0;net8.0;net9.0;net10.0</TargetFrameworks>
```

The .NET SDK selects the matching DataArc asset during restore.

## Entity Framework Core alignment

A .NET 10 application should use EF Core 10 packages:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.*" />
```

Avoid mixing incompatible EF Core major versions across:

- `Microsoft.EntityFrameworkCore`;
- the selected provider;
- EF Core design-time packages;
- EF Core tooling.

## Database providers

`DataArc.EntityFrameworkCore` works with normal EF Core `DbContext` registration.

The public demos use Microsoft SQL Server.

Provider-specific behavior remains the responsibility of EF Core and the selected provider.

The commercial `DataArc.EntityFrameworkCore.SqlServer` package is specifically for SQL Server capabilities such as DataArc-owned transactions and relational DDL generation.

## IDbContextFactory lifetime

`AddDbContextFactory<TContext>()` registers `IDbContextFactory<TContext>` as a singleton by default.

Contexts created by the factory are independent caller-owned instances:

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();
```

They are not the same thing as a `DbContext` instance resolved directly from a request scope.

When an ASP.NET Core application uses factory-created contexts with DataArc scoped infrastructure, register the factory as scoped:

```csharp
services.AddDbContextFactory<ApplicationDbContext>(
    options =>
        options.UseSqlServer(connectionString),
    ServiceLifetime.Scoped);
```

This keeps the factory-created context and DataArc infrastructure aligned with the active application scope.

## Demo target framework

The current public demonstration projects target .NET 10.

The packages themselves continue to support .NET 6 through .NET 10.

## DataArc package versioning

Keep related DataArc packages on the same release version.

Example:

```xml
<PackageReference Include="DataArc.EntityFrameworkCore" Version="2.0.0" />
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

`DataArc.EntityFrameworkCore.SqlServer` already carries `DataArc.EntityFrameworkCore` as a dependency. An explicit free-package reference is still valid when the consuming project directly uses that package's API.

Avoid mixing different DataArc release versions in the same dependency graph.
