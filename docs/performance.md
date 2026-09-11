---
title: Performance
description: DataArc.EntityFrameworkCore bulk execution performance and managed-memory allocation characteristics.
---

# Performance

DataArc.EntityFrameworkCore is designed for high-throughput SQL Server bulk execution with low and predictable managed-memory allocation.

Memory efficiency is particularly important for bulk workloads running inside:

- APIs
- Background workers
- Integration services
- Import processes
- Synchronization workloads
- Batch processing
- Long-lived application services

## AddBulkAsync benchmark

| Records | Mean | Allocated Memory |
| ------: | ---: | ---------------: |
| 5,000 | **14.46 ms** | **1.26 MB** |
| 10,000 | **56.64 ms** | **2.45 MB** |
| 25,000 | **73.62 ms** | **5.99 MB** |
| 50,000 | **257.65 ms** | **11.93 MB** |
| 100,000 | **453.30 ms** | **23.73 MB** |

At 100,000 entities, the benchmark completed in approximately **453 ms** while allocating **23.73 MB of managed memory**.

## Benchmark configuration

The published benchmark snapshot was produced using BenchmarkDotNet with:

- `InvocationCount=1`
- `IterationCount=10`
- `UnrollFactor=1`

Actual performance varies depending on hardware, database configuration, entity structure, runtime version, connection conditions, and execution environment.

Benchmark results should be evaluated as measured characteristics of the tested environment rather than universal performance guarantees.
