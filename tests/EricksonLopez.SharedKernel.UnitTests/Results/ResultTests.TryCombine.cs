using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void Try_Action_OnSuccess_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = Result.Try(
            () => { /* no exception */ },
            ex => Error.Unexpected(TestValues.Strings.ErrorCode, ex.Message));

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Try_Action_OnException_ShouldReturnFailure()
    {
        // Arrange
        var errorMessage = "boom";

        // Act
        var result = Result.Try(
            () => throw new InvalidOperationException(errorMessage),
            ex => Error.Unexpected(TestValues.Strings.ErrorCode, ex.Message));

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorType(ErrorType.Unexpected);
        result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        result.Error.Description.Should().Be(errorMessage);
    }

    [Fact]
    public void Try_Func_OnSuccess_ShouldReturnValue()
    {
        // Arrange & Act
        var result = Result.Try(
            () => TestValues.Numbers.Positive,
            ex => Error.Unexpected(TestValues.Strings.ErrorCode, ex.Message));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void Try_Func_OnException_ShouldReturnFailure()
    {
        // Arrange & Act
        var result = Result.Try(
            () => int.Parse("not-a-number"),
            ex => Error.Validation(TestValues.Strings.ErrorCode, ex.Message));

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorType(ErrorType.Validation);
        result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
    }

    [Fact]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        // Arrange
        var r1 = Result.Success();
        var r2 = Result.Success();
        var r3 = Result.Success();

        // Act
        var result = Result.Combine(r1, r2, r3);

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Combine_OneFailure_ShouldReturnThatError()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var r1 = Result.Success();
        var r2 = Result.Failure(error);
        var r3 = Result.Success();

        // Act
        var result = Result.Combine(r1, r2, r3);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Combine_MultipleFailures_ShouldReturnCompoundError()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var r1 = Result.Failure(error1);
        var r2 = Result.Success();
        var r3 = Result.Failure(error2);

        // Act
        var result = Result.Combine(r1, r2, r3);

        // Assert
        result.ShouldBeFailure();
        result.Error.Code.Should().Be("Result.CombinedErrors");
        result.Error.HasInnerErrors.Should().BeTrue();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Empty_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = Result.Combine();

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Combine_Typed_AllSuccess_ShouldReturnValueList()
    {
        // Arrange
        var r1 = Result.Success(TestValues.Numbers.Zero);
        var r2 = Result.Success(TestValues.Numbers.Positive);
        var r3 = Result.Success(TestValues.Numbers.AlternativePositive);

        // Act
        var result = Result.Combine<int>(r1, r2, r3);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().BeEquivalentTo([TestValues.Numbers.Zero, TestValues.Numbers.Positive, TestValues.Numbers.AlternativePositive]);
    }

    [Fact]
    public void Combine_Typed_WithFailure_ShouldAggregateErrors()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var r1 = Result.Success(TestValues.Numbers.Zero);
        var r2 = Result.Failure<int>(error1);
        var r3 = Result.Failure<int>(error2);

        // Act
        var result = Result.Combine<int>(r1, r2, r3);

        // Assert
        result.ShouldBeFailure();
        result.Error.HasInnerErrors.Should().BeTrue();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Typed_OneFailure_ShouldReturnThatError()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var r1 = Result.Success(TestValues.Numbers.Zero);
        var r2 = Result.Failure<int>(error);
        var r3 = Result.Success(TestValues.Numbers.AlternativePositive);

        // Act
        var result = Result.Combine<int>(r1, r2, r3);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
        result.Error.HasInnerErrors.Should().BeFalse();
    }

    [Fact]
    public void Combine_Typed_Empty_ShouldReturnSuccess()
    {
        // Arrange & Act
        var result = Result.Combine<int>();

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Combine_Tuple2_AllSuccess_ShouldReturnTuple()
    {
        // Arrange
        var r1 = Result.Success(TestValues.Strings.Sample);
        var r2 = Result.Success(TestValues.Numbers.Positive);

        // Act
        var result = Result.Combine(r1, r2);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be((TestValues.Strings.Sample, TestValues.Numbers.Positive));
    }

    [Fact]
    public void Combine_Tuple2_WithFailure_ShouldReturnError()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var r1 = Result.Success(TestValues.Strings.Sample);
        var r2 = Result.Failure<int>(error);

        // Act
        var result = Result.Combine(r1, r2);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Combine_Tuple3_AllSuccess_ShouldReturnTuple()
    {
        // Arrange
        var r1 = Result.Success(TestValues.Strings.Sample);
        var r2 = Result.Success(TestValues.Numbers.Positive);
        var r3 = Result.Success(true);

        // Act
        var result = Result.Combine(r1, r2, r3);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be((TestValues.Strings.Sample, TestValues.Numbers.Positive, true));
    }

    [Fact]
    public void Combine_Tuple2_WithMultipleFailures_ShouldAggregateErrors()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var r1 = Result.Failure<string>(error1);
        var r2 = Result.Failure<int>(error2);

        // Act
        var result = Result.Combine(r1, r2);

        // Assert
        result.ShouldBeFailure();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Tuple2_WithFirstSuccessSecondFailure_ShouldReturnError()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var r1 = Result.Success(TestValues.Strings.Sample);
        var r2 = Result.Failure<int>(error);

        // Act
        var result = Result.Combine(r1, r2);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Combine_Tuple2_WithFirstFailureSecondSuccess_ShouldReturnError()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var r1 = Result.Failure<string>(error);
        var r2 = Result.Success(TestValues.Numbers.Positive);

        // Act
        var result = Result.Combine(r1, r2);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Combine_Tuple3_WithFailures_ShouldAggregateErrors()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var error3 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act - 1 failure
        var r1 = Result.Combine(Result.Failure<string>(error1), Result.Success(TestValues.Numbers.Positive), Result.Success(true));
        var r2 = Result.Combine(Result.Success(TestValues.Strings.Sample), Result.Failure<int>(error2), Result.Success(true));
        var r3 = Result.Combine(Result.Success(TestValues.Strings.Sample), Result.Success(TestValues.Numbers.Positive), Result.Failure<bool>(error3));

        // Assert - 1 failure
        r1.ShouldBeFailure();
        r1.ShouldHaveError(error1);
        r2.ShouldBeFailure();
        r2.ShouldHaveError(error2);
        r3.ShouldBeFailure();
        r3.ShouldHaveError(error3);

        // Act - multiple failures
        var r4 = Result.Combine(Result.Failure<string>(error1), Result.Failure<int>(error2), Result.Failure<bool>(error3));
        
        // Assert - multiple failures
        r4.ShouldBeFailure();
        r4.Error.InnerErrors.Should().HaveCount(3);
    }

    [Fact]
    public void FullPipeline_ShouldComposeCorrectly()
    {
        // Arrange
        var logged = false;

        // Act
        var result = Result.Success(10)
            .Ensure(v => v > 0, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => v * 2)
            .Tap(v => logged = true)
            .Bind(v => v <= 100
                ? Result.Success($"Value: {v}")
                : Result.Failure<string>(Error.Failure(TestValues.Strings.ErrorCode, "Too large")));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be("Value: 20");
        logged.Should().BeTrue();
    }

    [Fact]
    public void FullPipeline_FailureShortCircuits()
    {
        // Arrange
        var mapExecuted = false;

        // Act
        var result = Result.Success(TestValues.Numbers.Negative)
            .Ensure(v => v > 0, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => { mapExecuted = true; return v * 2; })
            .Tap(v => { });

        // Assert
        result.ShouldBeFailure();
        mapExecuted.Should().BeFalse();
    }
}
