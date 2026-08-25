// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;

using EricksonLopez.SharedKernel.EntityFrameworkCore;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;
using Microsoft.EntityFrameworkCore;

public sealed class TestSharedKernelDbContext : DbContext
{
    private readonly DomainEventsInterceptor? _interceptor;

    public DbSet<CustomerAggregate> Customers => Set<CustomerAggregate>();
    public DbSet<PlainEntity> PlainEntities => Set<PlainEntity>();

    public TestSharedKernelDbContext(DbContextOptions<TestSharedKernelDbContext> options, DomainEventsInterceptor? interceptor = null)
        : base(options)
    {
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_interceptor is not null)
        {
            optionsBuilder.AddInterceptors(_interceptor);
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureStrongId<CustomerId, Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.IgnoreDomainEvents();
        modelBuilder.Entity<CustomerAggregate>().HasKey(c => c.Id);
        modelBuilder.Entity<PlainEntity>().HasKey(p => p.Id);
    }
}


