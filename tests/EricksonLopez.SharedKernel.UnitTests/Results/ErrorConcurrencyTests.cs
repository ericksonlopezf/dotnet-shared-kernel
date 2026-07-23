namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed class ErrorConcurrencyTests
{
    [Fact]
    public void Error_WhenCreatedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        const int count = 1000;
        var errors = new Error[count];

        // Act
        Parallel.For(0, count, i =>
        {
            errors[i] = Error.Failure($"Code.{i}", $"Message.{i}");
        });

        // Assert
        for (var i = 0; i < count; i++)
        {
            errors[i].Should().NotBeNull();
            errors[i].Code.Should().Be($"Code.{i}");
            errors[i].Description.Should().Be($"Message.{i}");
        }
    }

    [Fact]
    public void None_WhenAccessedConcurrently_ShouldReturnSameInstance()
    {
        // Arrange & Act
        var none1 = Error.None;
        var none2 = Error.None;

        // Assert
        none1.Should().BeSameAs(none2);

        Parallel.For(0, 1000, i =>
        {
            var none = Error.None;
            none.Should().BeSameAs(none1);
        });
    }
}
