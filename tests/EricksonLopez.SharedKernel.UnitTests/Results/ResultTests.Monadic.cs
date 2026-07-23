using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.ShouldBeSuccess();
        mapped.Value.Should().Be(TestValues.Numbers.Positive * 2);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.ShouldBeFailure();
        mapped.ShouldHaveError(error);
    }

    [Fact]
    public void Bind_OnSuccess_ShouldInvokeNext()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var bound = result.Bind(x => Result.Success(x + 1));

        // Assert
        bound.ShouldBeSuccess();
        bound.Value.Should().Be(TestValues.Numbers.Positive + 1);
    }

    [Fact]
    public void Bind_OnFailure_ShouldNotInvokeNext()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);
        var invoked = false;

        // Act
        var bound = result.Bind(x =>
        {
            invoked = true;
            return Result.Success(x);
        });

        // Assert
        invoked.Should().BeFalse();
        bound.ShouldBeFailure();
        bound.ShouldHaveError(error);
    }

    [Fact]
    public void MapError_NonGeneric_OnFailure_ShouldTransformError()
    {
        // Arrange
        var originalError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(originalError);

        // Act
        var mapped = result.MapError(e =>
            Error.Failure(TestValues.Strings.AlternativeErrorCode, $"Operation failed: {e.Description}"));

        // Assert
        mapped.ShouldBeFailure();
        mapped.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
        mapped.Error.Description.Should().Be($"Operation failed: {originalError.Description}");
    }

    [Fact]
    public void MapError_NonGeneric_OnSuccess_ShouldReturnUnchanged()
    {
        // Arrange
        var result = Result.Success();
        var mapperInvoked = false;

        // Act
        var mapped = result.MapError(e => { mapperInvoked = true; return e; });

        // Assert
        mapped.ShouldBeSuccess();
        mapperInvoked.Should().BeFalse();
    }

    [Fact]
    public void MapError_Generic_OnFailure_ShouldTransformError()
    {
        // Arrange
        var originalError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(originalError);

        // Act
        var mapped = result.MapError(e =>
            Error.Unavailable(TestValues.Strings.AlternativeErrorCode, $"Adapted: {e.Code}"));

        // Assert
        mapped.ShouldBeFailure();
        mapped.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
        mapped.ShouldHaveErrorType(ErrorType.Unavailable);
    }

    [Fact]
    public void MapError_Generic_OnSuccess_ShouldPreserveValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var mapped = result.MapError(e => Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Assert
        mapped.ShouldBeSuccess();
        mapped.Value.Should().Be(TestValues.Numbers.Positive);
    }
}
