// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.TestingUtilities.Fakes;

/// <summary>
/// Non-StrongId reference type used to verify negative type resolution in serialization converters and factories.
/// </summary>
public class NonStrongIdType
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Non-StrongId class used to verify negative type scanning in reflection registries.
/// </summary>
public class NonStrongIdClass
{
    public string Name { get; set; } = string.Empty;
}


