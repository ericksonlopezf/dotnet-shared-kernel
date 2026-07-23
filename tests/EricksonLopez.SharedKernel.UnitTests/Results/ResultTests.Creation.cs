using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Success_WithValue_ShouldExposeValue()
    {
        // Act
        var result = Result.Success(TestValues.Numbers.Positive);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act
        var result = Result.Failure(error);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure<string>(Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    [Fact]
    public void Success_WithNullValue_ShouldNotThrow()
    {
        // Act
        var act = () => Result.Success<string>(null!);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act
        Result<string> result = error;

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void ImplicitConversion_FromErrorToNonGenericResult_ShouldCreateFailure()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        
        // Act
        Result result = error;

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        // Act
        Result<int> result = TestValues.Numbers.AlternativePositive;

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.AlternativePositive);
    }

    [Fact]
    public void Constructor_SuccessWithNonNoneError_ShouldThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act
        var act = () => new ExposedResult(isSuccess: true, error);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful result cannot have an error*");
    }

    [Fact]
    public void Constructor_FailureWithErrorNone_ShouldThrow()
    {
        // Act
        var act = () => new ExposedResult(isSuccess: false, Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failure result must have an error*");
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrowWithExactMessage()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<string>(error);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*Cannot access Value of a failed result. Error: {error}*");
    }

    [Fact]
    public void Success_NonGeneric_ReturnsSameInstance_EveryCall()
    {
        // Act
        var r1 = Result.Success();
        var r2 = Result.Success();
        var r3 = Result.Success();

        // Assert
        r1.Should().BeSameAs(r2, "non-generic Success() is a cached singleton");
        r2.Should().BeSameAs(r3);
    }

    [Fact]
    public void Success_Generic_WithReferenceType_ShouldExposeExactReference()
    {
        // Arrange
        var obj = new object();

        // Act
        var result = Result.Success(obj);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().BeSameAs(obj, "the value reference must be preserved exactly");
    }
}

file sealed class ExposedResult(bool isSuccess, Error error) : Result(isSuccess, error);
