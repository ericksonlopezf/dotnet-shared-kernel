using EricksonLopez.SharedKernel;
using NetArchTest.Rules;

namespace EricksonLopez.SharedKernel.ArchitectureTests;

public class ArchitectureTests
{
    private const string SharedKernelNamespace = "EricksonLopez.SharedKernel";

    [Fact]
    public void SharedKernel_ShouldNot_HaveUnwantedDependencies()
    {
        // NOTE: Use ShouldNot().HaveDependencyOnAny() — NOT individual .And().NotHaveDependencyOn()
        // chains, which produce a double-negation that can evaluate incorrectly.
        var result = Types.InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "MediatR",
                "Microsoft.EntityFrameworkCore",
                "Dapper",
                "EricksonLopez.Pagination",
                "Microsoft.Extensions",
                "Microsoft.AspNetCore",
                "System.Reflection.Emit",
                "Newtonsoft.Json",
                "System.Text.Json",
                "System.Collections.Concurrent"
            )
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "The SharedKernel must have zero dependencies on infrastructure, " +
                     "serialization, DI, ORMs, or concurrent collections.");
    }

    [Fact]
    public void SharedKernel_ShouldNot_ContainCastleProxyAwareness()
    {
        // Verifies that GetUnproxiedType() (removed in FINDING-002) has not been re-introduced.
        // Castle.Proxies is an ORM infrastructure concern that must not leak into the domain layer.
        var result = Types.InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Castle")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Castle DynamicProxy awareness is an infrastructure concern — " +
                     "it must never appear in the SharedKernel domain layer.");
    }
}

