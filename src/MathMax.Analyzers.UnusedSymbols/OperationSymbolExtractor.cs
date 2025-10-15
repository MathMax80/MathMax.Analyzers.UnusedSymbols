namespace MathMax.Analyzers.UnusedSymbols;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Extracts target symbols from various types of code operations.
/// Centralizes the logic for identifying which symbols are being referenced.
/// </summary>
public static class OperationSymbolExtractor
{
    public static ISymbol? ExtractTargetSymbol(IOperation operation)
    {
        return operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IObjectCreationOperation objectCreation => (ISymbol?)objectCreation.Constructor ?? objectCreation.Type,
            IPropertyReferenceOperation propertyRef => propertyRef.Property,
            IFieldReferenceOperation fieldRef => fieldRef.Field,
            IEventReferenceOperation eventRef => eventRef.Event,
            IMemberReferenceOperation memberRef => memberRef.Member,
            ITypeOfOperation typeOf => typeOf.Type,
            IConversionOperation conversion => conversion.Type,
            _ => null
        };
    }
}
