namespace EricksonLopez.SharedKernel.UnitTests.Results;

public sealed partial class ResultExtensionsTests
{
    [Fact]
    public async Task AsyncPipeline_ShouldComposeNaturally()
    {
        // Arrange
        var logged = false;
        var initialTask = SuccessTask(10);

        // Act
        var result = await initialTask
            .Ensure(v => v > 0, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => v * 2)
            .Tap(v => logged = true)
            .Bind(v => Task.FromResult(
                v <= 100
                    ? Result.Success($"Value: {v}")
                    : Result.Failure<string>(Error.Failure(TestValues.Strings.ErrorCode, "Too large"))));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be("Value: 20");
        logged.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncPipeline_FailureShortCircuits()
    {
        // Arrange
        var mapExecuted = false;
        var initialTask = SuccessTask(TestValues.Numbers.Negative);

        // Act
        var result = await initialTask
            .Ensure(v => v > 0, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => { mapExecuted = true; return v * 2; });

        // Assert
        result.ShouldBeFailure();
        mapExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task MapError_InPipeline_ShouldComposeWithOtherMethods()
    {
        // Arrange
        var initialTask = FailureTask<int>(TestError);

        // Act
        var result = await initialTask
            .MapError(e => Error.Failure(TestValues.Strings.AlternativeErrorCode, $"Wrapped: {e.Description}"))
            .Recover(e => Result.Success(TestValues.Numbers.Negative));

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(TestValues.Numbers.Negative);
    }

    [Fact]
    public async Task ValueTask_Generic_SuccessPipeline()
    {
        // Arrange
        var tap1 = false;
        var tap2 = false;
        var finallyExecuted = false;
        var initialTask = ValueTask.FromResult(Result.Success(10));

        // Act
        var result = await initialTask
            .Ensure(v => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Ensure(v => ValueTask.FromResult(true), Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => v * 2)
            .Map(v => ValueTask.FromResult(v * 2))
            .Tap(v => tap1 = true)
            .Tap(v => { tap2 = true; return ValueTask.CompletedTask; })
            .TapError(e => { })
            .Bind(v => Result.Success(v + 1))
            .Bind(v => ValueTask.FromResult(Result.Success(v + 1)))
            .Recover(e => Result.Success(TestValues.Numbers.Zero))
            .Recover(e => ValueTask.FromResult(Result.Success(TestValues.Numbers.Zero)))
            .MapError(e => Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Finally(r => finallyExecuted = true);

        // Assert
        result.ShouldBeSuccess();
        result.Value.Should().Be(42);
        tap1.Should().BeTrue();
        tap2.Should().BeTrue();
        finallyExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task ValueTask_Generic_FailurePipeline()
    {
        // Arrange
        var initialTask = ValueTask.FromResult(Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage)));

        // Act
        var result = await initialTask
            .Ensure(v => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Ensure(v => ValueTask.FromResult(true), Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Map(v => v * 2)
            .Map(v => ValueTask.FromResult(v * 2))
            .Tap(v => { })
            .Tap(v => ValueTask.CompletedTask)
            .TapError(e => { })
            .Bind(v => Result.Success(v + 1))
            .Bind(v => ValueTask.FromResult(Result.Success(v + 1)))
            .MapError(e => Error.Unexpected(TestValues.Strings.AlternativeErrorCode, "W"))
            .Finally(r => { });

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
    }

    [Fact]
    public async Task ValueTask_Generic_Recover_FailurePath()
    {
        // Arrange
        var initialTask1 = ValueTask.FromResult(Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage)));
        var initialTask2 = ValueTask.FromResult(Result.Failure<int>(Error.Failure(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage)));

        // Act
        var result1 = await initialTask1
            .Recover(e => Result.Success(1))
            .Recover(e => ValueTask.FromResult(Result.Success(2)));

        var result2 = await initialTask2
            .Recover(e => ValueTask.FromResult(Result.Success(2)));

        // Assert
        result1.Value.Should().Be(1);
        result2.Value.Should().Be(2);
    }

    [Fact]
    public async Task ValueTask_Generic_Match()
    {
        // Arrange
        var successTask = ValueTask.FromResult(Result.Success(10));
        var failureTask = ValueTask.FromResult(Result.Failure<int>(TestError));

        // Act
        var v1 = await successTask.Match(v => v, e => 0);
        var v2 = await failureTask.Match(v => v, e => 0);

        // Assert
        v1.Should().Be(10);
        v2.Should().Be(0);
    }

    [Fact]
    public async Task NonGeneric_Task_SuccessPipeline()
    {
        // Arrange
        var tap = false;
        var initialTask = Task.FromResult(Result.Success());

        // Act
        var result = await initialTask
            .Ensure(() => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Tap(() => tap = true)
            .TapError(e => { })
            .MapError(e => Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Finally(r => { });

        // Assert
        result.ShouldBeSuccess();
        tap.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_Task_FailurePipeline()
    {
        // Arrange
        var tap = false;
        var tapError = false;
        var initialTask = Task.FromResult(Result.Failure(TestError));

        // Act
        var result = await initialTask
            .Ensure(() => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Tap(() => tap = true)
            .TapError(e => tapError = true)
            .MapError(e => Error.Unexpected(TestValues.Strings.AlternativeErrorCode, "W"))
            .Finally(r => { });

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
        tap.Should().BeFalse();
        tapError.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_Task_Match()
    {
        // Arrange
        var successTask = Task.FromResult(Result.Success());
        var failureTask = Task.FromResult(Result.Failure(TestError));

        // Act
        var v1 = await successTask.Match(() => 1, e => 0);
        var v2 = await failureTask.Match(() => 1, e => 0);

        // Assert
        v1.Should().Be(1);
        v2.Should().Be(0);
    }

    [Fact]
    public async Task NonGeneric_ValueTask_SuccessPipeline()
    {
        // Arrange
        var tap = false;
        var initialTask = ValueTask.FromResult(Result.Success());

        // Act
        var result = await initialTask
            .Ensure(() => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Tap(() => tap = true)
            .TapError(e => { })
            .MapError(e => Error.Unexpected(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Finally(r => { });

        // Assert
        result.ShouldBeSuccess();
        tap.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_ValueTask_FailurePipeline()
    {
        // Arrange
        var tapError = false;
        var initialTask = ValueTask.FromResult(Result.Failure(TestError));

        // Act
        var result = await initialTask
            .Ensure(() => true, Error.Validation(TestValues.Strings.ErrorCode, TestValues.Strings.ErrorMessage))
            .Tap(() => { })
            .TapError(e => tapError = true)
            .MapError(e => Error.Unexpected(TestValues.Strings.AlternativeErrorCode, "W"))
            .Finally(r => { });

        // Assert
        result.ShouldBeFailure();
        result.ShouldHaveErrorCode(TestValues.Strings.AlternativeErrorCode);
        tapError.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_ValueTask_Match()
    {
        // Arrange
        var successTask = ValueTask.FromResult(Result.Success());
        var failureTask = ValueTask.FromResult(Result.Failure(TestError));

        // Act
        var v1 = await successTask.Match(() => 1, e => 0);
        var v2 = await failureTask.Match(() => 1, e => 0);

        // Assert
        v1.Should().Be(1);
        v2.Should().Be(0);
    }

    [Fact]
    public async Task Ensure_FailurePaths()
    {
        // Arrange
        var error = Error.Validation(TestValues.Strings.ErrorCode, "B");
        var successTask = Task.FromResult(Result.Success());
        var successValueTask = ValueTask.FromResult(Result.Success());
        var successValueTaskT = ValueTask.FromResult(Result.Success(1));

        // Act
        var t1 = await successTask.Ensure(() => false, error);
        var vt1 = await successValueTask.Ensure(() => false, error);
        var vt2 = await successValueTaskT.Ensure(v => false, error);
        var vt3 = await successValueTaskT.Ensure(v => ValueTask.FromResult(false), error);

        // Assert
        t1.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        vt1.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        vt2.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        vt3.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
    }

    [Fact]
    public async Task Task_Generic_FinallyAndEnsure()
    {
        // Arrange
        var finallyExecuted = false;
        var initialTask = Task.FromResult(Result.Success(1));

        // Act
        var r1 = await initialTask
            .Ensure(v => Task.FromResult(false), Error.Validation(TestValues.Strings.ErrorCode, "B"))
            .TapError(e => { })
            .Finally(r => finallyExecuted = true);
        
        // Assert
        r1.ShouldHaveErrorCode(TestValues.Strings.ErrorCode);
        finallyExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task TapError_AsyncAction_TaskResultT_OnFailure_ShouldExecute()
    {
        // Arrange
        var executed = false;
        var initialTask = Task.FromResult(Result.Failure<int>(TestError));

        // Act
        var r = await initialTask
            .TapError(e => { executed = true; return Task.CompletedTask; });
            
        // Assert
        r.ShouldBeFailure();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task TapError_AsyncAction_TaskResult_OnFailure_ShouldExecute()
    {
        // Arrange
        var executed = false;
        var initialTask = Task.FromResult(Result.Failure(TestError));

        // Act
        var r = await initialTask
            .TapError(e => { executed = true; return Task.CompletedTask; });
            
        // Assert
        r.ShouldBeFailure();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task TapError_AsyncAction_ValueTaskResultT_OnFailure_ShouldExecute()
    {
        // Arrange
        var executed = false;
        var initialTask = ValueTask.FromResult(Result.Failure<int>(TestError));

        // Act
        var r = await initialTask
            .TapError(e => { executed = true; return ValueTask.CompletedTask; });
            
        // Assert
        r.ShouldBeFailure();
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task TapError_AsyncAction_ValueTaskResult_OnFailure_ShouldExecute()
    {
        // Arrange
        var executed = false;
        var initialTask = ValueTask.FromResult(Result.Failure(TestError));

        // Act
        var r = await initialTask
            .TapError(e => { executed = true; return ValueTask.CompletedTask; });
            
        // Assert
        r.ShouldBeFailure();
        executed.Should().BeTrue();
    }
}
