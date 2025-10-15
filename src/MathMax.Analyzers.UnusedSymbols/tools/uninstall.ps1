param($installPath, $toolsPath, $package, $project)

# This script runs when the NuGet package is uninstalled
Write-Host "Uninstalling MathMax.Analyzers.UnusedSymbols analyzer..."
Write-Host "The analyzer has been removed from your project's analysis."