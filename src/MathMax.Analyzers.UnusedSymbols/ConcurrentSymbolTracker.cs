namespace MathMax.Analyzers.UnusedSymbols;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Thread-safe implementation of symbol tracking for compilation analysis.
/// Uses concurrent collections to support parallel analysis execution.
/// </summary>
public class ConcurrentSymbolTracker : ISymbolTracker
{
    private readonly ConcurrentDictionary<ISymbol, bool> _declaredSymbols;
    private readonly ConcurrentDictionary<ISymbol, bool> _referencedSymbols;

    public ConcurrentSymbolTracker()
    {
        _declaredSymbols = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
        _referencedSymbols = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
    }

    public void RecordDeclaredSymbol(ISymbol symbol)
    {
        if (symbol != null)
        {
            _declaredSymbols.TryAdd(symbol, true);
        }
    }

    public void RecordReferencedSymbol(ISymbol symbol)
    {
        if (symbol != null)
        {
            _referencedSymbols.TryAdd(symbol, true);
            
            // Also mark containing type as referenced for member symbols
            if (symbol.ContainingType != null)
            {
                _referencedSymbols.TryAdd(symbol.ContainingType, true);
            }
        }
    }

    public bool IsSymbolReferenced(ISymbol symbol)
    {
        if (symbol == null) return false;

        // Check if symbol itself is referenced
        if (_referencedSymbols.ContainsKey(symbol))
            return true;

        // For named types, check if any members are referenced
        if (symbol is INamedTypeSymbol namedType)
        {
            return namedType.GetMembers().Any(member => _referencedSymbols.ContainsKey(member));
        }

        // For methods, check overridden method chain
        if (symbol is IMethodSymbol method)
        {
            var baseMethod = method.OverriddenMethod;
            while (baseMethod != null)
            {
                if (_referencedSymbols.ContainsKey(baseMethod))
                    return true;
                baseMethod = baseMethod.OverriddenMethod;
            }
        }

        return false;
    }

    public IEnumerable<ISymbol> GetUnreferencedDeclaredSymbols()
    {
        return _declaredSymbols.Keys.Where(symbol => !IsSymbolReferenced(symbol));
    }
}
