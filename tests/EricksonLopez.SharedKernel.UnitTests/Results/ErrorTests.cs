using System.Threading.Tasks;
using static VerifyXunit.Verifier;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed class ErrorTests
{
    [Fact]
    public void SimpleError_ShouldHaveNoInnerErrors()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        error.HasInnerErrors.Should().BeFalse();
        error.InnerErrors.Should().BeEmpty();
    }

    [Fact]
    public void ErrorWithInnerErrors_ShouldExposeChildren()
    {
        // Arrange
        var inner1 = Error.Validation(TestValues.Strings.ErrorCode, "First inner");
        var inner2 = Error.Validation(TestValues.Strings.AlternativeErrorCode, "Second inner");

        // Act
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1, inner2);

        // Assert
        error.HasInnerErrors.Should().BeTrue();
        error.InnerErrors.Should().HaveCount(2);
        error.InnerErrors[0].Should().Be(inner1);
        error.InnerErrors[1].Should().Be(inner2);
    }

    [Fact]
    public void ErrorWithEmptyInnerErrors_ShouldTreatAsNoInnerErrors()
    {
        // Arrange & Act
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, Array.Empty<Error>());

        // Assert
        error.HasInnerErrors.Should().BeFalse();
        error.InnerErrors.Should().BeEmpty();
    }

    [Fact]
    public void ErrorEquality_SameCodeDescriptionType_WithoutInnerErrors_ShouldBeEqual()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        (error1 == error2).Should().BeTrue();
        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    public void ErrorEquality_DifferentInnerErrors_ShouldNotBeEqual()
    {
        // Arrange
        var withInner = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage,
            Error.Validation(TestValues.Strings.AlternativeErrorCode, "Inner"));
        var withoutInner = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        (withInner == withoutInner).Should().BeFalse();
    }

    [Fact]
    public void ToString_WithInnerErrors_ShouldIncludeCount()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage,
            Error.Validation("A", "B"),
            Error.Validation("C", "D"));

        // Act & Assert
        error.ToString().Should().Contain("2 inner errors");
    }

    [Fact]
    public void ToString_WithoutInnerErrors_ShouldNotIncludeCount()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        error.ToString().Should().NotContain("inner errors");
        error.ToString().Should().Be($"[Failure] {TestValues.Strings.ErrorCode}: {TestValues.Strings.ErrorMessage}");
    }

    [Fact]
    public void Unavailable_ShouldCreateCorrectType()
    {
        // Arrange & Act
        var error = Error.Unavailable(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Assert
        error.Type.Should().Be(ErrorType.Unavailable);
        error.Code.Should().Be(TestValues.Strings.ErrorCode);
    }

    [Fact]
    public void Unexpected_ShouldCreateCorrectType()
    {
        // Arrange & Act
        var error = Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Assert
        error.Type.Should().Be(ErrorType.Unexpected);
        error.Code.Should().Be(TestValues.Strings.ErrorCode);
    }

    [Fact]
    public void Error_None_ShouldHaveEmptyCodeAndDescription()
    {
        // Arrange & Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Description.Should().BeEmpty();
        error.Type.Should().Be(ErrorType.Failure);
        error.HasInnerErrors.Should().BeFalse();
        error.InnerErrors.Should().BeEmpty();
    }

    [Fact]
    public Task Error_ToString_ShouldMatchFormat_WithSnapshot()
    {
        // Arrange
        var simple = Error.Failure("Simple.Error", "A simple failure");
        var withInner = Error.Validation("Complex.Error", "A complex error", 
            Error.Validation("Field1", "Required"),
            Error.Validation("Field2", "Too short"));

        var output = $"Simple:\n{simple}\n\nComplex:\n{withInner}";

        // Act & Assert
        return Verify(output);
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
    public void AllFactoryMethods_ShouldCreateCorrectErrorType(string typeName)
    {
        // Arrange
        var expectedType = Enum.Parse<ErrorType>(typeName);

        // Act
        var error = typeName switch
        {
            nameof(ErrorType.Failure) => Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Validation) => Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.NotFound) => Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Conflict) => Error.Conflict(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Unauthorized) => Error.Unauthorized(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Forbidden) => Error.Forbidden(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Unavailable) => Error.Unavailable(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            nameof(ErrorType.Unexpected) => Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage),
            _ => throw new InvalidOperationException()
        };

        // Assert
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
        // Arrange
        var expectedType = Enum.Parse<ErrorType>(typeName);
        var inner1 = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);

        // Act
        var error = typeName switch
        {
            nameof(ErrorType.Failure) => Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Validation) => Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.NotFound) => Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Conflict) => Error.Conflict(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Unauthorized) => Error.Unauthorized(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Forbidden) => Error.Forbidden(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Unavailable) => Error.Unavailable(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            nameof(ErrorType.Unexpected) => Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1),
            _ => throw new InvalidOperationException()
        };

        // Assert
        error.Type.Should().Be(expectedType);
        error.HasInnerErrors.Should().BeTrue();
        error.InnerErrors.Should().HaveCount(1).And.Contain(inner1);
    }

    // ─── Equality contract ────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameReference_ShouldBeTrue()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        error.Equals(error).Should().BeTrue("ReferenceEquals short-circuit must return true");
    }

    [Fact]
    public void Equals_NullOther_ShouldBeFalse()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        error.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_NullLeft_ShouldBeFalse()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        Error? nullError = null;

        // Act & Assert
        (nullError == error).Should().BeFalse();
        (error == nullError).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_BothNull_ShouldBeTrue()
    {
        // Arrange
        Error? left = null;
        Error? right = null;

        // Act & Assert
        (left == right).Should().BeTrue("two null errors are equal by value semantics");
    }

    [Fact]
    public void InequalityOperator_DifferentErrors_ShouldBeTrue()
    {
        // Arrange
        var error1 = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_EqualErrors_ShouldProduceSameHash()
    {
        // Arrange
        var error1 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        error1.GetHashCode().Should().Be(error2.GetHashCode(),
            "equal objects must have equal hash codes");
    }

    [Fact]
    public void GetHashCode_WithInnerErrors_DiffersFromWithoutInnerErrors()
    {
        // Arrange
        var simple = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var compound = Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage,
            Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage));

        // Act & Assert
        simple.GetHashCode().Should().NotBe(compound.GetHashCode(),
            "inner errors contribute to the hash code");
    }

    [Fact]
    public void GetHashCode_IsStableWithinSession()
    {
        // Arrange
        var error = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act
        var h1 = error.GetHashCode();
        var h2 = error.GetHashCode();
        var h3 = error.GetHashCode();

        // Assert
        h1.Should().Be(h2).And.Be(h3);
    }

    [Fact]
    public void Equals_DifferentType_SameCodeAndDescription_ShouldNotBeEqual()
    {
        // Arrange — same code+description, different ErrorType
        var failure = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var notFound = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

        // Act & Assert
        (failure == notFound).Should().BeFalse(
            "ErrorType is part of the equality contract");
    }

    [Fact]
    public void Equals_SameCodeDifferentDescription_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);
        var error2 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.AlternativeErrorMessage);

        // Act & Assert
        (error1 == error2).Should().BeFalse();
    }

    [Fact]
    public void Equals_CompoundErrors_SameStructure_ShouldBeEqual()
    {
        // Arrange
        var inner = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var compound1 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner);
        var compound2 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner);

        // Act & Assert
        (compound1 == compound2).Should().BeTrue("structural equality includes inner errors");
        compound1.GetHashCode().Should().Be(compound2.GetHashCode());
    }

    [Fact]
    public void Equals_CompoundErrors_DifferentInnerOrder_ShouldNotBeEqual()
    {
        // Arrange — same inner errors but different order
        var inner1 = Error.Validation("A", "First");
        var inner2 = Error.Validation("B", "Second");

        var compound1 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner1, inner2);
        var compound2 = Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage, inner2, inner1);

        // Act & Assert
        (compound1 == compound2).Should().BeFalse(
            "SequenceEqual is order-sensitive for inner errors");
    }
}
