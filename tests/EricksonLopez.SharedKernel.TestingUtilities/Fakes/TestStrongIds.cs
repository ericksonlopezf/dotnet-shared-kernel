// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.DomainPrimitives.Validation;

namespace EricksonLopez.SharedKernel.TestingUtilities.Fakes;

public readonly record struct OrderId(Guid Value) : IStrongId<OrderId, Guid>
{
    public static string PrimitiveName => nameof(OrderId);
    public bool IsDefault => Value == Guid.Empty;
    public static OrderId Empty => new(Guid.Empty);
    public static OrderId Create() => new(Guid.NewGuid());

    public static OrderId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(value));
        return new OrderId(value);
    }

    public static bool TryCreate(Guid value, out OrderId result, out PrimitiveError validationError)
    {
        if (value == Guid.Empty)
        {
            result = default;
            validationError = new PrimitiveError("EMPTY", "OrderId cannot be empty.");
            return false;
        }

        result = new OrderId(value);
        validationError = default;
        return true;
    }
}

public readonly record struct LongOrderId(long Value) : IStrongId<LongOrderId, long>
{
    public static string PrimitiveName => nameof(LongOrderId);
    public bool IsDefault => Value == 0;
    public static LongOrderId Empty => new(0);
    public static LongOrderId Create() => throw new NotSupportedException();
    public static LongOrderId Create(long value) => new(value);

    public static bool TryCreate(long value, out LongOrderId result, out PrimitiveError validationError)
    {
        result = new LongOrderId(value);
        validationError = default;
        return true;
    }
}

public readonly record struct CustomerId(Guid Value) : IStrongId<CustomerId, Guid>
{
    public static string PrimitiveName => nameof(CustomerId);
    public bool IsDefault => Value == Guid.Empty;
    public static CustomerId Empty => new(Guid.Empty);
    public static CustomerId Create() => new(Guid.NewGuid());

    public static CustomerId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(value));
        return new CustomerId(value);
    }

    public static bool TryCreate(Guid value, out CustomerId result, out PrimitiveError validationError)
    {
        if (value == Guid.Empty)
        {
            result = default;
            validationError = new PrimitiveError("EMPTY", "CustomerId cannot be empty.");
            return false;
        }

        result = new CustomerId(value);
        validationError = default;
        return true;
    }

    public static CustomerId New() => new(Guid.NewGuid());
}

public readonly record struct ProductCode(string Value) : IStrongId<ProductCode, string>
{
    public static string PrimitiveName => nameof(ProductCode);
    public bool IsDefault => string.IsNullOrEmpty(Value);
    public static ProductCode Empty => new(string.Empty);
    public static ProductCode Create() => throw new NotSupportedException();

    public static ProductCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("ProductCode cannot be empty.", nameof(value));
        if (value == "FORMAT_ERR" || value == "FORMAT_ERROR")
            throw new FormatException("Format error.");
        return new ProductCode(value);
    }

    public static bool TryCreate(string value, out ProductCode result, out PrimitiveError validationError)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default;
            validationError = new PrimitiveError("EMPTY", "ProductCode cannot be empty.");
            return false;
        }

        result = new ProductCode(value);
        validationError = default;
        return true;
    }
}

public readonly record struct DepartmentId(int Value) : IStrongId<DepartmentId, int>
{
    public static string PrimitiveName => nameof(DepartmentId);
    public bool IsDefault => Value == 0;
    public static DepartmentId Empty => new(0);
    public static DepartmentId Create() => throw new NotSupportedException();

    public static DepartmentId Create(int value)
    {
        if (value < 0)
            throw new ArgumentException("DepartmentId cannot be negative.", nameof(value));
        return new DepartmentId(value);
    }

    public static bool TryCreate(int value, out DepartmentId result, out PrimitiveError validationError)
    {
        if (value < 0)
        {
            result = default;
            validationError = new PrimitiveError("NEGATIVE", "DepartmentId cannot be negative.");
            return false;
        }

        result = new DepartmentId(value);
        validationError = default;
        return true;
    }
}

public readonly record struct SequenceId(long Value) : IStrongId<SequenceId, long>
{
    public static string PrimitiveName => nameof(SequenceId);
    public bool IsDefault => Value == 0;
    public static SequenceId Empty => new(0);
    public static SequenceId Create() => throw new NotSupportedException();
    public static SequenceId Create(long value) => new(value);

    public static bool TryCreate(long value, out SequenceId result, out PrimitiveError validationError)
    {
        result = new SequenceId(value);
        validationError = default;
        return true;
    }
}

public readonly record struct Quantity(int Value) : IStrongId<Quantity, int>
{
    public static string PrimitiveName => nameof(Quantity);
    public bool IsDefault => Value == 0;
    public static Quantity Empty => new(0);
    public static Quantity Create() => throw new NotSupportedException();

    public static Quantity Create(int value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(value));
        return new Quantity(value);
    }

    public static bool TryCreate(int value, out Quantity result, out PrimitiveError validationError)
    {
        if (value < 0)
        {
            result = default;
            validationError = new PrimitiveError("NEGATIVE", "Quantity cannot be negative.");
            return false;
        }

        result = new Quantity(value);
        validationError = default;
        return true;
    }
}

public readonly record struct DateOnlyId(DateOnly Value) : IStrongId<DateOnlyId, DateOnly>
{
    public static string PrimitiveName => nameof(DateOnlyId);
    public bool IsDefault => Value == default;
    public static DateOnlyId Empty => new(default);
    public static DateOnlyId Create() => throw new NotSupportedException();

    public static DateOnlyId Create(DateOnly value)
    {
        if (value == default)
            throw new ArgumentException("DateOnlyId cannot be default.", nameof(value));
        return new DateOnlyId(value);
    }

    public static bool TryCreate(DateOnly value, out DateOnlyId result, out PrimitiveError validationError)
    {
        if (value == default)
        {
            result = default;
            validationError = new PrimitiveError("DEFAULT", "DateOnlyId cannot be default.");
            return false;
        }

        result = new DateOnlyId(value);
        validationError = default;
        return true;
    }
}

public readonly record struct NumericRangeId(int Value) : IStrongId<NumericRangeId, int>
{
    public static string PrimitiveName => nameof(NumericRangeId);
    public bool IsDefault => Value == 0;
    public static NumericRangeId Empty => new(0);
    public static NumericRangeId Create() => throw new NotSupportedException();

    public static NumericRangeId Create(int value)
    {
        if (value is < 1 or > 100)
            throw new OverflowException("Value is outside permissible range [1, 100].");
        return new NumericRangeId(value);
    }

    public static bool TryCreate(int value, out NumericRangeId result, out PrimitiveError validationError)
    {
        if (value is < 1 or > 100)
        {
            result = default;
            validationError = new PrimitiveError("RANGE", "Value is outside permissible range [1, 100].");
            return false;
        }

        result = new NumericRangeId(value);
        validationError = default;
        return true;
    }
}

public abstract class AbstractStrongId : IStrongId<AbstractStrongId, string>
{
    public static string PrimitiveName => nameof(AbstractStrongId);
    public bool IsDefault => false;
    public static AbstractStrongId Empty => throw new NotImplementedException();
    public static AbstractStrongId Create() => throw new NotImplementedException();
    public string Value => string.Empty;
    public static AbstractStrongId Create(string value) => throw new NotImplementedException();
    public static bool TryCreate(string value, out AbstractStrongId result, out PrimitiveError validationError) => throw new NotImplementedException();
    public virtual bool Equals(AbstractStrongId? other) => other is not null && ReferenceEquals(this, other);
    public override bool Equals(object? obj) => obj is AbstractStrongId other && Equals(other);
    public override int GetHashCode() => 0;
}

public interface ICustomStrongId : IStrongId<OrderId, Guid>
{
}
