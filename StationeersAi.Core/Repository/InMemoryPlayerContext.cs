using System.Collections.Generic;
using StationeersAi.Entity;

namespace StationeersAi.Repository;

/// <summary>In-memory player context. Used by tests.</summary>
public sealed class InMemoryPlayerContext : IPlayerContext
{
    private readonly List<ItemStack> _inventory = new List<ItemStack>();

    public string? Name { get; set; }

    public Player? Status { get; set; }

    public IReadOnlyList<ItemStack> Inventory => _inventory;

    public void Carry(ItemStack stack)
    {
        _inventory.Add(stack);
    }
}
