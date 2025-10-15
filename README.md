# MathMax.Analyzers.UnusedSymbols

[![NuGet](https://img.shields.io/nuget/v/MathMax.Analyzers.UnusedSymbols.svg)](https://www.nuget.org/packages/MathMax.Analyzers.UnusedSymbols)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MathMax.Analyzers.UnusedSymbols.svg)](https://www.nuget.org/packages/MathMax.Analyzers.UnusedSymbols)

A Roslyn analyzer that detects unused symbols (types, methods, properties, fields, events) in C# code. This analyzer helps maintain clean codebases by identifying declared symbols that are not referenced anywhere in the analyzed compilation.

## Features

- **Comprehensive Symbol Detection**: Identifies unused types, methods, properties, fields, and events
- **Smart Exclusions**: Automatically excludes common externally-invoked symbols like:
  - MVC and Web API controllers
  - Public entry points
  - Symbols with special attributes
- **SOLID Design**: Built with clean architecture principles for maintainability and testability
- **Performance Optimized**: Uses concurrent execution for fast analysis

## Installation

### 🔧 Option 1 — Install via NuGet package

Install the NuGet package in your project:

```xml
<PackageReference Include="MathMax.Analyzers.UnusedSymbols" Version="0.1.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

### 📦 Option 2 — Install via Package Manager Console

```powershell
Install-Package MathMax.Analyzers.UnusedSymbols
```

### 🧩 Option 3 — Use it as a global analyzer (no code changes)

If you don't want to modify .csproj files, you can add the analyzer as a global analyzer via MSBuild properties.

In your Directory.Build.props (in the root of your solution):

```xml
<Project>
  <ItemGroup>
    <Analyzer Include="..\Path\To\UnusedSymbolsAnalyzer.dll" />
  </ItemGroup>
</Project>
```

Then simply:

```powershell
dotnet build
```

## Diagnostic Rules

| Rule ID | Category | Severity | Description |
|---------|----------|----------|-------------|
| USG001  | Usage    | Warning  | Symbol appears to be unused |

## Configuration

The analyzer can be configured using an `.editorconfig` file:

```ini
# Enable/disable the analyzer
dotnet_diagnostic.USG001.severity = warning

# Or suppress for specific files
[{bin,obj}/**/*.cs]
dotnet_diagnostic.USG001.severity = none
```

## Examples

The analyzer will flag unused symbols like:

```csharp
public class MyClass
{
    private int _unusedField; // USG001: '_unusedField' is declared but appears to be unused
    
    private void UnusedMethod() // USG001: 'UnusedMethod' is declared but appears to be unused
    {
        // Implementation
    }
    
    public void UsedMethod() // No warning - this method is used
    {
        // Implementation
    }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.