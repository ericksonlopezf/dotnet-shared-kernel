// Copyright © Erickson Lopez. MIT License.
using System;
using Dapper;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Provides static registration methods for standard BCL Dapper TypeHandlers.
/// </summary>
public static class DapperBclTypeHandlerRegistry
{
    private static readonly DateOnlyTypeHandler _dateOnlyHandler = new();
    private static readonly DateTimeOffsetTypeHandler _dateTimeOffsetHandler = new();
    private static readonly TimeOnlyTypeHandler _timeOnlyHandler = new();

    /// <summary>
    /// Registers all standard BCL type handlers (<see cref="DateOnly"/>, <see cref="DateTimeOffset"/>, <see cref="TimeOnly"/>) with Dapper.
    /// </summary>
    /// <remarks>
    /// This method is fully AOT-safe and uses zero runtime reflection or dynamic code generation.
    /// </remarks>
    public static void RegisterAll()
    {
        SqlMapper.AddTypeHandler(_dateOnlyHandler);
        SqlMapper.AddTypeHandler(_dateTimeOffsetHandler);
        SqlMapper.AddTypeHandler(_timeOnlyHandler);
    }
}
