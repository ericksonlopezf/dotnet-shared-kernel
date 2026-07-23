using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void Match_NonGeneric_OnSuccess_ShouldInvokeSuccessFunc()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var output = result.Match(() => TestValues.Strings.BecauseExpectedSuccess, e => $"fail: {e.Code}");

        // Assert
        output.Should().Be(TestValues.Strings.BecauseExpectedSuccess);
    }

    [Fact]
    public void Match_NonGeneric_OnFailure_ShouldInvokeFailureFunc()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);

        // Act
        var output = result.Match(() => TestValues.Strings.BecauseExpectedSuccess, e => $"fail: {e.Code}");

        // Assert
        output.Should().Be($"fail: {TestValues.Strings.ErrorCode}");
    }

    [Fact]
    public void Match_Generic_OnSuccess_ShouldInvokeSuccessFunc()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var output = result.Match(v => $"value: {v}", e => $"fail: {e.Code}");

        // Assert
        output.Should().Be($"value: {TestValues.Numbers.Positive}");
    }

    [Fact]
    public void Match_Generic_OnFailure_ShouldInvokeFailureFunc()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        var output = result.Match(v => $"value: {v}", e => $"fail: {e.Code}");

        // Assert
        output.Should().Be($"fail: {TestValues.Strings.ErrorCode}");
    }

    [Fact]
    public void Tap_NonGeneric_OnSuccess_ShouldExecuteAction()
    {
        // Arrange
        var executed = false;
        var result = Result.Success();

        // Act
        var tapped = result.Tap(() => executed = true);

        // Assert
        executed.Should().BeTrue();
        tapped.ShouldBeSuccess();
    }

    [Fact]
    public void Tap_NonGeneric_OnFailure_ShouldNotExecuteAction()
    {
        // Arrange
        var executed = false;
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);

        // Act
        result.Tap(() => executed = true);

        // Assert
        executed.Should().BeFalse();
    }

    [Fact]
    public void Tap_Generic_OnSuccess_ShouldExecuteWithValue()
    {
        // Arrange
        int? captured = null;
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var tapped = result.Tap(v => captured = v);

        // Assert
        captured.Should().Be(TestValues.Numbers.Positive);
        tapped.ShouldBeSuccess();
        tapped.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void Tap_Generic_OnFailure_ShouldNotExecute()
    {
        // Arrange
        int? captured = null;
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        result.Tap(v => captured = v);

        // Assert
        captured.Should().BeNull();
    }

    [Fact]
    public void TapError_NonGeneric_OnFailure_ShouldExecuteWithError()
    {
        // Arrange
        Error? captured = null;
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);

        // Act
        var tapped = result.TapError(e => captured = e);

        // Assert
        captured.Should().Be(error);
        tapped.ShouldBeFailure();
    }

    [Fact]
    public void TapError_NonGeneric_OnSuccess_ShouldNotExecute()
    {
        // Arrange
        Error? captured = null;
        var result = Result.Success();

        // Act
        result.TapError(e => captured = e);

        // Assert
        captured.Should().BeNull();
    }

    [Fact]
    public void TapError_Generic_OnFailure_ShouldExecuteWithError()
    {
        // Arrange
        Error? captured = null;
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        var tapped = result.TapError(e => captured = e);

        // Assert
        captured.Should().Be(error);
        tapped.ShouldBeFailure();
    }

    [Fact]
    public void TapError_Generic_OnSuccess_ShouldNotExecute()
    {
        // Arrange
        Error? captured = null;
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        result.TapError(e => captured = e);

        // Assert
        captured.Should().BeNull();
    }

    [Fact]
    public void TapError_Generic_ShouldReturnTypedResult()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        var output = result
            .TapError(e => { })
            .Match(v => v, e => TestValues.Numbers.Negative);

        // Assert
        output.Should().Be(TestValues.Numbers.Negative);
    }
}
