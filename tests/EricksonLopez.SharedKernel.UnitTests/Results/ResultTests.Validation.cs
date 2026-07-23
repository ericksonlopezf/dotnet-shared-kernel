using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void Ensure_NonGeneric_WhenPredicateTrue_ShouldReturnSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var ensured = result.Ensure(() => true, Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Assert
        ensured.ShouldBeSuccess();
    }

    [Fact]
    public void Ensure_NonGeneric_WhenPredicateFalse_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Success();

        // Act
        var ensured = result.Ensure(() => false, error);

        // Assert
        ensured.ShouldBeFailure();
        ensured.ShouldHaveError(error);
    }

    [Fact]
    public void Ensure_NonGeneric_OnExistingFailure_ShouldShortCircuit()
    {
        // Arrange
        var originalError = Error.NotFound(TestValues.Strings.ErrorCode, "Original");
        var newError = Error.Failure(TestValues.Strings.AlternativeErrorCode, "New");
        var predicateEvaluated = false;
        var result = Result.Failure(originalError);

        // Act
        var ensured = result.Ensure(() => { predicateEvaluated = true; return true; }, newError);

        // Assert
        predicateEvaluated.Should().BeFalse();
        ensured.ShouldHaveError(originalError);
    }

    [Fact]
    public void Ensure_Generic_WhenPredicateTrue_ShouldPreserveValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var ensured = result.Ensure(v => v > 0, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Assert
        ensured.ShouldBeSuccess();
        ensured.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void Ensure_Generic_WhenPredicateFalse_ShouldReturnFailure()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Success(TestValues.Numbers.Negative);

        // Act
        var ensured = result.Ensure(v => v > 0, error);

        // Assert
        ensured.ShouldBeFailure();
        ensured.ShouldHaveError(error);
    }

    [Fact]
    public void Recover_OnFailure_ShouldApplyRecovery()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);

        // Act
        var recovered = result.Recover(e => Result.Success(TestValues.Numbers.Zero));

        // Assert
        recovered.ShouldBeSuccess();
        recovered.Value.Should().Be(TestValues.Numbers.Zero);
    }

    [Fact]
    public void Recover_OnSuccess_ShouldReturnUnchanged()
    {
        // Arrange
        var recoveryInvoked = false;
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var recovered = result.Recover(e => { recoveryInvoked = true; return Result.Success(TestValues.Numbers.Zero); });

        // Assert
        recoveryInvoked.Should().BeFalse();
        recovered.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void Recover_CanReturnFailure()
    {
        // Arrange
        var fallbackError = Error.Unavailable(TestValues.Strings.AlternativeErrorCode, "Both sources failed");
        var originalError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(originalError);

        // Act
        var recovered = result.Recover(e => Result.Failure<int>(fallbackError));

        // Assert
        recovered.ShouldBeFailure();
        recovered.ShouldHaveError(fallbackError);
    }

    [Fact]
    public void Finally_NonGeneric_AlwaysExecutes_OnSuccess()
    {
        // Arrange
        var executed = false;
        var result = Result.Success();

        // Act
        result.Finally(r => executed = true);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void Finally_NonGeneric_AlwaysExecutes_OnFailure()
    {
        // Arrange
        var executed = false;
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);

        // Act
        result.Finally(r => executed = true);

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public void Finally_Generic_AlwaysExecutes()
    {
        // Arrange
        bool? wasSuccessOnSuccess = null;
        bool? wasSuccessOnFailure = null;

        var successResult = Result.Success(TestValues.Numbers.Positive);
        var failureResult = Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        successResult.Finally(r => wasSuccessOnSuccess = r.IsSuccess);
        failureResult.Finally(r => wasSuccessOnFailure = r.IsSuccess);

        // Assert
        wasSuccessOnSuccess.Should().BeTrue();
        wasSuccessOnFailure.Should().BeFalse();
    }
}
