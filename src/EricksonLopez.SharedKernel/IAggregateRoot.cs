// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.SharedKernel;

/// <summary>
/// Defines an aggregate root that acts as the transactional consistency boundary of a domain model.
/// </summary>
public interface IAggregateRoot : IHasDomainEvents;
