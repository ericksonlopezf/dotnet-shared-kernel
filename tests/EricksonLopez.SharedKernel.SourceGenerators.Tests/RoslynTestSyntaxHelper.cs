// Copyright © Erickson Lopez. MIT License.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EricksonLopez.SharedKernel.SourceGenerators.Tests;

/// <summary>
/// Provides shared Roslyn syntax tree normalization and formatting utilities for source generator tests.
/// </summary>
internal static class RoslynTestSyntaxHelper
{
    /// <summary>
    /// Parses and normalizes C# source code to standard whitespace and newline representation for deterministic AST comparison.
    /// </summary>
    /// <param name="source">The C# code string to normalize.</param>
    /// <returns>A normalized, cross-platform formatted C# string.</returns>
    public static string Normalize(string source)
    {
        return source.Replace("\r\n", "\n").Trim();
    }
}
