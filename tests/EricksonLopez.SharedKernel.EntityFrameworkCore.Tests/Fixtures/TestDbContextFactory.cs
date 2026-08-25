// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fixtures;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides factory methods for configuring isolated in-memory DbContext options for testing.
/// </summary>
public static class TestDbContextFactory
{
    /// <summary>
    /// Creates isolated in-memory <see cref="DbContextOptions{TContext}"/> with a unique database name.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <returns>A new <see cref="DbContextOptions{TContext}"/> instance configured with an isolated database name.</returns>
    public static DbContextOptions<TContext> CreateInMemoryOptions<TContext>()
        where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
    }

    /// <summary>
    /// Creates in-memory <see cref="DbContextOptions{TContext}"/> with a specific database name.
    /// <para>
    /// <b>Intent:</b> This overload is preserved intentionally for advanced test scenarios requiring shared 
    /// state between different context instances or integration tests where the same in-memory database 
    /// must be accessed across multiple scopes.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">The DbContext type.</typeparam>
    /// <param name="databaseName">The database name.</param>
    /// <returns>A new <see cref="DbContextOptions{TContext}"/> instance.</returns>
    public static DbContextOptions<TContext> CreateInMemoryOptions<TContext>(string databaseName)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(databaseName);

        return new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
    }
}


