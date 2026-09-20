// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Dapper;

namespace EricksonLopez.SharedKernel.Dapper;

/// <summary>
/// A Dapper type handler for strongly-typed identifier types discovered by reflection conventions.
/// </summary>
/// <typeparam name="T">The strongly-typed identifier struct or class.</typeparam>
internal sealed class ConventionStrongIdTypeHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] T> : SqlMapper.TypeHandler<T>
{
    private readonly PropertyInfo _valueProp;
    private readonly ConstructorInfo _constructor;
    private readonly Func<object, T> _factory;
    private readonly Func<T, object?> _getter;
    private readonly DbType? _inferredDbType;

    public ConventionStrongIdTypeHandler(PropertyInfo valueProp, ConstructorInfo constructor)
    {
        _valueProp = valueProp ?? throw new ArgumentNullException(nameof(valueProp));
        _constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
        _factory = BuildFactory(constructor, valueProp.PropertyType);
        _getter = BuildGetter(valueProp);
        _inferredDbType = InferDbType(valueProp.PropertyType);
    }

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (_inferredDbType.HasValue)
        {
            parameter.DbType = _inferredDbType.Value;
        }

        if (value is null)
        {
            parameter.Value = DBNull.Value;
            return;
        }

        var innerValue = _getter(value);
        parameter.Value = innerValue ?? DBNull.Value;
    }

    public override T Parse(object value)
    {
        if (value is null || value is DBNull)
        {
            throw new DataException(
                $"Cannot map null database value to strong identifier '{typeof(T).FullName}'.");
        }

        if (value is Guid guid && _valueProp.PropertyType == typeof(Guid))
        {
            return _factory(guid);
        }

        if (value is string str && _valueProp.PropertyType == typeof(Guid) && Guid.TryParse(str, out var parsedGuid))
        {
            return _factory(parsedGuid);
        }

        if (value.GetType() == _valueProp.PropertyType)
        {
            return _factory(value);
        }

        try
        {
            var targetType = _valueProp.PropertyType;
            var converted = Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
            return _factory(converted);
        }
        catch (Exception ex)
        {
            throw new DataException(
                $"Cannot convert value '{value}' of type '{value.GetType().FullName}' to '{typeof(T).FullName}'.",
                ex);
        }
    }

    private static Func<object, T> BuildFactory(ConstructorInfo constructor, Type targetType)
    {
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            try
            {
                var param = Expression.Parameter(typeof(object), "value");
                var cast = Expression.Convert(param, targetType);
                var newExp = Expression.New(constructor, cast);
                var body = typeof(T).IsValueType
                    ? (Expression)newExp
                    : Expression.Convert(newExp, typeof(T));
                return Expression.Lambda<Func<object, T>>(body, param).Compile();
            }
            catch
            {
                // Fallback to reflection on compile failure
            }
        }

        return val => (T)constructor.Invoke([val]);
    }

    private static Func<T, object?> BuildGetter(PropertyInfo valueProp)
    {
        if (RuntimeFeature.IsDynamicCodeSupported)
        {
            try
            {
                var param = Expression.Parameter(typeof(T), "instance");
                var propAccess = Expression.Property(param, valueProp);
                var box = Expression.Convert(propAccess, typeof(object));
                return Expression.Lambda<Func<T, object?>>(box, param).Compile();
            }
            catch
            {
                // Fallback to reflection on compile failure
            }
        }

        return instance => valueProp.GetValue(instance);
    }

    private static DbType? InferDbType(Type type)
    {
        if (type == typeof(Guid)) return DbType.Guid;
        if (type == typeof(string)) return DbType.String;
        if (type == typeof(int)) return DbType.Int32;
        if (type == typeof(long)) return DbType.Int64;
        if (type == typeof(short)) return DbType.Int16;
        if (type == typeof(byte)) return DbType.Byte;
        if (type == typeof(bool)) return DbType.Boolean;
        if (type == typeof(DateTime)) return DbType.DateTime2;
        if (type == typeof(DateTimeOffset)) return DbType.DateTimeOffset;
        if (type == typeof(DateOnly)) return DbType.Date;
        if (type == typeof(TimeOnly)) return DbType.Time;
        if (type == typeof(decimal)) return DbType.Decimal;
        if (type == typeof(double)) return DbType.Double;
        if (type == typeof(float)) return DbType.Single;
        return null;
    }
}
