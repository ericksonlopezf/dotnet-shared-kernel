using System;
using System.Linq.Expressions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Domain;
using EricksonLopez.SharedKernel.Specifications;

namespace AotTest;

public sealed record UserCreated(Guid Id) : IDomainEvent;

public sealed class User : AggregateRoot<Guid>
{
    public string Name { get; private set; }

    public User(Guid id, string name)
    {
        Id = id;
        Name = name;
        RaiseDomainEvent(new UserCreated(id));
    }
}

public sealed class UserHasNameSpec : Specification<User>
{
    private readonly string _name;
    public UserHasNameSpec(string name) => _name = name;

    public override Expression<Func<User, bool>> ToExpression() => u => u.Name == _name;
    
    protected override bool Evaluate(User candidate) => candidate.Name == _name;
}

public class Program
{
    public static void Main()
    {
        var user = new User(Guid.NewGuid(), "Erick");
        var result = Result.Success(user);
        
        var spec = new UserHasNameSpec("Erick").And(new UserHasNameSpec("Erick"));
        var isMatch = spec.IsSatisfiedBy(user);
        
        Console.WriteLine($"IsSuccess: {result.IsSuccess}, Match: {isMatch}");
        
        var error = Error.Validation("User.Error", "Validation Error");
        Console.WriteLine(error.ToString());
    }
}
