param($installPath, $toolsPath, $package, $project)

# This script runs when the NuGet package is installed
Write-Host "Installing MathMax.Analyzers.UnusedSymbols analyzer..."
Write-Host "The analyzer will be automatically included in your project's analysis."