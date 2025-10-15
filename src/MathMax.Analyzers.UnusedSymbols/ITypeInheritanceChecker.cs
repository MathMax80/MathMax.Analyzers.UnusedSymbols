namespace MathMax.Analyzers.UnusedSymbols;

using Microsoft.CodeAnalysis;

/// <summary>
/// Defines contract for checking type inheritance relationships.
/// Follows DRY principle by centralizing inheritance logic.
/// </summary>
public interface ITypeInheritanceChecker
{
    /// <summary>
    /// Determines if a type inherits from or implements a base type by metadata name.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <param name="baseTypeMetadataName">The metadata name of the base type</param>
    /// <returns>True if inheritance relationship exists</returns>
    bool InheritsFromOrImplements(INamedTypeSymbol type, string baseTypeMetadataName);

    /// <summary>
    /// Determines if a type matches any of the specified metadata names.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <param name="metadataNames">Collection of metadata names to match against</param>
    /// <returns>True if type matches any of the names</returns>
    bool MatchesAnyMetadataName(INamedTypeSymbol type, params string[] metadataNames);
}
