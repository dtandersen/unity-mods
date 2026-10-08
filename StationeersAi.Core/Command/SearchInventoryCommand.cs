using System.Collections.Generic;
using StationeersAi.Entity;
using StationeersAi.Query;

namespace StationeersAi.Command;

/// <summary>Ask what the player carries, by name fragment.</summary>
public sealed record SearchInventoryRequest(string? Query);

/// <summary>The stacks that matched; empty when none did.</summary>
public sealed record InventoryResult(
    Status Status, List<string> Errors, IReadOnlyList<ItemStack> Items) : BaseResult(Status, Errors);

/// <summary>Domain operation: search what the player carries and wears.</summary>
public sealed class SearchInventoryCommand : Command<SearchInventoryRequest, InventoryResult>
{
    private readonly InventoryIndex _inventory;

    public SearchInventoryCommand(InventoryIndex inventory)
    {
        _inventory = inventory;
    }

    public InventoryResult Execute(SearchInventoryRequest request)
    {
        return new InventoryResult(Status.SUCCESS, new List<string>(), _inventory.Search(request.Query));
    }
}
