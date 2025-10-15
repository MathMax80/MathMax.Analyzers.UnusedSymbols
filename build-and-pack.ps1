# Build and Pack Script for MathMax.Analyzers.UnusedSymbols
param(
    [string]$Configuration = "Release",
    [string]$OutputPath = ".\nupkg",
    [switch]$SkipBuild,
    [switch]$Push,
    [string]$ApiKey = $env:NUGET_API_KEY,
    [string]$Source = "https://api.nuget.org/v3/index.json"
)

$ErrorActionPreference = "Stop"

Write-Host "🔧 Building and packaging MathMax.Analyzers.UnusedSymbols..." -ForegroundColor Green

# Set paths
$ProjectPath = "src\MathMax.Analyzers.UnusedSymbols\MathMax.Analyzers.UnusedSymbols.csproj"
$SolutionPath = "MathMax.Analyzers.UnusedSymbols.sln"

# Create output directory
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

try {
    # Clean previous builds
    if (-not $SkipBuild) {
        Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
        dotnet clean $SolutionPath --configuration $Configuration --verbosity minimal
    }

    # Restore packages
    Write-Host "📦 Restoring NuGet packages..." -ForegroundColor Yellow
    dotnet restore $SolutionPath

    # Build the solution
    if (-not $SkipBuild) {
        Write-Host "🔨 Building solution..." -ForegroundColor Yellow
        dotnet build $SolutionPath --configuration $Configuration --no-restore --verbosity minimal
    }

    # Run tests if they exist
    $TestProjects = Get-ChildItem -Path "tests" -Filter "*.csproj" -Recurse -ErrorAction SilentlyContinue
    if ($TestProjects) {
        Write-Host "🧪 Running tests..." -ForegroundColor Yellow
        dotnet test $SolutionPath --configuration $Configuration --no-build --verbosity minimal
    }

    # Pack the NuGet package
    Write-Host "📦 Creating NuGet package..." -ForegroundColor Yellow
    dotnet pack $ProjectPath --configuration $Configuration --no-build --output $OutputPath --verbosity minimal

    # List created packages
    $Packages = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending
    if ($Packages) {
        Write-Host "✅ Package(s) created successfully:" -ForegroundColor Green
        $Packages | ForEach-Object { 
            Write-Host "   📦 $($_.Name) ($([math]::Round($_.Length / 1KB, 2)) KB)" -ForegroundColor Cyan
        }
    }

    # Push to NuGet if requested
    if ($Push) {
        if (-not $ApiKey) {
            Write-Warning "⚠️  No API key provided. Set NUGET_API_KEY environment variable or use -ApiKey parameter."
            exit 1
        }

        $LatestPackage = $Packages | Select-Object -First 1
        Write-Host "🚀 Pushing package to NuGet..." -ForegroundColor Yellow
        Write-Host "   Package: $($LatestPackage.Name)" -ForegroundColor Cyan
        Write-Host "   Source: $Source" -ForegroundColor Cyan
        
        dotnet nuget push $LatestPackage.FullName --api-key $ApiKey --source $Source
        Write-Host "✅ Package pushed successfully!" -ForegroundColor Green
    }

    Write-Host "🎉 Build and pack completed successfully!" -ForegroundColor Green

} catch {
    Write-Error "❌ Build failed: $($_.Exception.Message)"
    exit 1
}