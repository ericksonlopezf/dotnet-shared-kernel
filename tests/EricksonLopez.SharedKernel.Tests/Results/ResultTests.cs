using EricksonLopez.SharedKernel.Results;
using AwesomeAssertions;

namespace EricksonLopez.SharedKernel.Tests.Results;

public sealed class ResultTests
{
    // ─── Success ─────────────────────────────────────────────────────────────

    [Fact]
    public void Success_ShouldHaveIsSuccessTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithValue_ShouldExposeValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    // ─── Failure ──────────────────────────────────────────────────────────────

    [Fact]
    public void Failure_ShouldHaveIsFailureTrue()
    {
        var error = Error.NotFound("User.NotFound", "User was not found.");
        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrow()
    {
        var result = Result.Failure<string>(Error.NotFound("X.NotFound", "Not found"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result*");
    }

    // ─── Guard clauses ────────────────────────────────────────────────────────

    [Fact]
    public void Success_WithNullValue_ShouldNotThrow()
    {
        var act = () => Result.Success<string>(null!);
        act.Should().NotThrow();
    }

    // ─── Implicit conversion ──────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailure()
    {
        var error = Error.Validation("Name.Empty", "Name cannot be empty.");
        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccess()
    {
        Result<int> result = 99;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    // ─── Map ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_ShouldTransformValue()
    {
        var result = Result.Success(5);
        var mapped = result.Map(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailure_ShouldPropagateError()
    {
        var error = Error.Failure("X.Error", "Something went wrong");
        var result = Result.Failure<int>(error);
        var mapped = result.Map(x => x.ToString());

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(error);
    }

    // ─── Bind ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_ShouldInvokeNext()
    {
        var result = Result.Success(5);
        var bound = result.Bind(x => Result.Success(x + 1));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be(6);
    }

    [Fact]
    public void Bind_OnFailure_ShouldNotInvokeNext()
    {
        var error = Error.Failure("X.Error", "Error");
        var result = Result.Failure<int>(error);
        var invoked = false;

        var bound = result.Bind(x =>
        {
            invoked = true;
            return Result.Success(x);
        });

        invoked.Should().BeFalse();
        bound.IsFailure.Should().BeTrue();
    }

    // ─── Match ────────────────────────────────────────────────────────────────

    [Fact]
    public void Match_NonGeneric_OnSuccess_ShouldInvokeSuccessFunc()
    {
        var result = Result.Success();
        var output = result.Match(() => "ok", e => $"fail: {e.Code}");

        output.Should().Be("ok");
    }

    [Fact]
    public void Match_NonGeneric_OnFailure_ShouldInvokeFailureFunc()
    {
        var result = Result.Failure(Error.NotFound("X", "Y"));
        var output = result.Match(() => "ok", e => $"fail: {e.Code}");

        output.Should().Be("fail: X");
    }

    [Fact]
    public void Match_Generic_OnSuccess_ShouldInvokeSuccessFunc()
    {
        var result = Result.Success(42);
        var output = result.Match(v => $"value: {v}", e => $"fail: {e.Code}");

        output.Should().Be("value: 42");
    }

    [Fact]
    public void Match_Generic_OnFailure_ShouldInvokeFailureFunc()
    {
        var result = Result.Failure<int>(Error.Failure("X", "Y"));
        var output = result.Match(v => $"value: {v}", e => $"fail: {e.Code}");

        output.Should().Be("fail: X");
    }

    // ─── Tap ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Tap_NonGeneric_OnSuccess_ShouldExecuteAction()
    {
        var executed = false;
        var result = Result.Success().Tap(() => executed = true);

        executed.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Tap_NonGeneric_OnFailure_ShouldNotExecuteAction()
    {
        var executed = false;
        Result.Failure(Error.Failure("X", "Y")).Tap(() => executed = true);

        executed.Should().BeFalse();
    }

    [Fact]
    public void Tap_Generic_OnSuccess_ShouldExecuteWithValue()
    {
        int? captured = null;
        var result = Result.Success(42).Tap(v => captured = v);

        captured.Should().Be(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_Generic_OnFailure_ShouldNotExecute()
    {
        int? captured = null;
        Result.Failure<int>(Error.Failure("X", "Y")).Tap(v => captured = v);

        captured.Should().BeNull();
    }

    [Fact]
    public void TapError_NonGeneric_OnFailure_ShouldExecuteWithError()
    {
        Error? captured = null;
        var error = Error.NotFound("X", "Y");
        var result = Result.Failure(error).TapError(e => captured = e);

        captured.Should().Be(error);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TapError_NonGeneric_OnSuccess_ShouldNotExecute()
    {
        Error? captured = null;
        Result.Success().TapError(e => captured = e);

        captured.Should().BeNull();
    }

    // ─── TapError ─────────────────────────────────────────────────────────────

    [Fact]
    public void TapError_OnFailure_ShouldExecuteWithError()
    {
        Error? captured = null;
        var error = Error.NotFound("X", "Y");
        var result = Result.Failure<int>(error).TapError(e => captured = e);

        captured.Should().Be(error);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TapError_OnSuccess_ShouldNotExecute()
    {
        Error? captured = null;
        Result.Success(42).TapError(e => captured = e);

        captured.Should().BeNull();
    }

    [Fact]
    public void TapError_Generic_ShouldReturnTypedResult()
    {
        // Verify the chain stays typed (returns Result<int>, not Result)
        var result = Result.Failure<int>(Error.Failure("X", "Y"))
            .TapError(e => { })
            .Match(v => v, e => -1);

        result.Should().Be(-1);
    }

    // ─── Ensure ───────────────────────────────────────────────────────────────

    [Fact]
    public void Ensure_NonGeneric_WhenPredicateTrue_ShouldReturnSuccess()
    {
        var result = Result.Success()
            .Ensure(() => true, Error.Failure("X", "Y"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_NonGeneric_WhenPredicateFalse_ShouldReturnFailure()
    {
        var error = Error.Failure("X", "Condition not met");
        var result = Result.Success()
            .Ensure(() => false, error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Ensure_NonGeneric_OnExistingFailure_ShouldShortCircuit()
    {
        var originalError = Error.NotFound("X", "Original");
        var predicateEvaluated = false;

        var result = Result.Failure(originalError)
            .Ensure(() => { predicateEvaluated = true; return true; }, Error.Failure("Y", "New"));

        predicateEvaluated.Should().BeFalse();
        result.Error.Should().Be(originalError);
    }

    [Fact]
    public void Ensure_Generic_WhenPredicateTrue_ShouldPreserveValue()
    {
        var result = Result.Success(42)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Ensure_Generic_WhenPredicateFalse_ShouldReturnFailure()
    {
        var error = Error.Validation("X", "Must be positive");
        var result = Result.Success(-1)
            .Ensure(v => v > 0, error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    // ─── Recover ──────────────────────────────────────────────────────────────

    [Fact]
    public void Recover_OnFailure_ShouldApplyRecovery()
    {
        var result = Result.Failure<int>(Error.NotFound("X", "Y"))
            .Recover(e => Result.Success(0));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void Recover_OnSuccess_ShouldReturnUnchanged()
    {
        var recoveryInvoked = false;
        var result = Result.Success(42)
            .Recover(e => { recoveryInvoked = true; return Result.Success(0); });

        recoveryInvoked.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Recover_CanReturnFailure()
    {
        var fallbackError = Error.Unavailable("Service.Down", "Both sources failed");
        var result = Result.Failure<int>(Error.NotFound("X", "Y"))
            .Recover(e => Result.Failure<int>(fallbackError));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(fallbackError);
    }

    // ─── Finally ──────────────────────────────────────────────────────────────

    [Fact]
    public void Finally_NonGeneric_AlwaysExecutes_OnSuccess()
    {
        var executed = false;
        Result.Success().Finally(r => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void Finally_NonGeneric_AlwaysExecutes_OnFailure()
    {
        var executed = false;
        Result.Failure(Error.Failure("X", "Y")).Finally(r => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void Finally_Generic_AlwaysExecutes()
    {
        bool? wasSuccess = null;

        Result.Success(42).Finally(r => wasSuccess = r.IsSuccess);
        wasSuccess.Should().BeTrue();

        Result.Failure<int>(Error.Failure("X", "Y")).Finally(r => wasSuccess = r.IsSuccess);
        wasSuccess.Should().BeFalse();
    }

    // ─── Try ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Try_Action_OnSuccess_ShouldReturnSuccess()
    {
        var result = Result.Try(
            () => { /* no exception */ },
            ex => Error.Unexpected("X", ex.Message));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Try_Action_OnException_ShouldReturnFailure()
    {
        var result = Result.Try(
            () => throw new InvalidOperationException("boom"),
            ex => Error.Unexpected("Op.Failed", ex.Message));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Unexpected);
        result.Error.Description.Should().Be("boom");
    }

    [Fact]
    public void Try_Func_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Try(
            () => 42,
            ex => Error.Unexpected("X", ex.Message));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Try_Func_OnException_ShouldReturnFailure()
    {
        var result = Result.Try(
            () => int.Parse("not-a-number"),
            ex => Error.Validation("Parse.Failed", ex.Message));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    // ─── Combine ──────────────────────────────────────────────────────────────

    [Fact]
    public void Combine_AllSuccess_ShouldReturnSuccess()
    {
        var result = Result.Combine(
            Result.Success(),
            Result.Success(),
            Result.Success());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_OneFailure_ShouldReturnThatError()
    {
        var error = Error.NotFound("X", "Y");
        var result = Result.Combine(
            Result.Success(),
            Result.Failure(error),
            Result.Success());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Combine_MultipleFailures_ShouldReturnCompoundError()
    {
        var result = Result.Combine(
            Result.Failure(Error.Validation("A", "A-desc")),
            Result.Success(),
            Result.Failure(Error.Validation("B", "B-desc")));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Result.CombinedErrors");
        result.Error.HasInnerErrors.Should().BeTrue();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Empty_ShouldReturnSuccess()
    {
        var result = Result.Combine();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Combine_Typed_AllSuccess_ShouldReturnValueList()
    {
        var result = Result.Combine<int>(
            Result.Success(1),
            Result.Success(2),
            Result.Success(3));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void Combine_Typed_WithFailure_ShouldAggregateErrors()
    {
        var result = Result.Combine<int>(
            Result.Success(1),
            Result.Failure<int>(Error.Validation("X", "invalid")),
            Result.Failure<int>(Error.Validation("Y", "invalid")));

        result.IsFailure.Should().BeTrue();
        result.Error.HasInnerErrors.Should().BeTrue();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Typed_OneFailure_ShouldReturnThatError()
    {
        var error = Error.Validation("X", "invalid");
        var result = Result.Combine<int>(
            Result.Success(1),
            Result.Failure<int>(error),
            Result.Success(3));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.HasInnerErrors.Should().BeFalse();
    }

    [Fact]
    public void Combine_Typed_Empty_ShouldReturnSuccess()
    {
        var result = Result.Combine<int>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Combine_Tuple2_AllSuccess_ShouldReturnTuple()
    {
        var result = Result.Combine(
            Result.Success("hello"),
            Result.Success(42));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(("hello", 42));
    }

    [Fact]
    public void Combine_Tuple2_WithFailure_ShouldReturnError()
    {
        var error = Error.NotFound("X", "Y");
        var result = Result.Combine(
            Result.Success("hello"),
            Result.Failure<int>(error));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Combine_Tuple3_AllSuccess_ShouldReturnTuple()
    {
        var result = Result.Combine(
            Result.Success("hello"),
            Result.Success(42),
            Result.Success(true));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(("hello", 42, true));
    }

    [Fact]
    public void Combine_Tuple2_WithMultipleFailures_ShouldAggregateErrors()
    {
        var result = Result.Combine(
            Result.Failure<string>(Error.Validation("X", "Y")),
            Result.Failure<int>(Error.Validation("Z", "W")));

        result.IsFailure.Should().BeTrue();
        result.Error.InnerErrors.Should().HaveCount(2);
    }

    [Fact]
    public void Combine_Tuple2_WithFirstSuccessSecondFailure_ShouldReturnError()
    {
        var error = Error.Validation("X", "Y");
        var result = Result.Combine(
            Result.Success("hello"),
            Result.Failure<int>(error));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Combine_Tuple2_WithFirstFailureSecondSuccess_ShouldReturnError()
    {
        var error = Error.Validation("X", "Y");
        var result = Result.Combine(
            Result.Failure<string>(error),
            Result.Success(42));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Combine_Tuple3_WithFailures_ShouldAggregateErrors()
    {
        var error1 = Error.Validation("X", "Y");
        var error2 = Error.Validation("A", "B");
        var error3 = Error.Validation("C", "D");

        // 1 failure
        var r1 = Result.Combine(Result.Failure<string>(error1), Result.Success(1), Result.Success(true));
        r1.IsFailure.Should().BeTrue();
        r1.Error.Should().Be(error1);

        var r2 = Result.Combine(Result.Success(""), Result.Failure<int>(error2), Result.Success(true));
        r2.IsFailure.Should().BeTrue();
        r2.Error.Should().Be(error2);

        var r3 = Result.Combine(Result.Success(""), Result.Success(1), Result.Failure<bool>(error3));
        r3.IsFailure.Should().BeTrue();
        r3.Error.Should().Be(error3);

        // multiple failures
        var r4 = Result.Combine(Result.Failure<string>(error1), Result.Failure<int>(error2), Result.Failure<bool>(error3));
        r4.IsFailure.Should().BeTrue();
        r4.Error.InnerErrors.Should().HaveCount(3);
    }

    // ─── Pipeline composition ─────────────────────────────────────────────────

    [Fact]
    public void FullPipeline_ShouldComposeCorrectly()
    {
        var logged = false;

        var result = Result.Success(10)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"))
            .Map(v => v * 2)
            .Tap(v => logged = true)
            .Bind(v => v <= 100
                ? Result.Success($"Value: {v}")
                : Result.Failure<string>(Error.Failure("X", "Too large")));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Value: 20");
        logged.Should().BeTrue();
    }

    [Fact]
    public void FullPipeline_FailureShortCircuits()
    {
        var mapExecuted = false;

        var result = Result.Success(-5)
            .Ensure(v => v > 0, Error.Validation("X", "Must be positive"))
            .Map(v => { mapExecuted = true; return v * 2; })
            .Tap(v => { });

        result.IsFailure.Should().BeTrue();
        mapExecuted.Should().BeFalse();
    }

    // ─── TryGetValue ──────────────────────────────────────────────────────────

    [Fact]
    public void TryGetValue_OnSuccess_ShouldReturnTrueAndValue()
    {
        var result = Result.Success(42);

        var got = result.TryGetValue(out var value);

        got.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_OnFailure_ShouldReturnFalse()
    {
        var result = Result.Failure<int>(Error.Failure("X", "Y"));

        var got = result.TryGetValue(out var value);

        got.Should().BeFalse();
        value.Should().Be(default(int));
    }

    [Fact]
    public void TryGetValue_ReferenceType_OnFailure_ShouldReturnNull()
    {
        var result = Result.Failure<string>(Error.Failure("X", "Y"));

        var got = result.TryGetValue(out var value);

        got.Should().BeFalse();
        value.Should().BeNull();
    }

    // ─── TryGetError ──────────────────────────────────────────────────────────

    [Fact]
    public void TryGetError_NonGeneric_OnFailure_ShouldReturnTrueAndError()
    {
        var expectedError = Error.NotFound("X", "Y");
        var result = Result.Failure(expectedError);

        var got = result.TryGetError(out var error);

        got.Should().BeTrue();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void TryGetError_NonGeneric_OnSuccess_ShouldReturnFalse()
    {
        var result = Result.Success();

        var got = result.TryGetError(out var error);

        got.Should().BeFalse();
    }

    [Fact]
    public void TryGetError_Generic_OnFailure_ShouldReturnTrueAndError()
    {
        var expectedError = Error.Validation("X", "Y");
        var result = Result.Failure<int>(expectedError);

        var got = result.TryGetError(out var error);

        got.Should().BeTrue();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void TryGetError_Generic_OnSuccess_ShouldReturnFalse()
    {
        var result = Result.Success(42);

        var got = result.TryGetError(out var error);

        got.Should().BeFalse();
    }

    // ─── GetValueOrDefault ────────────────────────────────────────────────────

    [Fact]
    public void GetValueOrDefault_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);

        result.GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ShouldReturnDefault()
    {
        var result = Result.Failure<int>(Error.Failure("X", "Y"));

        result.GetValueOrDefault(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOrDefault_WithFunc_OnSuccess_ShouldReturnValue()
    {
        var result = Result.Success(42);
        var fallbackInvoked = false;

        var value = result.GetValueOrDefault(e => { fallbackInvoked = true; return 0; });

        value.Should().Be(42);
        fallbackInvoked.Should().BeFalse();
    }

    [Fact]
    public void GetValueOrDefault_WithFunc_OnFailure_ShouldInvokeFallback()
    {
        var result = Result.Failure<int>(Error.NotFound("X", "Not found"));

        var value = result.GetValueOrDefault(e => e.Type == ErrorType.NotFound ? -1 : -2);

        value.Should().Be(-1);
    }

    // ─── MapError ─────────────────────────────────────────────────────────────

    [Fact]
    public void MapError_NonGeneric_OnFailure_ShouldTransformError()
    {
        var result = Result.Failure(Error.NotFound("Repo.NotFound", "Entity not found"));

        var mapped = result.MapError(e =>
            Error.Failure("App.Error", $"Operation failed: {e.Description}"));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Code.Should().Be("App.Error");
        mapped.Error.Description.Should().Be("Operation failed: Entity not found");
    }

    [Fact]
    public void MapError_NonGeneric_OnSuccess_ShouldReturnUnchanged()
    {
        var result = Result.Success();
        var mapperInvoked = false;

        var mapped = result.MapError(e => { mapperInvoked = true; return e; });

        mapped.IsSuccess.Should().BeTrue();
        mapperInvoked.Should().BeFalse();
    }

    [Fact]
    public void MapError_Generic_OnFailure_ShouldTransformError()
    {
        var result = Result.Failure<int>(Error.NotFound("X", "Not found"));

        var mapped = result.MapError(e =>
            Error.Unavailable("Service.Down", $"Adapted: {e.Code}"));

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Code.Should().Be("Service.Down");
        mapped.Error.Type.Should().Be(ErrorType.Unavailable);
    }

    [Fact]
    public void MapError_Generic_OnSuccess_ShouldPreserveValue()
    {
        var result = Result.Success(42);

        var mapped = result.MapError(e => Error.Failure("X", "Y"));

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    // ─── ToResult ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToResult_OnSuccess_ShouldReturnSuccessResult()
    {
        var typed = Result.Success(42);

        var result = typed.ToResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_OnFailure_ShouldPreserveError()
    {
        var error = Error.NotFound("X", "Not found");
        var typed = Result.Failure<int>(error);

        var result = typed.ToResult();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    // ─── Deconstruct ──────────────────────────────────────────────────────────

    [Fact]
    public void Deconstruct_OnSuccess_ShouldExposeComponents()
    {
        var result = Result.Success(42);

        var (isSuccess, value, error) = result;

        isSuccess.Should().BeTrue();
        value.Should().Be(42);
        error.Should().Be(Error.None);
    }

    [Fact]
    public void Deconstruct_OnFailure_ShouldExposeComponents()
    {
        var expectedError = Error.NotFound("X", "Not found");
        var result = Result.Failure<string>(expectedError);

        var (isSuccess, value, error) = result;

        isSuccess.Should().BeFalse();
        value.Should().BeNull();
        error.Should().Be(expectedError);
    }

    [Fact]
    public void Deconstruct_CanBeUsedInIfStatement()
    {
        var result = Result.Success("hello");

        var (ok, value, _) = result;
        var output = ok ? value!.ToUpper() : "FALLBACK";

        output.Should().Be("HELLO");
    }

    // ─── Internal Constructor Guard Clauses ───────────────────────────────────

    [Fact]
    public void Constructor_SuccessWithError_ShouldThrow()
    {
        // Result is protected, we can invoke it via reflection or indirectly.
        // Actually, Result<T> constructor calls base(true, Error.None) and we can't easily hit this
        // except via reflection, but for coverage we can use a small derived class or reflection.
        var action = () => Activator.CreateInstance(typeof(Result), 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, 
            null, [true, Error.Failure("X", "Y")], null);

        action.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*successful result cannot have an error*");
    }

    [Fact]
    public void Constructor_FailureWithoutError_ShouldThrow()
    {
        var action = () => Activator.CreateInstance(typeof(Result), 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, 
            null, [false, Error.None], null);

        action.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*failure result must have an error*");
    }

    [Fact]
    public void ImplicitConversion_FromErrorToNonGenericResult_ShouldCreateFailure()
    {
        var error = Error.Validation("Name.Empty", "Name cannot be empty.");
        Result result = error; // Uses implicit operator Result(Error error)

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }
}
