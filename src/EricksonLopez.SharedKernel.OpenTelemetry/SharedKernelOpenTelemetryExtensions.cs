// Copyright © Erickson Lopez. MIT License.
using System;
using EricksonLopez.SharedKernel.OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering SharedKernel diagnostic sources with OpenTelemetry tracing and metrics builders.
/// </summary>
public static class SharedKernelOpenTelemetryExtensions
{
    /// <summary>
    /// Adds the SharedKernel diagnostic <see cref="System.Diagnostics.ActivitySource"/> to the tracer provider builder.
    /// </summary>
    /// <param name="builder">The tracer provider builder to configure.</param>
    /// <returns>The configured tracer provider builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static TracerProviderBuilder AddSharedKernelInstrumentation(this TracerProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddSource(SharedKernelInstrumentation.ActivitySourceName);
    }

    /// <summary>
    /// Adds the SharedKernel diagnostic <see cref="System.Diagnostics.Metrics.Meter"/> to the meter provider builder.
    /// </summary>
    /// <param name="builder">The meter provider builder to configure.</param>
    /// <returns>The configured meter provider builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/></exception>
    public static MeterProviderBuilder AddSharedKernelInstrumentation(this MeterProviderBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddMeter(SharedKernelInstrumentation.ActivitySourceName);
    }
}
