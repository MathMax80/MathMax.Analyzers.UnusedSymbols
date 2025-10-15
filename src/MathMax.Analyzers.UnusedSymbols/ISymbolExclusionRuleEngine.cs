namespace MathMax.Analyzers.UnusedSymbols;

using Microsoft.CodeAnalysis;

/// <summary>
/// Defines contract for determining if symbols should be excluded from unused symbol analysis.
/// Follows Open/Closed Principle by allowing extension of exclusion rules without modification.
/// </summary>
public interface ISymbolExclusionRuleEngine
{
    /// <summary>
    /// Determines if a symbol should be excluded from unused symbol analysis.
    /// </summary>
    /// <param name="symbol">The symbol to evaluate</param>
    /// <param name="compilation">The current compilation context</param>
    /// <returns>True if symbol should be excluded from analysis</returns>
    bool ShouldExcludeFromAnalysis(ISymbol symbol, Compilation compilation);
}
