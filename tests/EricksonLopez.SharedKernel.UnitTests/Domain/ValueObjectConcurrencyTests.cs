namespace EricksonLopez.SharedKernel.UnitTests.Domain;

public sealed class ValueObjectConcurrencyTests
{
    private sealed class ConcurrencyTestValueObject : ValueObject
    {
        public int Id { get; }
        public string Value { get; }

        public ConcurrencyTestValueObject(int id, string value)
        {
            Id = id;
            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Id;
            yield return Value;
        }
    }

    [Fact]
    public void ValueObject_WhenAccessedConcurrently_ShouldBeThreadSafe()
    {
        // Arrange
        var vo1 = new ConcurrencyTestValueObject(1, "test");
        var vo2 = new ConcurrencyTestValueObject(1, "test");
        
        // Act & Assert
        Parallel.For(0, 1000, i =>
        {
            vo1.Equals(vo2).Should().BeTrue();
            vo1.GetHashCode().Should().Be(vo2.GetHashCode());
            
            var vo3 = new ConcurrencyTestValueObject(i, $"test-{i}");
            vo1.Equals(vo3).Should().BeFalse();
        });
    }

    [Fact]
    public void GetHashCode_WhenCalledConcurrently_ShouldBeConsistent()
    {
        // Arrange
        var vo = new ConcurrencyTestValueObject(42, "thread-safe");
        var expectedHash = vo.GetHashCode();

        // Act & Assert
        Parallel.For(0, 1000, _ =>
        {
            vo.GetHashCode().Should().Be(expectedHash);
        });
    }
}
