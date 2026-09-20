// Copyright © Erickson Lopez. MIT License.
using System;
using System.Data;
using EricksonLopez.SharedKernel.Dapper.Tests.Fakes;
using Xunit;

namespace EricksonLopez.SharedKernel.Dapper.Tests;

public sealed class BclTypeHandlerTests
{
    [Fact]
    public void DateOnlyTypeHandler_SetValue_SetsDateTypeAndValue()
    {
        var handler = new DateOnlyTypeHandler();
        var parameter = new FakeDbDataParameter();
        var date = new DateOnly(2026, 9, 1);

        handler.SetValue(parameter, date);

        Assert.Equal(DbType.Date, parameter.DbType);
        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0), parameter.Value);
    }

    [Fact]
    public void DateOnlyTypeHandler_Parse_HandlesVariousTypes()
    {
        var handler = new DateOnlyTypeHandler();
        var expected = new DateOnly(2026, 9, 1);

        Assert.Equal(expected, handler.Parse(expected));
        Assert.Equal(expected, handler.Parse(new DateTime(2026, 9, 1, 15, 30, 0)));
        Assert.Equal(expected, handler.Parse("2026-09-01"));
    }

    [Fact]
    public void DateTimeOffsetTypeHandler_SetValue_SetsDateTimeOffsetTypeAndValue()
    {
        var handler = new DateTimeOffsetTypeHandler();
        var parameter = new FakeDbDataParameter();
        var dto = new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.FromHours(-4));

        handler.SetValue(parameter, dto);

        Assert.Equal(DbType.DateTimeOffset, parameter.DbType);
        Assert.Equal(dto, parameter.Value);
    }

    [Fact]
    public void DateTimeOffsetTypeHandler_Parse_NormalizesToUtcWhenGivenDateTime()
    {
        var handler = new DateTimeOffsetTypeHandler();
        var dt = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Unspecified);

        var parsed = handler.Parse(dt);

        Assert.Equal(TimeSpan.Zero, parsed.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 1, 12, 0, 0, TimeSpan.Zero), parsed);
    }

    [Fact]
    public void TimeOnlyTypeHandler_SetValue_SetsTimeTypeAndValue()
    {
        var handler = new TimeOnlyTypeHandler();
        var parameter = new FakeDbDataParameter();
        var time = new TimeOnly(14, 30, 45);

        handler.SetValue(parameter, time);

        Assert.Equal(DbType.Time, parameter.DbType);
        Assert.Equal(new TimeSpan(14, 30, 45), parameter.Value);
    }

    [Fact]
    public void TimeOnlyTypeHandler_Parse_HandlesVariousTypes()
    {
        var handler = new TimeOnlyTypeHandler();
        var expected = new TimeOnly(14, 30, 45);

        Assert.Equal(expected, handler.Parse(expected));
        Assert.Equal(expected, handler.Parse(new TimeSpan(14, 30, 45)));
        Assert.Equal(expected, handler.Parse("14:30:45"));
        Assert.Throws<DataException>(() => handler.Parse("invalid-time-format"));
    }

    [Fact]
    public void DapperBclTypeHandlerRegistry_RegisterAll_ExecutesWithoutException()
    {
        var exception = Record.Exception(() => DapperBclTypeHandlerRegistry.RegisterAll());
        Assert.Null(exception);
    }
}
