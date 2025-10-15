# Validate NuGet Package Script
param(
    [string]$PackagePath = ".\nupkg\MathMax.Analyzers.UnusedSymbols.0.1.0.nupkg"
)

$ErrorActionPreference = "Stop"

Write-Host "🔍 Validating NuGet package: $PackagePath" -ForegroundColor Green

if (-not (Test-Path $PackagePath)) {
    Write-Error "❌ Package not found: $PackagePath"
    exit 1
}

# Create temp directory for extraction
$TempDir = Join-Path $env:TEMP "nuget-validate-$(Get-Random)"
New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

try {
    Write-Host "📦 Extracting package contents..." -ForegroundColor Yellow
    
    # Extract the package (it's just a zip file)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($PackagePath, $TempDir)
    
    Write-Host "📁 Package contents:" -ForegroundColor Yellow
    Get-ChildItem -Path $TempDir -Recurse | ForEach-Object {
        $RelativePath = $_.FullName.Replace($TempDir, "").TrimStart('\')
        if ($_.PSIsContainer) {
            Write-Host "   📁 $RelativePath/" -ForegroundColor Cyan
        } else {
            $Size = [math]::Round($_.Length / 1KB, 2)
            Write-Host "   📄 $RelativePath ($Size KB)" -ForegroundColor White
        }
    }
    
    # Check for required analyzer files
    $AnalyzerPath = Join-Path $TempDir "analyzers\dotnet\cs"
    if (Test-Path $AnalyzerPath) {
        Write-Host "✅ Analyzer assemblies found in correct location" -ForegroundColor Green
        Get-ChildItem -Path $AnalyzerPath -Filter "*.dll" | ForEach-Object {
            Write-Host "   🔧 $($_.Name)" -ForegroundColor Green
        }
    } else {
        Write-Warning "⚠️  No analyzer assemblies found in expected location (analyzers\dotnet\cs)"
    }
    
    # Check for tools
    $ToolsPath = Join-Path $TempDir "tools"
    if (Test-Path $ToolsPath) {
        Write-Host "✅ Tools directory found" -ForegroundColor Green
        Get-ChildItem -Path $ToolsPath | ForEach-Object {
            Write-Host "   🛠️  $($_.Name)" -ForegroundColor Green
        }
    }
    
    # Check nuspec file
    $NuspecFile = Get-ChildItem -Path $TempDir -Filter "*.nuspec" | Select-Object -First 1
    if ($NuspecFile) {
        Write-Host "✅ Package metadata found: $($NuspecFile.Name)" -ForegroundColor Green
        
        # Read and display key metadata
        [xml]$Nuspec = Get-Content $NuspecFile.FullName
        $Metadata = $Nuspec.package.metadata
        
        Write-Host "   📋 Package Details:" -ForegroundColor Yellow
        Write-Host "      ID: $($Metadata.id)" -ForegroundColor White
        Write-Host "      Version: $($Metadata.version)" -ForegroundColor White
        Write-Host "      Authors: $($Metadata.authors)" -ForegroundColor White
        Write-Host "      Description: $($Metadata.description.Substring(0, [Math]::Min(100, $Metadata.description.Length)))..." -ForegroundColor White
        Write-Host "      Tags: $($Metadata.tags)" -ForegroundColor White
    }
    
    Write-Host "🎉 Package validation completed!" -ForegroundColor Green
    
} catch {
    Write-Error "❌ Validation failed: $($_.Exception.Message)"
    exit 1
} finally {
    # Clean up temp directory
    if (Test-Path $TempDir) {
        Remove-Item -Path $TempDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}