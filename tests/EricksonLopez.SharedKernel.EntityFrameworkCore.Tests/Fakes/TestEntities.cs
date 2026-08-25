// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.SharedKernel.EntityFrameworkCore.Tests.Fakes;

using EricksonLopez.SharedKernel;
using EricksonLopez.SharedKernel.TestingUtilities.Fakes;

public sealed record CustomerRegisteredEvent(CustomerId CustomerId, string Name) : DomainEvent;

public sealed record CustomerNameUpdatedEvent(CustomerId CustomerId, string NewName) : DomainEvent;

public sealed class CustomerAggregate : AggregateRoot<CustomerId>
{
    public string Name { get; private set; }

    // Parameterless constructor for EF Core materialization
    private CustomerAggregate() : base(CustomerId.Create(Guid.NewGuid()))
    {
        Name = string.Empty;
    }

    public CustomerAggregate(CustomerId id, string name) : base(id)
    {
        Name = name;
        RaiseDomainEvent(new CustomerRegisteredEvent(id, name));
    }

    public void UpdateName(string newName)
    {
        Name = newName;
        RaiseDomainEvent(new CustomerNameUpdatedEvent(Id, newName));
    }
}

public sealed class PlainEntity
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
}


