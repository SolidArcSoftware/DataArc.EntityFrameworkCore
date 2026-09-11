---
title: DataArc.EntityFrameworkCore
description: Free high-performance, memory-efficient SQL Server bulk operations for Entity Framework Core on .NET 6 through .NET 10.
---

# DataArc.EntityFrameworkCore

**High-performance bulk operations for Entity Framework Core without replacing Entity Framework Core.**

DataArc.EntityFrameworkCore extends standard Entity Framework Core `DbContext` usage with high-performance, memory-efficient SQL Server bulk operations designed for data-intensive .NET workloads.

Applications continue to use normal Entity Framework Core APIs, dependency injection, transactions, and `IDbContextFactory<TContext>`.

Supports **.NET 6 through .NET 10**.

## Free to use

**DataArc.EntityFrameworkCore is available free of charge.**

Use DataArc.EntityFrameworkCore in personal, commercial, government, and enterprise .NET applications without purchasing a DataArc license, registering a license key, or activating the DataArc runtime.

There is no time-limited trial.

## Core capabilities

DataArc.EntityFrameworkCore provides:

- High-performance SQL Server bulk inserts
- High-performance SQL Server bulk deletes
- Memory-efficient bulk execution with low and predictable managed-memory allocation
- Transaction-aware bulk inserts and deletes
- Participation in existing Entity Framework Core transactions
- Direct operation on standard `DbContext` instances
- Compatibility with `IDbContextFactory<TContext>`
- Optional database schema selection
- Existing Entity Framework Core connection and metadata usage
- No DataArc-specific dependency injection required
- No DataArc execution context required
- No DataArc license key
- No runtime activation
- No time-limited trial

## Start here

- [Getting started](getting-started.md)
- [Bulk operations](bulk-operations.md)
- [Transactions](transactions.md)
- [DbContext integration](dbcontext-integration.md)
- [Performance](performance.md)
- [Licensing](licensing.md)
- [When persistence requirements grow](orchestration-framework.md)
