// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Globalization;
using Dapper;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Dapper type handler for mapping <see cref="TimeOnly"/> to and from database parameters and values.
/// </summary>
public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    /// <inheritdoc/>
    public override TimeOnly Parse(object value) =>
        value switch
        {
            TimeOnly timeOnly => timeOnly,
            TimeSpan timeSpan => TimeOnly.FromTimeSpan(timeSpan),
            DateTime dateTime => TimeOnly.FromDateTime(dateTime),
            string str when TimeOnly.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => parsed,
            _ => throw new DataException($"Cannot convert value of type '{value?.GetType().FullName ?? "null"}' to {nameof(TimeOnly)}.")
        };

    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        parameter.DbType = DbType.Time;
        parameter.Value = value.ToTimeSpan();
    }
}
