# Publishing Guide for MathMax.Analyzers.UnusedSymbols

This guide provides step-by-step instructions for publishing the analyzer to NuGet.

## Prerequisites

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org)
2. **API Key**: Generate an API key from your NuGet.org account settings
3. **.NET SDK**: Ensure you have .NET SDK installed (6.0 or later)

## Quick Publishing

### Option 1: Using the Build Script

The project includes a PowerShell script that automates the entire process:

```powershell
# Build, pack, and publish in one command
.\build-and-pack.ps1 -Push -ApiKey "your-api-key-here"

# Or set environment variable and run
$env:NUGET_API_KEY = "your-api-key-here"
.\build-and-pack.ps1 -Push
```

### Option 2: Manual Steps

1. **Clean and Build**:
   ```powershell
   dotnet clean
   dotnet build --configuration Release
   ```

2. **Create Package**:
   ```powershell
   dotnet pack src\MathMax.Analyzers.UnusedSymbols\MathMax.Analyzers.UnusedSymbols.csproj --configuration Release --output .\nupkg
   ```

3. **Validate Package** (optional):
   ```powershell
   .\validate-package.ps1
   ```

4. **Publish to NuGet**:
   ```powershell
   dotnet nuget push .\nupkg\MathMax.Analyzers.UnusedSymbols.0.1.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
   ```

## Version Management

The project uses the following versioning approach:

- **Package Version**: Set in `MathMax.Analyzers.UnusedSymbols.csproj`
- **Assembly Version**: Managed in `Directory.Build.props`
- **Release Notes**: Update `PackageReleaseNotes` in the project file

### Updating Version for New Release

1. Update version in `Directory.Build.props`:
   ```xml
   <Version>1.1.0</Version>
   ```

2. Update package version in project file:
   ```xml
   <PackageVersion>1.1.0</PackageVersion>
   ```

3. Update release notes:
   ```xml
   <PackageReleaseNotes>Version 1.1.0 - Added new features, bug fixes</PackageReleaseNotes>
   ```

4. Move items from `AnalyzerReleases.Unshipped.md` to `AnalyzerReleases.Shipped.md`

## Package Validation

Before publishing, always validate the package:

```powershell
.\validate-package.ps1 .\nupkg\MathMax.Analyzers.UnusedSymbols.0.1.0.nupkg
```

This checks:
- ✅ Analyzer DLL is in correct location (`analyzers\dotnet\cs\`)
- ✅ Package metadata is complete
- ✅ Required files are included

## Package Contents

A properly packaged analyzer should contain:

```
analyzers/
  dotnet/
    cs/
      MathMax.Analyzers.UnusedSymbols.dll
      MathMax.Analyzers.UnusedSymbols.pdb
tools/
  install.ps1
  uninstall.ps1
MathMax.Analyzers.UnusedSymbols.nuspec
```

## Testing the Package

### Local Testing

1. Create a test project:
   ```powershell
   mkdir TestProject
   cd TestProject
   dotnet new console
   ```

2. Add the local package:
   ```xml
   <PackageReference Include="MathMax.Analyzers.UnusedSymbols" Version="0.1.0">
     <PrivateAssets>all</PrivateAssets>
     <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
   </PackageReference>
   ```

3. Test with unused code:
   ```csharp
   public class Program
   {
       private static int _unusedField; // Should trigger USG001
       
       public static void Main(string[] args)
       {
           Console.WriteLine("Hello World!");
       }
   }
   ```

4. Build and verify warnings:
   ```powershell
   dotnet build
   ```

## Troubleshooting

### Common Issues

1. **No warnings appear**: Check that the analyzer is correctly referenced and the package contains the DLL in the right location.

2. **Build errors**: Ensure all dependencies are compatible and the target framework is correct.

3. **Package validation fails**: Run the validation script to identify missing files or incorrect structure.

### Getting Help

- Check the [Roslyn Analyzer documentation](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- Review NuGet packaging guidelines for analyzers
- Examine other successful analyzer packages for reference

## CI/CD Integration

For automated publishing, set up GitHub Actions or Azure DevOps with:

1. Store API key as a secret
2. Use the build script in your pipeline:
   ```yaml
   - name: Build and Publish
     run: .\build-and-pack.ps1 -Push -ApiKey ${{ secrets.NUGET_API_KEY }}
   ```

## Security Considerations

- ⚠️ Never commit API keys to source control
- Use environment variables or secure secret storage
- Consider using GitHub Actions secrets or Azure Key Vault
- Regularly rotate API keys

---

**Happy Publishing! 🚀**