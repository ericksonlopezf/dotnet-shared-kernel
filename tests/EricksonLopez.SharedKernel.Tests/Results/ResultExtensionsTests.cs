using EricksonLopez.SharedKernel.Results;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Results;

public sealed class ResultExtensionsTests
{
    private static Task<Result<T>> SuccessTask<T>(T value)
        => Task.FromResult(Result.Success(value));

    private static Task<Result<T>> FailureTask<T>(Error error)
        => Task.FromResult(Result.Failure<T>(error));

    private static readonly Error TestError = Error.NotFound("Test.NotFound", "Not found");

    // ─── Map async ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Map_SyncMapper_OnSuccess_ShouldTransform()
    {
        var result = await SuccessTask(5).Map(x => x * 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task Map_SyncMapper_OnFailure_ShouldPropagateError()
    {
        var result = await FailureTask<int>(TestError).Map(x => x * 2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TestError);
    }

    [Fact]
    public async Task Map_AsyncMapper_OnSuccess_ShouldTransform()
    {
        var result = await SuccessTask(5)
            .Map(async x =>
            {
                await Task.Delay(1);
                return x * 3;
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    [Fact]
    public async Task Map_AsyncMapper_OnFailure_ShouldNotInvokeMapper()
    {
        var invoked = false;
        var result = await FailureTask<int>(TestError)
            .Map(async x =>
            {
                invoked = true;
                await Task.Delay(1);
                return x * 3;
            });

        invoked.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    // ─── Bind async ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Bind_SyncBinder_OnSuccess_ShouldChain()
    {
        var result = await SuccessTask(5)
            .Bind(x => Result.Success(x + 1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(6);
    }

    [Fact]
    public async Task Bind_AsyncBinder_OnSuccess_ShouldChain()
    {
        var result = await SuccessTask(5)
            .Bind(async x =>
            {
                await Task.Delay(1);
                return Result.Success(x.ToString());
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task Bind_AsyncBinder_OnFailure_ShouldNotInvoke()
    {
        var invoked = false;
        var result = await FailureTask<int>(TestError)
            .Bind(async x =>
            {
                invoked = true;
                await Task.Delay(1);
                return Result.Success(x.ToString());
            });

        invoked.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }

    // ─── Match async ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Match_OnSuccess_ShouldInvokeSuccessFunc()
    {
        var output = await SuccessTask(42)
            .Match(v => $"value: {v}", e => $"error: {e.Code}");

        output.Should().Be("value: 42");
    }

    [Fact]
    public async Task Match_OnFailure_ShouldInvokeFailureFunc()
    {
        var output = await FailureTask<int>(TestError)
            .Match(v => $"value: {v}", e => $"error: {e.Code}");

        output.Should().Be("error: Test.NotFound");
    }

    // ─── Tap async ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Tap_Sync_OnSuccess_ShouldExecute()
    {
        int? captured = null;
        var result = await SuccessTask(42).Tap(v => captured = v);

        captured.Should().Be(42);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Tap_Async_OnSuccess_ShouldExecute()
    {
        int? captured = null;
        var result = await SuccessTask(42)
            .Tap(async v =>
            {
                await Task.Delay(1);
                captured = v;
            });

        captured.Should().Be(42);
        result.IsSuccess.Should().BeTrue();
    }

    // ─── Ensure async ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Ensure_SyncPredicate_WhenTrue_ShouldPass()
    {
        var result = await SuccessTask(42)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Ensure_SyncPredicate_WhenFalse_ShouldFail()
    {
        var error = Error.Validation("X", "Must be positive");
        var result = await SuccessTask(-1)
            .Ensure(v => v > 0, error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task Ensure_AsyncPredicate_ShouldWork()
    {
        var result = await SuccessTask(42)
            .Ensure(async v =>
            {
                await Task.Delay(1);
                return v > 0;
            }, Error.Validation("X", "Must be positive"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Ensure_AsyncPredicate_OnFailure_ShouldSkip()
    {
        var invoked = false;
        var result = await FailureTask<int>(TestError)
            .Ensure(async v =>
            {
                invoked = true;
                await Task.Delay(1);
                return v > 0;
            }, Error.Validation("X", "Y"));

        result.IsFailure.Should().BeTrue();
        invoked.Should().BeFalse();
    }

    // ─── Recover async ────────────────────────────────────────────────────────

    [Fact]
    public async Task Recover_Sync_OnFailure_ShouldApply()
    {
        var result = await FailureTask<int>(TestError)
            .Recover(e => Result.Success(0));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Recover_Async_OnFailure_ShouldApply()
    {
        var result = await FailureTask<int>(TestError)
            .Recover(async e =>
            {
                await Task.Delay(1);
                return Result.Success(0);
            });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task Recover_OnSuccess_ShouldSkip()
    {
        var recoveryInvoked = false;
        var result = await SuccessTask(42)
            .Recover(e => { recoveryInvoked = true; return Result.Success(0); });

        recoveryInvoked.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Recover_Async_OnSuccess_ShouldSkip()
    {
        var recoveryInvoked = false;
        var result = await SuccessTask(42)
            .Recover(async e => 
            { 
                recoveryInvoked = true; 
                await Task.Delay(1); 
                return Result.Success(0); 
            });

        recoveryInvoked.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    // ─── Full async pipeline ──────────────────────────────────────────────────

    [Fact]
    public async Task AsyncPipeline_ShouldComposeNaturally()
    {
        var logged = false;

        var result = await SuccessTask(10)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"))
            .Map(v => v * 2)
            .Tap(v => logged = true)
            .Bind(v => Task.FromResult(
                v <= 100
                    ? Result.Success($"Value: {v}")
                    : Result.Failure<string>(Error.Failure("X", "Too large"))));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Value: 20");
        logged.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncPipeline_FailureShortCircuits()
    {
        var mapExecuted = false;

        var result = await SuccessTask(-5)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"))
            .Map(v => { mapExecuted = true; return v * 2; });

        result.IsFailure.Should().BeTrue();
        mapExecuted.Should().BeFalse();
    }

    // ─── MapError async ───────────────────────────────────────────────────────

    [Fact]
    public async Task MapError_Async_OnFailure_ShouldTransformError()
    {
        var result = await FailureTask<int>(TestError)
            .MapError(e => Error.Unavailable("Adapted", $"Was: {e.Code}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Adapted");
        result.Error.Type.Should().Be(ErrorType.Unavailable);
    }

    [Fact]
    public async Task MapError_Async_OnSuccess_ShouldReturnUnchanged()
    {
        var mapperInvoked = false;
        var result = await SuccessTask(42)
            .MapError(e => { mapperInvoked = true; return e; });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        mapperInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task MapError_InPipeline_ShouldComposeWithOtherMethods()
    {
        var result = await FailureTask<int>(TestError)
            .MapError(e => Error.Failure("App.Error", $"Wrapped: {e.Description}"))
            .Recover(e => Result.Success(-1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-1);
    }

    // ─── ValueTask Pipeline ───────────────────────────────────────────────────

    [Fact]
    public async Task ValueTask_Generic_SuccessPipeline()
    {
        var tap1 = false;
        var tap2 = false;
        var finallyExecuted = false;

        var result = await ValueTask.FromResult(Result.Success(10))
            .Ensure(v => true, Error.Validation("X", "Y"))
            .Ensure(v => ValueTask.FromResult(true), Error.Validation("X", "Y"))
            .Map(v => v * 2)
            .Map(v => ValueTask.FromResult(v * 2))
            .Tap(v => tap1 = true)
            .Tap(v => { tap2 = true; return ValueTask.CompletedTask; })
            .TapError(e => { })
            .Bind(v => Result.Success(v + 1))
            .Bind(v => ValueTask.FromResult(Result.Success(v + 1)))
            .Recover(e => Result.Success(0))
            .Recover(e => ValueTask.FromResult(Result.Success(0)))
            .MapError(e => Error.Unexpected("X", "Y"))
            .Finally(r => finallyExecuted = true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        tap1.Should().BeTrue();
        tap2.Should().BeTrue();
        finallyExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task ValueTask_Generic_FailurePipeline()
    {
        var result = await ValueTask.FromResult(Result.Failure<int>(Error.Failure("X", "Y")))
            .Ensure(v => true, Error.Validation("X", "Y"))
            .Ensure(v => ValueTask.FromResult(true), Error.Validation("X", "Y"))
            .Map(v => v * 2)
            .Map(v => ValueTask.FromResult(v * 2))
            .Tap(v => { })
            .Tap(v => ValueTask.CompletedTask)
            .TapError(e => { })
            .Bind(v => Result.Success(v + 1))
            .Bind(v => ValueTask.FromResult(Result.Success(v + 1)))
            .MapError(e => Error.Unexpected("Z", "W"))
            .Finally(r => { });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Z");
    }

    [Fact]
    public async Task ValueTask_Generic_Recover_FailurePath()
    {
        var result = await ValueTask.FromResult(Result.Failure<int>(Error.Failure("X", "Y")))
            .Recover(e => Result.Success(1))
            .Recover(e => ValueTask.FromResult(Result.Success(2))); // Won't execute because it's already recovered

        result.Value.Should().Be(1);

        var result2 = await ValueTask.FromResult(Result.Failure<int>(Error.Failure("X", "Y")))
            .Recover(e => ValueTask.FromResult(Result.Success(2)));

        result2.Value.Should().Be(2);
    }

    [Fact]
    public async Task ValueTask_Generic_Match()
    {
        var v1 = await ValueTask.FromResult(Result.Success(10)).Match(v => v, e => 0);
        var v2 = await ValueTask.FromResult(Result.Failure<int>(TestError)).Match(v => v, e => 0);

        v1.Should().Be(10);
        v2.Should().Be(0);
    }

    // ─── Non-Generic Task/ValueTask Pipelines ─────────────────────────────────

    [Fact]
    public async Task NonGeneric_Task_SuccessPipeline()
    {
        var tap = false;
        var result = await Task.FromResult(Result.Success())
            .Ensure(() => true, Error.Validation("X", "Y"))
            .Tap(() => tap = true)
            .TapError(e => { })
            .MapError(e => Error.Unexpected("X", "Y"))
            .Finally(r => { });

        result.IsSuccess.Should().BeTrue();
        tap.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_Task_FailurePipeline()
    {
        var tap = false;
        var tapError = false;
        var result = await Task.FromResult(Result.Failure(TestError))
            .Ensure(() => true, Error.Validation("X", "Y"))
            .Tap(() => tap = true)
            .TapError(e => tapError = true)
            .MapError(e => Error.Unexpected("Z", "W"))
            .Finally(r => { });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Z");
        tap.Should().BeFalse();
        tapError.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_Task_Match()
    {
        var v1 = await Task.FromResult(Result.Success()).Match(() => 1, e => 0);
        var v2 = await Task.FromResult(Result.Failure(TestError)).Match(() => 1, e => 0);

        v1.Should().Be(1);
        v2.Should().Be(0);
    }

    [Fact]
    public async Task NonGeneric_ValueTask_SuccessPipeline()
    {
        var tap = false;
        var result = await ValueTask.FromResult(Result.Success())
            .Ensure(() => true, Error.Validation("X", "Y"))
            .Tap(() => tap = true)
            .TapError(e => { })
            .MapError(e => Error.Unexpected("X", "Y"))
            .Finally(r => { });

        result.IsSuccess.Should().BeTrue();
        tap.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_ValueTask_FailurePipeline()
    {
        var tapError = false;
        var result = await ValueTask.FromResult(Result.Failure(TestError))
            .Ensure(() => true, Error.Validation("X", "Y"))
            .Tap(() => { })
            .TapError(e => tapError = true)
            .MapError(e => Error.Unexpected("Z", "W"))
            .Finally(r => { });

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Z");
        tapError.Should().BeTrue();
    }

    [Fact]
    public async Task NonGeneric_ValueTask_Match()
    {
        var v1 = await ValueTask.FromResult(Result.Success()).Match(() => 1, e => 0);
        var v2 = await ValueTask.FromResult(Result.Failure(TestError)).Match(() => 1, e => 0);

        v1.Should().Be(1);
        v2.Should().Be(0);
    }

    // ─── Additional Missing Branches ──────────────────────────────────────────

    [Fact]
    public async Task Ensure_FailurePaths()
    {
        var t1 = await Task.FromResult(Result.Success()).Ensure(() => false, Error.Validation("A", "B"));
        t1.Error.Code.Should().Be("A");

        var vt1 = await ValueTask.FromResult(Result.Success()).Ensure(() => false, Error.Validation("A", "B"));
        vt1.Error.Code.Should().Be("A");

        var vt2 = await ValueTask.FromResult(Result.Success(1)).Ensure(v => false, Error.Validation("A", "B"));
        vt2.Error.Code.Should().Be("A");

        var vt3 = await ValueTask.FromResult(Result.Success(1)).Ensure(v => ValueTask.FromResult(false), Error.Validation("A", "B"));
        vt3.Error.Code.Should().Be("A");
    }

    [Fact]
    public async Task Task_Generic_FinallyAndEnsure()
    {
        var finallyExecuted = false;
        var r1 = await Task.FromResult(Result.Success(1))
            .Ensure(v => Task.FromResult(false), Error.Validation("A", "B"))
            .TapError(e => { })
            .Finally(r => finallyExecuted = true);
        
        r1.Error.Code.Should().Be("A");
        finallyExecuted.Should().BeTrue();
    }
}
