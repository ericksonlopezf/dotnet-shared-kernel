using EricksonLopez.SharedKernel.Domain;
using EricksonLopez.SharedKernel.Pagination;
using EricksonLopez.SharedKernel.Results;
using EricksonLopez.SharedKernel.Specifications;

// ─── 1. Result pattern ────────────────────────────────────────────────────────

Console.WriteLine("=== Result Pattern ===");

var success = CreateUser("Erickson", 30);
if (success.IsSuccess)
    Console.WriteLine($"User created: {success.Value.Name}, Age: {success.Value.Age}");

var failure = CreateUser("", 30);
if (failure.IsFailure)
    Console.WriteLine($"Failed: [{failure.Error.Type}] {failure.Error.Code} — {failure.Error.Description}");

// ─── 2. Fluent pipeline ──────────────────────────────────────────────────────

Console.WriteLine("\n=== Fluent Pipeline ===");

var result = CreateUser("Alice", 25)
    .Ensure(u => u.Age >= 18, Error.Validation("User.Underage", "Must be 18+"))
    .Map(u => u with { Name = u.Name.ToUpperInvariant() })
    .Tap(u => Console.WriteLine($"  Tap: processing {u.Name}"))
    .TapError(e => Console.WriteLine($"  TapError: {e.Description}"));

Console.WriteLine($"Pipeline result: {(result.IsSuccess ? result.Value.Name : result.Error.Description)}");

// ─── 3. Match (exhaustive handling) ──────────────────────────────────────────

Console.WriteLine("\n=== Match ===");

var message = CreateUser("Bob", 42).Match(
    user => $"Welcome, {user.Name}!",
    error => $"Error: {error.Description}");
Console.WriteLine(message);

// ─── 4. Try-pattern and safe access ──────────────────────────────────────────

Console.WriteLine("\n=== Try-pattern & Safe Access ===");

if (CreateUser("Charlie", 28).TryGetValue(out var user))
    Console.WriteLine($"TryGetValue: {user.Name}");

var name = CreateUser("", 0)
    .Map(u => u.Name)
    .GetValueOrDefault("Anonymous");
Console.WriteLine($"GetValueOrDefault: {name}");

// ─── 5. Deconstruct ──────────────────────────────────────────────────────────

Console.WriteLine("\n=== Deconstruct ===");

var (ok, value, error) = CreateUser("Diana", 35);
Console.WriteLine(ok
    ? $"Deconstructed: {value!.Name}"
    : $"Deconstructed error: {error.Description}");

// ─── 6. Error with InnerErrors ───────────────────────────────────────────────

Console.WriteLine("\n=== Compound Errors ===");

var compound = Error.Validation("User.Invalid", "Multiple validation failures",
    Error.Validation("User.Name.Empty", "Name is required"),
    Error.Validation("User.Age.Invalid", "Age must be positive"));

Console.WriteLine($"Compound error: {compound}");
Console.WriteLine($"HasInnerErrors: {compound.HasInnerErrors}");
foreach (var inner in compound.InnerErrors)
    Console.WriteLine($"  → [{inner.Code}] {inner.Description}");

// ─── 7. MapError (layer adaptation) ─────────────────────────────────────────

Console.WriteLine("\n=== MapError ===");

var adapted = CreateUser("", 0)
    .MapError(e => Error.Failure("App.RegistrationFailed", $"Registration failed: {e.Description}"));

Console.WriteLine($"Adapted error: [{adapted.Error.Type}] {adapted.Error.Description}");

// ─── 8. Try (exception bridge) ───────────────────────────────────────────────

Console.WriteLine("\n=== Try (Exception Bridge) ===");

var parsed = Result.Try(
    () => int.Parse("not_a_number"),
    ex => Error.Unexpected("Parse.Failed", ex.Message));
Console.WriteLine($"Try result: {(parsed.IsSuccess ? parsed.Value : parsed.Error.Description)}");

// ─── 9. Combine ──────────────────────────────────────────────────────────────

Console.WriteLine("\n=== Combine ===");

var combined = Result.Combine(
    CreateUser("User1", 25),
    CreateUser("User2", 30));

if (combined.IsSuccess)
{
    var (u1, u2) = combined.Value;
    Console.WriteLine($"Combined: {u1.Name} + {u2.Name}");
}

var failedCombine = Result.Combine(
    CreateUser("", 0),
    CreateUser("Valid", 25));
Console.WriteLine($"Failed combine: {failedCombine.Error.Description}");

// ─── 10. ToResult (drop value) ───────────────────────────────────────────────

Console.WriteLine("\n=== ToResult ===");

Result nonGeneric = CreateUser("Eve", 22).ToResult();
Console.WriteLine($"ToResult: IsSuccess={nonGeneric.IsSuccess}");

// ─── 11. AggregateRoot with Domain Events ────────────────────────────────────

Console.WriteLine("\n=== AggregateRoot & Domain Events ===");

var orderId = Guid.NewGuid();
var order = OrderSample.Create(orderId, "New laptop");
Console.WriteLine($"Order created: {order.Id}");
Console.WriteLine($"Domain events: {order.DomainEvents.Count}");
Console.WriteLine($"Event type: {order.DomainEvents[0].GetType().Name}");

order.ClearDomainEvents();
Console.WriteLine($"After clear: {order.DomainEvents.Count} events");

// ─── 12. ValueObject ─────────────────────────────────────────────────────────

Console.WriteLine("\n=== ValueObject ===");

var money1 = new MoneySample(100m, "USD");
var money2 = new MoneySample(100m, "USD");
var money3 = new MoneySample(200m, "EUR");

Console.WriteLine($"money1 == money2: {money1 == money2}");  // true
Console.WriteLine($"money1 == money3: {money1 == money3}");  // false

// ─── 13. Specification ───────────────────────────────────────────────────────

Console.WriteLine("\n=== Specification Pattern ===");

var products = new[]
{
    new ProductSample("Widget", 10m, true),
    new ProductSample("Gadget", 500m, true),
    new ProductSample("Doohickey", 5m, false),
    new ProductSample("Thingamajig", 75m, true),
};

var spec = new ActiveSpec() & new AffordableSpec(100m);
var filtered = products.Where(spec.IsSatisfiedBy).ToList();
Console.WriteLine($"Active & Affordable: {string.Join(", ", filtered.Select(p => p.Name))}");

// ─── 14. Pagination ──────────────────────────────────────────────────────────

Console.WriteLine("\n=== Pagination ===");

var parameters = PaginationParameters.Of(page: 2, pageSize: 3);
var allItems = Enumerable.Range(1, 10).ToList();
var pageItems = allItems.Skip(parameters.Skip).Take(parameters.PageSize);
var page = PagedList<int>.Create(pageItems, allItems.Count, parameters);

Console.WriteLine($"Page {page.Page} of {page.TotalPages} (total: {page.TotalCount})");
Console.WriteLine($"Items: [{string.Join(", ", page.Items)}]");
Console.WriteLine($"HasPrevious: {page.HasPreviousPage}, HasNext: {page.HasNextPage}");

Console.WriteLine("\n✅ All samples completed successfully!");

// ─── Helper functions ─────────────────────────────────────────────────────────

static Result<UserSample> CreateUser(string name, int age)
{
    if (string.IsNullOrWhiteSpace(name))
        return Error.Validation("User.NameEmpty", "Name cannot be empty.");

    return new UserSample(name, age);
}

// ─── Types ────────────────────────────────────────────────────────────────────

sealed record UserSample(string Name, int Age);

sealed class MoneySample(decimal amount, string currency) : ValueObject
{
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return amount;
        yield return currency;
    }
}

sealed record ProductSample(string Name, decimal Price, bool IsActive);

sealed record OrderCreatedEvent(Guid OrderId) : IDomainEvent;

sealed class OrderSample : AggregateRoot<Guid>
{
    public string Description { get; private set; } = string.Empty;

    public static OrderSample Create(Guid id, string description)
    {
        var order = new OrderSample { Id = id, Description = description };
        order.RaiseDomainEvent(new OrderCreatedEvent(id));
        return order;
    }
}

sealed class AffordableSpec(decimal maxPrice) : Specification<ProductSample>
{
    public override System.Linq.Expressions.Expression<Func<ProductSample, bool>> ToExpression()
        => p => p.Price <= maxPrice;
}

sealed class ActiveSpec : Specification<ProductSample>
{
    public override System.Linq.Expressions.Expression<Func<ProductSample, bool>> ToExpression()
        => p => p.IsActive;
}
