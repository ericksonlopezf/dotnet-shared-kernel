using System.Threading.Tasks;
using static VerifyXunit.Verifier;

namespace EricksonLopez.SharedKernel.UnitTests.Results;

/// <summary>
/// Snapshot tests that lock in the string representations of core types.
/// A failing snapshot means a breaking change in observable output — update
/// intentionally with `dotnet test -- Verify.UseClipboard=true`.
/// </summary>
public sealed class ResultSnapshotTests
{
    [Fact]
    public Task Error_None_Sentinel_Snapshot()
    {
        // Arrange
        var output = $"Code='{Error.None.Code}' Desc='{Error.None.Description}' Type={Error.None.Type} HasInner={Error.None.HasInnerErrors}";

        // Act & Assert
        return Verify(output);
    }

    [Fact]
    public Task Error_CompoundTree_ThreeLevels_Snapshot()
    {
        // Arrange
        var leaf1 = Error.Validation("User.Name.Required", "Name is required");
        var leaf2 = Error.Validation("User.Email.Invalid", "Invalid email format");
        var leaf3 = Error.Validation("User.Age.TooLow", "Must be at least 18");

        var mid = Error.Validation("User.PersonalInfo.Invalid", "Personal info is invalid", leaf1, leaf2);
        var root = Error.Validation("User.Invalid", "User validation failed", mid, leaf3);

        // Act & Assert
        return Verify(root.ToString());
    }

    [Fact]
    public Task AllErrorTypes_ToString_Format_Snapshot()
    {
        // Arrange — one of each ErrorType
        var errors = new[]
        {
            Error.Failure("Domain.Failure", "Generic failure"),
            Error.Validation("Domain.Validation", "Validation failed"),
            Error.NotFound("Domain.NotFound", "Resource not found"),
            Error.Conflict("Domain.Conflict", "State conflict"),
            Error.Unauthorized("Domain.Unauthorized", "Auth required"),
            Error.Forbidden("Domain.Forbidden", "Insufficient permissions"),
            Error.Unavailable("Domain.Unavailable", "Service unavailable"),
            Error.Unexpected("Domain.Unexpected", "Unexpected error"),
        };

        var output = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));

        // Act & Assert
        return Verify(output);
    }
}
