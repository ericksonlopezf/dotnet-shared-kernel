namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed class ResultConcurrencyTests
{
    [Fact]
    public async Task Combine_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        const int taskCount = 1000;
        var results = new Result[taskCount];
        var random = new Random();

        // Act
        var tasks = Enumerable.Range(0, taskCount).Select(i => Task.Run(() =>
        {
            var isSuccess = random.Next(2) == 0;
            if (isSuccess)
            {
                results[i] = Result.Success();
            }
            else
            {
                results[i] = Result.Failure(Error.Failure($"Error.{i}", "Concurrent error"));
            }
        }));

        await Task.WhenAll(tasks);

        var combinedResult = Result.Combine(results);

        // Assert
        if (results.Any(r => r.IsFailure))
        {
            combinedResult.ShouldBeFailure();
        }
        else
        {
            combinedResult.ShouldBeSuccess();
        }
    }

    [Fact]
    public void Map_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        var result = Result.Success(0);

        // Act & Assert
        Parallel.For(0, 1000, i =>
        {
            var mapped = result.Map(x => x + i);
            
            mapped.ShouldBeSuccess();
            mapped.Value.Should().Be(i);
        });
    }

    [Fact]
    public void ImplicitConversion_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Act & Assert
        Parallel.For(0, 1000, i =>
        {
            Result<int> successResult = i;
            successResult.Value.Should().Be(i);

            var error = Error.Failure($"Err.{i}", "Message");
            Result<int> errorResult = error;
            errorResult.ShouldHaveError(error);
        });
    }
}
