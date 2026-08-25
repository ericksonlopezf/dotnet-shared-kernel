// Copyright © Erickson Lopez. MIT License.
using EricksonLopez.SharedKernel.UnitTests.Common;
namespace EricksonLopez.SharedKernel.UnitTests.Common;

/// <summary>
/// Provides expressive and deterministic values to avoid duplication and "magic strings" in domain tests.
/// </summary>
public static class TestValues
{
    public static class Strings
    {
        public const string Sample = "SampleString";
        public const string UserName = "Alice";
        public const string AlternativeUserName = "Bob";
        public const string EntityIdString = "ORD-001";
        public const string AlternativeEntityIdString = "ORD-002";
        public const string ProductCode = "SKU-12345";
        public const string AlternativeProductCode = "PROD-999";
        public const string Street = "Main St";
        public const string AlternativeStreet = "Second St";
        public const string City = "Santo Domingo";
        public const string AlternativeCity = "Santiago";
        public const string PostalCode = "10101";
        public const string UsdCurrency = "USD";
        public const string EurCurrency = "EUR";
    }

    public static class Numbers
    {
        public const int Positive = 42;
        public const int AlternativePositive = 99;
        public const int Zero = 0;
        public const int Negative = -1;
        public const long SequenceId = 100054321L;
        public const decimal Hundred = 100m;
        public const decimal TwoHundred = 200m;
    }
}
