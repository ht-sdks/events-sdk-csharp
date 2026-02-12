# Agent Instructions

This file provides instructions for AI agents working on this C# repository.

## Project Overview

- **Language**: C#
- **Build Tool**: .NET SDK (dotnet CLI)
- **Package Manager**: NuGet
- **Testing**: xunit (with Moq for mocking)
- **CI**: GitHub Actions

### Project Structure

```
events-sdk-csharp/                # Main SDK library (published to NuGet as Hightouch.Events.CSharp)
  Hightouch/Events/               # Core SDK source code
    Analytics.cs                  # Main analytics client
    Configuration.cs              # Client configuration
    Events.cs                     # Event types
    Plugins.cs                    # Plugin system
    Types.cs                      # Shared types
    Plugins/                      # Built-in plugins (ContextPlugin, HightouchDestination, etc.)
    Policies/                     # Flush policies (Count, Frequency, Startup)
    Utilities/                    # HTTP client, storage, logging, event pipeline
    Serialization/                # JSON serialization (System.Text.Json / Newtonsoft)
    Concurrent/                   # Threading and dispatch utilities
    Compat/                       # Migration helpers from Analytics.NET / Xamarin
    Sovran/                       # State management (store, actions, subscribers)

Tests/                            # xunit test project
  AnalyticsTest.cs
  EventsTest.cs
  PluginsTest.cs
  StateTest.cs
  Utilities/                      # Utility tests (storage, logging, HTTP, etc.)
  Plugins/                        # Plugin tests
  Policies/                       # Flush policy tests
  Compat/                         # Migration tests
  Utils/                          # Test stubs and helpers

Samples/                          # Example projects
  ConsoleSample/                  # Basic console usage
  AspNetSample/                   # ASP.NET with dependency injection
  AspNetMvcSample/                # ASP.NET MVC with dependency injection
  UnitySample/                    # Unity integration (custom HTTP client, lifecycle)
  XamarinSample/                  # Xamarin (iOS, Android)
```

### Target Frameworks

- **SDK** (`events-sdk-csharp`): `netstandard1.3` and `netstandard2.0`
- **Tests**: `netcoreapp3.1`, `net5.0`, `net6.0`, `net8.0`
- **Samples**: `net6.0`

### JSON Serialization

The SDK uses different JSON libraries depending on target framework:
- `netstandard1.3`: Newtonsoft.Json (`Newtonsoft.Json` 13.0.1)
- `netstandard2.0`: System.Text.Json (`System.Text.Json` 8.0.4)

Conditional compilation (`#if`) is used in serialization code. Be mindful of this when editing serialization-related files.

---

## Updating Dependencies

### 1. Pre-flight Checks

```bash
# Check .NET SDK version
dotnet --version

# List all installed SDKs (CI uses 3.1.x, 5.0.x, 6.0.x, 8.0.x)
dotnet --list-sdks

# Ensure you're at the repository root
pwd  # Should be: /path/to/events-sdk-csharp
```

### 2. Establish Test Baseline

```bash
# Restore all NuGet packages
dotnet restore

# Build the entire solution
dotnet build

# Run all tests
dotnet test
```

Record the number of passing tests before making any changes. The test suite currently has **139 tests** across four target frameworks (netcoreapp3.1, net5.0, net6.0, net8.0). This ensures you can verify nothing broke after upgrading.

### 3. Check for Security Advisories

```bash
# List known vulnerabilities in dependencies
dotnet list package --vulnerable
```

Review any vulnerabilities. Note: The SDK currently has a known advisory on `System.Text.Json` 8.0.4 (GHSA-8g4q-xg66-9fp4).

### 4. Check Outdated Packages

```bash
# Check all outdated packages across the solution
dotnet list package --outdated
```

This shows:
- **Requested**: Version specified in `.csproj`
- **Resolved**: Currently installed version
- **Latest**: Newest available version

To check specific projects:

```bash
# SDK project
dotnet list events-sdk-csharp/events-sdk-csharp.csproj package --outdated

# Test project
dotnet list Tests/Tests.csproj package --outdated
```

### 5. Upgrade Dependencies

#### Option A: Update a Specific Package

```bash
# Update a package in a specific project
dotnet add events-sdk-csharp/events-sdk-csharp.csproj package <PackageName>

# Update a test dependency
dotnet add Tests/Tests.csproj package <PackageName>
```

This will update the package to its latest version. To pin a specific version:

```bash
dotnet add events-sdk-csharp/events-sdk-csharp.csproj package <PackageName> --version <Version>
```

#### Option B: Edit `.csproj` Files Directly

For more control, edit the `<PackageReference>` entries in the `.csproj` files directly:

- `events-sdk-csharp/events-sdk-csharp.csproj` — SDK dependencies
- `Tests/Tests.csproj` — Test dependencies

Then restore:

```bash
dotnet restore
```

#### Option C: Use dotnet-outdated Tool (recommended)

```bash
# Install globally (one-time)
dotnet tool install --global dotnet-outdated-tool

# Check what can be updated
dotnet-outdated

# Auto-upgrade all packages to latest
dotnet-outdated --upgrade
```

### 6. Rebuild and Test

```bash
# Clean previous build artifacts
dotnet clean

# Rebuild the entire solution
dotnet build

# Run all tests
dotnet test
```

Compare test results to the baseline. Fix any failures before proceeding.

### 7. Verify CI Would Pass

The CI workflow (`.github/workflows/ci.yml`) runs on `ubuntu-latest` with .NET SDKs 3.1.x, 5.0.x, 6.0.x, and 8.0.x. The steps are:

```bash
dotnet build
dotnet test
```

If you have multiple SDKs installed, ensure tests pass across all target frameworks:

```bash
# Run tests for a specific framework
dotnet test --framework net6.0
dotnet test --framework net8.0
dotnet test --framework net5.0
dotnet test --framework netcoreapp3.1
```

---

## Key Dependencies

### SDK (`events-sdk-csharp/events-sdk-csharp.csproj`)

| Package | Version | Condition | Purpose |
|---------|---------|-----------|---------|
| `Newtonsoft.Json` | 13.0.1 | `netstandard1.3` only | JSON serialization for older frameworks |
| `System.Text.Json` | 8.0.4 | `netstandard2.0` only | JSON serialization for modern frameworks |

### Tests (`Tests/Tests.csproj`)

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.NET.Test.Sdk` | 16.11.0 | Test SDK host |
| `Microsoft.TestPlatform.ObjectModel` | 17.3.0 | Test platform model |
| `Moq` | 4.10.1 | Mocking framework |
| `xunit` | 2.4.2 | Test framework |
| `xunit.runner.visualstudio` | 2.4.3 | Test runner |
| `coverlet.collector` | 3.1.0 | Code coverage |
| `System.Runtime.Serialization.Formatters` | 4.3.0 | Serialization support |

---

## Version Bumping

### Semantic Versioning

- **PATCH** (0.0.6 → 0.0.7): Bug fixes, dependency updates, no new features
- **MINOR** (0.0.6 → 0.1.0): New backwards-compatible features
- **MAJOR** (0.0.6 → 1.0.0): Breaking API changes

Dependency updates are typically **PATCH** bumps.

### Files to Update

1. `events-sdk-csharp/events-sdk-csharp.csproj` → `<Version>` element
2. The release process validates the git tag matches the `<Version>` in the `.csproj`

---

## Publishing to NuGet

Publishing is automated via GitHub Actions (`.github/workflows/release.yml`).

### Release Process

1. Update the `<Version>` in `events-sdk-csharp/events-sdk-csharp.csproj`
2. Create a GitHub Release with a tag matching the version (e.g., `0.0.7`)
3. GitHub Actions will automatically:
   - Verify the tag matches the package version
   - Run `dotnet pack`
   - Publish to NuGet via `dotnet nuget push`

---

## CI/CD

- **CI config**: `.github/workflows/ci.yml`
- **Release config**: `.github/workflows/release.yml`
- **CI triggers**: Push to `main`, pull requests to `main`, manual dispatch
- **CI steps**: Install OpenSSL 1.1, `dotnet build`, `dotnet test`
- **.NET versions in CI**: 3.1.x, 5.0.x, 6.0.x, 8.0.x

### OpenSSL 1.1 Requirement

The CI installs OpenSSL 1.1 (`libssl1.1`) because `ubuntu-latest` (Ubuntu 24.04+) only ships OpenSSL 3.x. .NET 3.1 and 5.0 require OpenSSL 1.x at runtime, so the CI downloads it from the Ubuntu 20.04 archive. If the EOL target frameworks (`netcoreapp3.1`, `net5.0`) are ever removed from the test matrix, this step can be dropped.

### CI Failures After Dependency Updates

1. **Build errors**: Check for API changes in updated packages. Pay special attention to the conditional JSON serialization (Newtonsoft vs System.Text.Json).
2. **Test failures**: Review changelogs of updated packages for breaking changes.
3. **Framework compatibility**: The SDK targets `netstandard1.3` and `netstandard2.0`. Ensure updated packages still support these frameworks.

---

## Common Issues

### JSON Serialization Differences

The SDK uses conditional compilation for JSON handling:
- `netstandard1.3` → Newtonsoft.Json
- `netstandard2.0` → System.Text.Json

When updating either JSON library, ensure both code paths still work. The serialization code lives in:
- `events-sdk-csharp/Hightouch/Events/Serialization/JsonConvertersForNewton.cs`
- `events-sdk-csharp/Hightouch/Events/Serialization/JsonConvertersForMS.cs`
- `events-sdk-csharp/Hightouch/Events/Serialization/JsonUtility.cs`

### Target Framework Compatibility

When upgrading NuGet packages, verify they still support `netstandard1.3`. This is the oldest target and most likely to lose support in newer package versions. If a package drops `netstandard1.3` support, you may need to:
- Pin the older version for that target framework using conditional `<PackageReference>` items
- Or consider dropping `netstandard1.3` support (breaking change)

### Obsolete API Warnings

The codebase has intentional `[Obsolete]` attributes on migration helpers (`Compat/Migration.cs`). Build warnings from these are expected and not a sign of problems.

### End-of-Life Frameworks in Tests

The test project targets `netcoreapp3.1` and `net5.0`, which are end-of-life. These may encounter issues with newer system libraries or packages. If updating test infrastructure packages, ensure they still support these older frameworks or consider removing them from the test matrix.

---

## Quick Reference

| Task | Command |
|------|---------|
| Restore packages | `dotnet restore` |
| Build solution | `dotnet build` |
| Run all tests | `dotnet test` |
| Run tests (specific framework) | `dotnet test --framework net8.0` |
| Clean build artifacts | `dotnet clean` |
| Check outdated packages | `dotnet list package --outdated` |
| Check vulnerabilities | `dotnet list package --vulnerable` |
| Update a package | `dotnet add <project> package <name>` |
| Pack for NuGet | `dotnet pack` (in `events-sdk-csharp/`) |
| Run specific test class | `dotnet test --filter "FullyQualifiedName~AnalyticsTest"` |
| Run specific test | `dotnet test --filter "FullyQualifiedName~AnalyticsTest.TestMethod"` |
