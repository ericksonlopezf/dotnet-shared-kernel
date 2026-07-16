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

// Map and Bind chaining
var result = CreateUser("Alice", 25)
    .Map(u => u with { Name = u.Name.ToUpperInvariant() })
    .Bind(u => u.Age >= 18 ? Result.Success(u) : Result.Failure<UserSample>(Error.Validation("User.Underage", "Must be 18+")));

Console.WriteLine($"Chain result: {(result.IsSuccess ? result.Value.Name : result.Error.Description)}");

// ─── 2. ValueObject ───────────────────────────────────────────────────────────

Console.WriteLine("\n=== ValueObject ===");

var money1 = new MoneySample(100m, "USD");
var money2 = new MoneySample(100m, "USD");
var money3 = new MoneySample(200m, "EUR");

Console.WriteLine($"money1 == money2: {money1 == money2}");  // true
Console.WriteLine($"money1 == money3: {money1 == money3}");  // false

// ─── 3. Specification ─────────────────────────────────────────────────────────

Console.WriteLine("\n=== Specification Pattern ===");

var products = new[]
{
    new ProductSample("Widget", 10m, true),
    new ProductSample("Gadget", 500m, true),
    new ProductSample("Doohickey", 5m, false),
    new ProductSample("Thingamajig", 75m, true),
};

var affordable = new AffordableSpec(100m);
var active = new ActiveSpec();
var affordableAndActive = affordable & active;

var filtered = products.Where(affordableAndActive.IsSatisfiedBy).ToList();
Console.WriteLine($"Affordable AND active products: {string.Join(", ", filtered.Select(p => p.Name))}");

// ─── 4. Pagination ────────────────────────────────────────────────────────────

Console.WriteLine("\n=== Pagination ===");

var parameters = PaginationParameters.Of(page: 2, pageSize: 3);
var allItems = Enumerable.Range(1, 10).ToList();
var pageItems = allItems.Skip(parameters.Skip).Take(parameters.PageSize);
var page = PagedList<int>.Create(pageItems, allItems.Count, parameters);

Console.WriteLine($"Page {page.Page} of {page.TotalPages} (total: {page.TotalCount} items)");
Console.WriteLine($"Items: [{string.Join(", ", page.Items)}]");
Console.WriteLine($"HasPrevious: {page.HasPreviousPage}, HasNext: {page.HasNextPage}");

// ─── Helpers ──────────────────────────────────────────────────────────────────

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
