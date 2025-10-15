namespace MathMax.Analyzers.UnusedSymbols;

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

#region Implementations

#endregion

/// <summary>
/// Analyzes C# code to detect unused symbols (types, methods, properties, fields, events).
/// Follows SOLID principles with separate concerns for symbol tracking, exclusion rules, and diagnostics.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnusedSymbolAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "USG001";

    private static readonly LocalizableString Title = "Symbol appears to be unused";
    private static readonly LocalizableString MessageFormat = "'{0}' is declared but appears to be unused in this compilation";
    private static readonly LocalizableString Description = "Detects declared symbols (types, methods, properties, fields, events) that are not referenced anywhere in the analyzed compilation. Excludes MVC / Web API controllers and common externally-invoked symbols.";
    private const string Category = "Usage";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, 
        isEnabledByDefault: true, description: Description, 
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <summary>
    /// Initializes the analyzer with dependency injection following Dependency Inversion Principle.
    /// Separates concerns into focused, testable components.
    /// </summary>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(RegisterCompilationAnalysis);
    }

    /// <summary>
    /// Sets up compilation-scoped analysis with injected dependencies.
    /// Follows Single Responsibility Principle by delegating to specialized components.
    /// </summary>
    private static void RegisterCompilationAnalysis(CompilationStartAnalysisContext compilationStartContext)
    {
        // Create dependencies following Dependency Inversion Principle
        var symbolTracker = CreateSymbolTracker();
        var exclusionRuleEngine = CreateSymbolExclusionRuleEngine();

        RegisterSymbolDeclarationTracking(compilationStartContext, symbolTracker, exclusionRuleEngine);
        RegisterSymbolReferenceTracking(compilationStartContext, symbolTracker);
        
        // Register compilation end action to report unused symbols
        compilationStartContext.RegisterCompilationEndAction(compilationEndContext =>
        {
            var unusedSymbols = symbolTracker.GetUnreferencedDeclaredSymbols();

            foreach (var symbol in unusedSymbols)
            {
                ReportUnusedSymbolDiagnostic(compilationEndContext, symbol);
            }
        });
    }

    /// <summary>
    /// Factory method for creating symbol tracker. Enables testability and follows DIP.
    /// </summary>
    private static ISymbolTracker CreateSymbolTracker()
    {
        return new ConcurrentSymbolTracker();
    }



    /// <summary>
    /// Factory method for creating exclusion rule engine. Enables testability and follows DIP.
    /// </summary>
    private static ISymbolExclusionRuleEngine CreateSymbolExclusionRuleEngine()
    {
        var inheritanceChecker = new TypeInheritanceChecker();
        return new CompositeSymbolExclusionRuleEngine(inheritanceChecker);
    }

    /// <summary>
    /// Registers symbol analysis to track declared symbols of interest.
    /// Follows Single Responsibility Principle by focusing only on symbol declaration tracking.
    /// </summary>
    private static void RegisterSymbolDeclarationTracking(
        CompilationStartAnalysisContext compilationStartContext,
        ISymbolTracker symbolTracker,
        ISymbolExclusionRuleEngine exclusionRuleEngine)
    {
        var trackedSymbolKinds = new[]
        {
            SymbolKind.NamedType,
            SymbolKind.Method,
            SymbolKind.Property,
            SymbolKind.Field,
            SymbolKind.Event
        };

        compilationStartContext.RegisterSymbolAction(symbolContext =>
        {
            var symbol = symbolContext.Symbol;

            if (!exclusionRuleEngine.ShouldExcludeFromAnalysis(symbol, symbolContext.Compilation))
            {
                symbolTracker.RecordDeclaredSymbol(symbol);
            }
        }, trackedSymbolKinds);
    }

    /// <summary>
    /// Registers operation analysis to track symbol references.
    /// Follows Single Responsibility Principle by focusing only on reference tracking.
    /// </summary>
    private static void RegisterSymbolReferenceTracking(
        CompilationStartAnalysisContext compilationStartContext,
        ISymbolTracker symbolTracker)
    {
        var trackedOperationKinds = new[]
        {
            OperationKind.Invocation,
            OperationKind.ObjectCreation,
            OperationKind.PropertyReference,
            OperationKind.FieldReference,
            OperationKind.EventReference,
            OperationKind.TypeOf,
            OperationKind.Conversion
        };

        compilationStartContext.RegisterOperationAction(operationContext =>
        {
            var targetSymbol = OperationSymbolExtractor.ExtractTargetSymbol(operationContext.Operation);
            if (targetSymbol != null)
            {
                symbolTracker.RecordReferencedSymbol(targetSymbol);
            }
        }, trackedOperationKinds);
    }



    /// <summary>
    /// Reports diagnostic for a specific unused symbol.
    /// Follows DAMP principle with descriptive method name and clear purpose.
    /// </summary>
    private static void ReportUnusedSymbolDiagnostic(CompilationAnalysisContext context, ISymbol symbol)
    {
        var primaryLocation = symbol.Locations.FirstOrDefault(loc => loc.IsInSource);
        if (primaryLocation != null)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                primaryLocation,
                symbol.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat));

            context.ReportDiagnostic(diagnostic);
        }
    }


}