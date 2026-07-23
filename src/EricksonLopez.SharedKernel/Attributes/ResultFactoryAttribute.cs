using System;

namespace EricksonLopez.SharedKernel.Attributes;

/// <summary>
/// Marker attribute for Source Generators.
/// Indicates that a method acts as a factory for generating Results.
/// The source generator can use this to generate helper methods or interceptors.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ResultFactoryAttribute : Attribute
{
}
