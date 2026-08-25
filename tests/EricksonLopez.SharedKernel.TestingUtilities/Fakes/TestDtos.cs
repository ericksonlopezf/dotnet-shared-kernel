// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.SharedKernel.TestingUtilities.Fakes;

/// <summary>
/// Sample Data Transfer Object (DTO) containing multiple strongly-typed identifiers for serialization tests.
/// </summary>
public class OrderDto
{
    public OrderId Id { get; set; }
    public ProductCode Code { get; set; }
    public Quantity Quantity { get; set; }
}

