# Agent Instructions

This file provides instructions for AI agents working on this C# repository.

## Project Overview

- **Language**: C#
- **Build Tool**: .NET SDK (dotnet CLI)
- **Package Manager**: NuGet
- **Testing**: xunit (with Moq for mocking)
- **CI**: GitHub Actions
- **SDK target frameworks**: `netstandard1.3`, `netstandard2.0`
- **Test target frameworks**: `netcoreapp3.1`, `net5.0`, `net6.0`, `net8.0`

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
