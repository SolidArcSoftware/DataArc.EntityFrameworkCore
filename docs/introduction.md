# Introduction

DataArc.EntityFrameworkCore 2.0 is designed around one principle:

> **EF Core remains EF Core. DataArc adds execution capabilities where they are useful.**

Applications continue to use normal `DbContext` types, `IDbContextFactory<TContext>`, LINQ queries, EF Core transactions, and standard dependency injection.

DataArc adds bulk and parallel persistence execution without requiring repositories, command/query wrappers, execution-context interfaces, or DataArc-specific `DbContext` abstractions.

The free `DataArc.EntityFrameworkCore` package focuses on general EF Core execution.

The commercial `DataArc.EntityFrameworkCore.SqlServer` package adds SQL Server-specific capabilities such as DataArc-owned transaction execution and multi-context DDL generation.

Continue with [Getting Started](getting-started.md).
