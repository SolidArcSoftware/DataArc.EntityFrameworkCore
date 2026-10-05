# Licensing

DataArc 2.0 separates free EF Core execution capabilities from commercial SQL Server-specific capabilities.

For current pricing, commercial terms, purchases, and activation support, use the [DataArc website](https://www.dataarc.dev).

The applicable license agreement and current official terms remain authoritative.

## Free package

`DataArc.EntityFrameworkCore` can be used without a DataArc runtime license.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore" Version="2.0.0" />
```

Register the free path with:

```csharp
services.AddDataArcCore();
```

Free capabilities documented in this guide include:

- direct bulk insertion;
- caller-owned transactional bulk execution;
- `AsParallel()`;
- fluent `Add`, `AddRange`, `Update`, `Remove`, and `AddBulk`;
- `SaveChangesParallelAsync()`.

## Commercial SQL Server package

`DataArc.EntityFrameworkCore.SqlServer` contains SQL Server-specific commercial capabilities.

```xml
<PackageReference Include="DataArc.EntityFrameworkCore.SqlServer" Version="2.0.0" />
```

Examples include:

- DataArc-owned SQL transaction execution;
- `AsParallelTransaction()`;
- `CommitTransactionParallelAsync()`;
- multi-context database coordination;
- relational database-definition tooling;
- the multi-context DDL Builder.

## License configuration

A commercial application can configure an explicit key:

```csharp
services
    .AddDataArcCore(options =>
        options.UseKey(licenseKey))
    .ConfigureDataArc();
```

Where the deployment uses the server-key path:

```csharp
services
    .AddDataArcCore(options =>
        options.UseServerKey())
    .ConfigureDataArc();
```

A common test/bootstrap shape is:

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

Keep license values outside source control.

Use normal secret-management mechanisms such as environment variables, .NET user secrets, or the deployment platform's secret store.

## NuGet availability and entitlement

Package availability and commercial entitlement are separate concerns.

```text
NuGet package available
    !=
commercial entitlement granted
```

Installing `DataArc.EntityFrameworkCore.SqlServer` does not remove the requirement to satisfy the applicable commercial license terms.

## Trial and evaluation

Where a trial or evaluation path is offered, use the current DataArc portal and product terms for the applicable duration, activation rules, and deployment restrictions.

Trial terms can change independently of a documentation release, so this guide does not hard-code a trial duration.

## Containers, CI/CD, and deployment

Use the current product terms for supported container, CI/CD, and server deployment scenarios.

Do not commit:

- private license keys;
- activation material;
- server credentials;
- portal credentials;
- machine-specific secrets.

## Package boundaries

The free and commercial package split is intentional:

```text
DataArc.EntityFrameworkCore
    -> free general EF Core execution

DataArc.EntityFrameworkCore.SqlServer
    -> commercial SQL Server-specific execution and DDL tooling
```

The SQL Server package depends on the free package.

## Support

For current product and licensing information:

https://www.dataarc.dev

For support:

support@dataarc.dev
