// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.Dapper.Tests.Fakes;

using System.Reflection;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;

public class FakeThrowingAssembly : Assembly
{
    public override Type[] GetTypes()
    {
        throw new ReflectionTypeLoadException(
            [typeof(OrderId), typeof(AbstractStrongId), typeof(ICustomStrongId), null],
            [new InvalidOperationException("Type load failure")]);
    }
}


