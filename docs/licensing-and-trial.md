# Trial and Licensing

DataArc.EntityFrameworkCore is commercially licensed software.

For current pricing, licensing terms, purchases, and activation support, visit the [DataArc portal](https://www.dataarc.dev).

A valid trial or paid product license is required to activate the product. Activation is handled through the DataArc licensing API.

This page describes the activation behavior implemented by the current licensing service. The applicable license agreement and current commercial terms published by Solid Arc Software remain authoritative.

## Trial period

The current DataArc trial period is **14 days**.

The trial is intended to let a developer or organisation evaluate DataArc against a real application workload before purchasing a paid license.

Typical evaluation areas include:

- execution-context registration;
- query and command pipelines;
- bulk operations;
- parallel execution;
- structured execution results;
- database and script generation;
- compatibility with the target EF Core provider and application architecture.

```text
Install DataArc
    -> request trial activation
    -> receive a signed activation result
    -> evaluate for 14 days
    -> purchase a paid license for continued use
```

The trial is for evaluation and is not a permanent free production license.

## How activation works

The runtime submits an activation request to the DataArc licensing API. This information is transmitted to and processed by the DataArc licensing API for license administration and activation management.

## Important notice: machine data collection

DataArc collects machine information when a trial or paid license is activated.

The activation request may include:

- machine identifiers, including available network-adapter identifiers;
- machine name;
- operating-system user name;
- domain name;
- product and license information required to process the activation.

This information is used to:

- associate the activation with the applicable machine;
- identify an existing activation for that machine;
- enforce purchased activation quantities;
- prevent unauthorised or duplicate activation;
- administer the trial or paid product license.

## Paid activation

A paid activation uses the license key issued through the DataArc purchase and licensing process.
The package available on NuGet and the commercial entitlement are separate concerns:

```text
NuGet package availability
    !=
commercial product entitlement
```
Installing a package does not by itself grant a paid license.

## Containers and CI/CD

Trial licenses are not supported for container or CI/CD activation paths.

The licensing service returns an error when a trial activation is attempted in a container or CI/CD scenario that requires a paid activation.

```text
Container or CI/CD activation
    + trial license
        -> activation rejected
        -> paid license required
```

A paid license key is required for supported container or CI/CD activation scenarios.

## Public source and commercial licensing

The public DataArc repositories and demonstration code make the product easier to inspect and evaluate.

Public visibility does not automatically make the commercial packages permissively licensed.

```text
Public repository
    !=
MIT license
    !=
unrestricted redistribution
```
Read the applicable repository license and commercial agreement before:

- redistributing DataArc packages;
- sharing license keys or activation responses;
- republishing package binaries;
- modifying or reselling licensed components;
- embedding licensed components in another distributable product.

## Current terms and support

Use the official DataArc resources for current information about:

- product pricing;
- package entitlements;
- license duration;
- purchase terms;
- supported deployment scenarios;
- activation support.

When this documentation conflicts with the signed commercial agreement or current official terms, the agreement and official terms take precedence.

## Summary

The current licensing flow is:

```text
Trial
    -> activation request
    -> 14-day product activation
    -> signed activation result

Paid license
    -> license key resolved by licensing service
    -> product and package entitlements loaded
    -> machine identifiers matched or recorded
    -> signed activation result

Container or CI/CD
    -> trial activation not supported
    -> paid license required
```
