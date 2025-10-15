# Test Script for MathMax.Analyzers.UnusedSymbols
param(
    [string]$TestProjectPath = ".\test-project"
)

$ErrorActionPreference = "Stop"

Write-Host "🧪 Testing MathMax.Analyzers.UnusedSymbols..." -ForegroundColor Green

# Clean up any existing test project
if (Test-Path $TestProjectPath) {
    Write-Host "🧹 Cleaning up existing test project..." -ForegroundColor Yellow
    Remove-Item -Path $TestProjectPath -Recurse -Force
}

try {
    # Create test project
    Write-Host "📁 Creating test project..." -ForegroundColor Yellow
    dotnet new console -o $TestProjectPath -f net8.0

    # Add the analyzer package reference
    $PackagePath = Get-ChildItem -Path ".\nupkg" -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $PackagePath) {
        Write-Error "❌ No NuGet package found. Run 'dotnet pack' first."
        exit 1
    }

    Write-Host "📦 Adding analyzer package reference..." -ForegroundColor Yellow
    $ProjectFile = Join-Path $TestProjectPath "test-project.csproj"
    
    # Read and modify the project file
    [xml]$Project = Get-Content $ProjectFile
    
    # Add PackageReference
    $ItemGroup = $Project.CreateElement("ItemGroup")
    $PackageReference = $Project.CreateElement("PackageReference")
    $PackageReference.SetAttribute("Include", "MathMax.Analyzers.UnusedSymbols")
    $PackageReference.SetAttribute("Version", "0.1.0")
    
    $PrivateAssets = $Project.CreateElement("PrivateAssets")
    $PrivateAssets.InnerText = "all"
    $PackageReference.AppendChild($PrivateAssets)
    
    $IncludeAssets = $Project.CreateElement("IncludeAssets")
    $IncludeAssets.InnerText = "runtime; build; native; contentfiles; analyzers"
    $PackageReference.AppendChild($IncludeAssets)
    
    $ItemGroup.AppendChild($PackageReference)
    $Project.Project.AppendChild($ItemGroup)
    
    # Add local package source
    $PropertyGroup = $Project.CreateElement("PropertyGroup")
    $RestoreAdditionalProjectSources = $Project.CreateElement("RestoreAdditionalProjectSources")
    $RestoreAdditionalProjectSources.InnerText = (Resolve-Path ".\nupkg").Path
    $PropertyGroup.AppendChild($RestoreAdditionalProjectSources)
    $Project.Project.AppendChild($PropertyGroup)
    
    $Project.Save($ProjectFile)

    # Create test code with unused symbols
    Write-Host "📝 Creating test code with unused symbols..." -ForegroundColor Yellow
    $TestCode = @"
using System;

namespace TestProject
{
    public class Program
    {
        // This field should trigger USG001
        private static int _unusedField = 42;
        
        // This method should trigger USG001
        private static void UnusedMethod()
        {
            Console.WriteLine("This method is never called");
        }
        
        // This property should trigger USG001
        public static string UnusedProperty { get; set; } = "unused";
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            UsedMethod();
        }
        
        // This method should NOT trigger USG001 (it's used)
        private static void UsedMethod()
        {
            Console.WriteLine("This method is used");
        }
    }
    
    // This class should trigger USG001
    public class UnusedClass
    {
        public void DoSomething() { }
    }
}
"@

    $CodeFile = Join-Path $TestProjectPath "Program.cs"
    Set-Content -Path $CodeFile -Value $TestCode

    # Build the test project and capture output
    Write-Host "🔨 Building test project..." -ForegroundColor Yellow
    Push-Location $TestProjectPath
    
    try {
        $BuildOutput = dotnet build 2>&1
        $BuildExitCode = $LASTEXITCODE
        
        Write-Host "📊 Build Results:" -ForegroundColor Yellow
        Write-Host $BuildOutput
        
        # Check for analyzer warnings
        $USG001Warnings = $BuildOutput | Where-Object { $_ -match "USG001" }
        
        if ($USG001Warnings) {
            Write-Host "✅ Analyzer is working! Found the following warnings:" -ForegroundColor Green
            $USG001Warnings | ForEach-Object {
                Write-Host "   ⚠️  $_" -ForegroundColor Yellow
            }
            
            # Count warnings
            $WarningCount = ($USG001Warnings | Measure-Object).Count
            Write-Host "📈 Total USG001 warnings: $WarningCount" -ForegroundColor Cyan
            
            if ($WarningCount -ge 4) { # Expecting at least 4 warnings
                Write-Host "🎉 Test PASSED! Analyzer detected unused symbols correctly." -ForegroundColor Green
            } else {
                Write-Warning "⚠️  Test PARTIAL - Expected more warnings. Analyzer may not be detecting all unused symbols."
            }
        } else {
            Write-Warning "❌ Test FAILED - No USG001 warnings found. Analyzer may not be working correctly."
            Write-Host "Build output:" -ForegroundColor Red
            Write-Host $BuildOutput -ForegroundColor Red
        }
        
    } finally {
        Pop-Location
    }
    
} catch {
    Write-Error "❌ Test failed: $($_.Exception.Message)"
    exit 1
} finally {
    # Clean up test project
    if (Test-Path $TestProjectPath) {
        Write-Host "🧹 Cleaning up test project..." -ForegroundColor Yellow
        Remove-Item -Path $TestProjectPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host "🏁 Test completed!" -ForegroundColor Green