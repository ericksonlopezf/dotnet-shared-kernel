// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Globalization;
using Dapper;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// Dapper type handler for mapping <see cref="DateTimeOffset"/> to and from database parameters and values with strict UTC normalization.
/// </summary>
public sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    /// <inheritdoc/>
    public override DateTimeOffset Parse(object value) =>
        value switch
        {
            null or DBNull => throw new DataException("Cannot map null database value to non-nullable DateTimeOffset."),
            DateTimeOffset dto => dto,
            DateTime dt => dt.Kind switch
            {
                DateTimeKind.Utc => new DateTimeOffset(dt, TimeSpan.Zero),
                DateTimeKind.Local => new DateTimeOffset(dt.ToUniversalTime(), TimeSpan.Zero),
                _ => new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc), TimeSpan.Zero)
            },
            string str => DateTimeOffset.Parse(str, CultureInfo.InvariantCulture),
            _ => DateTimeOffset.Parse(value.ToString()!, CultureInfo.InvariantCulture)
        };

    /// <inheritdoc/>
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        parameter.DbType = DbType.DateTimeOffset;
        parameter.Value = value;
    }
}
