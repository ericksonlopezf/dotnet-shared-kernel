// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.Dapper.Tests.Fakes;

/// <summary>
/// A non-IStrongId record struct following standard ID convention for testing convention registration.
/// </summary>
public readonly record struct FakeConventionUserId(Guid Value);
