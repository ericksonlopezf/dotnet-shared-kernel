// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Globalization;
using Dapper;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Dapper type handler for mapping <see cref="DateOnly"/> to and from database parameters and values.
/// </summary>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    /// <inheritdoc/>
    public override DateOnly Parse(object value) =>
        value switch
        {
            null or DBNull => throw new DataException("Cannot map null database value to non-nullable DateOnly."),
            DateOnly dateOnly => dateOnly,
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            string str when DateOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => parsed,
            _ => DateOnly.FromDateTime(Convert.ToDateTime(value, CultureInfo.InvariantCulture))
        };

    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        parameter.DbType = DbType.Date;
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
    }
}
