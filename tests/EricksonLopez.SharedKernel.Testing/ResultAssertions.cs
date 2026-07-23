using AwesomeAssertions;
using EricksonLopez.SharedKernel.Results;

namespace EricksonLopez.SharedKernel.Testing;

public static class ResultAssertionsExtensions
{
    public static void ShouldBeSuccess(this Result result, string because = "", params object[] becauseArgs)
    {
        if (result.IsFailure)
            result.IsSuccess.Should().BeTrue($"Expected success but failed with error: {result.Error}. {because}", becauseArgs);
    }

    public static void ShouldBeFailure(this Result result, string because = "", params object[] becauseArgs)
    {
        result.IsFailure.Should().BeTrue($"Expected failure but was successful. {because}", becauseArgs);
    }

    public static void ShouldHaveError(this Result result, Error expectedError, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Should().Be(expectedError, because, becauseArgs);
    }

    public static void ShouldBeSuccess<T>(this Result<T> result, string because = "", params object[] becauseArgs)
    {
        if (result.IsFailure)
            result.IsSuccess.Should().BeTrue($"Expected success but failed with error: {result.Error}. {because}", becauseArgs);
    }

    public static void ShouldBeFailure<T>(this Result<T> result, string because = "", params object[] becauseArgs)
    {
        result.IsFailure.Should().BeTrue($"Expected failure but was successful. {because}", becauseArgs);
    }

    public static void ShouldHaveError<T>(this Result<T> result, Error expectedError, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Should().Be(expectedError, because, becauseArgs);
    }

    public static void ShouldHaveErrorType(this Result result, ErrorType expectedType, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Type.Should().Be(expectedType, because, becauseArgs);
    }

    public static void ShouldHaveErrorCode(this Result result, string expectedCode, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Code.Should().Be(expectedCode, because, becauseArgs);
    }

    public static void ShouldHaveErrorType<T>(this Result<T> result, ErrorType expectedType, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Type.Should().Be(expectedType, because, becauseArgs);
    }

    public static void ShouldHaveErrorCode<T>(this Result<T> result, string expectedCode, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Code.Should().Be(expectedCode, because, becauseArgs);
    }

    // ── Compound error helpers ────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is a failure whose error contains exactly
    /// <paramref name="expectedCount"/> inner errors.
    /// </summary>
    public static void ShouldHaveInnerErrors(this Result result, int expectedCount, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.HasInnerErrors.Should().BeTrue(
            $"Expected a compound error with {expectedCount} inner errors, but the error has no inner errors. {because}", becauseArgs);
        result.Error.InnerErrors.Should().HaveCount(expectedCount, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the result is a failure whose error contains exactly
    /// <paramref name="expectedCount"/> inner errors.
    /// </summary>
    public static void ShouldHaveInnerErrors<T>(this Result<T> result, int expectedCount, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.HasInnerErrors.Should().BeTrue(
            $"Expected a compound error with {expectedCount} inner errors, but the error has no inner errors. {because}", becauseArgs);
        result.Error.InnerErrors.Should().HaveCount(expectedCount, because, becauseArgs);
    }

    // ── Description helpers ───────────────────────────────────────────────────

    /// <summary>
    /// Asserts that the result is a failure whose error description equals
    /// <paramref name="expectedDescription"/>.
    /// </summary>
    public static void ShouldHaveDescription(this Result result, string expectedDescription, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Description.Should().Be(expectedDescription, because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the result is a failure whose error description equals
    /// <paramref name="expectedDescription"/>.
    /// </summary>
    public static void ShouldHaveDescription<T>(this Result<T> result, string expectedDescription, string because = "", params object[] becauseArgs)
    {
        result.ShouldBeFailure(because, becauseArgs);
        result.Error.Description.Should().Be(expectedDescription, because, becauseArgs);
    }
}
