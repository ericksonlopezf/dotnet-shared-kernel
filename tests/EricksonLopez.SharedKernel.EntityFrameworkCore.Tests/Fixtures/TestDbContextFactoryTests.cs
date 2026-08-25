// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fixtures;

using System.Linq;
using AwesomeAssertions;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Xunit;

public class TestDbContextFactoryTests
{
    [Fact]
    public void CreateInMemoryOptions_WithoutParameters_GeneratesUniqueIsolatedOptions()
    {
        var options1 = TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>();
        var options2 = TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>();

        options1.Should().NotBeNull();
        options2.Should().NotBeNull();

#pragma warning disable EF1001 // Internal EF Core API usage for assertion verification
        var ext1 = options1.Extensions.OfType<InMemoryOptionsExtension>().FirstOrDefault();
        var ext2 = options2.Extensions.OfType<InMemoryOptionsExtension>().FirstOrDefault();

        ext1.Should().NotBeNull();
        ext2.Should().NotBeNull();
        ext1!.StoreName.Should().NotBe(ext2!.StoreName);
#pragma warning restore EF1001
    }

    [Fact]
    public void CreateInMemoryOptions_WithSpecificDatabaseName_ConfiguresSpecifiedDatabase()
    {
        const string dbName = "CustomIsolatedTestDb";
        var options = TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>(dbName);

        options.Should().NotBeNull();

#pragma warning disable EF1001 // Internal EF Core API usage for assertion verification
        var ext = options.Extensions.OfType<InMemoryOptionsExtension>().FirstOrDefault();
        ext.Should().NotBeNull();
        ext!.StoreName.Should().Be(dbName);
#pragma warning restore EF1001
    }

    [Fact]
    public async Task CreateInMemoryOptions_WithSameDatabaseName_SharesStateAcrossContextInstances()
    {
        var dbName = $"SharedTestDb_{Guid.NewGuid():N}";
        var options1 = TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>(dbName);
        var options2 = TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>(dbName);

        await using (var context1 = new TestSharedKernelDbContext(options1))
        {
            context1.PlainEntities.Add(new PlainEntity { Id = 100, Description = "PersistedInContext1" });
            await context1.SaveChangesAsync();
        }

        await using (var context2 = new TestSharedKernelDbContext(options2))
        {
            var entity = await context2.PlainEntities.FindAsync(100);
            entity.Should().NotBeNull();
            entity!.Description.Should().Be("PersistedInContext1");
        }
    }

    [Fact]
    public void CreateInMemoryOptions_WithNullDatabaseName_ThrowsArgumentNullException()
    {
        var act = () => TestDbContextFactory.CreateInMemoryOptions<TestSharedKernelDbContext>(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("databaseName");
    }
}



