// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Dapper;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.DomainPrimitives.Validation;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.Dapper;
using EricksonLopez.SharedKernel.Dapper.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace EricksonLopez.SharedKernel.Dapper.Tests;

/// <summary>
/// Verifies StrongId type handlers and dynamic reflection registration against Dapper's SqlMapper.
/// <para>
/// <b>Architectural Note on State Isolation (FIRST Principle - Independent):</b><br/>
/// Dapper's <see cref="SqlMapper"/> maintains a process-wide static type-handler cache that cannot be
/// torn down between test executions without undocumented internal reflection hacks.
/// Decorating this test class with <see cref="CollectionAttribute"/> ensures serialized execution
/// against the static registry while maintaining deterministic, idempotent assertions.
/// </para>
/// </summary>
[Collection("DapperRegistryTests")]
public class StrongIdDapperTests
{
    #region StrongIdTypeHandler Tests

    [Fact]
    public void SetValue_WithNullParameter_ThrowsArgumentNullException()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();
        var id = OrderId.Create(Guid.NewGuid());

        var act = () => handler.SetValue(null!, id);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("parameter");
    }

    [Fact]
    public void SetValue_WithValidValue_SetsParameterValueToPrimitive()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();
        var id = OrderId.Create(Guid.NewGuid());
        var parameter = new FakeDbDataParameter();

        handler.SetValue(parameter, id);

        parameter.Value.Should().Be(id.Value);
    }

    [Fact]
    public void SetValue_WithNullValue_SetsParameterValueToDBNull()
    {
        var handler = new StrongIdTypeHandler<ProductCode, string>();
        var parameter = new FakeDbDataParameter();

        handler.SetValue(parameter, default);

        parameter.Value.Should().Be(DBNull.Value);
    }

    [Fact]
    public void SetValue_WithStructStrongId_HoldingReferenceType_ExecutesWithoutAllocating()
    {
        var handler = new StrongIdTypeHandler<ProductCode, string>();
        var code = ProductCode.Create("PROD-100");
        var parameter = new FakeDbDataParameter();

        // Warmup
        handler.SetValue(parameter, code);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
        {
            handler.SetValue(parameter, code);
        }
        long after = GC.GetAllocatedBytesForCurrentThread();

        (after - before).Should().Be(0, because: "StrongIdTypeHandler.SetValue for struct IDs must not box the strong ID wrapper itself per FND-SK-008.");
    }

    [Fact]
    public void SetValue_WithStructStrongId_DoesNotDoubleBoxStrongIdEnvelope()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();
        var id = OrderId.Create(Guid.NewGuid());
        var parameter = new FakeDbDataParameter();

        // Warmup
        handler.SetValue(parameter, id);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
        {
            handler.SetValue(parameter, id);
        }
        long after = GC.GetAllocatedBytesForCurrentThread();

        // 1,000 * 32 bytes = 32,000 bytes (only the primitive Guid is boxed for ADO.NET object parameter; OrderId is not boxed)
        (after - before).Should().Be(1_000 * 32, because: "Only the primitive Guid is boxed into IDbDataParameter.Value; the OrderId struct envelope is never boxed per FND-SK-008.");
    }

    [Fact]
    public void Parse_WithNullValue_ThrowsDataException()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();

        var act = () => handler.Parse(null!);

        act.Should().Throw<DataException>()
            .WithMessage($"*Cannot map a null database value to the non-nullable strong identifier '{typeof(OrderId).FullName}'.*");
    }

    [Fact]
    public void Parse_WithDBNull_ThrowsDataException()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();

        var act = () => handler.Parse(DBNull.Value);

        act.Should().Throw<DataException>()
            .WithMessage($"*Cannot map a null database value to the non-nullable strong identifier '{typeof(OrderId).FullName}'.*");
    }

    [Fact]
    public void Parse_WithIncompatibleType_ThrowsDataException()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();

        var act = () => handler.Parse(12345);

        act.Should().Throw<DataException>()
            .WithMessage($"*Database type '{typeof(int).FullName}' is incompatible with strong identifier '{typeof(OrderId).FullName}'*");
    }

    [Fact]
    public void Parse_WithValidPrimitive_ReturnsStrongId()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();
        var guid = Guid.NewGuid();

        var result = handler.Parse(guid);

        result.Should().Be(OrderId.Create(guid));
        result.Value.Should().Be(guid);
    }

    [Fact]
    public void Parse_WhenCreateThrowsArgumentException_ThrowsDataExceptionWithInnerException()
    {
        var handler = new StrongIdTypeHandler<OrderId, Guid>();

        var act = () => handler.Parse(Guid.Empty);

        var ex = act.Should().Throw<DataException>()
            .WithMessage($"*The database value is invalid for strong identifier '{typeof(OrderId).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<ArgumentException>();
    }

    [Fact]
    public void Parse_WhenCreateThrowsFormatException_ThrowsDataExceptionWithInnerException()
    {
        var handler = new StrongIdTypeHandler<ProductCode, string>();

        var act = () => handler.Parse("FORMAT_ERR");

        var ex = act.Should().Throw<DataException>()
            .WithMessage($"*The database value is invalid for strong identifier '{typeof(ProductCode).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<FormatException>();
    }

    [Fact]
    public void Parse_WhenCreateThrowsOverflowException_ThrowsDataExceptionWithInnerException()
    {
        var handler = new StrongIdTypeHandler<NumericRangeId, int>();

        var act = () => handler.Parse(999);

        var ex = act.Should().Throw<DataException>()
            .WithMessage($"*The database value is invalid for strong identifier '{typeof(NumericRangeId).FullName}'.*")
            .Which;

        ex.InnerException.Should().BeOfType<OverflowException>();
    }

    #endregion

    #region DapperStrongIdRegistry Tests

#pragma warning disable CS0618
    [Fact]
    public void Register_RegistersSpecificStrongIdTypeHandler()
    {
        DapperStrongIdRegistry.Register<DepartmentId, int>();

        SqlMapper.LookupDbType(typeof(DepartmentId), "col", false, out var handler);
        handler.Should().NotBeNull();
        handler.Should().BeOfType<StrongIdTypeHandler<DepartmentId, int>>();

        var param = new FakeDbDataParameter();
        handler!.SetValue(param, DepartmentId.Create(101));
        param.Value.Should().Be(101);
    }

    [Fact]
    public void RegisterFromAssembly_WithNullAssembly_ThrowsArgumentNullException()
    {
        var act = () => DapperStrongIdRegistry.RegisterFromAssembly(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assembly");
    }

    [Fact]
    public void RegisterFromAssembly_ScansAndRegistersAllStrongIdsInAssembly()
    {
        DapperStrongIdRegistry.RegisterFromAssembly(typeof(OrderId).Assembly);

        SqlMapper.LookupDbType(typeof(OrderId), "col", false, out var orderHandler);
        orderHandler.Should().NotBeNull();
        orderHandler.Should().BeOfType<StrongIdTypeHandler<OrderId, Guid>>();

        SqlMapper.LookupDbType(typeof(ProductCode), "col", false, out var productHandler);
        productHandler.Should().NotBeNull();
        productHandler.Should().BeOfType<StrongIdTypeHandler<ProductCode, string>>();

        // Abstract and interface types must NOT be registered
        SqlMapper.LookupDbType(typeof(AbstractStrongId), "col", false, out var abstractHandler);
        abstractHandler.Should().BeNull();

        SqlMapper.LookupDbType(typeof(ICustomStrongId), "col", false, out var interfaceHandler);
        interfaceHandler.Should().BeNull();
    }

    [Fact]
    public void RegisterFromAssembly_WhenAssemblyHasNoStrongIds_CompletesWithoutError()
    {
        // Scanning an assembly with zero IStrongId implementations must complete smoothly as a no-op
        var assembly = typeof(string).Assembly;

        var act = () => DapperStrongIdRegistry.RegisterFromAssembly(assembly);

        act.Should().NotThrow(because: "RegisterFromAssembly on an assembly containing no strong IDs must safely complete without exceptions.");
    }

    [Fact]
    public void RegisterFromAssembly_WhenReflectionTypeLoadExceptionThrown_RegistersAvailableTypes()
    {
        var assembly = new FakeThrowingAssembly();
        var act = () => DapperStrongIdRegistry.RegisterFromAssembly(assembly);
        act.Should().NotThrow();

        SqlMapper.LookupDbType(typeof(OrderId), "col", false, out var handler);
        handler.Should().NotBeNull();
        handler.Should().BeOfType<StrongIdTypeHandler<OrderId, Guid>>();
    }

    [Fact]
    public void RegisterFromAssembly_WhenInvokedConcurrentlyFromMultipleThreads_IsIdempotentAndThreadSafe()
    {
        var assembly = typeof(OrderId).Assembly;

        var act = () => Parallel.For(0, 50, _ =>
        {
            DapperStrongIdRegistry.RegisterFromAssembly(assembly);
        });

        act.Should().NotThrow(because: "Concurrent registrations from multiple startup threads must be safe and idempotent.");

        SqlMapper.LookupDbType(typeof(OrderId), "col", false, out var orderHandler);
        orderHandler.Should().NotBeNull();
        orderHandler.Should().BeOfType<StrongIdTypeHandler<OrderId, Guid>>();
    }

    [Fact]
    public void RegisterFromAssemblies_WithNullAssemblies_ThrowsArgumentNullException()
    {
        var act = () => DapperStrongIdRegistry.RegisterFromAssemblies(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("assemblies");
    }

    [Fact]
    public void RegisterFromAssemblies_WithValidAssemblies_RegistersTypes()
    {
        var assembly1 = typeof(OrderId).Assembly;
        var assembly2 = typeof(StrongIdDapperTests).Assembly;

        var act = () => DapperStrongIdRegistry.RegisterFromAssemblies(assembly1, assembly2);

        act.Should().NotThrow();

        SqlMapper.LookupDbType(typeof(OrderId), "col", false, out var orderHandler);
        orderHandler.Should().NotBeNull();
        orderHandler.Should().BeOfType<StrongIdTypeHandler<OrderId, Guid>>();
    }

    [Fact]
    public void RegisterFromAssemblies_WithNullElementInArray_SkipsNullAndRegistersValidAssemblies()
    {
        var assembly1 = typeof(OrderId).Assembly;

        var act = () => DapperStrongIdRegistry.RegisterFromAssemblies(assembly1, null!);

        act.Should().NotThrow();

        SqlMapper.LookupDbType(typeof(OrderId), "col", false, out var orderHandler);
        orderHandler.Should().NotBeNull();
        orderHandler.Should().BeOfType<StrongIdTypeHandler<OrderId, Guid>>();
    }
#pragma warning restore CS0618


    #endregion

    #region Property-Based Tests (FsCheck)

    [Property]
    public Property DapperHandler_ParseAndSetValue_PreservesGuid(Guid idValue)
    {
        // Discard invalid domain generator values (Guid.Empty) via FsCheck precondition filtering
        if (idValue == Guid.Empty)
            return false.When(false);

        var handler = new StrongIdTypeHandler<OrderId, Guid>();
        return VerifyParseAndSetValue(handler, idValue, OrderId.Create(idValue), v => v != Guid.Empty);
    }

    [Property]
    public Property DapperHandler_ParseAndSetValue_PreservesInt(PositiveInt positiveInt)
    {
        var handler = new StrongIdTypeHandler<DepartmentId, int>();
        return VerifyParseAndSetValue(handler, positiveInt.Get, DepartmentId.Create(positiveInt.Get), v => v >= 0);
    }

    [Property]
    public Property DapperHandler_ParseAndSetValue_PreservesString(NonNull<string> nonNullString)
    {
        var raw = nonNullString.Get;
        // Discard whitespace and synthetic error token strings via FsCheck precondition filtering
        if (string.IsNullOrWhiteSpace(raw) || raw == "FORMAT_ERR" || raw == "FORMAT_ERROR")
            return false.When(false);

        var handler = new StrongIdTypeHandler<ProductCode, string>();
        return VerifyParseAndSetValue(handler, raw, ProductCode.Create(raw), _ => true);
    }

    private static Property VerifyParseAndSetValue<TStrongId, TPrimitive>(
        StrongIdTypeHandler<TStrongId, TPrimitive> handler,
        TPrimitive rawValue,
        TStrongId expectedParsed,
        Func<TPrimitive, bool> condition)
        where TStrongId : IStrongId<TStrongId, TPrimitive>
        where TPrimitive : notnull, IEquatable<TPrimitive>
    {
        var parsed = handler.Parse(rawValue);
        var param = new FakeDbDataParameter();
        handler.SetValue(param, parsed);

        bool isValid = parsed.Equals(expectedParsed) && param.Value!.Equals(rawValue);
        return isValid.When(condition(rawValue));
    }

    public readonly record struct ConcreteShortId(short Value) : IStrongId<ConcreteShortId, short>
    {
        public static string PrimitiveName => nameof(ConcreteShortId);
        public bool IsDefault => Value == 0;
        public static ConcreteShortId Empty => new(0);
        public static ConcreteShortId Create() => new(0);
        public static ConcreteShortId Create(short value) => new(value);
        public static bool TryCreate(short value, out ConcreteShortId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteByteId(byte Value) : IStrongId<ConcreteByteId, byte>
    {
        public static string PrimitiveName => nameof(ConcreteByteId);
        public bool IsDefault => Value == 0;
        public static ConcreteByteId Empty => new(0);
        public static ConcreteByteId Create() => new(0);
        public static ConcreteByteId Create(byte value) => new(value);
        public static bool TryCreate(byte value, out ConcreteByteId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteBoolId(bool Value) : IStrongId<ConcreteBoolId, bool>
    {
        public static string PrimitiveName => nameof(ConcreteBoolId);
        public bool IsDefault => !Value;
        public static ConcreteBoolId Empty => new(false);
        public static ConcreteBoolId Create() => new(false);
        public static ConcreteBoolId Create(bool value) => new(value);
        public static bool TryCreate(bool value, out ConcreteBoolId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteDateTimeId(DateTime Value) : IStrongId<ConcreteDateTimeId, DateTime>
    {
        public static string PrimitiveName => nameof(ConcreteDateTimeId);
        public bool IsDefault => Value == default;
        public static ConcreteDateTimeId Empty => new(default);
        public static ConcreteDateTimeId Create() => new(default);
        public static ConcreteDateTimeId Create(DateTime value) => new(value);
        public static bool TryCreate(DateTime value, out ConcreteDateTimeId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteDateTimeOffsetId(DateTimeOffset Value) : IStrongId<ConcreteDateTimeOffsetId, DateTimeOffset>
    {
        public static string PrimitiveName => nameof(ConcreteDateTimeOffsetId);
        public bool IsDefault => Value == default;
        public static ConcreteDateTimeOffsetId Empty => new(default);
        public static ConcreteDateTimeOffsetId Create() => new(default);
        public static ConcreteDateTimeOffsetId Create(DateTimeOffset value) => new(value);
        public static bool TryCreate(DateTimeOffset value, out ConcreteDateTimeOffsetId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteTimeOnlyId(TimeOnly Value) : IStrongId<ConcreteTimeOnlyId, TimeOnly>
    {
        public static string PrimitiveName => nameof(ConcreteTimeOnlyId);
        public bool IsDefault => Value == default;
        public static ConcreteTimeOnlyId Empty => new(default);
        public static ConcreteTimeOnlyId Create() => new(default);
        public static ConcreteTimeOnlyId Create(TimeOnly value) => new(value);
        public static bool TryCreate(TimeOnly value, out ConcreteTimeOnlyId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteDecimalId(decimal Value) : IStrongId<ConcreteDecimalId, decimal>
    {
        public static string PrimitiveName => nameof(ConcreteDecimalId);
        public bool IsDefault => Value == 0;
        public static ConcreteDecimalId Empty => new(0);
        public static ConcreteDecimalId Create() => new(0);
        public static ConcreteDecimalId Create(decimal value) => new(value);
        public static bool TryCreate(decimal value, out ConcreteDecimalId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteDoubleId(double Value) : IStrongId<ConcreteDoubleId, double>
    {
        public static string PrimitiveName => nameof(ConcreteDoubleId);
        public bool IsDefault => Value == 0;
        public static ConcreteDoubleId Empty => new(0);
        public static ConcreteDoubleId Create() => new(0);
        public static ConcreteDoubleId Create(double value) => new(value);
        public static bool TryCreate(double value, out ConcreteDoubleId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public readonly record struct ConcreteFloatId(float Value) : IStrongId<ConcreteFloatId, float>
    {
        public static string PrimitiveName => nameof(ConcreteFloatId);
        public bool IsDefault => Value == 0;
        public static ConcreteFloatId Empty => new(0);
        public static ConcreteFloatId Create() => new(0);
        public static ConcreteFloatId Create(float value) => new(value);
        public static bool TryCreate(float value, out ConcreteFloatId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
    }

    public sealed class ConcreteClassId : IStrongId<ConcreteClassId, string>
    {
        public string Value { get; }
        public ConcreteClassId(string value) { Value = value; }
        public static string PrimitiveName => nameof(ConcreteClassId);
        public bool IsDefault => string.IsNullOrEmpty(Value);
        public static ConcreteClassId Empty => new(string.Empty);
        public static ConcreteClassId Create() => new(string.Empty);
        public static ConcreteClassId Create(string value) => new(value);
        public static bool TryCreate(string value, out ConcreteClassId result, out PrimitiveError validationError) { result = new(value); validationError = default; return true; }
        public bool Equals(ConcreteClassId? other) => other is not null && Value == other.Value;
        public override bool Equals(object? obj) => obj is ConcreteClassId other && Equals(other);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
    }

    [Fact]
    public void SetValue_InfersAllSupportedPrimitiveDbTypes()
    {
        void Check<TSelf, TValue>(TSelf id, DbType expected)
            where TSelf : notnull, IStrongId<TSelf, TValue>
            where TValue : notnull, IEquatable<TValue>
        {
            var handler = new StrongIdTypeHandler<TSelf, TValue>();
            var param = new FakeDbDataParameter();
            handler.SetValue(param, id);
            param.DbType.Should().Be(expected);
        }

        Check<OrderId, Guid>(OrderId.Create(), DbType.Guid);
        Check<ProductCode, string>(ProductCode.Create("CODE-1"), DbType.String);
        Check<DepartmentId, int>(DepartmentId.Create(1), DbType.Int32);
        Check<SequenceId, long>(SequenceId.Create(100L), DbType.Int64);
        Check<DateOnlyId, DateOnly>(DateOnlyId.Create(new DateOnly(2026, 9, 21)), DbType.Date);
        Check<ConcreteShortId, short>(new((short)10), DbType.Int16);
        Check<ConcreteByteId, byte>(new((byte)5), DbType.Byte);
        Check<ConcreteBoolId, bool>(new(true), DbType.Boolean);
        Check<ConcreteDateTimeId, DateTime>(new(DateTime.UtcNow), DbType.DateTime2);
        Check<ConcreteDateTimeOffsetId, DateTimeOffset>(new(DateTimeOffset.UtcNow), DbType.DateTimeOffset);
        Check<ConcreteTimeOnlyId, TimeOnly>(new(new TimeOnly(12, 0)), DbType.Time);
        Check<ConcreteDecimalId, decimal>(new(100.5m), DbType.Decimal);
        Check<ConcreteDoubleId, double>(new(100.5), DbType.Double);
        Check<ConcreteFloatId, float>(new(100.5f), DbType.Single);
    }

    [Fact]
    public void SetValue_WithClassStrongIdNull_SetsDBNull()
    {
        var handler = new StrongIdTypeHandler<ConcreteClassId, string>();
        var param = new FakeDbDataParameter();
        handler.SetValue(param, null);
        param.Value.Should().Be(DBNull.Value);
    }

    #endregion
}





