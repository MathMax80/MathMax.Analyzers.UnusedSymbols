namespace MathMax.Analyzers.UnusedSymbols;

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides efficient type inheritance and metadata name checking.
/// Eliminates duplicate inheritance checking logic throughout the codebase.
/// </summary>
public class TypeInheritanceChecker : ITypeInheritanceChecker
{
    public bool InheritsFromOrImplements(INamedTypeSymbol type, string baseTypeMetadataName)
    {
        if (type == null || string.IsNullOrEmpty(baseTypeMetadataName))
            return false;

        // Check inheritance chain
        var current = type;
        while (current != null)
        {
            if (DoesTypeMatchMetadataName(current, baseTypeMetadataName))
                return true;
            current = current.BaseType;
        }

        // Check implemented interfaces
        return type.AllInterfaces.Any(interfaceType => DoesTypeMatchMetadataName(interfaceType, baseTypeMetadataName));
    }

    public bool MatchesAnyMetadataName(INamedTypeSymbol type, params string[] metadataNames)
    {
        if (type == null || metadataNames == null || metadataNames.Length == 0)
            return false;

        return metadataNames.Any(name => DoesTypeMatchMetadataName(type, name));
    }

    private static bool DoesTypeMatchMetadataName(INamedTypeSymbol type, string metadataName)
    {
        var displayString = type.ToDisplayString();
        return displayString == metadataName || 
               type.MetadataName == metadataName || 
               displayString.EndsWith(metadataName, StringComparison.Ordinal);
    }
}
