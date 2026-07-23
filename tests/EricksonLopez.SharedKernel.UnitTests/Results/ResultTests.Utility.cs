using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using EricksonLopez.SharedKernel.UnitTests.Common;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultTests
{
    [Fact]
    public void TryGetValue_OnSuccess_ShouldReturnTrueAndValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeTrue();
        value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void TryGetValue_OnFailure_ShouldReturnFalse()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [Fact]
    public void TryGetValue_ReferenceType_OnFailure_ShouldReturnNull()
    {
        // Arrange
        var result = Result.Failure<string>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var got = result.TryGetValue(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryGetError_NonGeneric_OnFailure_ShouldReturnTrueAndError()
    {
        // Arrange
        var expectedError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(expectedError);

        // Act
        var got = result.TryGetError(out var error);

        // Assert
        got.Should().BeTrue();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void TryGetError_NonGeneric_OnSuccess_ShouldReturnFalse()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var got = result.TryGetError(out var error);

        // Assert
        got.Should().BeFalse();
    }

    [Fact]
    public void TryGetError_Generic_OnFailure_ShouldReturnTrueAndError()
    {
        // Arrange
        var expectedError = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(expectedError);

        // Act
        var got = result.TryGetError(out var error);

        // Assert
        got.Should().BeTrue();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void TryGetError_Generic_OnSuccess_ShouldReturnFalse()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var got = result.TryGetError(out var error);

        // Assert
        got.Should().BeFalse();
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var value = result.GetValueOrDefault(TestValues.Numbers.Zero);

        // Assert
        value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var value = result.GetValueOrDefault(TestValues.Numbers.AlternativePositive);

        // Assert
        value.Should().Be(TestValues.Numbers.AlternativePositive);
    }

    [Fact]
    public void GetValueOrDefault_WithFunc_OnSuccess_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);
        var fallbackInvoked = false;

        // Act
        var value = result.GetValueOrDefault(e => { fallbackInvoked = true; return TestValues.Numbers.Zero; });

        // Assert
        value.Should().Be(TestValues.Numbers.Positive);
        fallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public void GetValueOrDefault_WithFunc_OnFailure_ShouldInvokeFallback()
    {
        // Arrange
        var result = Result.Failure<int>(Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var value = result.GetValueOrDefault(e => e.Type == ErrorType.NotFound ? TestValues.Numbers.Negative : TestValues.Numbers.AlternativeNegative);

        // Assert
        value.Should().Be(TestValues.Numbers.Negative);
    }

    [Fact]
    public void ToResult_OnSuccess_ShouldReturnSuccessResult()
    {
        // Arrange
        var typed = Result.Success(TestValues.Numbers.Positive);

        // Act
        var result = typed.ToResult();

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public void ToResult_OnFailure_ShouldPreserveError()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var typed = Result.Failure<int>(error);

        // Act
        var result = typed.ToResult();

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public void Deconstruct_OnSuccess_ShouldExposeComponents()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);

        // Act
        var (isSuccess, value, error) = result;

        // Assert
        isSuccess.Should().BeTrue();
        value.Should().Be(TestValues.Numbers.Positive);
        error.Should().Be(Error.None);
    }

    [Fact]
    public void Deconstruct_OnFailure_ShouldExposeComponents()
    {
        // Arrange
        var expectedError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<string>(expectedError);

        // Act
        var (isSuccess, value, error) = result;

        // Assert
        isSuccess.Should().BeFalse();
        value.Should().BeNull();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void Deconstruct_CanBeUsedInIfStatement()
    {
        // Arrange
        var result = Result.Success("hello");

        // Act
        var (ok, value, _) = result;
        var output = ok ? value!.ToUpper() : "FALLBACK";

        // Assert
        output.Should().Be("HELLO");
    }
}
