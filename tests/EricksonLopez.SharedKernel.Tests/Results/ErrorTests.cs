using EricksonLopez.SharedKernel.Results;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Results;

public sealed class ErrorTests
{
    // ─── InnerErrors ──────────────────────────────────────────────────────────

    [Fact]
    public void SimpleError_ShouldHaveNoInnerErrors()
    {
        var error = Error.Failure("X.Error", "Something failed");

        error.HasInnerErrors.Should().BeFalse();
        error.InnerErrors.Should().BeEmpty();
    }

    [Fact]
    public void ErrorWithInnerErrors_ShouldExposeChildren()
    {
        var inner1 = Error.Validation("Name.Required", "Name is required");
        var inner2 = Error.Validation("Email.Invalid", "Invalid email format");

        var error = Error.Validation("User.Invalid", "Validation failed", inner1, inner2);

        error.HasInnerErrors.Should().BeTrue();
        error.InnerErrors.Should().HaveCount(2);
        error.InnerErrors[0].Should().Be(inner1);
        error.InnerErrors[1].Should().Be(inner2);
    }

    [Fact]
    public void ErrorWithEmptyInnerErrors_ShouldTreatAsNoInnerErrors()
    {
        var error = Error.Failure("X.Error", "Error", Array.Empty<Error>());

        error.HasInnerErrors.Should().BeFalse();
        error.InnerErrors.Should().BeEmpty();
    }

    [Fact]
    public void ErrorEquality_SameCodeDescriptionType_WithoutInnerErrors_ShouldBeEqual()
    {
        var error1 = Error.Validation("X.Error", "Error");
        var error2 = Error.Validation("X.Error", "Error");

        (error1 == error2).Should().BeTrue();
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    public void ErrorEquality_DifferentInnerErrors_ShouldNotBeEqual()
    {
        var withInner = Error.Validation("X.Error", "Error",
            Error.Validation("A", "B"));
        var withoutInner = Error.Validation("X.Error", "Error");

        // Records include ALL fields in equality — InnerErrors makes them different
        (withInner == withoutInner).Should().BeFalse();
    }

    [Fact]
    public void ToString_WithInnerErrors_ShouldIncludeCount()
    {
        var error = Error.Validation("User.Invalid", "Validation failed",
            Error.Validation("A", "B"),
            Error.Validation("C", "D"));

        error.ToString().Should().Contain("2 inner errors");
    }

    [Fact]
    public void ToString_WithoutInnerErrors_ShouldNotIncludeCount()
    {
        var error = Error.Failure("X.Error", "Error");

        error.ToString().Should().NotContain("inner errors");
        error.ToString().Should().Be("[Failure] X.Error: Error");
    }

    // ─── New ErrorType values ─────────────────────────────────────────────────

    [Fact]
    public void Unavailable_ShouldCreateCorrectType()
    {
        var error = Error.Unavailable("Payment.ServiceDown", "Payment service is unavailable");

        error.Type.Should().Be(ErrorType.Unavailable);
        error.Code.Should().Be("Payment.ServiceDown");
    }

    [Fact]
    public void Unexpected_ShouldCreateCorrectType()
    {
        var error = Error.Unexpected("System.Error", "An unexpected error occurred");

        error.Type.Should().Be(ErrorType.Unexpected);
        error.Code.Should().Be("System.Error");
    }

    // ─── Existing factory methods still work ──────────────────────────────────

    [Theory]
    [InlineData(nameof(ErrorType.Failure))]
    [InlineData(nameof(ErrorType.Validation))]
    [InlineData(nameof(ErrorType.NotFound))]
    [InlineData(nameof(ErrorType.Conflict))]
    [InlineData(nameof(ErrorType.Unauthorized))]
    [InlineData(nameof(ErrorType.Forbidden))]
    [InlineData(nameof(ErrorType.Unavailable))]
    [InlineData(nameof(ErrorType.Unexpected))]
    public void AllFactoryMethods_ShouldCreateCorrectErrorType(string typeName)
    {
        var expectedType = Enum.Parse<ErrorType>(typeName);

        var error = typeName switch
        {
            nameof(ErrorType.Failure) => Error.Failure("X", "Y"),
            nameof(ErrorType.Validation) => Error.Validation("X", "Y"),
            nameof(ErrorType.NotFound) => Error.NotFound("X", "Y"),
            nameof(ErrorType.Conflict) => Error.Conflict("X", "Y"),
            nameof(ErrorType.Unauthorized) => Error.Unauthorized("X", "Y"),
            nameof(ErrorType.Forbidden) => Error.Forbidden("X", "Y"),
            nameof(ErrorType.Unavailable) => Error.Unavailable("X", "Y"),
            nameof(ErrorType.Unexpected) => Error.Unexpected("X", "Y"),
            _ => throw new InvalidOperationException()
        };

        error.Type.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(nameof(ErrorType.Failure))]
    [InlineData(nameof(ErrorType.Validation))]
    [InlineData(nameof(ErrorType.NotFound))]
    [InlineData(nameof(ErrorType.Conflict))]
    [InlineData(nameof(ErrorType.Unauthorized))]
    [InlineData(nameof(ErrorType.Forbidden))]
    [InlineData(nameof(ErrorType.Unavailable))]
    [InlineData(nameof(ErrorType.Unexpected))]
    public void AllFactoryMethods_WithInnerErrors_ShouldCreateCorrectErrorTypeAndStoreInnerErrors(string typeName)
    {
        var expectedType = Enum.Parse<ErrorType>(typeName);
        var inner1 = Error.Validation("A", "B");

        var error = typeName switch
        {
            nameof(ErrorType.Failure) => Error.Failure("X", "Y", inner1),
            nameof(ErrorType.Validation) => Error.Validation("X", "Y", inner1),
            nameof(ErrorType.NotFound) => Error.NotFound("X", "Y", inner1),
            nameof(ErrorType.Conflict) => Error.Conflict("X", "Y", inner1),
            nameof(ErrorType.Unauthorized) => Error.Unauthorized("X", "Y", inner1),
            nameof(ErrorType.Forbidden) => Error.Forbidden("X", "Y", inner1),
            nameof(ErrorType.Unavailable) => Error.Unavailable("X", "Y", inner1),
            nameof(ErrorType.Unexpected) => Error.Unexpected("X", "Y", inner1),
            _ => throw new InvalidOperationException()
        };

        error.Type.Should().Be(expectedType);
        error.HasInnerErrors.Should().BeTrue();
        error.InnerErrors.Should().HaveCount(1).And.Contain(inner1);
    }
}
