namespace EricksonLopez.SharedKernel.UnitTests.Testing;

public sealed class ResultAssertionsTests
{
    // ─── Non-Generic Result ───────────────────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_WhenResultIsSuccess_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Success();
        
        // Act
        var act = () => result.ShouldBeSuccess();
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeSuccess_WhenResultIsFailure_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));
        
        // Act
        var act = () => result.ShouldBeSuccess(TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage($"*Expected success but failed with error*{TestValues.Strings.BecauseMustFail}*");
    }

    [Fact]
    public void ShouldBeFailure_WhenResultIsFailure_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Failure(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));
        
        // Act
        var act = () => result.ShouldBeFailure();
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeFailure_WhenResultIsSuccess_ShouldThrow()
    {
        // Arrange
        var result = Result.Success();
        
        // Act
        var act = () => result.ShouldBeFailure(TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage($"*Expected failure but was successful*{TestValues.Strings.BecauseMustFail}*");
    }

    [Fact]
    public void ShouldHaveError_WhenErrorMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);
        
        // Act
        var act = () => result.ShouldHaveError(error);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveError_WhenResultIsSuccess_ShouldThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Success();
        
        // Act
        var act = () => result.ShouldHaveError(error);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected failure but was successful*");
    }

    [Fact]
    public void ShouldHaveErrorType_WhenTypeMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);
        
        // Act
        var act = () => result.ShouldHaveErrorType(ErrorType.NotFound);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveErrorType_WhenTypeMismatches_ShouldThrow()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);
        
        // Act
        var act = () => result.ShouldHaveErrorType(ErrorType.NotFound, TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected*NotFound*but found*Validation*");
    }

    [Fact]
    public void ShouldHaveErrorCode_WhenCodeMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);
        
        // Act
        var act = () => result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveErrorCode_WhenCodeMismatches_ShouldThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(error);
        
        // Act
        var act = () => result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode, TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected*result.Error.Code*");
    }

    // ─── Generic Result<T> ────────────────────────────────────────────────────

    [Fact]
    public void ShouldBeSuccess_Generic_WhenResultIsSuccess_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);
        
        // Act
        var act = () => result.ShouldBeSuccess();
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeSuccess_Generic_WhenResultIsFailure_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));
        
        // Act
        var act = () => result.ShouldBeSuccess(TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage($"*Expected success but failed with error*{TestValues.Strings.BecauseMustFail}*");
    }

    [Fact]
    public void ShouldBeFailure_Generic_WhenResultIsFailure_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));
        
        // Act
        var act = () => result.ShouldBeFailure();
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldBeFailure_Generic_WhenResultIsSuccess_ShouldThrow()
    {
        // Arrange
        var result = Result.Success(TestValues.Numbers.Positive);
        
        // Act
        var act = () => result.ShouldBeFailure(TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage($"*Expected failure but was successful*{TestValues.Strings.BecauseMustFail}*");
    }

    [Fact]
    public void ShouldHaveError_Generic_WhenErrorMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<int>(error);
        
        // Act
        var act = () => result.ShouldHaveError(error);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveError_Generic_WhenResultIsSuccess_ShouldThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Success(TestValues.Numbers.Positive);
        
        // Act
        var act = () => result.ShouldHaveError(error);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected failure but was successful*");
    }

    [Fact]
    public void ShouldHaveErrorType_Generic_WhenTypeMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<string>(error);
        
        // Act
        var act = () => result.ShouldHaveErrorType(ErrorType.NotFound);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveErrorType_Generic_WhenTypeMismatches_ShouldThrow()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<string>(error);
        
        // Act
        var act = () => result.ShouldHaveErrorType(ErrorType.NotFound, TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected*NotFound*but found*Validation*");
    }

    [Fact]
    public void ShouldHaveErrorCode_Generic_WhenCodeMatches_ShouldNotThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<bool>(error);
        
        // Act
        var act = () => result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        
        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveErrorCode_Generic_WhenCodeMismatches_ShouldThrow()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure<bool>(error);
        
        // Act
        var act = () => result.ShouldHaveErrorCode(TestValues.Strings.ErrorCode, TestValues.Strings.BecauseMustFail);
        
        // Assert
        act.Should().Throw<Exception>().WithMessage("*Expected*result.Error.Code*");
    }

    // ─── ShouldHaveInnerErrors ────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveInnerErrors_NonGeneric_WhenCountMatches_ShouldNotThrow()
    {
        // Arrange
        var inner1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var inner2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var compound = Error.Failure("Root.Error", "Two inner errors", inner1, inner2);
        var result = Result.Failure(compound);

        // Act
        var act = () => result.ShouldHaveInnerErrors(2);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveInnerErrors_NonGeneric_WhenCountMismatches_ShouldThrow()
    {
        // Arrange
        var inner = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var compound = Error.Failure("Root.Error", "One inner error", inner);
        var result = Result.Failure(compound);

        // Act
        var act = () => result.ShouldHaveInnerErrors(3);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveInnerErrors_NonGeneric_WhenNoInnerErrors_ShouldThrow()
    {
        // Arrange
        var simple = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var result = Result.Failure(simple);

        // Act
        var act = () => result.ShouldHaveInnerErrors(1);

        // Assert
        act.Should().Throw<Exception>().WithMessage("*no inner errors*");
    }

    [Fact]
    public void ShouldHaveInnerErrors_Generic_WhenCountMatches_ShouldNotThrow()
    {
        // Arrange
        var inner1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var inner2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var compound = Error.Failure("Root.Error", "Two inner errors", inner1, inner2);
        var result = Result.Failure<int>(compound);

        // Act
        var act = () => result.ShouldHaveInnerErrors(2);

        // Assert
        act.Should().NotThrow();
    }

    // ─── ShouldHaveDescription ────────────────────────────────────────────────

    [Fact]
    public void ShouldHaveDescription_NonGeneric_WhenDescriptionMatches_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Failure(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var act = () => result.ShouldHaveDescription(TestValues.Strings.ErrorMessage);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveDescription_NonGeneric_WhenDescriptionMismatches_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var act = () => result.ShouldHaveDescription(TestValues.Strings.AlternativeErrorMessage);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void ShouldHaveDescription_Generic_WhenDescriptionMatches_ShouldNotThrow()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var act = () => result.ShouldHaveDescription(TestValues.Strings.ErrorMessage);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldHaveDescription_Generic_WhenDescriptionMismatches_ShouldThrow()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage));

        // Act
        var act = () => result.ShouldHaveDescription(TestValues.Strings.AlternativeErrorMessage, TestValues.Strings.BecauseMustFail);

        // Assert
        act.Should().Throw<Exception>();
    }
}
