using System;
using EricksonLopez.SharedKernel.Domain;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Specifications;

// Test AOT compatibility of the SharedKernel types

var result = Result.Success("AOT Test successful");
if (result.TryGetValue(out var val))
{
    Console.WriteLine(val);
}

var error = Error.Validation("Console.Aot", "Testing AOT warnings");
var failed = Result<string>.Failure(error);
Console.WriteLine(failed.Error.Code);

var entity = new TestEntity(Guid.NewGuid());
Console.WriteLine($"Entity: {entity.Id}");

var spec = new ActiveSpec().And(new NotEmptySpec());
Console.WriteLine($"Spec satisfied? {spec.IsSatisfiedBy(entity)}");

sealed class TestEntity : Entity<Guid>
{
    public TestEntity(Guid id) => Id = id;
    public bool IsActive { get; set; } = true;
}

sealed class ActiveSpec : Specification<TestEntity>
{
    public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression() => x => x.IsActive;
}

sealed class NotEmptySpec : Specification<TestEntity>
{
    public override System.Linq.Expressions.Expression<Func<TestEntity, bool>> ToExpression() => x => x.Id != Guid.Empty;
}
