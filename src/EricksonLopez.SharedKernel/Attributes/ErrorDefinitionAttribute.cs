using System;

namespace EricksonLopez.SharedKernel.Attributes;

/// <summary>
/// Marker attribute for Source Generators.
/// Indicates that a struct, class, or record defines domain errors.
/// The source generator can use this to generate typed error classes or documentation.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public sealed class ErrorDefinitionAttribute : Attribute
{
}
