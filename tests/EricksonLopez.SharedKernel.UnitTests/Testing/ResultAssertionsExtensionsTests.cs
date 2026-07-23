using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Testing;
using Xunit.Sdk;
using Xunit;

namespace EricksonLopez.SharedKernel.UnitTests.Testing;

public sealed class ResultAssertionsExtensionsTests
{
    private static readonly Error TestError = Error.Validation("Test", "Test validation error");

    // ─── Result (Non-Generic) ─────────────────────────────────────────────────

    [Fact]
    public void Result_ShouldBeSuccess_WhenSuccess_DoesNotThrow()
    {
        var result = Result.Success();
        var act = () => result.ShouldBeSuccess();
        act.Should().NotThrow();
    }

    [Fact]
    public void Result_ShouldBeSuccess_WhenFailure_Throws()
    {
        var result = Result.Failure(TestError);
        var act = () => result.ShouldBeSuccess("Because I said so");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Because I said so*");
    }

    [Fact]
    public void Result_ShouldBeFailure_WhenFailure_DoesNotThrow()
    {
        var result = Result.Failure(TestError);
        var act = () => result.ShouldBeFailure();
        act.Should().NotThrow();
    }

    [Fact]
    public void Result_ShouldBeFailure_WhenSuccess_Throws()
    {
        var result = Result.Success();
        var act = () => result.ShouldBeFailure("Testing failure");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Expected failure but was successful. Testing failure*");
    }

    [Fact]
    public void Result_ShouldHaveError_WhenErrorMatches_DoesNotThrow()
    {
        var result = Result.Failure(TestError);
        var act = () => result.ShouldHaveError(TestError);
        act.Should().NotThrow();
    }

    [Fact]
    public void Result_ShouldHaveError_WhenSuccess_Throws()
    {
        var result = Result.Success();
        var act = () => result.ShouldHaveError(TestError, "Testing error");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Expected failure but was successful. Testing error*");
    }

    [Fact]
    public void Result_ShouldHaveError_WhenErrorMismatch_Throws()
    {
        var result = Result.Failure(Error.Validation("Other", "Other error"));
        var act = () => result.ShouldHaveError(TestError, "Testing mismatch");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*because Testing mismatch*");
    }

    // ─── Result<T> (Generic) ──────────────────────────────────────────────────

    [Fact]
    public void ResultT_ShouldBeSuccess_WhenSuccess_DoesNotThrow()
    {
        var result = Result.Success(42);
        var act = () => result.ShouldBeSuccess();
        act.Should().NotThrow();
    }

    [Fact]
    public void ResultT_ShouldBeSuccess_WhenFailure_Throws()
    {
        var result = Result.Failure<int>(TestError);
        var act = () => result.ShouldBeSuccess("Because generic");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Because generic*");
    }

    [Fact]
    public void ResultT_ShouldBeFailure_WhenFailure_DoesNotThrow()
    {
        var result = Result.Failure<int>(TestError);
        var act = () => result.ShouldBeFailure();
        act.Should().NotThrow();
    }

    [Fact]
    public void ResultT_ShouldBeFailure_WhenSuccess_Throws()
    {
        var result = Result.Success(42);
        var act = () => result.ShouldBeFailure("Testing generic failure");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Expected failure but was successful. Testing generic failure*");
    }

    [Fact]
    public void ResultT_ShouldHaveError_WhenErrorMatches_DoesNotThrow()
    {
        var result = Result.Failure<int>(TestError);
        var act = () => result.ShouldHaveError(TestError);
        act.Should().NotThrow();
    }

    [Fact]
    public void ResultT_ShouldHaveError_WhenSuccess_Throws()
    {
        var result = Result.Success(42);
        var act = () => result.ShouldHaveError(TestError, "Testing generic error");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*Expected failure but was successful. Testing generic error*");
    }

    [Fact]
    public void ResultT_ShouldHaveError_WhenErrorMismatch_Throws()
    {
        var result = Result.Failure<int>(Error.Validation("Other", "Other error"));
        var act = () => result.ShouldHaveError(TestError, "Testing generic mismatch");
        
        act.Should().Throw<XunitException>()
           .WithMessage("*because Testing generic mismatch*");
    }

    // ─── ShouldHaveErrorType / Code (Non-Generic & Generic) ───────────────────

    [Fact]
    public void Result_ShouldHaveErrorType_WhenTypeMatches_DoesNotThrow()
    {
        var result = Result.Failure(TestError);
        var act = () => result.ShouldHaveErrorType(ErrorType.Validation);
        act.Should().NotThrow();
    }

    [Fact]
    public void Result_ShouldHaveErrorType_WhenTypeMismatches_Throws()
    {
        var result = Result.Failure(TestError);
        var act = () => result.ShouldHaveErrorType(ErrorType.NotFound);
        act.Should().Throw<XunitException>().WithMessage("*Expected*NotFound*Validation*");
    }

    [Fact]
    public void ResultT_ShouldHaveErrorCode_WhenCodeMatches_DoesNotThrow()
    {
        var result = Result.Failure<int>(TestError);
        var act = () => result.ShouldHaveErrorCode("Test");
        act.Should().NotThrow();
    }

    [Fact]
    public void ResultT_ShouldHaveErrorCode_WhenCodeMismatches_Throws()
    {
        var result = Result.Failure<int>(TestError);
        var act = () => result.ShouldHaveErrorCode("OtherCode");
        act.Should().Throw<XunitException>().WithMessage("*Test*OtherCode*");
    }
}
