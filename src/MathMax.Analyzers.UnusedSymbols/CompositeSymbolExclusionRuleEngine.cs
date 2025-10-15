namespace MathMax.Analyzers.UnusedSymbols;

using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;

/// <summary>
/// Composite rule engine that evaluates multiple exclusion criteria for symbols.
/// Follows Single Responsibility and Open/Closed principles by allowing rule composition.
/// </summary>
public class CompositeSymbolExclusionRuleEngine : ISymbolExclusionRuleEngine
{
    private readonly ITypeInheritanceChecker _inheritanceChecker;
    private readonly string[] _controllerBaseTypes = {
        "Microsoft.AspNetCore.Mvc.ControllerBase",
        "System.Web.Mvc.Controller"
    };
    private readonly string[] _controllerAttributes = {
        "Microsoft.AspNetCore.Mvc.ApiControllerAttribute",
        "System.Web.Http.ApiControllerAttribute"
    };
    private readonly string[] _usageIndicatingAttributes = {
        "UsedImplicitly", "Preserve", "KeepAttribute", "DataContractAttribute"
    };

    public CompositeSymbolExclusionRuleEngine(ITypeInheritanceChecker inheritanceChecker)
    {
        _inheritanceChecker = inheritanceChecker ?? throw new ArgumentNullException(nameof(inheritanceChecker));
    }

    public bool ShouldExcludeFromAnalysis(ISymbol symbol, Compilation compilation)
    {
        if (symbol == null) return true;

        return ShouldExcludeImplicitlyDeclaredSymbol(symbol) ||
               ShouldExcludeUntrackedAccessibilitySymbol(symbol) ||
               ShouldExcludeAttributeTypeSymbol(symbol) ||
               ShouldExcludeEntryPointSymbol(symbol, compilation) ||
               ShouldExcludeControllerSymbol(symbol) ||
               ShouldExcludeInterfaceImplementationSymbol(symbol) ||
               ShouldExcludeOverrideSymbol(symbol) ||
               ShouldExcludeExternallyUsedSymbol(symbol);
    }

    private static bool ShouldExcludeImplicitlyDeclaredSymbol(ISymbol symbol)
    {
        return symbol.IsImplicitlyDeclared;
    }

    private static bool ShouldExcludeUntrackedAccessibilitySymbol(ISymbol symbol)
    {
        return symbol.DeclaredAccessibility switch
        {
            Accessibility.Public or Accessibility.Protected or 
            Accessibility.ProtectedOrInternal or Accessibility.Private => false,
            _ => true
        };
    }

    private bool ShouldExcludeAttributeTypeSymbol(ISymbol symbol)
    {
        return symbol is INamedTypeSymbol namedType && 
               _inheritanceChecker.InheritsFromOrImplements(namedType, "System.Attribute");
    }

    private static bool ShouldExcludeEntryPointSymbol(ISymbol symbol, Compilation compilation)
    {
        var entryPoint = compilation.GetEntryPoint(CancellationToken.None);
        return SymbolEqualityComparer.Default.Equals(symbol, entryPoint);
    }

    private bool ShouldExcludeControllerSymbol(ISymbol symbol)
    {
        var containingType = symbol is INamedTypeSymbol namedType ? namedType : symbol.ContainingType;
        if (containingType == null) return false;

        return IsControllerByInheritance(containingType) ||
               IsControllerByAttribute(containingType) ||
               IsControllerByNamingConvention(containingType);
    }

    private bool IsControllerByInheritance(INamedTypeSymbol type)
    {
        return _controllerBaseTypes.Any(baseType => _inheritanceChecker.InheritsFromOrImplements(type, baseType));
    }

    private bool IsControllerByAttribute(INamedTypeSymbol type)
    {
        return type.GetAttributes().Any(attr => 
            _controllerAttributes.Any(controllerAttr => 
                attr.AttributeClass?.ToDisplayString() == controllerAttr));
    }

    private static bool IsControllerByNamingConvention(INamedTypeSymbol type)
    {
        return type.Name.EndsWith("Controller", StringComparison.Ordinal);
    }

    private static bool ShouldExcludeInterfaceImplementationSymbol(ISymbol symbol)
    {
        return symbol switch
        {
            IMethodSymbol method => HasExplicitInterfaceImplementation(method) || IsImplicitInterfaceImplementation(method),
            IPropertySymbol property => property.ExplicitInterfaceImplementations.Length > 0,
            _ => false
        };
    }

    private static bool HasExplicitInterfaceImplementation(IMethodSymbol method)
    {
        return method.ExplicitInterfaceImplementations.Length > 0;
    }

    private static bool IsImplicitInterfaceImplementation(IMethodSymbol method)
    {
        return method.ContainingType?.AllInterfaces.Any() == true && 
               method.ContainingType.FindImplementationForInterfaceMember(method) != null;
    }

    private static bool ShouldExcludeOverrideSymbol(ISymbol symbol)
    {
        return symbol.IsOverride;
    }

    private bool ShouldExcludeExternallyUsedSymbol(ISymbol symbol)
    {
        return symbol.GetAttributes().Any(attr =>
        {
            var attributeName = attr.AttributeClass?.Name ?? string.Empty;
            return _usageIndicatingAttributes.Any(usageAttr => attributeName.Contains(usageAttr));
        });
    }
}
