// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Dapper;
using EricksonLopez.DomainPrimitives;
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

    #endregion
}





