using EricksonLopez.SharedKernel.Results;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Results;

public sealed class ResultTests
{
    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithValue_ShouldExposeValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    // ─── Failure ──────────────────────────────────────────────────────────────

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        var error = Error.NotFound("User.NotFound", "User was not found.");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<string>(Error.NotFound("X.NotFound", "Not found"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    // ─── Guard clauses ────────────────────────────────────────────────────────

    [Fact]
    public void Success_WithNullValue_ShouldNotThrow()
    {
        var act = () => Result.Success<string>(null!);
        act.Should().NotThrow();
    }

    // ─── Implicit conversion ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        var error = Error.Validation("Name.Empty", "Name cannot be empty.");
        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 99;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    // ─── Map ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(5);
        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var error = Error.Failure("X.Error", "Something went wrong");
        var result = Result.Failure<int>(error);
        var mapped = result.Map(x => x.ToString());

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    // ─── Bind ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_ShouldInvokeNext()
    {
        var result = Result.Success(5);
        var bound = result.Bind(x => Result.Success(x + 1));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(6);
    }

    [Fact]
    public void Bind_OnFailure_ShouldNotInvokeNext()
    {
        var error = Error.Failure("X.Error", "Error");
        var result = Result.Failure<int>(error);
        var invoked = false;

        var bound = result.Bind(x =>
        {
            invoked = true;
            return Result.Success(x);
        });

        invoked.Should().BeFalse();
        bound.IsFailure.Should().BeTrue();
    }
}
