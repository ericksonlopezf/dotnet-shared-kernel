namespace EricksonLopez.SharedKernel.UnitTests.Common;

/// <summary>
/// Provides expressive and deterministic values to avoid duplication and "magic strings" in tests.
/// Contains reusable atomic values instead of building complex objects (strict Object Mother).
/// </summary>
public static class TestValues
{
    public static class Strings
    {
        public const string Sample = "SampleString";
        public const string ErrorCode = "Test.ErrorCode";
        public const string AlternativeErrorCode = "Test.AlternativeErrorCode";
        public const string ErrorMessage = "A standard test error message.";
        public const string AlternativeErrorMessage = "An alternative test error message.";
        public const string BecauseMustFail = "Because it must fail";
        public const string BecauseExpectedSuccess = "Because success is expected";
    }

    public static class Numbers
    {
        public const int Positive = 42;
        public const int AlternativePositive = 99;
        public const int Zero = 0;
        public const int Negative = -1;
        public const int AlternativeNegative = -2;
    }

    public static class Domain
    {
        public const string Currency = "USD";
        public const string AlternativeCurrency = "EUR";
        public const decimal Amount = 100m;
        public const decimal AlternativeAmount = 200m;
        public const string ProductName = "Widget A";
        public const string AlternativeProductName = "Widget B";
        public const string OrderDescription = "Order A";
        public const string AlternativeOrderDescription = "Order B";
    }
}
