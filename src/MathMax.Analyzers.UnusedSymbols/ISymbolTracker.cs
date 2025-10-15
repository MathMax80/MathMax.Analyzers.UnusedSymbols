namespace MathMax.Analyzers.UnusedSymbols;

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

/// <summary>
/// Defines contract for tracking symbol declarations and references during compilation analysis.
/// Follows Single Responsibility Principle by focusing solely on symbol state management.
/// </summary>
public interface ISymbolTracker
{
    /// <summary>
    /// Records a symbol as declared in the compilation.
    /// </summary>
    /// <param name="symbol">The symbol that was declared</param>
    void RecordDeclaredSymbol(ISymbol symbol);

    /// <summary>
    /// Records a symbol as referenced/used in the compilation.
    /// </summary>
    /// <param name="symbol">The symbol that was referenced</param>
    void RecordReferencedSymbol(ISymbol symbol);

    /// <summary>
    /// Determines if a symbol has been recorded as referenced.
    /// </summary>
    /// <param name="symbol">The symbol to check</param>
    /// <returns>True if the symbol has been referenced</returns>
    bool IsSymbolReferenced(ISymbol symbol);

    /// <summary>
    /// Gets all declared symbols that haven't been referenced.
    /// </summary>
    /// <returns>Collection of unreferenced symbols</returns>
    IEnumerable<ISymbol> GetUnreferencedDeclaredSymbols();
}
