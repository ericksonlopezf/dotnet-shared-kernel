namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultExtensionsTests
{
    [Fact]
    public async Task Match_OnSuccess_ShouldInvokeSuccessFunc()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var output = await initialTask
            .Match(v => $"value: {v}", e => $"error: {e.Code}");

        // Assert
        output.Should().Be($"value: {TestValues.Numbers.Positive}");
    }

    [Fact]
    public async Task Match_OnFailure_ShouldInvokeFailureFunc()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var output = await initialTask
            .Match(v => $"value: {v}", e => $"error: {e.Code}");

        // Assert
        output.Should().Be($"error: {TestValues.Strings.ErrorCode}");
    }

    [Fact]
    public async Task Tap_Sync_OnSuccess_ShouldExecute()
    {
        // Arrange
        int? captured = null;
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask.Tap(v => captured = v);

        // Assert
        captured.Should().Be(TestValues.Numbers.Positive);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Tap_Async_OnSuccess_ShouldExecute()
    {
        // Arrange
        int? captured = null;
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Tap(async v =>
            {
                await Task.Delay(1);
                captured = v;
            });

        // Assert
        captured.Should().Be(TestValues.Numbers.Positive);
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_SyncPredicate_WhenTrue_ShouldPass()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Ensure(v => v > 0, Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public async Task Ensure_SyncPredicate_WhenFalse_ShouldFail()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage);
        var initialTask = SuccessTask(TestValues.Numbers.Negative);

        // Act
        var result = await initialTask.Ensure(v => v > 0, error);

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveError(error);
    }

    [Fact]
    public async Task Ensure_AsyncPredicate_ShouldWork()
    {
        // Arrange
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Ensure(async v =>
            {
                await Task.Delay(1);
                return v > 0;
            }, Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage));

        // Assert
        result.ShouldBeSuccess();
    }

    [Fact]
    public async Task Ensure_AsyncPredicate_OnFailure_ShouldSkip()
    {
        // Arrange
        var invoked = false;
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .Ensure(async v =>
            {
                invoked = true;
                await Task.Delay(1);
                return v > 0;
            }, Error.Validation(TestValues.Strings.AlternativeErrorCode, TestValues.Strings.AlternativeErrorMessage));

        // Assert
        result.ShouldBeFailure();
        invoked.Should().BeFalse();
    }

    [Fact]
    public async Task Recover_Sync_OnFailure_ShouldApply()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .Recover(e => Result.Success(TestValues.Numbers.Zero));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Zero);
    }

    [Fact]
    public async Task Recover_Async_OnFailure_ShouldApply()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .Recover(async e =>
            {
                await Task.Delay(1);
                return Result.Success(TestValues.Numbers.Zero);
            });

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Zero);
    }

    [Fact]
    public async Task Recover_OnSuccess_ShouldSkip()
    {
        // Arrange
        var recoveryInvoked = false;
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Recover(e => { recoveryInvoked = true; return Result.Success(TestValues.Numbers.Zero); });

        // Assert
        recoveryInvoked.Should().BeFalse();
        result.Value.Should().Be(TestValues.Numbers.Positive);
    }

    [Fact]
    public async Task Recover_Async_OnSuccess_ShouldSkip()
    {
        // Arrange
        var recoveryInvoked = false;
        var initialTask = SuccessTask(TestValues.Numbers.Positive);

        // Act
        var result = await initialTask
            .Recover(async e => 
            { 
                recoveryInvoked = true; 
                await Task.Delay(1); 
                return Result.Success(TestValues.Numbers.Zero); 
            });

        // Assert
        recoveryInvoked.Should().BeFalse();
        result.Value.Should().Be(TestValues.Numbers.Positive);
    }
}
