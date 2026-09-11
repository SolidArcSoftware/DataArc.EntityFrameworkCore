---
title: When Persistence Requirements Grow
description: Understand the relationship between free DataArc.EntityFrameworkCore bulk APIs and the commercial DataArc Orchestration Framework.
---

# When Persistence Requirements Grow

DataArc.EntityFrameworkCore is the free Entity Framework Core integration point into the wider DataArc platform.

Applications that later require more advanced persistence coordination can move into the commercially licensed **DataArc Orchestration Framework** without abandoning standard Entity Framework Core usage.

## Capabilities available through the Framework

The DataArc Orchestration Framework adds capabilities for applications that require broader coordination, including:

- coordinated execution across multiple `DbContext` instances;
- command and query orchestration;
- transaction-aware multi-context workflows;
- modular persistence boundaries;
- composition of multiple `DbContext` models into relational databases;
- deterministic database and DDL management;
- DataArc execution contexts and runtime coordination.

## Keep ordinary Entity Framework Core

Moving into the Framework does not require abandoning ordinary Entity Framework Core usage.

Applications can continue to use familiar `DbContext`, `IDbContextFactory<TContext>`, LINQ, transactions, and dependency injection patterns while opting into broader DataArc coordination where those capabilities provide value.

## Licensing boundary

The free DataArc.EntityFrameworkCore APIs remain available without purchasing or activating the DataArc Orchestration Framework.

The Framework is licensed separately.
