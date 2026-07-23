namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultExtensionsTests
{
    private static Task<Result<T>> SuccessTask<T>(T value)
        => Task.FromResult(Result.Success(value));

    private static Task<Result<T>> FailureTask<T>(Error error)
        => Task.FromResult(Result.Failure<T>(error));

    private static readonly Error TestError = Error.NotFound(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage);

    [Fact]
    public async Task Map_SyncMapper_OnSuccess_ShouldTransform()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask.Map(x => x * 2);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive * 2);
    }

    [Fact]
    public async Task Map_SyncMapper_OnFailure_ShouldPropagateError()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask.Map(x => x * 2);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(TestError);
    }

    [Fact]
    public async Task Map_AsyncMapper_OnSuccess_ShouldTransform()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Map(async x =>
            {
                await Task.Delay(1);
                return x * 3;
            });

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive * 3);
    }

    [Fact]
    public async Task Map_AsyncMapper_OnFailure_ShouldNotInvokeMapper()
    {
        // Arrange
        var invoked = false;
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .Map(async x =>
            {
                invoked = true;
                await Task.Delay(1);
                return x * 3;
            });

        // Assert
        invoked.Should().BeFalse();
        result.ShouldBeFailure();
    }

    [Fact]
    public async Task Bind_SyncBinder_OnSuccess_ShouldChain()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Bind(x => Result.Success(x + 1));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive + 1);
    }

    [Fact]
    public async Task Bind_AsyncBinder_OnSuccess_ShouldChain()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Bind(async x =>
            {
                await Task.Delay(1);
                return Result.Success(x.ToString());
            });

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive.ToString());
    }

    [Fact]
    public async Task Bind_AsyncBinder_OnFailure_ShouldNotInvoke()
    {
        // Arrange
        var invoked = false;
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .Bind(async x =>
            {
                invoked = true;
                await Task.Delay(1);
                return Result.Success(x.ToString());
            });

        // Assert
        invoked.Should().BeFalse();
        result.ShouldBeFailure();
    }

    [Fact]
    public async Task MapError_Async_OnFailure_ShouldTransformError()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .MapError(e => Error.Unavailable(TestValues.Strings.AlternativeErrorCode, $"Was: {e.Code}"));

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
        result.ShouldHaveErrorType(ErrorType.Unavailable);
    }

    [Fact]
    public async Task MapError_Async_OnSuccess_ShouldReturnUnchanged()
    {
        // Arrange
        var mapperInvoked = false;
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .MapError(e => { mapperInvoked = true; return e; });

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive);
        mapperInvoked.Should().BeFalse();
    }
}
