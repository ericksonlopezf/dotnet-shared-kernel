// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.Dapper.Tests.Fakes;

using System.Data;

#pragma warning disable CS8766, CS8767
public class FakeDbDataParameter : IDbDataParameter
{
    public DbType DbType { get; set; }
    public ParameterDirection Direction { get; set; }
    public bool IsNullable => true;
    public string? ParameterName { get; set; } = string.Empty;
    public string? SourceColumn { get; set; } = string.Empty;
    public DataRowVersion SourceVersion { get; set; }
    public object? Value { get; set; }
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public int Size { get; set; }
}
#pragma warning restore CS8766, CS8767


