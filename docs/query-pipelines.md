# Queries With EF Core

DataArc.EntityFrameworkCore 2.0 does not require a separate query pipeline.

Reads use normal Entity Framework Core.

## Query through IDbContextFactory

```csharp
await using var dbContext =
    await dbContextFactory.CreateDbContextAsync();

var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Salary > salaryThreshold)
    .ToListAsync();
```

This keeps query behavior familiar:

- LINQ remains LINQ;
- `IQueryable<TEntity>` remains available;
- projections remain normal EF Core projections;
- paging remains normal EF Core paging;
- provider translation remains EF Core's responsibility.

## Projection

```csharp
var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.IsActive)
    .Select(employee => new EmployeeSummary
    {
        Id = employee.Id,
        Name = employee.Name,
        Salary = employee.Salary
    })
    .ToListAsync();
```

## Paging

```csharp
var page = await dbContext.Employee!
    .AsNoTracking()
    .OrderBy(employee => employee.Id)
    .Skip(pageIndex * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

## Reads across several DbContexts

Separate contexts remain separate query boundaries.

Read each context independently and coordinate in application code:

```csharp
await using var hrDbContext =
    await hrDbContextFactory.CreateDbContextAsync();

await using var financeDbContext =
    await financeDbContextFactory.CreateDbContextAsync();

var employee = await hrDbContext.Employee!
    .AsNoTracking()
    .SingleAsync(x => x.Id == employeeId);

var payroll = await financeDbContext.PayrollRecord!
    .AsNoTracking()
    .SingleOrDefaultAsync(x => x.EmployeeId == employeeId);
```

DataArc does not pretend that independent databases are one EF Core query provider.

## One physical database with several contexts

Several `DbContext` models can still participate in one physical relational database.

That does not make one context automatically able to query another context's `DbSet`.

Each context remains an explicit EF Core model boundary.

The commercial DDL Builder can compose those models at the database-definition level.

## When DataArc enters the flow

A common application shape is:

```text
Read with normal EF Core
        |
        v
apply application/domain decisions
        |
        v
write with normal EF Core or DataArc execution
```

Example:

```csharp
var employees = await dbContext.Employee!
    .AsNoTracking()
    .Where(employee => employee.Salary > salaryThreshold)
    .ToListAsync();

foreach (var employee in employees)
{
    employee.Salary +=
        employee.Salary * salaryAdjustmentBaseRate;
}

await destinationDbContext
    .AsParallel()
    .AddBulk(employees, batchSize)
    .SaveChangesParallelAsync();
```

Use EF Core for querying. Add DataArc where persistence execution provides value.
