# Trial and Licensing

DataArc.EntityFrameworkCore is commercially licensed software.

The package can be evaluated through a limited trial before a paid license is activated.

This page explains the intended licensing model at a practical level. The applicable license agreement and current commercial terms published by Solid Arc Software remain the authoritative source.

## Trial period

A DataArc trial is valid for seven days.

The trial is intended to allow a developer or organisation to:

- install the package;
- configure execution contexts;
- run the demonstration solution;
- test query and command pipelines;
- evaluate bulk and multi-context workflows;
- determine whether the framework fits the target application.

The trial is for evaluation.

It should not be treated as a permanent free production license.

```text
Install package
    -> activate trial
    -> evaluate for seven days
    -> purchase and activate a paid license
       or remove the commercial package
```

## Paid licensing

A paid DataArc.EntityFrameworkCore license permits continued use after the trial period, subject to the purchased license terms.

The current product, pricing, and licensing options should be obtained from the DataArc website because commercial terms can change independently of the documentation version.

Use the product page for:

- current price;
- included rights;
- license duration;
- organisation or developer scope;
- supported deployment environments;
- purchasing and activation instructions.

Do not rely on a price copied into source code or old documentation as the current commercial offer.

## Package installation does not grant a paid license

Installing the NuGet package does not, by itself, grant unrestricted commercial use.

```text
NuGet package available
    !=
paid commercial license granted
```

The package distribution mechanism and the commercial license are separate concerns.

A valid trial or paid license is required according to the applicable product terms.

## Trial activation

Trial activation associates the evaluation with the relevant runtime or machine information required by the licensing process.

The exact activation flow is provided through the DataArc portal and package runtime.

Depending on the product version, activation data may include machine or runtime identifiers such as network-adapter information.

Only provide activation information through the official DataArc website or the documented package activation process.

Do not submit licensing or payment information to unverified third-party domains.

## Runtime environment

DataArc supports explicit runtime-environment identification through:

```text
DataArcRuntimeEnvironment
```

This value can be supplied as an environment variable where required by the deployed application.

Example:

```powershell
$env:DataArcRuntimeEnvironment = "Development"
```

For a deployed environment:

```powershell
$env:DataArcRuntimeEnvironment = "Production"
```

Use an environment name that matches the application's actual deployment context.

Typical values may include:

- `Development`
- `QA`
- `Staging`
- `Production`

The exact value should follow the environment configuration used by the organisation and the licensing instructions for the installed package version.

## Local development

A developer evaluating or using DataArc locally should:

1. install the required NuGet packages;
2. configure the supported execution contexts;
3. activate the trial or paid license;
4. set the runtime environment where required;
5. start the application;
6. confirm that license validation succeeds before continuing development.

Keep licensing configuration outside source control when it contains private activation information.

Do not commit:

- private activation keys;
- machine-specific license files;
- purchase details;
- secrets;
- private portal credentials.

## Configuration and secrets

Licensing values that behave like credentials should be stored through normal secure configuration mechanisms.

Suitable options include:

- environment variables;
- .NET user secrets for local development;
- a managed secret store;
- deployment-platform secret configuration;
- protected server configuration.

For local .NET development:

```powershell
dotnet user-secrets set "DataArc:LicenseKey" "YOUR-LICENSE-VALUE"
```

The exact configuration key must match the licensing instructions for the installed product version.

The example demonstrates secure storage, not a guaranteed package key name.

Do not invent configuration names in production code. Use the names provided by the product activation instructions.

## Containers

Paid DataArc licenses can be used in containerised deployments subject to the purchased license terms and runtime configuration.

Container environments require deliberate configuration because container instances may be:

- recreated;
- scaled horizontally;
- assigned different network identities;
- deployed across several hosts;
- started by an orchestrator.

Do not assume that a machine-bound development activation can simply be copied into an arbitrary container fleet.

Before deployment, confirm:

- that the purchased license permits container use;
- how runtime identification is configured;
- how many environments or instances are covered;
- where activation values are stored;
- how secrets are injected;
- how scaling and replacement instances are handled.

## Container configuration

Supply required licensing configuration through the container runtime rather than baking private values into the image.

Example:

```yaml
services:
  api:
    image: your-application:latest
    environment:
      DataArcRuntimeEnvironment: Production
```

Private licensing values should be provided through the platform's secret mechanism.

Avoid:

```dockerfile
ENV DataArcLicenseKey=private-value
```

inside a committed Dockerfile.

A secret embedded in an image can be recovered from image layers and copied into unauthorised environments.

## CI and build servers

A CI build that restores and compiles the package is different from a deployed runtime that executes licensed functionality.

Keep build pipelines free of unnecessary production license values.

Where tests execute licensed runtime behavior, provide only the configuration required for the authorised test environment.

Use separate configuration for:

- developer workstations;
- CI tests;
- QA;
- staging;
- production.

Do not reuse one private activation value across every environment unless the purchased license explicitly permits it.

## Production deployment

Before deploying to production, verify:

- the trial has not expired;
- the correct paid product has been purchased;
- the production runtime is covered by the license;
- the runtime environment is correctly identified;
- required activation data is available;
- private values are stored securely;
- container or scale-out rights are understood;
- operational teams know how to respond to a license-validation failure.

A production release should not depend on an evaluation trial.

## License validation failures

When license validation fails, investigate the configuration rather than repeatedly reinstalling packages.

Check:

- whether the trial expired;
- whether the correct product license was activated;
- whether the runtime environment is configured;
- whether machine or runtime identity changed;
- whether a container was recreated without required configuration;
- whether the license is valid for the current deployment;
- whether private configuration values are available to the process;
- whether the system clock is correct.

Do not disable or bypass license validation.

If the configuration appears correct, use the official support or contact channel published by Solid Arc Software.

## Product boundaries

DataArc.EntityFrameworkCore and DataArc.Orchestration.Framework are distinct products.

A license for one product should not be assumed to grant rights to the other unless the purchased commercial terms explicitly state that it does.

```text
DataArc.EntityFrameworkCore license
    -> covers the EF Core product according to its terms

DataArc.Orchestration.Framework license
    -> covers the orchestration product according to its terms
```

Confirm the product selected during purchase and activation.

## Open source repository and commercial licensing

The public repository allows developers to inspect the demonstration, documentation, and available source published by Solid Arc Software.

Public visibility does not automatically make every component permissively licensed.

```text
Publicly visible source
    !=
MIT license
    !=
unrestricted commercial use
```

Always read the repository license and commercial terms before:

- copying source;
- modifying package internals;
- redistributing binaries;
- embedding the product in another commercial package;
- publishing a fork;
- offering hosted access to the licensed software.

## Redistribution

Do not redistribute DataArc packages, activation material, license files, or private licensed source unless the applicable agreement explicitly permits it.

A customer application may reference the package according to its license.

That does not necessarily grant the customer the right to:

- republish the package under a different name;
- include the package in a public package feed;
- share activation keys;
- distribute license files;
- resell the framework itself.

## Evaluation checklist

During the seven-day trial, evaluate the product against a real technical scenario.

A useful evaluation should confirm:

- execution-context registration works with the target provider;
- application code can target the required context boundaries;
- query pipelines support the required reads;
- command pipelines support the required writes;
- bulk operations behave correctly with realistic data;
- parallel execution is suitable for independent contexts;
- transaction boundaries are understood;
- structured results provide enough operational feedback;
- performance is measured using representative workloads;
- the architecture is simpler or safer than the existing alternative.

A trial should answer a concrete adoption question.

It should not be consumed only by installing the package and reading the API surface.

## Purchasing decision

A purchase is justified when DataArc materially improves the target system through:

- clearer execution-context boundaries;
- consistent multi-context routing;
- reduced custom command and query infrastructure;
- explicit bulk and parallel execution;
- improved workflow visibility;
- reduced architecture-remediation effort;
- lower risk in complex EF Core operations.

Do not purchase the framework merely because the application uses EF Core.

Many simple applications do not need an additional execution layer.

DataArc is intended for systems where explicit execution control provides enough value to justify the dependency and license cost.

## Current terms and support

For authoritative and current information, use the official DataArc resources:

- DataArc product page;
- DataArc pricing page;
- the license included with the repository or package;
- the official purchase and activation workflow;
- Solid Arc Software's published contact channel.

When this documentation conflicts with the current signed license agreement or official commercial terms, the agreement and official terms take precedence.

## Summary

The licensing model is:

```text
Seven-day evaluation trial
    -> evaluate technical fit

Paid license
    -> continued authorised use
    -> deployment rights according to purchased terms
```

Remember:

- package installation is not the same as a paid license;
- a trial is for evaluation and expires after seven days;
- paid licensing is required for continued use;
- container use must match the purchased terms;
- runtime environment configuration may be required;
- private activation information must be stored securely;
- DataArc.EntityFrameworkCore and DataArc.Orchestration.Framework are separate products;
- public source visibility does not imply unrestricted redistribution;
- the current official license and commercial terms are authoritative.
