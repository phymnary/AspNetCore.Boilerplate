# Phymnary.SugarPot.AspNetCore.Host

Host bootstrapping utilities for SugarPot ASP.NET Core applications.

This package currently provides a focused configuration extension to keep startup behavior consistent across services.

## What this package provides

- `ConfigurationExtensions.AddDefaults(IConfigurationBuilder builder, string env)`

This extension adds configuration sources in the following order:

1. `appsettings.json` (required)
2. `appsettings.{env}.json` (optional)
3. `appsettings.{env}.user.json` (optional)
4. Environment variables

Because later providers override earlier ones, environment variables remain the final override layer.

## Installation

```bash
dotnet add package Phymnary.SugarPot.AspNetCore.Host
```

## Usage

### WebApplication

```csharp
using Phymnary.SugarPot.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddDefaults(builder.Environment.EnvironmentName);
```

### Generic Host

```csharp
using Microsoft.Extensions.Hosting;
using Phymnary.SugarPot.AspNetCore.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddDefaults(builder.Environment.EnvironmentName);
```

## Why use it

- Standardizes configuration loading across services
- Supports environment-specific and user-local override files
- Preserves common ASP.NET Core environment variable override behavior

## Related packages

- Phymnary.SugarPot.AspNetCore.Api
- Phymnary.SugarPot.AspNetCore.EntityFrameworkCore

## Target frameworks

Target frameworks are managed by project and solution build configuration.

## Contributing

Issues and pull requests are welcome.

## License

See the repository root for license details.
