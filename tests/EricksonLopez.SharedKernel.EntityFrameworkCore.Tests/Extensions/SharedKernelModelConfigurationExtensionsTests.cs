// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Extensions;

using AwesomeAssertions;
using EricksonLopez.DomainPrimitives;
using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class SharedKernelModelConfigurationExtensionsTests
{
    private class TestStrongIdEntity
    {
        public CustomerId Id { get; set; }
    }

    private class TestStrongIdDbContext : DbContext
    {
        public TestStrongIdDbContext(DbContextOptions<TestStrongIdDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongId<CustomerId, Guid>();
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestStrongIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestStrongIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongId_WithValidStrongId_ConfiguresValueConverter()
    {
        var options = new DbContextOptionsBuilder<TestStrongIdDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestStrongIdDbContext(options);
        
        var entityType = context.Model.FindEntityType(typeof(TestStrongIdEntity))!;
        var property = entityType.FindProperty(nameof(TestStrongIdEntity.Id))!;

        property.GetValueConverter().Should().BeOfType<StrongIdValueConverter<CustomerId, Guid>>();
    }
    [Fact]
    public void ConfigureStrongId_WithNullBuilder_ThrowsArgumentNullException()
    {
        ModelConfigurationBuilder builder = null!;

        var act = () => builder.ConfigureStrongId<CustomerId, Guid>();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationBuilder");
    }

    [Fact]
    public void IgnoreDomainEvents_WithNullBuilder_ThrowsArgumentNullException()
    {
        ModelBuilder builder = null!;

        var act = () => builder.IgnoreDomainEvents();

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("modelBuilder");
    }

    [Fact]
    public void IgnoreDomainEvents_IgnoresDrainDomainEventsOnlyOnDomainEventEntities()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<CustomerAggregate>();
        modelBuilder.Entity<PlainEntity>();

        var returnedBuilder = modelBuilder.IgnoreDomainEvents();
        returnedBuilder.Should().BeSameAs(modelBuilder);

        var customerEntity = modelBuilder.Model.FindEntityType(typeof(CustomerAggregate))!;
        var plainEntity = modelBuilder.Model.FindEntityType(typeof(PlainEntity))!;

        customerEntity.IsIgnored(nameof(IHasDomainEvents.DrainDomainEvents)).Should().BeTrue();
        plainEntity.IsIgnored(nameof(IHasDomainEvents.DrainDomainEvents)).Should().BeFalse();
    }

    private class TestAssemblyStrongIdDbContext : DbContext
    {
        public TestAssemblyStrongIdDbContext(DbContextOptions<TestAssemblyStrongIdDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssembly(typeof(CustomerId).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestStrongIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestStrongIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_WithValidAssembly_ConfiguresValueConverters()
    {
        var options = new DbContextOptionsBuilder<TestAssemblyStrongIdDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAssemblyStrongIdDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(TestStrongIdEntity))!;
        var property = entityType.FindProperty(nameof(TestStrongIdEntity.Id))!;

        property.GetValueConverter().Should().BeOfType<StrongIdValueConverter<CustomerId, Guid>>();
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_WithNullBuilder_ThrowsArgumentNullException()
    {
        ModelConfigurationBuilder builder = null!;

        var act = () => builder.ConfigureStrongIdsFromAssembly(typeof(CustomerId).Assembly);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationBuilder");
    }

    private class TestNullAssemblyDbContext : DbContext
    {
        public Exception? ThrownException { get; private set; }

        public TestNullAssemblyDbContext(DbContextOptions<TestNullAssemblyDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            try
            {
                configurationBuilder.ConfigureStrongIdsFromAssembly(null!);
            }
            catch (Exception ex)
            {
                ThrownException = ex;
            }

            base.ConfigureConventions(configurationBuilder);
        }
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_WithNullAssembly_ThrowsArgumentNullException()
    {
        var options = new DbContextOptionsBuilder<TestNullAssemblyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNullAssemblyDbContext(options);
        _ = context.Model; // Force model building

        context.ThrownException.Should().NotBeNull();
        context.ThrownException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("assembly");
    }

    [Fact]
    public void ConfigureStrongIdsFromAssemblies_WithNullBuilder_ThrowsArgumentNullException()
    {
        ModelConfigurationBuilder builder = null!;

        var act = () => builder.ConfigureStrongIdsFromAssemblies(typeof(CustomerId).Assembly);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationBuilder");
    }

    private class TestNullAssembliesDbContext : DbContext
    {
        public Exception? ThrownException { get; private set; }

        public TestNullAssembliesDbContext(DbContextOptions<TestNullAssembliesDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            try
            {
                configurationBuilder.ConfigureStrongIdsFromAssemblies(null!);
            }
            catch (Exception ex)
            {
                ThrownException = ex;
            }

            base.ConfigureConventions(configurationBuilder);
        }
    }

    [Fact]
    public void ConfigureStrongIdsFromAssemblies_WithNullAssemblies_ThrowsArgumentNullException()
    {
        var options = new DbContextOptionsBuilder<TestNullAssembliesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNullAssembliesDbContext(options);
        _ = context.Model; // Force model building

        context.ThrownException.Should().NotBeNull();
        context.ThrownException.Should().BeOfType<ArgumentNullException>()
            .Which.ParamName.Should().Be("assemblies");
    }

    private class TestMultiAssemblyDbContext : DbContext
    {
        public TestMultiAssemblyDbContext(DbContextOptions<TestMultiAssemblyDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssemblies(
                typeof(CustomerId).Assembly,
                typeof(SharedKernelModelConfigurationExtensionsTests).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        public DbSet<TestStrongIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssemblies_WithValidAssemblies_RegistersConverters()
    {
        var options = new DbContextOptionsBuilder<TestMultiAssemblyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestMultiAssemblyDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(TestStrongIdEntity))!;
        var property = entityType.FindProperty(nameof(TestStrongIdEntity.Id))!;

        property.GetValueConverter().Should().BeOfType<StrongIdValueConverter<CustomerId, Guid>>();
    }

    [Fact]
    public void ConfigureStrongIdsFromAssemblies_WithNullBuilderAndEmptyAssemblies_ThrowsArgumentNullException()
    {
        ModelConfigurationBuilder builder = null!;

        var act = () => builder.ConfigureStrongIdsFromAssemblies(Array.Empty<System.Reflection.Assembly>());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configurationBuilder");
    }

    private class TestThrowingAssemblyDbContext : DbContext
    {
        public TestThrowingAssemblyDbContext(DbContextOptions<TestThrowingAssemblyDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssembly(new FakeThrowingAssembly());
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestStrongIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestStrongIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_WhenReflectionTypeLoadExceptionThrown_RegistersAvailableConcreteTypes()
    {
        var options = new DbContextOptionsBuilder<TestThrowingAssemblyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestThrowingAssemblyDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(TestStrongIdEntity))!;
        var property = entityType.FindProperty(nameof(TestStrongIdEntity.Id))!;

        property.GetValueConverter().Should().BeOfType<StrongIdValueConverter<CustomerId, Guid>>();
    }

    private class TestNullElementAssembliesDbContext : DbContext
    {
        public TestNullElementAssembliesDbContext(DbContextOptions<TestNullElementAssembliesDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssemblies(
                null!,
                typeof(CustomerId).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestStrongIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestStrongIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssemblies_WithNullElementInArray_SkipsNullAndRegistersValidAssembly()
    {
        var options = new DbContextOptionsBuilder<TestNullElementAssembliesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestNullElementAssembliesDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(TestStrongIdEntity))!;
        var property = entityType.FindProperty(nameof(TestStrongIdEntity.Id))!;

        property.GetValueConverter().Should().BeOfType<StrongIdValueConverter<CustomerId, Guid>>();
    }

    private class TestAbstractIdEntity
    {
        public int Id { get; set; }
        public AbstractStrongId AbstractProp { get; set; } = null!;
    }

    private class TestAbstractIdDbContext : DbContext
    {
        public TestAbstractIdDbContext(DbContextOptions<TestAbstractIdDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssembly(typeof(AbstractStrongId).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestAbstractIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestAbstractIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_DoesNotRegisterAbstractStrongIdTypes()
    {
        var options = new DbContextOptionsBuilder<TestAbstractIdDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestAbstractIdDbContext(options);

        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>(
            because: "Abstract strong IDs must be skipped during assembly scanning and cannot be mapped as scalar value converters.");
    }

    private class TestInterfaceIdEntity
    {
        public int Id { get; set; }
        public ICustomStrongId InterfaceProp { get; set; } = null!;
    }

    private class TestInterfaceIdDbContext : DbContext
    {
        public TestInterfaceIdDbContext(DbContextOptions<TestInterfaceIdDbContext> options) : base(options)
        {
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.ConfigureStrongIdsFromAssembly(typeof(ICustomStrongId).Assembly);
            base.ConfigureConventions(configurationBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestInterfaceIdEntity>().HasKey(e => e.Id);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TestInterfaceIdEntity> Entities { get; set; } = null!;
    }

    [Fact]
    public void ConfigureStrongIdsFromAssembly_DoesNotRegisterInterfaceStrongIdTypes()
    {
        var options = new DbContextOptionsBuilder<TestInterfaceIdDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new TestInterfaceIdDbContext(options);

        var act = () => _ = context.Model;

        act.Should().Throw<InvalidOperationException>(
            because: "Interface strong IDs must be skipped during assembly scanning and cannot be mapped as scalar value converters.");
    }
}






