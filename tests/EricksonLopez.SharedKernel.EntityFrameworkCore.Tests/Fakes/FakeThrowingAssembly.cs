// Copyright © Erickson Lopez. MIT License.
using System;
using System.Reflection;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;

public class FakeThrowingAssembly : Assembly
{
    public override Type[] GetTypes()
    {
        throw new ReflectionTypeLoadException(
            [typeof(CustomerId), typeof(AbstractStrongId), typeof(ICustomStrongId), null],
            [new InvalidOperationException("Type load failure")]);
    }
}
